namespace ATS.Services.AuditTrail;

/// <summary>
/// Records every ATS command in the audit trail.
/// Two filters decide what lands here. The <c>ICommand&lt;TResponse&gt;</c> constraint
/// limits it to writes: MediatR only applies an open behaviour whose generic constraints
/// the request satisfies, so a query never enters this pipeline at all - a compile-time
/// guarantee rather than a runtime type check, which is why the constraint is here and not
/// an <c>is</c> test in the body. ValidationBehavior uses the same trick.
/// The <see cref="IsAtsCommand"/> check then limits it to this module, because behaviour
/// registration is container-wide rather than per-module.
/// </summary>
public class AtsAuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : ICommand<TResponse>
{
	private readonly IAtsAuditWriter _auditWriter;
	private readonly IAtsAuditChangeCollector _changeCollector;
	private readonly ICurrentUser _currentUser;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly AtsAuditOptions _options;
	private readonly ILogger<AtsAuditBehavior<TRequest, TResponse>> _logger;

	public AtsAuditBehavior(
		IAtsAuditWriter auditWriter,
		IAtsAuditChangeCollector changeCollector,
		ICurrentUser currentUser,
		IHttpContextAccessor httpContextAccessor,
		IOptions<AtsAuditOptions> options,
		ILogger<AtsAuditBehavior<TRequest, TResponse>> logger)
	{
		_auditWriter = auditWriter;
		_changeCollector = changeCollector;
		_currentUser = currentUser;
		_httpContextAccessor = httpContextAccessor;
		_options = options.Value;
		_logger = logger;
	}

	// Resolved once per closed generic type rather than per request: neither answer can
	// change for a given command type.
	//
	// AddOpenBehavior registers IPipelineBehavior<,> into the one shared container, and
	// every module calls its own Add*MediaTR against that same collection - so without
	// this namespace check the ATS behaviour also wraps Auth's LoginWeb, Logout and
	// IsAuthenticated commands. Those already go to PlatformLogging; this table is the
	// ATS trail. LoggingBehavior.GetApplicationName reads the root namespace the same way.
	private static readonly bool IsAtsCommand =
		typeof(TRequest).Namespace?.StartsWith("ATS.", StringComparison.Ordinal) == true;

	private static readonly bool IsAudited =
		!typeof(TRequest).IsDefined(typeof(SkipAuditAttribute), inherit: false);

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		if (!_options.Enabled || !IsAtsCommand || !IsAudited)
		{
			return await next();
		}

		var stopwatch = Stopwatch.StartNew();

		try
		{
			var response = await next();

			stopwatch.Stop();

			Record(request, stopwatch, AuditOutcome.Success, failureReason: null);

			return response;
		}
		catch (Exception exception)
		{
			stopwatch.Stop();

			// The attempt is recorded and the exception continues to the global handler,
			// so the caller still gets its normal error response. An audited failure is
			// exactly the case someone will come looking for later.
			Record(request, stopwatch, AuditOutcome.Failure, exception.Message);

			throw;
		}
	}

	private void Record(
		TRequest request,
		Stopwatch stopwatch,
		string outcome,
		string? failureReason)
	{
		try
		{
			// Every caller value is read here, inside the request scope. The drain runs on
			// its own scope long after the response was sent, where there is no
			// HttpContext and no claims principal to read.
			var entry = new AtsAuditEntry
			{
				AuditEntryId = Guid.CreateVersion7(),
				OccurredAt = DateTime.UtcNow,
				Action = ResolveAction(),
				Area = ResolveArea(),
				Outcome = outcome,
				FailureReason = Truncate(failureReason, 500),
				DurationMs = (int)Math.Min(stopwatch.ElapsedMilliseconds, int.MaxValue),
				UserId = _currentUser.UserId,
				UserEmail = Truncate(_currentUser.Email, 255),
				UserFullName = Truncate(_currentUser.FullName, 255),
				AtsRoleId = _currentUser.AtsRoleId,
				AtsClientId = _currentUser.AtsClientId,
				IsPlatformSuperAdmin = _currentUser.IsPlatformSuperAdmin,
				IpAddress = ResolveIpAddress(),
				TraceId = Truncate(Activity.Current?.TraceId.ToString(), 64),
				Payload = AtsAuditRedactor.Redact(request),

				// Read after the handler ran: the interceptor fills the collector during
				// SaveChanges, so before this point there is nothing to read.
				Changes = SerializeChanges()
			};

			_auditWriter.TryEnqueue(entry);
		}
		catch (Exception exception)
		{
			// Nothing about recording an action may break the action itself. A broken
			// audit path is a logged defect, not a failed request.
			_logger.LogError(
				exception,
				"Failed to record an ATS audit entry for {Request}",
				typeof(TRequest).Name);
		}
	}

	/// <summary>
	/// Serializes the field changes the interceptor collected, or null when the command
	/// changed nothing EF tracked - an ExecuteUpdateAsync path, or a handler that only
	/// read. Null rather than "[]" so the dialog can tell "nothing was captured" apart
	/// from "nothing changed".
	/// Deliberately not redacted: the value of a diff is the actual old value, which is
	/// what the user asked for. That makes this column as sensitive as the source data.
	/// </summary>
	private string? SerializeChanges()
	{
		var changes = _changeCollector.Changes;

		if (changes.Count == 0)
		{
			return null;
		}

		var json = JsonSerializer.Serialize(changes);

		// Same cap as the payload: a bulk save must not write an unbounded row.
		return json.Length > AtsAuditRedactor.MaxPayloadCharacters
			? AtsAuditRedactor.OversizedPayload
			: json;
	}

	// "AddUserCommand" reads as "AddUser" on screen. The suffixes are the two naming
	// conventions the ATS slices actually use.
	private static string ResolveAction()
	{
		var name = typeof(TRequest).Name;

		foreach (var suffix in new[] { "CommandRequest", "HandlerRequest", "Command" })
		{
			if (name.EndsWith(suffix, StringComparison.Ordinal) && name.Length > suffix.Length)
			{
				return name[..^suffix.Length];
			}
		}

		return Truncate(name, 120) ?? name;
	}

	// ATS.Features.UserManagement.Command.AddUser -> "UserManagement". The feature folder
	// is the closest thing the codebase has to a business area for a command.
	private static string ResolveArea()
	{
		var segments = typeof(TRequest).Namespace?
			.Split('.', StringSplitOptions.RemoveEmptyEntries)
			?? [];

		var featuresIndex = Array.IndexOf(segments, "Features");

		return featuresIndex >= 0 && featuresIndex + 1 < segments.Length
			? Truncate(segments[featuresIndex + 1], 80)!
			: "Unknown";
	}

	private string? ResolveIpAddress() =>
		Truncate(
			_httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
			64);

	private static string? Truncate(string? value, int maxLength) =>
		value is not null && value.Length > maxLength
			? value[..maxLength]
			: value;
}
