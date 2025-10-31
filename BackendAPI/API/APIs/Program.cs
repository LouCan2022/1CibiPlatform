﻿using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;

builder.Services.ConfigureEnvironment(builder);

builder.Services.AddControllers();

builder.Services
	.AddLoggingConfiguration(builder.Configuration)
	.AddModuleMediaTR()
	.AddModuleCarter()
	.AddModuleServices()
	.AddJwtAuthentication(builder.Configuration, builder.Environment)
	.AddModuleInfrastructure(builder.Configuration)
	.AddEndpointsApiExplorer()
	.AddSwaggerGen();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
	options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
	options.KnownNetworks.Clear();
	options.KnownProxies.Clear();
});

var app = builder.Build();

await app.UseEnvironmentAsync();

app.UseCustomMiddlewares();

app.Run();

public partial class Program { }