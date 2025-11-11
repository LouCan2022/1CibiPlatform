using Auth.Data.Entities;
using Auth.Features.UserManagement.Query.GetUsers;
using FluentAssertions;
using Test.BackendAPI.Infrastructure;

namespace Test.BackendAPI.Modules.Auth.IntegrationTests;

public class UserManagementIntegrationTests : BaseIntegrationTest
{
	public UserManagementIntegrationTests(IntegrationTestWebAppFactory factory)
		: base(factory)
	{
	}

	[Fact]
	public async Task GetUsers_ShouldReturnPaginatedUsersList()
	{
		// Arrange
		await SeedUserData();

		var query = new GetUsersQueryRequest(PageNumber: 1, PageSize: 4);

		// Act
		var result = await _sender.Send(query);

		// Assert
		result.Should().NotBeNull();
		result.Users.Data.Count().Should().Be(4);
	}

	[Fact]
	public async Task GetUsers_ShouldReturnUserList_BasedOnSearchTerm()
	{
		// Arrange
		await SeedUserData();

		var query = new GetUsersQueryRequest(PageNumber: 1, PageSize: 1, SearchTerm: "Admin1");

		// Act
		var result = await _sender.Send(query);
		// Assert
		result.Should().NotBeNull();

		result.Users.Count.Should().Be(1);
		result.Users.Data.ElementAt(0).firstName.Should().Be("Admin1");
		result.Users.Data.ElementAt(0).email.Should().Be("john@example1.com");

	}

	[Fact]
	public async Task GetUsers_ShouldReturnEmptyList_WhenNoUsersExist()
	{
		// Arrange
		var query = new GetUsersQueryRequest(PageNumber: 1, PageSize: 5);
		// Act
		var result = await _sender.Send(query);
		// Assert
		result.Should().NotBeNull();
		result.Users.Count.Should().Be(0);
	}

	[Fact]
	public async Task GetUsers_ShouldReturnCorrectPage_WhenPageNumberAndSizeAreSpecified()
	{
		// Arrange
		await SeedUserData();
		var query = new GetUsersQueryRequest(PageNumber: 1, PageSize: 2);

		// Act
		var result = await _sender.Send(query);

		// Assert
		result.Users.PageIndex.Should().Be(1);
		result.Users.PageSize.Should().Be(2);
	}

	[Fact]
	public async Task GetUsers_ShouldReturnEmptyList_WhenPageNumberExceedsTotalPages()
	{
		// Arrange
		await SeedUserData();

		var query = new GetUsersQueryRequest(PageNumber: 3, PageSize: 2);

		// Act
		var result = await _sender.Send(query);

		// Assert
		result.Users.Data.Count().Should().Be(0);
	}



	private async Task SeedUserData()
	{
		var users = new List<Authusers>
		{
			new Authusers
			{
				Id = Guid.NewGuid(),
				Email = "john@example1.com",
				PasswordHash = _passwordHasherService.HashPassword("p@ssw0rd!"),
				FirstName = "Admin1",
				LastName = ""
			},
			new Authusers
			{
				Id = Guid.NewGuid(),
				Email = "john@example2.com",
				PasswordHash = _passwordHasherService.HashPassword("p@ssw0rd!"),
				FirstName = "Admin2",
				LastName = ""
			},
			new Authusers
			{
				Id = Guid.NewGuid(),
				Email = "john@example3.com",
				PasswordHash = _passwordHasherService.HashPassword("p@ssw0rd!"),
				FirstName = "Admin3",
				LastName = ""
			},
			new Authusers
			{
				Id = Guid.NewGuid(),
				Email = "john@example4.com",
				PasswordHash = _passwordHasherService.HashPassword("p@ssw0rd!"),
				FirstName = "Admin4",
				LastName = ""
			},

		};
		_dbContext.AuthUsers.AddRange(users);
		await _dbContext.SaveChangesAsync();
	}
}
