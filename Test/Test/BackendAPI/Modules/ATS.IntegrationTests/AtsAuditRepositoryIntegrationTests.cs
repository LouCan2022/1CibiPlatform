using ATS.Constants;
using ATS.Data.Context;
using ATS.Data.Entities;
using ATS.Data.Interceptors;
using ATS.Data.Repository.AuditTrail;
using ATS.Services.AuditTrail;

// Aliased because this test's own namespace contains an "ATS" segment, which makes a bare
// ATS.Data.DTO reference ambiguous.
using AuditTrailListDTO = ATS.Data.DTO.AuditTrailListDTO;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Test.BackendAPI.Infrastructure.ATS.Infrastracture;

namespace Test.BackendAPI.Modules.ATS.IntegrationTests;

public class AtsAuditRepositoryIntegrationTests : BaseIntegrationTest
{
	private readonly AtsAuditRepository _repository;

	public AtsAuditRepositoryIntegrationTests(IntegrationTestWebAppFactory factory)
		: base(factory)
	{
		_repository = new AtsAuditRepository(_dbContext);
	}

	private async Task<AtsAuditEntry> SeedEntryAsync(
		DateTime? occurredAt = null,
		string outcome = AuditOutcome.Success,
		string action = "AddClient",
		string area = "ClientManagement",
		string userFullName = "Jane Cruz",
		string userEmail = "jane.cruz@cibi.com.ph",
		string? site = "24 - 7 INTOUCH- CEBU",
		string payload = """{"Name":"Acme"}""")
	{
		var entry = new AtsAuditEntry
		{
			AuditEntryId = Guid.CreateVersion7(),
			OccurredAt = occurredAt ?? DateTime.UtcNow,
			Action = action,
			Area = area,
			Outcome = outcome,
			FailureReason = outcome == AuditOutcome.Failure ? "It already exists." : null,
			DurationMs = 42,
			UserId = Guid.CreateVersion7(),
			UserEmail = userEmail,
			UserFullName = userFullName,
			AtsRoleId = 3,
			AtsClientId = 7,
			Site = site,
			IsPlatformSuperAdmin = false,
			IpAddress = "10.1.2.3",
			TraceId = "0af7651916cd43dd8448eb211c80319c",
			Payload = payload
		};

		_dbContext.AuditTrail.Add(entry);
		await _dbContext.SaveChangesAsync();

		return entry;
	}

	private Task<List<AuditTrailListDTO>> PageAsync(
		DateTime? afterOccurredAt = null,
		Guid? afterEntryId = null,
		int take = 10,
		string? outcome = null,
		string? action = null,
		string? area = null,
		string? searchTerm = null,
		DateTime? startDate = null,
		DateTime? endDate = null) =>
		_repository.GetAuditTrailPageAsync(
			afterOccurredAt,
			afterEntryId,
			take,
			outcome,
			action,
			area,
			searchTerm,
			startDate,
			endDate,
			CancellationToken.None);

	[Fact]
	public async Task GetAuditTrailPageAsync_ShouldReturnNewestFirst()
	{
		var now = DateTime.UtcNow;

		await SeedEntryAsync(now.AddMinutes(-10), action: "Oldest");
		await SeedEntryAsync(now.AddMinutes(-5), action: "Middle");
		await SeedEntryAsync(now, action: "Newest");

		var page = await PageAsync();

		page.Select(entry => entry.Action)
			.Should().ContainInOrder("Newest", "Middle", "Oldest");
	}

	// The keyset walk is the part most likely to silently repeat or skip a row, which in
	// an audit trail would mean an action appearing twice or not at all.
	[Fact]
	public async Task GetAuditTrailPageAsync_ShouldWalkEveryEntryExactlyOnce()
	{
		var now = DateTime.UtcNow;

		for (var index = 0; index < 7; index++)
		{
			await SeedEntryAsync(now.AddMinutes(-index), action: $"Action{index}");
		}

		var seen = new List<Guid>();
		DateTime? afterOccurredAt = null;
		Guid? afterEntryId = null;

		while (true)
		{
			var page = await PageAsync(afterOccurredAt, afterEntryId, take: 3);

			if (page.Count == 0)
			{
				break;
			}

			seen.AddRange(page.Select(entry => entry.AuditEntryId));

			afterOccurredAt = page[^1].OccurredAt;
			afterEntryId = page[^1].AuditEntryId;
		}

		seen.Should().HaveCount(7);
		seen.Should().OnlyHaveUniqueItems();
	}

	// Entries written in the same batch can share a timestamp to the microsecond, so the
	// id tiebreaker is what keeps the walk from stalling or repeating.
	[Fact]
	public async Task GetAuditTrailPageAsync_ShouldNotRepeatRows_WhenTimestampsAreIdentical()
	{
		var sharedTimestamp = DateTime.UtcNow;

		for (var index = 0; index < 4; index++)
		{
			await SeedEntryAsync(sharedTimestamp, action: $"Action{index}");
		}

		var firstPage = await PageAsync(take: 2);

		var secondPage = await PageAsync(
			firstPage[^1].OccurredAt,
			firstPage[^1].AuditEntryId,
			take: 2);

		secondPage.Should().HaveCount(2);

		secondPage.Select(entry => entry.AuditEntryId)
			.Should().NotIntersectWith(firstPage.Select(entry => entry.AuditEntryId));
	}

	[Fact]
	public async Task GetAuditTrailPageAsync_ShouldFilterByOutcome()
	{
		await SeedEntryAsync(outcome: AuditOutcome.Success, action: "Worked");
		await SeedEntryAsync(outcome: AuditOutcome.Failure, action: "Broke");

		var failures = await PageAsync(outcome: AuditOutcome.Failure);

		var entry = failures.Should().ContainSingle().Subject;

		entry.Action.Should().Be("Broke");
		entry.FailureReason.Should().Be("It already exists.");
	}

	[Fact]
	public async Task GetAuditTrailPageAsync_ShouldFilterByActionAndArea()
	{
		await SeedEntryAsync(action: "AddUser", area: "UserManagement");
		await SeedEntryAsync(action: "AddClient", area: "ClientManagement");

		var byAction = await PageAsync(action: "AddUser");
		var byArea = await PageAsync(area: "ClientManagement");

		byAction.Should().ContainSingle().Which.Area.Should().Be("UserManagement");
		byArea.Should().ContainSingle().Which.Action.Should().Be("AddClient");
	}

	[Theory]
	[InlineData("jane")]
	[InlineData("JANE")]
	[InlineData("cibi.com.ph")]
	[InlineData("AddClient")]
	[InlineData("ClientManagement")]
	[InlineData("CEBU")]
	[InlineData("intouch")]
	public async Task GetAuditTrailPageAsync_ShouldSearchWhoAndWhat(string searchTerm)
	{
		await SeedEntryAsync();
		await SeedEntryAsync(
			action: "EditPackage",
			area: "PackageManagement",
			userFullName: "Pedro Santos",
			userEmail: "pedro@example.com",
			site: "CIBI MANILA");

		var results = await PageAsync(searchTerm: searchTerm);

		results.Should().ContainSingle().Which.UserFullName.Should().Be("Jane Cruz");
	}

	// A platform super admin who was never granted an ATS module has no UserDetails row,
	// so the drain leaves Site null. The row must still be searchable by everything else.
	[Fact]
	public async Task GetAuditTrailPageAsync_ShouldStillMatch_WhenTheEntryHasNoSite()
	{
		await SeedEntryAsync(site: null);

		var results = await PageAsync(searchTerm: "jane");

		results.Should().ContainSingle().Which.Site.Should().BeNull();
	}

	[Fact]
	public async Task GetAuditTrailPageAsync_ShouldNotSearchThePayload()
	{
		// The payload holds redacted content and is not indexed; searching it would also
		// let a term reach masked values.
		await SeedEntryAsync(payload: """{"Name":"Distinctivename"}""");

		var results = await PageAsync(searchTerm: "Distinctivename");

		results.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAuditTrailPageAsync_ShouldFilterByDateRange()
	{
		var now = DateTime.UtcNow;

		await SeedEntryAsync(now.AddDays(-10), action: "TooOld");
		await SeedEntryAsync(now, action: "InRange");

		var results = await PageAsync(startDate: now.AddDays(-1), endDate: now);

		results.Should().ContainSingle().Which.Action.Should().Be("InRange");
	}

	[Fact]
	public async Task GetAuditTrailPageAsync_ShouldRoundTripEveryStoredField()
	{
		var seeded = await SeedEntryAsync();

		var entry = (await PageAsync()).Should().ContainSingle().Subject;

		entry.AuditEntryId.Should().Be(seeded.AuditEntryId);
		entry.UserId.Should().Be(seeded.UserId);
		entry.UserEmail.Should().Be(seeded.UserEmail);
		entry.UserFullName.Should().Be(seeded.UserFullName);
		entry.AtsRoleId.Should().Be(3);
		entry.AtsClientId.Should().Be(7);
		entry.Site.Should().Be("24 - 7 INTOUCH- CEBU");
		entry.IsPlatformSuperAdmin.Should().BeFalse();
		entry.IpAddress.Should().Be("10.1.2.3");
		entry.TraceId.Should().Be(seeded.TraceId);
		entry.DurationMs.Should().Be(42);

		// jsonb is a parsed representation, not the original text: Postgres re-emits it
		// with its own spacing and key order, so the payload is compared as JSON rather
		// than byte for byte. The detail panel re-indents it anyway.
		JsonDocument.Parse(entry.Payload).RootElement
			.GetProperty("Name").GetString()
			.Should().Be("Acme");
	}

	[Fact]
	public async Task CountAuditTrailAsync_ShouldRespectTheSameFilters()
	{
		await SeedEntryAsync(outcome: AuditOutcome.Success);
		await SeedEntryAsync(outcome: AuditOutcome.Success);
		await SeedEntryAsync(outcome: AuditOutcome.Failure);

		var all = await _repository.CountAuditTrailAsync(
			null, null, null, null, null, null, CancellationToken.None);

		var failures = await _repository.CountAuditTrailAsync(
			AuditOutcome.Failure, null, null, null, null, null, CancellationToken.None);

		all.Should().Be(3);
		failures.Should().Be(1);
	}

	[Fact]
	public async Task GetOutcomeCountsAsync_ShouldCountEveryBucketRegardlessOfTheSelectedOne()
	{
		await SeedEntryAsync(outcome: AuditOutcome.Success);
		await SeedEntryAsync(outcome: AuditOutcome.Success);
		await SeedEntryAsync(outcome: AuditOutcome.Failure);

		var counts = await _repository.GetOutcomeCountsAsync(
			null, null, null, null, null, CancellationToken.None);

		counts.Success.Should().Be(2);
		counts.Failure.Should().Be(1);
		counts.Total.Should().Be(3);
	}

	[Fact]
	public async Task GetOutcomeCountsAsync_ShouldStillApplyTheSearchAndDateFilters()
	{
		var now = DateTime.UtcNow;

		await SeedEntryAsync(now, outcome: AuditOutcome.Success);
		await SeedEntryAsync(now.AddDays(-10), outcome: AuditOutcome.Failure);

		var counts = await _repository.GetOutcomeCountsAsync(
			null, null, null, now.AddDays(-1), now, CancellationToken.None);

		// The chips track the table's search and date filters; only the outcome filter is
		// excluded, so every bucket keeps showing its size while one is selected.
		counts.Success.Should().Be(1);
		counts.Failure.Should().Be(0);
		counts.Total.Should().Be(1);
	}

	// The interceptor is what makes before/after values possible, so it is exercised
	// against a real save rather than mocked: load a row, change one field, save, and
	// confirm the collector saw the original value EF would otherwise discard.
	[Fact]
	public async Task ChangeInterceptor_ShouldCaptureBeforeAndAfter_ForATrackedEdit()
	{
		var collector = new AtsAuditChangeCollector();

		var role = new RoleDetails
		{
			RoleName = "Interceptor Test Role",
			RoleDescription = "Before",
			IsActive = true,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		_dbContext.RoleDetails.Add(role);
		await _dbContext.SaveChangesAsync();

		// A context of its own, so the interceptor under test is the only one attached and
		// the seeding save above is not part of what it observes.
		var options = new DbContextOptionsBuilder<ATSDBContext>()
			.UseNpgsql(_dbContext.Database.GetConnectionString())
			.AddInterceptors(new AtsAuditChangeInterceptor(collector))
			.Options;

		await using (var context = new ATSDBContext(options))
		{
			// Load, mutate, save - the pattern every settings service uses.
			var tracked = await context.RoleDetails.SingleAsync(x => x.RoleId == role.RoleId);

			tracked.RoleDescription = "After";
			tracked.IsActive = false;

			await context.SaveChangesAsync();
		}

		var change = collector.Changes.Should().ContainSingle().Subject;

		change.Entity.Should().Be(nameof(RoleDetails));
		change.Key.Should().Be(role.RoleId.ToString());
		change.State.Should().Be("Modified");

		change.Changes["RoleDescription"].From.Should().Be("Before");
		change.Changes["RoleDescription"].To.Should().Be("After");
		change.Changes["IsActive"].From.Should().Be("True");
		change.Changes["IsActive"].To.Should().Be("False");

		// Only what actually changed - an untouched column must not appear.
		change.Changes.Should().NotContainKey(nameof(RoleDetails.RoleName));
	}

	// Regression: the first version of the interceptor only read the change tracker, so
	// this pattern produced no diff at all - EditPackage rows came back with Changes null.
	// The package, role and module repositories all fetch with AsNoTracking() and then
	// call DbSet.Update() on the detached entity, which marks every property modified with
	// OriginalValue equal to CurrentValue. The stored row is the only before-image.
	[Fact]
	public async Task ChangeInterceptor_ShouldCaptureBeforeAndAfter_ForADetachedUpdate()
	{
		var collector = new AtsAuditChangeCollector();

		var role = new RoleDetails
		{
			RoleName = "Detached Test Role",
			RoleDescription = "Before",
			IsActive = true,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		_dbContext.RoleDetails.Add(role);
		await _dbContext.SaveChangesAsync();

		var options = new DbContextOptionsBuilder<ATSDBContext>()
			.UseNpgsql(_dbContext.Database.GetConnectionString())
			.AddInterceptors(new AtsAuditChangeInterceptor(collector))
			.Options;

		await using (var context = new ATSDBContext(options))
		{
			// Exactly what ATSRepository.Packages/Roles/Modules do.
			var detached = await context.RoleDetails
				.AsNoTracking()
				.SingleAsync(x => x.RoleId == role.RoleId);

			detached.RoleDescription = "After";
			detached.IsActive = false;

			context.RoleDetails.Update(detached);
			await context.SaveChangesAsync();
		}

		var change = collector.Changes.Should().ContainSingle().Subject;

		change.Changes["RoleDescription"].From.Should().Be("Before");
		change.Changes["RoleDescription"].To.Should().Be("After");
		change.Changes["IsActive"].From.Should().Be("True");
		change.Changes["IsActive"].To.Should().Be("False");

		// The detached Update() flags every property as modified, so this is what proves
		// the diff is computed by value rather than from IsModified.
		change.Changes.Should().NotContainKey(nameof(RoleDetails.RoleName));
		change.Changes.Should().NotContainKey(nameof(RoleDetails.RoleId));
	}

	[Fact]
	public async Task ChangeInterceptor_ShouldCaptureNothing_WhenADetachedUpdateChangedNothing()
	{
		var collector = new AtsAuditChangeCollector();

		var role = new RoleDetails
		{
			RoleName = "Unchanged Test Role",
			RoleDescription = "Same",
			IsActive = true,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		_dbContext.RoleDetails.Add(role);
		await _dbContext.SaveChangesAsync();

		var options = new DbContextOptionsBuilder<ATSDBContext>()
			.UseNpgsql(_dbContext.Database.GetConnectionString())
			.AddInterceptors(new AtsAuditChangeInterceptor(collector))
			.Options;

		await using (var context = new ATSDBContext(options))
		{
			var detached = await context.RoleDetails
				.AsNoTracking()
				.SingleAsync(x => x.RoleId == role.RoleId);

			// Saved without touching anything: a re-submitted form with no edits must not
			// record a row claiming every column changed.
			context.RoleDetails.Update(detached);
			await context.SaveChangesAsync();
		}

		collector.Changes.Should().BeEmpty();
	}

	[Fact]
	public async Task ChangeInterceptor_ShouldCaptureNothing_WhenTheSaveOnlyRead()
	{
		var collector = new AtsAuditChangeCollector();

		var options = new DbContextOptionsBuilder<ATSDBContext>()
			.UseNpgsql(_dbContext.Database.GetConnectionString())
			.AddInterceptors(new AtsAuditChangeInterceptor(collector))
			.Options;

		await using (var context = new ATSDBContext(options))
		{
			await context.RoleDetails.AsNoTracking().ToListAsync();
			await context.SaveChangesAsync();
		}

		// A query-only command must leave Changes null rather than an empty diff.
		collector.Changes.Should().BeEmpty();
	}

	// Mirrors AtsAuditDrainService.ResolveSitesAsync. Site is the one recorded field that
	// is not a claim, so the drain looks it up per batch; UserDetails is keyed
	// (UserId, ModuleId) and carries the same Site on every row, which is the assumption
	// the grouping depends on.
	[Fact]
	public async Task SiteResolution_ShouldFindOneSitePerUser_AcrossTheirModuleGrants()
	{
		var role = new RoleDetails
		{
			RoleName = "Audit Test Role",
			RoleDescription = "Seeded by AtsAuditRepositoryIntegrationTests",
			IsActive = true,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		var firstModule = new ModuleDetails
		{
			ModuleName = "Audit Test Module A",
			ModuleDescription = "Seeded by AtsAuditRepositoryIntegrationTests",
			IsActive = true,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		var secondModule = new ModuleDetails
		{
			ModuleName = "Audit Test Module B",
			ModuleDescription = "Seeded by AtsAuditRepositoryIntegrationTests",
			IsActive = true,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		_dbContext.RoleDetails.Add(role);
		_dbContext.ModuleDetails.AddRange(firstModule, secondModule);
		await _dbContext.SaveChangesAsync();

		var userId = Guid.CreateVersion7();

		// Two module grants, one user, one site.
		_dbContext.UserDetails.AddRange(
			NewUserDetails(userId, role.RoleId, firstModule.ModuleId),
			NewUserDetails(userId, role.RoleId, secondModule.ModuleId));

		await _dbContext.SaveChangesAsync();

		var userIds = new[] { userId };

		var sitesByUser = await _dbContext.UserDetails
			.AsNoTracking()
			.Where(user => userIds.Contains(user.UserId))
			.GroupBy(user => user.UserId)
			.Select(group => new
			{
				UserId = group.Key,
				Site = group.Select(user => user.Site).FirstOrDefault()
			})
			.ToDictionaryAsync(row => row.UserId, row => row.Site);

		// One entry despite the two grants, so a batch cannot fan out into duplicates.
		sitesByUser.Should().ContainSingle();
		sitesByUser[userId].Should().Be("24 - 7 INTOUCH- CEBU");
	}

	[Fact]
	public async Task SiteResolution_ShouldFindNothing_ForAUserWithNoAtsAccess()
	{
		// A platform super admin who was never granted an ATS module. The drain leaves
		// Site null and the entry is still written.
		var userIds = new[] { Guid.CreateVersion7() };

		var sitesByUser = await _dbContext.UserDetails
			.AsNoTracking()
			.Where(user => userIds.Contains(user.UserId))
			.GroupBy(user => user.UserId)
			.Select(group => new
			{
				UserId = group.Key,
				Site = group.Select(user => user.Site).FirstOrDefault()
			})
			.ToDictionaryAsync(row => row.UserId, row => row.Site);

		sitesByUser.Should().BeEmpty();
	}

	private static UserDetails NewUserDetails(Guid userId, int roleId, int moduleId) => new()
	{
		UserId = userId,
		UserName = "Jane Cruz",
		UserEmail = "jane.cruz@cibi.com.ph",
		IsActive = true,
		Site = "24 - 7 INTOUCH- CEBU",
		RoleId = roleId,
		ModuleId = moduleId,
		CreatedAt = DateTime.UtcNow,
		UpdatedAt = DateTime.UtcNow
	};

	// Mirrors AtsAuditRetentionService.SweepAsync, which is the deletion this feature
	// depends on to keep the table bounded.
	[Fact]
	public async Task RetentionSweep_ShouldDeleteOnlyEntriesPastTheCutoff()
	{
		var now = DateTime.UtcNow;

		var expired = await SeedEntryAsync(now.AddDays(-45), action: "Expired");
		var kept = await SeedEntryAsync(now.AddDays(-5), action: "Kept");

		var cutoff = now.AddDays(-30);

		var expiredIds = _dbContext.AuditTrail
			.Where(entry => entry.OccurredAt < cutoff)
			.OrderBy(entry => entry.OccurredAt)
			.Select(entry => entry.AuditEntryId)
			.Take(5_000);

		var deleted = await _dbContext.AuditTrail
			.Where(entry => expiredIds.Contains(entry.AuditEntryId))
			.ExecuteDeleteAsync();

		deleted.Should().Be(1);

		var remaining = await _dbContext.AuditTrail.AsNoTracking().ToListAsync();

		remaining.Should().ContainSingle()
			.Which.AuditEntryId.Should().Be(kept.AuditEntryId);

		remaining.Should().NotContain(entry => entry.AuditEntryId == expired.AuditEntryId);
	}
}
