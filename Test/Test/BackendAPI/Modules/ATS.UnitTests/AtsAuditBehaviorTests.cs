using System.Text.Json;
using ATS.Configuration;
using ATS.Constants;
using ATS.Data.DTO;
using ATS.Data.Entities;
using ATS.Services.AuditTrail;
using ATS.Shared.Contracts;
using Auth.Shared.Contracts;
using BuildingBlocks.CQRS;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

// The command fakes these tests run against live in AtsAuditBehaviorTestCommands.cs,
// because they must sit in real module namespaces rather than this one.
using ATS.Features.ThingManagement.Command.AddThing;
using Auth.Features.Login.Command.LoginWeb;

namespace Test.BackendAPI.Modules.ATS.UnitTests;

public class AtsAuditBehaviorTests
{
	private static readonly Guid UserId = Guid.CreateVersion7();

	private readonly Mock<IAtsAuditWriter> _writer = new();
	private readonly Mock<ICurrentUser> _currentUser = new();
	private readonly List<AtsAuditEntry> _recorded = [];

	// The real collector rather than a mock: it is a plain accumulator, and using it means
	// the cap it enforces is exercised too.
	private readonly AtsAuditChangeCollector _changeCollector = new();

	public AtsAuditBehaviorTests()
	{
		_writer
			.Setup(x => x.TryEnqueue(It.IsAny<AtsAuditEntry>()))
			.Callback<AtsAuditEntry>(_recorded.Add)
			.Returns(true);

		_currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
		_currentUser.SetupGet(x => x.UserId).Returns(UserId);
		_currentUser.SetupGet(x => x.Email).Returns("jane.cruz@cibi.com.ph");
		_currentUser.SetupGet(x => x.FullName).Returns("Jane Cruz");
		_currentUser.SetupGet(x => x.AtsRoleId).Returns(3);
		_currentUser.SetupGet(x => x.AtsClientId).Returns(7);
		_currentUser.SetupGet(x => x.IsPlatformSuperAdmin).Returns(false);
	}

	private AtsAuditBehavior<TRequest, ThingResult> BehaviorFor<TRequest>(
		AtsAuditOptions? options = null)
		where TRequest : ICommand<ThingResult>
	{
		var httpContextAccessor = new Mock<IHttpContextAccessor>();
		var context = new DefaultHttpContext();
		context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.1.2.3");
		httpContextAccessor.SetupGet(x => x.HttpContext).Returns(context);

		return new AtsAuditBehavior<TRequest, ThingResult>(
			_writer.Object,
			_changeCollector,
			_currentUser.Object,
			httpContextAccessor.Object,
			Options.Create(options ?? new AtsAuditOptions()),
			Mock.Of<ILogger<AtsAuditBehavior<TRequest, ThingResult>>>());
	}

	private static RequestHandlerDelegate<ThingResult> Succeeds() =>
		() => Task.FromResult(new ThingResult(true));

	private static RequestHandlerDelegate<ThingResult> Throws(Exception exception) =>
		() => Task.FromException<ThingResult>(exception);

	[Fact]
	public async Task Handle_ShouldRecordASuccess_WhenTheHandlerCompletes()
	{
		var behavior = BehaviorFor<AddThingCommand>();

		var response = await behavior.Handle(
			new AddThingCommand("Acme"),
			Succeeds(),
			CancellationToken.None);

		Assert.True(response.Added);

		var entry = Assert.Single(_recorded);

		Assert.Equal(AuditOutcome.Success, entry.Outcome);
		Assert.Null(entry.FailureReason);

		// The suffix is trimmed so the screen reads "AddThing", not "AddThingCommand".
		Assert.Equal("AddThing", entry.Action);
	}

	[Fact]
	public async Task Handle_ShouldCaptureTheCallerAsTheyWereAtTheTime()
	{
		var behavior = BehaviorFor<AddThingCommand>();

		await behavior.Handle(new AddThingCommand("Acme"), Succeeds(), CancellationToken.None);

		var entry = Assert.Single(_recorded);

		// Denormalised onto the row: the drain runs outside the request scope, and a later
		// role change must not rewrite history.
		Assert.Equal(UserId, entry.UserId);
		Assert.Equal("jane.cruz@cibi.com.ph", entry.UserEmail);
		Assert.Equal("Jane Cruz", entry.UserFullName);
		Assert.Equal(3, entry.AtsRoleId);
		Assert.Equal(7, entry.AtsClientId);
		Assert.False(entry.IsPlatformSuperAdmin);
		Assert.Equal("10.1.2.3", entry.IpAddress);
	}

	[Fact]
	public async Task Handle_ShouldRedactThePayload()
	{
		var behavior = BehaviorFor<AddThingCommand>();

		await behavior.Handle(
			new AddThingCommand("Acme", SSS: "1111111110"),
			Succeeds(),
			CancellationToken.None);

		var entry = Assert.Single(_recorded);

		Assert.DoesNotContain("1111111110", entry.Payload, StringComparison.Ordinal);
		Assert.Contains("Acme", entry.Payload, StringComparison.Ordinal);
	}

	[Fact]
	public async Task Handle_ShouldRecordAFailureAndStillThrow_WhenTheHandlerThrows()
	{
		var behavior = BehaviorFor<AddThingCommand>();

		// The caller must still get its normal error; auditing the attempt does not
		// swallow it.
		var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
			behavior.Handle(
				new AddThingCommand("Acme"),
				Throws(new InvalidOperationException("The client already exists.")),
				CancellationToken.None));

		Assert.Equal("The client already exists.", exception.Message);

		var entry = Assert.Single(_recorded);

		Assert.Equal(AuditOutcome.Failure, entry.Outcome);
		Assert.Equal("The client already exists.", entry.FailureReason);
	}

	[Fact]
	public async Task Handle_ShouldNotFailTheRequest_WhenTheAuditWriterThrows()
	{
		// A broken audit path is a logged defect, never a failed request.
		_writer
			.Setup(x => x.TryEnqueue(It.IsAny<AtsAuditEntry>()))
			.Throws(new InvalidOperationException("The queue is broken."));

		var behavior = BehaviorFor<AddThingCommand>();

		var response = await behavior.Handle(
			new AddThingCommand("Acme"),
			Succeeds(),
			CancellationToken.None);

		Assert.True(response.Added);
	}

	// Regression: the first build of this screen filled up with Auth's LoginWeb, Logout,
	// IsAuthenticated and GetNewAccessToken rows. AddOpenBehavior registers
	// IPipelineBehavior<,> into the one shared container and every module calls its own
	// Add*MediaTR against it, so the ATS behaviour really does wrap other modules'
	// commands. Those already go to PlatformLogging; this table is the ATS trail.
	[Fact]
	public async Task Handle_ShouldRecordNothing_WhenTheCommandBelongsToAnotherModule()
	{
		var behavior = BehaviorFor<LoginWebCommand>();

		var response = await behavior.Handle(
			new LoginWebCommand("admin@cibi.com"),
			Succeeds(),
			CancellationToken.None);

		// Passed straight through: not audited, and not interfered with either.
		Assert.True(response.Added);
		Assert.Empty(_recorded);
	}

	[Fact]
	public async Task Handle_ShouldStillRecordAFailure_WhenAnAtsCommandThrows()
	{
		// The module filter must not be so broad that it also drops ATS failures - the
		// screenshot's failed GetNewAccessToken row was Auth's, not ATS's.
		var behavior = BehaviorFor<AddThingCommand>();

		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			behavior.Handle(
				new AddThingCommand("Acme"),
				Throws(new InvalidOperationException("Nope.")),
				CancellationToken.None));

		Assert.Single(_recorded);
	}

	[Fact]
	public async Task Handle_ShouldRecordTheFieldChangesTheHandlerMade()
	{
		var behavior = BehaviorFor<AddThingCommand>();

		// Stands in for the interceptor, which fills the collector during SaveChanges -
		// so the behaviour has to read it after next(), not before.
		RequestHandlerDelegate<ThingResult> savesChanges = () =>
		{
			_changeCollector.Add([
				new AtsEntityChangeDTO
				{
					Entity = "PackageDetails",
					Key = "12",
					State = "Modified",
					Changes = new Dictionary<string, AtsPropertyChangeDTO>
					{
						["IsActive"] = new("True", "False")
					}
				}
			]);

			return Task.FromResult(new ThingResult(true));
		};

		await behavior.Handle(new AddThingCommand("Acme"), savesChanges, CancellationToken.None);

		var entry = Assert.Single(_recorded);

		Assert.NotNull(entry.Changes);

		var changes = JsonSerializer.Deserialize<List<AtsEntityChangeDTO>>(entry.Changes!)!;
		var change = Assert.Single(changes);

		Assert.Equal("PackageDetails", change.Entity);
		Assert.Equal("12", change.Key);
		Assert.Equal("True", change.Changes["IsActive"].From);
		Assert.Equal("False", change.Changes["IsActive"].To);
	}

	[Fact]
	public async Task Handle_ShouldLeaveChangesNull_WhenNothingTrackedWasModified()
	{
		// The ExecuteUpdateAsync paths never populate the change tracker. Null rather than
		// "[]" so the dialog can tell "not captured" apart from "nothing changed".
		var behavior = BehaviorFor<AddThingCommand>();

		await behavior.Handle(new AddThingCommand("Acme"), Succeeds(), CancellationToken.None);

		Assert.Null(Assert.Single(_recorded).Changes);
	}

	[Fact]
	public async Task Handle_ShouldNotRedactTheDiff()
	{
		// Deliberate, and the reason this column is as sensitive as the source data: the
		// point of a diff is the actual old value. The payload is still masked.
		var behavior = BehaviorFor<AddThingCommand>();

		RequestHandlerDelegate<ThingResult> savesChanges = () =>
		{
			_changeCollector.Add([
				new AtsEntityChangeDTO
				{
					Entity = "PersonalDetails",
					State = "Modified",
					Changes = new Dictionary<string, AtsPropertyChangeDTO>
					{
						["SSS"] = new("1111111110", "2222222220")
					}
				}
			]);

			return Task.FromResult(new ThingResult(true));
		};

		await behavior.Handle(
			new AddThingCommand("Acme", SSS: "1111111110"),
			savesChanges,
			CancellationToken.None);

		var entry = Assert.Single(_recorded);

		Assert.Contains("1111111110", entry.Changes!, StringComparison.Ordinal);
		Assert.DoesNotContain("1111111110", entry.Payload, StringComparison.Ordinal);
	}

	[Fact]
	public async Task Handle_ShouldResolveTheAreaFromTheFeatureFolder()
	{
		var behavior = BehaviorFor<AddThingCommand>();

		await behavior.Handle(new AddThingCommand("Acme"), Succeeds(), CancellationToken.None);

		// ATS.Features.ThingManagement.Command.AddThing -> "ThingManagement".
		Assert.Equal("ThingManagement", Assert.Single(_recorded).Area);
	}

	[Fact]
	public async Task Handle_ShouldRecordNothing_WhenTheCommandOptsOut()
	{
		var behavior = BehaviorFor<ChattyCommand>();

		await behavior.Handle(new ChattyCommand("hello"), Succeeds(), CancellationToken.None);

		Assert.Empty(_recorded);
	}

	[Fact]
	public async Task Handle_ShouldRecordNothing_WhenAuditingIsDisabled()
	{
		var behavior = BehaviorFor<AddThingCommand>(new AtsAuditOptions { Enabled = false });

		var response = await behavior.Handle(
			new AddThingCommand("Acme"),
			Succeeds(),
			CancellationToken.None);

		Assert.True(response.Added);
		Assert.Empty(_recorded);
	}

	[Fact]
	public async Task Handle_ShouldTruncateAnOverLongFailureReason()
	{
		var behavior = BehaviorFor<AddThingCommand>();

		// The column is 500 characters; an over-long provider message must not fail the
		// write that records it.
		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			behavior.Handle(
				new AddThingCommand("Acme"),
				Throws(new InvalidOperationException(new string('x', 900))),
				CancellationToken.None));

		var entry = Assert.Single(_recorded);

		Assert.Equal(500, entry.FailureReason!.Length);
	}
}
