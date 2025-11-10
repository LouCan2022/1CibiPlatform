using Auth.Data.Context;
using Auth.Service;
using BuildingBlocks.SharedServices.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Test.BackendAPI.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
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
}

