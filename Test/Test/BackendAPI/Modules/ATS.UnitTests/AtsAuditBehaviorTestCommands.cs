using ATS.Services.AuditTrail;
using BuildingBlocks.CQRS;

// Fakes for AtsAuditBehaviorTests, in their own file because they must sit in real module
// namespaces rather than the test's own.
//
// AtsAuditBehavior audits only commands whose root namespace is ATS, and resolves Area
// from the segment after "Features", so a fake has to be namespaced like a genuine slice
// for either to be exercised.
namespace ATS.Features.ThingManagement.Command.AddThing
{
	public record AddThingCommand(string Name, string? SSS = null) : ICommand<ThingResult>;

	public record ThingResult(bool Added);

	[SkipAudit]
	public record ChattyCommand(string Question) : ICommand<ThingResult>;
}

// A command from another module. AddOpenBehavior registers IPipelineBehavior<,> into the
// one shared container, so Auth's commands really do reach the ATS behaviour at runtime -
// this fake is what proves the behaviour leaves them to PlatformLogging.
namespace Auth.Features.Login.Command.LoginWeb
{
	using ATS.Features.ThingManagement.Command.AddThing;

	public record LoginWebCommand(string Email) : ICommand<ThingResult>;
}
