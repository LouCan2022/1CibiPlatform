var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;

builder.Services.ConfigureEnvironment(builder);

builder.Services.AddControllers();

builder.Services
	.AddModuleMediaTR()
	.AddModuleCarter()
	.AddModuleServices()
	.AddJwtAuthentication(builder.Configuration)
	.AddModuleInfrastructure(builder.Configuration)
	.AddEndpointsApiExplorer()
	.AddSwaggerGen();


var app = builder.Build();

await app.UseEnvironmentAsync();

app.UseCustomMiddlewares();

app.Run();

public partial class Program { }