namespace ATS.Services.AuditTrail;

/// <summary>
/// Marks a command that must not be recorded in the audit trail.
/// Opt-out rather than opt-in on purpose: a new command is audited by default, so
/// forgetting to annotate it leaves a gap in the trail rather than a silent blind spot.
/// Reserved for commands that are not really state changes - the AI assistant's
/// conversational turns, whose question text would bury the log in noise.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SkipAuditAttribute : Attribute
{
}
