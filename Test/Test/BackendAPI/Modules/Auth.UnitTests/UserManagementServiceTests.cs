using Auth.DTO;
using BuildingBlocks.Pagination;
using FluentAssertions;
using Moq;
using Test.BackendAPI.Modules.Auth.UnitTests.Fixture;

namespace Test.BackendAPI.Modules.Auth.UnitTests;

public class UserManagementServiceTests : IClassFixture<AuthServiceFixture>
{
	private readonly AuthServiceFixture _fixture;

	public UserManagementServiceTests(AuthServiceFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task GetUsersAsync_ShouldReturnPaginatedResult()
	{
		// Arrange
		var service = _fixture.UserManagementService;
		var paginationRequest = new PaginationRequest
		{
			PageIndex = 1,
			PageSize = 10,
			SearchTerm = null
		};

		var userData = new List<UsersDTO>
			{
				new UsersDTO(Guid.NewGuid(), "user1@example.com", "sample1" , "sample2" , null, false),
				new UsersDTO(Guid.NewGuid(), "user2@example.com", "sample3" , "sample4" , null, false)
			};

		var expectedResult = new PaginatedResult<UsersDTO>(1, 2, 10, userData);

		var mockAuthRepository = _fixture
			.MockAuthRepository
			.Setup(x => x.GetUserAsync(paginationRequest, CancellationToken.None))
			.ReturnsAsync(expectedResult);

		// Act
		var result = await _fixture.UserManagementService.GetUsersAsync(paginationRequest, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.PageIndex.Should().Be(expectedResult.PageIndex);
		result.PageSize.Should().Be(expectedResult.PageSize);
		result.Count.Should().Be(expectedResult.Count);
		result.Data.Should().BeEquivalentTo(expectedResult.Data);
	}

	[Fact]
	public async Task GetUsersAsync_ShouldCallSearchUserAsync_WhenSearchTermProvided()
	{
		// Arrange
		var service = _fixture.UserManagementService;
		var paginationRequest = new PaginationRequest
		{
			PageIndex = 1,
			PageSize = 10,
			SearchTerm = "sample1"
		};

		var userData = new List<UsersDTO>
			{
				new UsersDTO(Guid.NewGuid(), "user1@example.com", "sample1" , "sample2" , null, false)
			};

		var expectedResult = new PaginatedResult<UsersDTO>(1, 2, 10, userData);

		var mockAuthRepository = _fixture
			.MockAuthRepository
			.Setup(x => x.SearchUserAsync(paginationRequest, CancellationToken.None))
			.ReturnsAsync(expectedResult);

		// Act
		var result = await _fixture.UserManagementService.GetUsersAsync(paginationRequest, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.PageIndex.Should().Be(expectedResult.PageIndex);
		result.PageSize.Should().Be(expectedResult.PageSize);
		result.Count.Should().Be(expectedResult.Count);
		result.Data.Should().BeEquivalentTo(expectedResult.Data);
	}
}
