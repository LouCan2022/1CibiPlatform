namespace ATS.Constants;

/// <summary>
/// Whether an audited command completed or threw. Public, like <see cref="TicketStatus"/>,
/// because the audit trail screen filters on this vocabulary.
/// </summary>
public static class AuditOutcome
{
	public const string Success = "Success";

	// The handler threw. The request still failed normally for the caller - the audit
	// entry records that the attempt was made, not that it was swallowed.
	public const string Failure = "Failure";

	public static readonly string[] All = [Success, Failure];
}
