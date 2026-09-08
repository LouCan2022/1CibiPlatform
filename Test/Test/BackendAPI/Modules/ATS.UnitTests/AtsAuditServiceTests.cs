using ATS.Constants;
using ATS.Data.DTO;
using ATS.Data.Repository.AuditTrail;
using ATS.Services.AuditTrail;
using Auth.Shared.Contracts;
using BuildingBlocks.Pagination;
using Microsoft.Extensions.Logging;
using Moq;

namespace Test.BackendAPI.Modules.ATS.UnitTests;

public class AtsAuditServiceTests
{
	private readonly Mock<IAtsAuditRepository> _repository = new();
	private readonly Mock<ICurrentUser> _currentUser = new();
	private readonly AtsAuditService _service;

	public AtsAuditServiceTests()
	{
		_service = new AtsAuditService(
			_repository.Object,
			_currentUser.Object,
			Mock.Of<ILogger<AtsAuditService>>());
	}

	private void GivenSuperAdmin()
	{
		_currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
		_currentUser.SetupGet(x => x.IsPlatformSuperAdmin).Returns(true);
	}

	private void GivenOrdinaryUser()
	{
		_currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
		_currentUser.SetupGet(x => x.IsPlatformSuperAdmin).Returns(false);
	}

	private static AuditTrailListDTO Entry(DateTime occurredAt) => new()
	{
		AuditEntryId = Guid.CreateVersion7(),
		OccurredAt = occurredAt,
		Action = "AddClient",
		Area = "ClientManagement",
		Outcome = AuditOutcome.Success
	};

	private void GivenPage(params AuditTrailListDTO[] rows) =>
		_repository
			.Setup(x => x.GetAuditTrailPageAsync(
				It.IsAny<DateTime?>(),
				It.IsAny<Guid?>(),
				It.IsAny<int>(),
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(rows.ToList());

	[Fact]
	public async Task GetAuditTrailAsync_ShouldReturnAnEmptyPage_WhenTheCallerIsNotASuperAdmin()
	{
		GivenOrdinaryUser();

		var result = await _service.GetAuditTrailAsync(
			new KeysetPaginationRequest(),
			outcome: null,
			action: null,
			area: null,
			CancellationToken.None);

		// An out-of-scope caller reads an empty list rather than a 403, the same as every
		// other ATS list - and the repository is never asked.
		Assert.Empty(result.Items);
		Assert.Equal(0, result.TotalCount);

		_repository.Verify(
			x => x.GetAuditTrailPageAsync(
				It.IsAny<DateTime?>(),
				It.IsAny<Guid?>(),
				It.IsAny<int>(),
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task GetOutcomeCountsAsync_ShouldReturnZeroes_WhenTheCallerIsNotASuperAdmin()
	{
		GivenOrdinaryUser();

		var counts = await _service.GetOutcomeCountsAsync(
			action: null,
			area: null,
			searchTerm: null,
			startDate: null,
			endDate: null,
			CancellationToken.None);

		Assert.Equal(0, counts.Total);
		Assert.Equal(0, counts.Success);
		Assert.Equal(0, counts.Failure);
	}

	[Fact]
	public async Task GetAuditTrailAsync_ShouldReturnTheRows_WhenTheCallerIsASuperAdmin()
	{
		GivenSuperAdmin();
		GivenPage(Entry(DateTime.UtcNow));

		var result = await _service.GetAuditTrailAsync(
			new KeysetPaginationRequest(),
			outcome: null,
			action: null,
			area: null,
			CancellationToken.None);

		Assert.Single(result.Items);

		// Last page, so no cursor is minted.
		Assert.Null(result.NextCursor);
	}

	[Fact]
	public async Task GetAuditTrailAsync_ShouldMintACursorAndTrimTheExtraRow_WhenMorePagesExist()
	{
		GivenSuperAdmin();

		var pageSize = 2;
		var newest = DateTime.UtcNow;

		// The repository is asked for pageSize + 1; the extra row only signals that a next
		// page exists and must not reach the caller.
		GivenPage(
			Entry(newest),
			Entry(newest.AddMinutes(-1)),
			Entry(newest.AddMinutes(-2)));

		var result = await _service.GetAuditTrailAsync(
			new KeysetPaginationRequest(PageSize: pageSize),
			outcome: null,
			action: null,
			area: null,
			CancellationToken.None);

		Assert.Equal(pageSize, result.Items.Count);
		Assert.NotNull(result.NextCursor);
	}

	[Fact]
	public async Task GetAuditTrailAsync_ShouldRoundTripItsOwnCursor()
	{
		GivenSuperAdmin();

		var newest = DateTime.UtcNow;
		GivenPage(Entry(newest), Entry(newest.AddMinutes(-1)));

		var firstPage = await _service.GetAuditTrailAsync(
			new KeysetPaginationRequest(PageSize: 1),
			outcome: null,
			action: null,
			area: null,
			CancellationToken.None);

		DateTime? seenAfter = null;

		_repository
			.Setup(x => x.GetAuditTrailPageAsync(
				It.IsAny<DateTime?>(),
				It.IsAny<Guid?>(),
				It.IsAny<int>(),
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<CancellationToken>()))
			.Callback((DateTime? after, Guid? _, int _, string? _, string? _, string? _, string? _, DateTime? _, DateTime? _, CancellationToken _) => seenAfter = after)
			.ReturnsAsync([]);

		await _service.GetAuditTrailAsync(
			new KeysetPaginationRequest(Cursor: firstPage.NextCursor, PageSize: 1),
			outcome: null,
			action: null,
			area: null,
			CancellationToken.None);

		// The cursor decodes back to the last row's timestamp, so the walk resumes rather
		// than restarting.
		Assert.NotNull(seenAfter);
	}

	[Fact]
	public async Task GetAuditTrailAsync_ShouldStartFromTheFirstPage_WhenTheCursorIsMalformed()
	{
		GivenSuperAdmin();
		GivenPage(Entry(DateTime.UtcNow));

		DateTime? seenAfter = DateTime.UtcNow;

		_repository
			.Setup(x => x.GetAuditTrailPageAsync(
				It.IsAny<DateTime?>(),
				It.IsAny<Guid?>(),
				It.IsAny<int>(),
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<CancellationToken>()))
			.Callback((DateTime? after, Guid? _, int _, string? _, string? _, string? _, string? _, DateTime? _, DateTime? _, CancellationToken _) => seenAfter = after)
			.ReturnsAsync([Entry(DateTime.UtcNow)]);

		var result = await _service.GetAuditTrailAsync(
			new KeysetPaginationRequest(Cursor: "not-a-cursor"),
			outcome: null,
			action: null,
			area: null,
			CancellationToken.None);

		// Self-heals rather than failing the request.
		Assert.Null(seenAfter);
		Assert.Single(result.Items);
	}

	[Theory]
	[InlineData("success", AuditOutcome.Success)]
	[InlineData("  FAILURE  ", AuditOutcome.Failure)]
	public async Task GetAuditTrailAsync_ShouldCanonicalizeTheOutcomeFilter(
		string supplied,
		string expected)
	{
		GivenSuperAdmin();

		string? seenOutcome = null;

		_repository
			.Setup(x => x.GetAuditTrailPageAsync(
				It.IsAny<DateTime?>(),
				It.IsAny<Guid?>(),
				It.IsAny<int>(),
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<CancellationToken>()))
			.Callback((DateTime? _, Guid? _, int _, string? outcome, string? _, string? _, string? _, DateTime? _, DateTime? _, CancellationToken _) => seenOutcome = outcome)
			.ReturnsAsync([]);

		await _service.GetAuditTrailAsync(
			new KeysetPaginationRequest(),
			supplied,
			action: null,
			area: null,
			CancellationToken.None);

		Assert.Equal(expected, seenOutcome);
	}

	[Fact]
	public async Task GetAuditTrailAsync_ShouldIgnoreAnUnknownOutcomeFilter()
	{
		GivenSuperAdmin();

		string? seenOutcome = "sentinel";

		_repository
			.Setup(x => x.GetAuditTrailPageAsync(
				It.IsAny<DateTime?>(),
				It.IsAny<Guid?>(),
				It.IsAny<int>(),
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<CancellationToken>()))
			.Callback((DateTime? _, Guid? _, int _, string? outcome, string? _, string? _, string? _, DateTime? _, DateTime? _, CancellationToken _) => seenOutcome = outcome)
			.ReturnsAsync([]);

		await _service.GetAuditTrailAsync(
			new KeysetPaginationRequest(),
			outcome: "Sideways",
			action: null,
			area: null,
			CancellationToken.None);

		// An unrecognised status would otherwise reach the repository as a literal filter
		// and silently return nothing.
		Assert.Null(seenOutcome);
	}
}
