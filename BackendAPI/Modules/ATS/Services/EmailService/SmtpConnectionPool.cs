namespace ATS.Services.EmailService;

/// <summary>
/// A small pool of authenticated MailKit SMTP sessions, leased one at a time.
///
/// This exists because the previous implementation built a <c>System.Net.Mail.SmtpClient</c>
/// per message, so 200 invitations meant 200 TCP connects, 200 TLS handshakes and 200
/// AUTH LOGINs from a single account. Providers rate limit authentication independently of
/// send volume, and that login burst - not the message count - is what stopped this sender
/// after 14 messages. A pooled session sends many messages under one login, which is the
/// shape an ordinary mail client has.
///
/// Singleton: a pool that does not outlive the request scope is not a pool.
/// </summary>
public sealed class SmtpConnectionPool : IAsyncDisposable
{
	private readonly ILogger<SmtpConnectionPool> _logger;
	private readonly AtsEmailDeliveryOptions _options;
	private readonly SemaphoreSlim _available;
	private readonly ConcurrentBag<PooledConnection> _idle = [];
	private readonly string _host;
	private readonly int _port;
	private readonly string _senderEmail;
	private readonly string _appPassword;

	private bool _disposed;

	public SmtpConnectionPool(
		IConfiguration configuration,
		IOptions<AtsEmailDeliveryOptions> options,
		ILogger<SmtpConnectionPool> logger)
	{
		_logger = logger;
		_options = options.Value;

		_senderEmail = configuration["Email:ATSGmail:SenderEmail"]
			?? throw new InvalidOperationException("Email:ATSGmail:SenderEmail not configured");
		_appPassword = configuration["Email:ATSGmail:AppPassword"]
			?? throw new InvalidOperationException("Email:ATSGmail:AppPassword not configured");
		_host = configuration["Email:Gmail:SmtpHost"] ?? "smtp.gmail.com";
		_port = int.Parse(configuration["Email:Gmail:SmtpPort"] ?? "587");

		var maxConnections = _options.MaxConcurrentConnections > 0
			? _options.MaxConcurrentConnections
			: new AtsEmailDeliveryOptions().MaxConcurrentConnections;

		_available = new SemaphoreSlim(maxConnections, maxConnections);
	}

	public string SenderEmail => _senderEmail;

	/// <summary>
	/// Leases a connected, authenticated session. Dispose the lease to return it to the
	/// pool. A lease whose connection faulted is discarded rather than returned, so the
	/// next caller never inherits a half-dead session.
	/// </summary>
	public async Task<SmtpLease> AcquireAsync(CancellationToken cancellationToken)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		await _available.WaitAsync(cancellationToken);

		try
		{
			// Reuse an idle session when one is healthy and has budget left; otherwise
			// build a fresh one. Retire-by-count is deliberate: providers cap how long a
			// single authenticated session may live, and a very old connection is also
			// more likely to have gone silently stale behind a NAT.
			while (_idle.TryTake(out var pooled))
			{
				if (pooled.IsUsable(_options.MaxMessagesPerConnection))
				{
					return new SmtpLease(this, pooled);
				}

				await pooled.DisposeAsync();
			}

			var connection = await CreateConnectionAsync(cancellationToken);

			return new SmtpLease(this, connection);
		}
		catch
		{
			// The slot must come back even when connecting failed, or a provider outage
			// permanently shrinks the pool.
			_available.Release();
			throw;
		}
	}

	private async Task<PooledConnection> CreateConnectionAsync(CancellationToken cancellationToken)
	{
		var client = new MailKit.Net.Smtp.SmtpClient
		{
			// Applies to every network operation on this client. Generous on purpose: a
			// tight timeout fires while the provider has already accepted the message,
			// which records a false failure and triggers a duplicate send on retry.
			Timeout = (int)TimeSpan.FromSeconds(_options.SendTimeoutSeconds).TotalMilliseconds
		};

		await client.ConnectAsync(
			_host,
			_port,
			MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable,
			cancellationToken);

		await client.AuthenticateAsync(_senderEmail, _appPassword, cancellationToken);

		_logger.LogInformation(
			"Opened SMTP session to {Host}:{Port} as {Sender}.",
			_host,
			_port,
			_senderEmail);

		return new PooledConnection(client);
	}

	internal async ValueTask ReturnAsync(PooledConnection connection, bool isHealthy)
	{
		try
		{
			if (isHealthy && !_disposed && connection.IsUsable(_options.MaxMessagesPerConnection))
			{
				_idle.Add(connection);
				return;
			}

			await connection.DisposeAsync();
		}
		finally
		{
			_available.Release();
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;

		while (_idle.TryTake(out var pooled))
		{
			await pooled.DisposeAsync();
		}

		_available.Dispose();
	}

	/// <summary>One authenticated session plus the count of messages it has carried.</summary>
	internal sealed class PooledConnection(MailKit.Net.Smtp.SmtpClient client) : IAsyncDisposable
	{
		public MailKit.Net.Smtp.SmtpClient Client { get; } = client;

		public int MessagesSent { get; private set; }

		public void RecordSend() => MessagesSent++;

		public bool IsUsable(int maxMessages) =>
			Client.IsConnected
			&& Client.IsAuthenticated
			&& MessagesSent < maxMessages;

		public async ValueTask DisposeAsync()
		{
			// A QUIT the server never hears is not worth failing over; the socket is being
			// torn down either way.
			if (Client.IsConnected)
			{
				await SideEffectGuard.RunAsync(
					() => Client.DisconnectAsync(true),
					NullLogger.Instance,
					"close an SMTP session");
			}

			Client.Dispose();
		}
	}
}

/// <summary>
/// A borrowed SMTP session. Mark it faulted when the connection itself misbehaved, so it
/// is torn down instead of handed to the next caller.
/// </summary>
public sealed class SmtpLease : IAsyncDisposable
{
	private readonly SmtpConnectionPool _pool;
	private readonly SmtpConnectionPool.PooledConnection _connection;

	private bool _faulted;

	internal SmtpLease(SmtpConnectionPool pool, SmtpConnectionPool.PooledConnection connection)
	{
		_pool = pool;
		_connection = connection;
	}

	public MailKit.Net.Smtp.SmtpClient Client => _connection.Client;

	public void RecordSend() => _connection.RecordSend();

	public void MarkFaulted() => _faulted = true;

	public ValueTask DisposeAsync() => _pool.ReturnAsync(_connection, !_faulted);
}
