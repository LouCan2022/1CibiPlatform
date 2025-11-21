using Auth.Data.Entities;
using Auth.DTO;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using FluentAssertions;
using Moq;
using Test.BackendAPI.Modules.Auth.UnitTests.Fixture;

namespace Test.BackendAPI.Modules.Auth.UnitTests;

public class RoleServiceTests : IClassFixture<AuthServiceFixture>
{
	private readonly AuthServiceFixture _fixture;

	public RoleServiceTests(AuthServiceFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task GetRolesAsync_ShouldReturnPaginatedResult()
	{
		// Arrange
		var paginationRequest = new PaginationRequest
		{
			PageIndex = 1,
			PageSize = 10,
			SearchTerm = null
		};

		var roleData = new List<RolesDTO>
		{
			new RolesDTO(1, "SuperAdmin", "SuperAdmin"),
			new RolesDTO(2, "Admin", "Admin")
		};

		var expectedResult = new PaginatedResult<RolesDTO>(1, 2, 10, roleData);

		var mockAuthRepository = _fixture
			.MockAuthRepository
			.Setup(x => x.GetRolesAsync(paginationRequest, CancellationToken.None))
			.ReturnsAsync(expectedResult);

		// Act
		var result = await _fixture.RoleService.GetRolesAsync(paginationRequest, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.PageIndex.Should().Be(expectedResult.PageIndex);
		result.PageSize.Should().Be(expectedResult.PageSize);
		result.Count.Should().Be(expectedResult.Count);
		result.Data.Should().BeEquivalentTo(expectedResult.Data);
	}

	[Fact]
	public async Task GetRolesAsync_ShouldCallSearchRoleAsync_WhenSearchTermProvided()
	{
		// Arrange
		var service = _fixture.UserManagementService;
		var paginationRequest = new PaginationRequest
		{
			PageIndex = 1,
			PageSize = 10,
			SearchTerm = "SuperAdmin"
		};

		var roleData = new List<RolesDTO>
		{
			new RolesDTO(1, "SuperAdmin", "SuperAdmin"),
		};

		var expectedResult = new PaginatedResult<RolesDTO>(1, 2, 10, roleData);

		var mockAuthRepository = _fixture
			.MockAuthRepository
			.Setup(x => x.SearchRoleAsync(paginationRequest, CancellationToken.None))
			.ReturnsAsync(expectedResult);

		// Act
		var result = await _fixture.RoleService.GetRolesAsync(paginationRequest, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.PageIndex.Should().Be(expectedResult.PageIndex);
		result.PageSize.Should().Be(expectedResult.PageSize);
		result.Count.Should().Be(expectedResult.Count);
		result.Data.Should().BeEquivalentTo(expectedResult.Data);
	}

	[Fact]
	public async Task DeleteRoleAsync_ShouldThrow_WhenNotFound()
	{
		// Arrange
		var roleId = 99;
		_fixture.MockAuthRepository
			.Setup(x => x.GetRoleAsync(roleId))
			.ReturnsAsync((AuthRole)null);

		// Act
		Func<Task> act = async () => await _fixture.RoleService.DeleteRoleAsync(roleId);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage($"Role with ID {roleId} was not found.");
	}

	[Fact]
	public async Task DeleteRoleAsync_ShouldReturnTrue_WhenDeleted()
	{
		// Arrange
		var roleId = 1;
		var existingRole = new AuthRole { RoleId = roleId, RoleName = "SuperAdmin", Description = "SuperAdmin"};

		_fixture.MockAuthRepository
			.Setup(x => x.GetRoleAsync(roleId))
			.ReturnsAsync(existingRole);

		_fixture.MockAuthRepository
			.Setup(x => x.DeleteRoleAsync(existingRole))
			.ReturnsAsync(true);

		// Act
		var result = await _fixture.RoleService.DeleteRoleAsync(roleId);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public async Task AddRoleAsync_ShouldReturnTrue_WhenAdded()
	{
		// Arrange
		var role = new AddRoleDTO { RoleName = "SuperAdmin", Description = "SuperAdmin"};

		_fixture.MockAuthRepository
			.Setup(x => x.AddRoleAsync(role))
			.ReturnsAsync(true);

		// Act
		var result = await _fixture.RoleService.AddRoleAsync(role);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public async Task EditRoleAsync_ShouldThrow_WhenRoleNotFound()
	{
		// Arrange
		var editDto = new EditRoleDTO { RoleId = 99, RoleName = "UpdatedRole", Description = "Updated"};

		_fixture.MockAuthRepository
			.Setup(x => x.GetRoleAsync(editDto.RoleId))
			.ReturnsAsync((AuthRole)null);

		// Act
		Func<Task> act = async () => await _fixture.RoleService.EditRoleAsync(editDto);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage($"Role with ID {editDto.RoleId} was not found.");
	}

	[Fact]
	public async Task EditApplicationAsync_ShouldReturnUpdatedDto_WhenSuccessful()
	{
		// Arrange
		var editDto = new EditRoleDTO { RoleId = 1, RoleName = "UpdatedRole", Description = "Updated"};
		var existingRole = new AuthRole { RoleId = 1, RoleName = "UpdatedRole", Description = "Old"};
		var updatedRole = new AuthRole { RoleId = 1, RoleName = "UpdatedRole", Description = "Updated"};

		_fixture.MockAuthRepository
			.Setup(x => x.GetRoleAsync(editDto.RoleId))
			.ReturnsAsync(existingRole);

		_fixture.MockAuthRepository
			.Setup(x => x.EditRoleAsync(existingRole))
			.ReturnsAsync(updatedRole);

		// Act
		var result = await _fixture.RoleService.EditRoleAsync(editDto);

		// Assert
		result.Should().NotBeNull();
		result.RoleId.Should().Be(updatedRole.RoleId);
		result.RoleName.Should().Be(updatedRole.RoleName);
		result.Description.Should().Be(updatedRole.Description);
	}
}