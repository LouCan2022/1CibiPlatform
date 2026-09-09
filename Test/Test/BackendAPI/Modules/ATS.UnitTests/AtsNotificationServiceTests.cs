using ATS.Constants;
using ATS.Data.DTO;
using ATS.Data.Entities;
using ATS.Data.Repository.Notifications;
using ATS.Hubs;
using ATS.Services.Notifications;
using BuildingBlocks.Pagination;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Test.BackendAPI.Modules.ATS.UnitTests;

/// <summary>
/// The two things that make the notification inbox trustworthy: a notification is stored
/// before it is pushed, and it only ever reaches the group belonging to its recipient.
/// </summary>
public class AtsNotificationServiceTests
{
	private readonly Mock<IAtsNotificationRepository> _repository = new();
	private readonly Mock<IHubContext<ATSHub, IATSClient>> _hubContext = new();
	private readonly Mock<IHubClients<IATSClient>> _clients = new();
	private readonly Mock<IATSClient> _client = new();
	private readonly AtsNotificationService _service;

	private static readonly Guid RecipientId = Guid.CreateVersion7();

	public AtsNotificationServiceTests()
	{
		_hubContext.Setup(x => x.Clients).Returns(_clients.Object);
		_clients.Setup(x => x.Group(It.IsAny<string>())).Returns(_client.Object);

		_service = new AtsNotificationService(
			_repository.Object,
			_hubContext.Object,
			new Mock<ILogger<AtsNotificationService>>().Object);
	}

	[Fact]
	public async Task RaiseAsync_ShouldPersistThenPush_WhenTheRecipientIsKnown()
	{
		AtsNotification? stored = null;

		_repository
			.Setup(x => x.AddAsync(It.IsAny<AtsNotification>(), It.IsAny<CancellationToken>()))
			.Callback<AtsNotification, CancellationToken>((notification, _) => stored = notification)
			.Returns(Task.CompletedTask);

		await _service.RaiseAsync(
			RecipientId,
			AtsNotificationType.ApplicationFormSubmitted,
			"Application form submitted",
			"Juan Dela Cruz completed the application form you sent.",
			"/s&i/ats/searchreport?search=Juan%20Dela%20Cruz",
			Guid.CreateVersion7(),
			CancellationToken.None);

		stored.Should().NotBeNull();
		stored!.RecipientUserId.Should().Be(RecipientId);
		stored.Type.Should().Be(AtsNotificationType.ApplicationFormSubmitted);
		stored.IsRead.Should().BeFalse();
		stored.NotificationId.Should().NotBe(Guid.Empty);

		// The group name has to be the canonical Guid string, because that is what
		// HubCallerContextExtensions.GetUserGroupName produces when the connection joins.
		_clients.Verify(x => x.Group(RecipientId.ToString()), Times.Once);
		_client.Verify(x => x.ReceiveNotification(It.IsAny<NotificationListDTO>()), Times.Once);
	}

	[Fact]
	public async Task RaiseAsync_ShouldNotPush_WhenPersistingFails()
	{
		_repository
			.Setup(x => x.AddAsync(It.IsAny<AtsNotification>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("database is unavailable"));

		// Must not throw: the caller has already committed the work this is announcing.
		await _service.RaiseAsync(
			RecipientId,
			AtsNotificationType.BulkUploadCompleted,
			"Bulk upload processed",
			"Your file created 12 orders.",
			"/s&i/ats/bulkuploads",
			Guid.CreateVersion7(),
			CancellationToken.None);

		// Pushing an unstored notification would show a toast for something absent from
		// the inbox, and leave the badge disagreeing with the list.
		_client.Verify(
			x => x.ReceiveNotification(It.IsAny<NotificationListDTO>()),
			Times.Never);
	}

	[Fact]
	public async Task RaiseAsync_ShouldSucceed_WhenThePushFailsAfterPersisting()
	{
		_repository
			.Setup(x => x.AddAsync(It.IsAny<AtsNotification>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		_client
			.Setup(x => x.ReceiveNotification(It.IsAny<NotificationListDTO>()))
			.ThrowsAsync(new InvalidOperationException("hub is unavailable"));

		// The row is saved, so the recipient still sees it on their next page load. A dead
		// socket must not surface as a failed submission.
		await _service.RaiseAsync(
			RecipientId,
			AtsNotificationType.OrderCompleted,
			"Order completed",
			"The order for Juan Dela Cruz is complete.",
			null,
			null,
			CancellationToken.None);

		_repository.Verify(
			x => x.AddAsync(It.IsAny<AtsNotification>(), It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task RaiseAsync_ShouldDoNothing_WhenThereIsNoRecipient()
	{
		// Orders placed through the public API have no ATS user behind them.
		await _service.RaiseAsync(
			Guid.Empty,
			AtsNotificationType.OrderCompleted,
			"Order completed",
			"body",
			null,
			null,
			CancellationToken.None);

		_repository.Verify(
			x => x.AddAsync(It.IsAny<AtsNotification>(), It.IsAny<CancellationToken>()),
			Times.Never);
		_client.Verify(
			x => x.ReceiveNotification(It.IsAny<NotificationListDTO>()),
			Times.Never);
	}

	[Fact]
	public async Task RaiseAsync_ShouldTruncate_WhenTheBodyExceedsTheColumnWidth()
	{
		AtsNotification? stored = null;

		_repository
			.Setup(x => x.AddAsync(It.IsAny<AtsNotification>(), It.IsAny<CancellationToken>()))
			.Callback<AtsNotification, CancellationToken>((notification, _) => stored = notification)
			.Returns(Task.CompletedTask);

		await _service.RaiseAsync(
			RecipientId,
			AtsNotificationType.TicketingFailed,
			new string('t', 400),
			new string('b', 900),
			null,
			null,
			CancellationToken.None);

		// A subject with an unusually long name must not turn a successful order into a
		// failed insert.
		stored.Should().NotBeNull();
		stored!.Title.Length.Should().Be(160);
		stored.Body.Length.Should().Be(500);
	}

	[Fact]
	public async Task RaiseForOrderAsync_ShouldAddressTheRequestorAndLinkToTheSubject()
	{
		var orderId = Guid.CreateVersion7();
		AtsNotification? stored = null;

		_repository
			.Setup(x => x.GetOrderTargetAsync(orderId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new NotificationOrderTargetDTO
			{
				EmailInvitationId = orderId,
				RequestorId = RecipientId,
				FirstName = "Juan",
				LastName = "Dela Cruz"
			});

		_repository
			.Setup(x => x.AddAsync(It.IsAny<AtsNotification>(), It.IsAny<CancellationToken>()))
			.Callback<AtsNotification, CancellationToken>((notification, _) => stored = notification)
			.Returns(Task.CompletedTask);

		await _service.RaiseForOrderAsync(
			orderId,
			AtsNotificationType.ApplicationFormSubmitted,
			CancellationToken.None);

		stored.Should().NotBeNull();
		stored!.RecipientUserId.Should().Be(RecipientId);
		stored.EntityId.Should().Be(orderId);
		stored.Body.Should().Contain("Juan Dela Cruz");

		// Deep-links to Orders & Reports pre-filtered to the subject, url-encoded.
		stored.LinkUrl.Should().Be("/s&i/ats/searchreport?search=Juan%20Dela%20Cruz");
	}

	[Fact]
	public async Task RaiseForOrderAsync_ShouldLinkToTheTicketingBoard_WhenTicketingFailed()
	{
		var orderId = Guid.CreateVersion7();
		AtsNotification? stored = null;

		_repository
			.Setup(x => x.GetOrderTargetAsync(orderId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new NotificationOrderTargetDTO
			{
				EmailInvitationId = orderId,
				RequestorId = RecipientId,
				FirstName = "Juan",
				LastName = "Dela Cruz"
			});

		_repository
			.Setup(x => x.AddAsync(It.IsAny<AtsNotification>(), It.IsAny<CancellationToken>()))
			.Callback<AtsNotification, CancellationToken>((notification, _) => stored = notification)
			.Returns(Task.CompletedTask);

		await _service.RaiseForOrderAsync(
			orderId,
			AtsNotificationType.TicketingFailed,
			CancellationToken.None);

		// A ticketing failure is actioned on the ticketing board, not the orders list.
		//
		// The LAST NAME alone, deliberately: that board ILIKEs FirstName and LastName as
		// separate columns, so "Juan Dela Cruz" would match neither and the link would open
		// an empty board. Orders & Reports concatenates them, which is why it gets the full
		// name - see the test above.
		stored!.LinkUrl.Should().Be("/s&i/ats/ticketingstatus?search=Dela%20Cruz");
	}

	[Fact]
	public async Task RaiseForOrderAsync_ShouldDoNothing_WhenTheOrderHasNoRequestor()
	{
		var orderId = Guid.CreateVersion7();

		_repository
			.Setup(x => x.GetOrderTargetAsync(orderId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new NotificationOrderTargetDTO
			{
				EmailInvitationId = orderId,
				RequestorId = null,
				FirstName = "Juan",
				LastName = "Dela Cruz"
			});

		await _service.RaiseForOrderAsync(
			orderId,
			AtsNotificationType.OrderCompleted,
			CancellationToken.None);

		_repository.Verify(
			x => x.AddAsync(It.IsAny<AtsNotification>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task RaiseForOrderAsync_ShouldDoNothing_WhenTheOrderIsUnknown()
	{
		var orderId = Guid.CreateVersion7();

		_repository
			.Setup(x => x.GetOrderTargetAsync(orderId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((NotificationOrderTargetDTO?)null);

		await _service.RaiseForOrderAsync(
			orderId,
			AtsNotificationType.OrderCompleted,
			CancellationToken.None);

		_repository.Verify(
			x => x.AddAsync(It.IsAny<AtsNotification>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task GetNotificationsAsync_ShouldReturnACursor_WhenMoreRowsExist()
	{
		const int pageSize = 2;

		// The repository is asked for pageSize + 1 so the service can tell whether another
		// page exists without a second count query.
		var rows = Enumerable.Range(0, pageSize + 1)
			.Select(index => new NotificationListDTO
			{
				NotificationId = Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow.AddMinutes(-index),
				Type = AtsNotificationType.OrderCompleted,
				Title = $"Order {index}",
				Body = "body"
			})
			.ToList();

		_repository
			.Setup(x => x.GetNotificationsPageAsync(
				RecipientId,
				null,
				null,
				pageSize + 1,
				false,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(rows);

		_repository
			.Setup(x => x.CountNotificationsAsync(RecipientId, false, It.IsAny<CancellationToken>()))
			.ReturnsAsync(3);

		var result = await _service.GetNotificationsAsync(
			RecipientId,
			new KeysetPaginationRequest(null, pageSize),
			unreadOnly: false,
			CancellationToken.None);

		result.Items.Should().HaveCount(pageSize);
		result.NextCursor.Should().NotBeNull();

		// The total is only counted on the first page; cursor pages reuse it.
		result.TotalCount.Should().Be(3);
	}
}
