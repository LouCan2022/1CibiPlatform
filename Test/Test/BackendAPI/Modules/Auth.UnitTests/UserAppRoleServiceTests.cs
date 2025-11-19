using Auth.Data.Entities;
using Auth.DTO;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using FluentAssertions;
using Moq;
using Test.BackendAPI.Modules.Auth.UnitTests.Fixture;

namespace Test.BackendAPI.Modules.Auth.UnitTests;

public class UserAppRoleServiceTests : IClassFixture<AuthServiceFixture>
{
	private readonly AuthServiceFixture _fixture;
	public UserAppRoleServiceTests(AuthServiceFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task GetAppSubRolesAsync_ShouldCallGetAppSubRoles_WhenNoSearchTerm()
	{
		// Arrange
		var paginationRequest = new PaginationRequest
		{
			PageIndex = 1,
			PageSize = 10,
			SearchTerm = null
		};

		var data = new List<AppSubRolesDTO>
			{
				new AppSubRolesDTO ( 1, Guid.NewGuid(), 1, 1, 1 ),
				new AppSubRolesDTO ( 2, Guid.NewGuid(), 2, 2, 2)
			};

		var expectedResult = new PaginatedResult<AppSubRolesDTO>(1, 1, 10, data);

		_fixture.MockAuthRepository
			.Setup(x => x.GetAppSubRolesAsync(paginationRequest, CancellationToken.None))
			.ReturnsAsync(expectedResult);

		// Act
		var result = await _fixture.AppSubRoleService.GetAppSubRolesAsync(paginationRequest, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.PageIndex.Should().Be(expectedResult.PageIndex);
		result.PageSize.Should().Be(expectedResult.PageSize);
		result.Count.Should().Be(expectedResult.Count);
		result.Data.Should().BeEquivalentTo(expectedResult.Data);
	}

	[Fact]
	public async Task GetAppSubRolesAsync_ShouldCallAppSubRoles_WhenSearchTermProvided()
	{
		// Arrange
		var paginationRequest = new PaginationRequest
		{
			PageIndex = 1,
			PageSize = 10,
			SearchTerm = "1"
		};

		var data = new List<AppSubRolesDTO>
			{
				new AppSubRolesDTO ( 1, Guid.NewGuid(), 1, 1, 1 ),
				new AppSubRolesDTO ( 2, Guid.NewGuid(), 2, 2, 2)
			};

		var expectedResult = new PaginatedResult<AppSubRolesDTO>(1, 1, 10, data);

		_fixture.MockAuthRepository
			.Setup(x => x.SearchAppSubRoleAsync(paginationRequest, CancellationToken.None))
			.ReturnsAsync(expectedResult);

		// Act
		var result = await _fixture.AppSubRoleService.GetAppSubRolesAsync(paginationRequest, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.PageIndex.Should().Be(expectedResult.PageIndex);
		result.PageSize.Should().Be(expectedResult.PageSize);
		result.Count.Should().Be(expectedResult.Count);
		result.Data.Should().BeEquivalentTo(expectedResult.Data);
	}

	[Fact]
	public async Task AddAppSubRoleAsync_ShouldReturnTrue_WhenSuccessful()
	{
		// Arrange
		var appSubRole = new AddAppSubRoleDTO { UserId = Guid.NewGuid(), AppId = 1, SubMenuId = 1, RoleId = 1, AssignedBy = Guid.NewGuid() };

		_fixture.MockAuthRepository
			.Setup(x => x.AddAppSubRoleAsync(appSubRole))
			.ReturnsAsync(true);

		// Act
		var result = await _fixture.AppSubRoleService.AddAppSubRoleAsync(appSubRole);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public async Task EditAppSubRolesync_ShouldThrow_WhenAppSubRoleNotFound()
	{
		// Arrange
		var editDto = new EditAppSubRoleDTO { AppSubRoleId = 1, UserId = Guid.NewGuid(), AppId = 1, SubMenuId = 1, RoleId = 1 };

		_fixture.MockAuthRepository
			.Setup(x => x.GetAppSubRoleAsync(editDto.AppSubRoleId))
			.ReturnsAsync((AuthUserAppRole?)null);

		// Act
		Func<Task> act = async () => await _fixture.AppSubRoleService.EditAppSubRoleAsync(editDto);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage($"AppSubRole with ID {editDto.AppSubRoleId} was not found.");
	}

	[Fact]
	public async Task EditAppSubRolesync_ShouldReturnUpdatedDto_WhenSuccessful()
	{
		// Arrange
		var editDto = new EditAppSubRoleDTO { AppSubRoleId = 1, UserId = Guid.NewGuid(), AppId = 2, SubMenuId = 2, RoleId = 2 };
		var existingAppSubRole = new AuthUserAppRole { AppRoleId = 1, UserId = Guid.NewGuid(), AppId = 1, Submenu = 1, RoleId = 1 };
		var updatedAppSubRole = new AuthUserAppRole { AppRoleId = 1, UserId = Guid.NewGuid(), AppId = 2, Submenu = 2, RoleId = 2 };

		_fixture.MockAuthRepository
			.Setup(x => x.GetAppSubRoleAsync(editDto.AppSubRoleId))
			.ReturnsAsync(existingAppSubRole);

		_fixture.MockAuthRepository
			.Setup(x => x.EditAppSubRoleAsync(existingAppSubRole))
			.ReturnsAsync(updatedAppSubRole);

		// Act
		var result = await _fixture.AppSubRoleService.EditAppSubRoleAsync(editDto);

		// Assert
		result.Should().NotBeNull();
		result.AppRoleId.Should().Be(updatedAppSubRole.AppRoleId);
		result.AppId.Should().Be(updatedAppSubRole.AppId);
		result.Submenu.Should().Be(updatedAppSubRole.Submenu);
		result.RoleId.Should().Be(updatedAppSubRole.RoleId);
	}

	[Fact]
	public async Task DeleteAppSubRoleAsync_ShouldThrow_WhenNotFound()
	{
		// Arrange
		var appSubRoleId = 99;

		_fixture.MockAuthRepository
			.Setup(x => x.GetAppSubRoleAsync(appSubRoleId))
			.ReturnsAsync((AuthUserAppRole?)null);

		// Act
		Func<Task> act = async () => await _fixture.AppSubRoleService.DeleteAppSubRoleAsync(appSubRoleId);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage($"AppSubRole with ID {appSubRoleId} was not found.");
	}

	[Fact]
	public async Task DeleteAppSubRoleAsync_ShouldReturnTrue_WhenSuccessful()
	{
		// Arrange
		var appSubRoleId = 1;
		var existingAppSubRole= new AuthUserAppRole { AppRoleId = appSubRoleId, UserId = Guid.NewGuid(), AppId = 1, Submenu = 1, RoleId = 1 };

		_fixture.MockAuthRepository
			.Setup(x => x.GetAppSubRoleAsync(appSubRoleId))
			.ReturnsAsync(existingAppSubRole);

		_fixture.MockAuthRepository
			.Setup(x => x.DeleteAppSubRoleAsync(existingAppSubRole))
			.ReturnsAsync(true);

		// Act
		var result = await _fixture.AppSubRoleService.DeleteAppSubRoleAsync(appSubRoleId);

		// Assert
		result.Should().BeTrue();
	}
}

