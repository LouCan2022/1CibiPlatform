using APIs.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;

//check env configuration
//if (builder.Environment.IsDevelopment())
//{
//    builder.Services.ConfigureCorsDev();
//}

//if (builder.Environment.IsProduction())
//{
//    builder.Services.ConfigureCorsProd();
//}


// Register services to the DI container.
// builder.services
//    .addjwtauthentication(builder.configuration)
//    .AddCarterModules(assembly)
//    .AddMediaTR(assembly)
//    .AddValidatorsFromAssembly(assembly)
//    .AddExceptionHandler<CustomExceptionHandler>()
//    .AddServices()
//    .AddMemoryCache()
//    .AddDbContext(builder.Configuration)
//    .AddEndpointsApiExplorer()
//    .AddSwaggerGen();

builder.Services
    .AddModuleServices()
    .AddJwtAuthentication(builder.Configuration)
    .AddModuleInfrastructure(builder.Configuration)
    .AddModuleCarter(assembly);


var app = builder.Build();


if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    await app.IntializeDatabaseAsync();
    app.UseSwagger();
    app.UseSwaggerUI();
}


// use exception handler after register
app.UseExceptionHandler(options => { })
   .UseHttpsRedirection()
   .UseAuthentication()
   .UseAuthorization();

//app.UseCors("CorsPolicy");


app.Run();

public partial class Program { }