using Auth.Data.Context;
using Auth.Service;
using BuildingBlocks.SharedServices.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Test.BackendAPI.Infrastructure.Auth.Infrastructure;

public class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
	private readonly IServiceScope _scope;
	protected readonly ISender _sender;
	protected readonly IPasswordHasherService _passwordHasherService;
	protected readonly IHashService _hashService;
	protected readonly AuthApplicationDbContext _dbContext;
	protected readonly IHttpContextAccessor _httpContextAccessor;
	protected readonly IConfiguration _configuration;

	protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
	{
		_scope = factory.Services.CreateScope();
		_sender = _scope.ServiceProvider.GetRequiredService<ISender>();
		_passwordHasherService = _scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
		_hashService = _scope.ServiceProvider.GetRequiredService<IHashService>();
		_dbContext = _scope.ServiceProvider.GetRequiredService<AuthApplicationDbContext>();
		_httpContextAccessor = _scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
		_configuration = _scope.ServiceProvider.GetRequiredService<IConfiguration>();
	}

	// Runs before each test. Ensures database tables used in tests are cleaned to avoid cross-test pollution.
	public async Task InitializeAsync()
	{
		try
		{
			if (_dbContext is not null)
			{

				var appSubRoles = _dbContext.AuthUserAppRoles.ToList();
				if (appSubRoles.Any())
				{
					_dbContext.AuthUserAppRoles.RemoveRange(appSubRoles);
					await _dbContext.SaveChangesAsync();
				}

				var users = _dbContext.AuthUsers.ToList();
				if (users.Any())
				{
					_dbContext.AuthUsers.RemoveRange(users);
					await _dbContext.SaveChangesAsync();
				}

				var applications = _dbContext.AuthApplications.ToList();
				if (applications.Any())
				{
					_dbContext.AuthApplications.RemoveRange(applications);
					await _dbContext.SaveChangesAsync();
				}

				var subMenus = _dbContext.AuthSubmenu.ToList();
				if (subMenus.Any())
				{
					_dbContext.AuthSubmenu.RemoveRange(subMenus);
					await _dbContext.SaveChangesAsync();
				}

				var roles = _dbContext.AuthRoles.ToList();
				if (roles.Any())
				{
					_dbContext.AuthRoles.RemoveRange(roles);
					await _dbContext.SaveChangesAsync();
				}
			}
		}
		catch (Exception ex)
		{
			throw new Exception("Error during database cleanup in InitializeAsync: " + ex.Message, ex);
		}
	}

	public Task DisposeAsync()
	{
		return Task.CompletedTask;
	}
}

