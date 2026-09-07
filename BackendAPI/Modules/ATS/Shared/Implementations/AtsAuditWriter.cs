using System.Threading.Channels;

namespace ATS.Shared.Implementations;

/// <summary>
/// The in-memory queue between an audited request and the database. Singleton, because the
/// queue has to outlive the request scope that writes to it.
/// The same shape PostgreSqlBatchingSink uses for platform logs: a bounded channel that
/// drops rather than blocks, so a slow or unreachable database degrades the audit trail
/// instead of the application.
/// </summary>
public sealed class AtsAuditWriter : IAtsAuditWriter
{
	private readonly Channel<AtsAuditEntry> _channel;
	private readonly ILogger<AtsAuditWriter> _logger;

	public AtsAuditWriter(
		IOptions<AtsAuditOptions> options,
		ILogger<AtsAuditWriter> logger)
	{
		_logger = logger;

		_channel = Channel.CreateBounded<AtsAuditEntry>(
			new BoundedChannelOptions(Math.Max(100, options.Value.BufferSize))
			{
				// One drain service reads it.
				SingleReader = true,

				// DropWrite over Wait: waiting would push database latency back onto the
				// request thread, which is the one thing this queue exists to prevent.
				FullMode = BoundedChannelFullMode.DropWrite
			});
	}

	public ChannelReader<AtsAuditEntry> Reader => _channel.Reader;

	public bool TryEnqueue(AtsAuditEntry entry)
	{
		if (_channel.Writer.TryWrite(entry))
		{
			return true;
		}

		// Logged rather than thrown. A dropped entry is a real gap in the trail, so it is
		// a warning, but it must not turn a successful command into a failed request.
		_logger.LogWarning(
			"The ATS audit queue is full; the entry for {Action} by {UserId} was dropped",
			entry.Action,
			entry.UserId);

		return false;
	}

	/// <summary>
	/// Stops the queue accepting entries, so the drain can finish the backlog and exit
	/// rather than waiting forever on a channel nobody will write to again.
	/// </summary>
	public void Complete() => _channel.Writer.TryComplete();
}
