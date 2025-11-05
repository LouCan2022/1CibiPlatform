using Auth.Data.Context;
using Auth.Service;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Test.BackendAPI.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
{
	private readonly IServiceScope _scope;
	protected readonly ISender _sender;
	protected readonly IPasswordHasherService _passwordHasherService;
	protected readonly AuthApplicationDbContext _dbContext;

	protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
	{
		_scope = factory.Services.CreateScope();
		_sender = _scope.ServiceProvider.GetRequiredService<ISender>();
		_passwordHasherService = _scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
		_dbContext = _scope.ServiceProvider.GetRequiredService<AuthApplicationDbContext>();
	}
}

