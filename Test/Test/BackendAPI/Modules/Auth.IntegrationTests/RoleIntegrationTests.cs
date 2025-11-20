using Auth.Data.Entities;
using Auth.DTO;
using Auth.Features.UserManagement.Command.AddRole;
using Auth.Features.UserManagement.Command.DeleteRole;
using Auth.Features.UserManagement.Command.EditRole;
using Auth.Features.UserManagement.Query.GetRoles;
using BuildingBlocks.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Test.BackendAPI.Infrastructure.Auth.Infrastructure;

namespace Test.BackendAPI.Modules.Auth.IntegrationTests;

public class RoleIntegrationTests : BaseIntegrationTest
{
	public RoleIntegrationTests(IntegrationTestWebAppFactory factory)
		: base(factory)
	{
	}

	[Fact]
	public async Task GetRoles_ShouldReturnPaginatedRoleList()
	{
		// Arrange
		await SeedRoleData();

		var query = new GetRolesQueryRequest(PageNumber: 1, PageSize: 3);

		// Act
		var result = await _sender.Send(query);

		// Assert
		result.Should().NotBeNull();
		result.Roles.Data.Count().Should().Be(3);
	}

	[Fact]
	public async Task GetRoless_ShouldReturnRoleList_BasedOnSearchTerm()
	{
		// Arrange
		await SeedRoleData();

		var query = new GetRolesQueryRequest(PageNumber: 1, PageSize: 1, SearchTerm: "SuperAdmin");

		// Act
		var result = await _sender.Send(query);

		// Assert
		result.Should().NotBeNull();

		result.Roles.Count.Should().Be(1);
		result.Roles.Data.ElementAt(0).roleName.Should().Be("SuperAdmin");
		result.Roles.Data.ElementAt(0).Description.Should().Be("SuperAdmin");
	}

	[Fact]
	public async Task GetRoles_ShouldReturnEmptyList_WhenNoRolesExist()
	{
		// Arrange
		var query = new GetRolesQueryRequest(PageNumber: 1, PageSize: 5);
		// Act
		var result = await _sender.Send(query);
		// Assert
		result.Should().NotBeNull();
		result.Roles.Count.Should().Be(0);
	}

	[Fact]
	public async Task GetRoles_ShouldReturnCorrectPage_WhenPageNumberAndSizeAreSpecified()
	{
		// Arrange
		await SeedRoleData();
		var query = new GetRolesQueryRequest(PageNumber: 1, PageSize: 2);

		// Act
		var result = await _sender.Send(query);

		// Assert
		result.Roles.PageIndex.Should().Be(1);
		result.Roles.PageSize.Should().Be(2);
	}

	[Fact]
	public async Task GetRoles_ShouldReturnEmptyList_WhenPageNumberExceedsTotalPages()
	{
		// Arrange
		await SeedRoleData();

		var query = new GetRolesQueryRequest(PageNumber: 3, PageSize: 2);

		// Act
		var result = await _sender.Send(query);

		// Assert
		result.Roles.Data.Count().Should().Be(0);
	}

	[Fact]
	public async Task AddRole_ShouldAddNewRoleSuccessfully()
	{
		var role = new AddRoleDTO
		{
			RoleName = "SuperAdmin",
			Description = "SuperAdmin"
		};
		// Arrange
		var command = new AddRoleCommand(role);

		// Act
		var result = await _sender.Send(command);

		// Assert

		result.Should().NotBeNull();
		result.isAdded.Should().BeTrue();
	}

	[Fact]
	public async Task EditRole_ShouldUpdateExistingRoleSuccessfully()
	{
		// Arrange
		await SeedRoleData();

		var existingRole = await _dbContext.AuthRoles
			.AsNoTracking()
			.FirstAsync(x => x.RoleId == 2);

		var role = new EditRoleDTO
		{
			RoleId = existingRole!.RoleId,
			RoleName = existingRole.RoleName + " Updated",
			Description = existingRole.Description + " Updated"
		};

		var command = new EditRoleCommand(role);

		// Act
		var result = await _sender.Send(command);

		// Assert
		result.Should().NotBeNull();
		result!.role.RoleName.Should().Be(role.RoleName);
		result!.role.Description.Should().Be(role.Description);
	}

	[Fact]
	public async Task EditRole_ShouldThrow_WhenRoleDoesNotExist()
	{
		// Arrange
		var role = new EditRoleDTO
		{
			RoleId = 1,
			RoleName = "SuperAdmin",
			Description = "SuperAdmin"
		};
		var command = new EditRoleCommand(role);

		// Act
		Func<Task> act = async () => await _sender.Send(command);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>().WithMessage($"Role with ID {role.RoleId} was not found."); ;
	}

	[Fact]
	public async Task DeleteRole_ShouldRemoveRoleSuccessfully()
	{
		// Arrange
		await SeedRoleData();
		var command = new DeleteRoleCommand(1);

		// Act
		var result = await _sender.Send(command);

		// Assert
		result.IsDeleted.Should().BeTrue();
	}

	[Fact]
	public async Task DeleteRole_ShouldThrow_WhenRoleDoesNotExist()
	{
		// Arrange
		var command = new DeleteRoleCommand(99);

		// Act
		Func<Task> act = async () => await _sender.Send(command);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>().WithMessage($"Role with ID 99 was not found."); ;
	}

	private async Task SeedRoleData()
	{
		var roles = new List<AuthRole>
		{
			new AuthRole
			{
				RoleId = 1,
				RoleName = "SuperAdmin",
				Description = "SuperAdmin",
			},
			new AuthRole
			{
				RoleId = 2,
				RoleName = "Admin",
				Description = "Admin",
			},
			new AuthRole
			{
				RoleId = 3,
				RoleName = "User",
				Description = "User",
			},
		};
		_dbContext.AuthRoles.AddRange(roles);
		await _dbContext.SaveChangesAsync();
	}
}
