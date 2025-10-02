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


builder.Services
    .AddModuleMediaTR()
    .AddModuleCarter()
    .AddModuleServices()
    .AddJwtAuthentication(builder.Configuration)
    .AddModuleInfrastructure(builder.Configuration)
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();



var app = builder.Build();

app.MapCarter();


if (app.Environment.IsDevelopment())
{
    await DatabaseExtensions.IntializeDatabaseAsync(app);
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction())
{
    await DatabaseExtensions.IntializeDatabaseAsync(app);
}

// use exception handler after register
app.UseExceptionHandler(options => { })
   .UseHttpsRedirection()
   .UseAuthentication()
   .UseAuthorization();

//app.UseCors("CorsPolicy");


app.Run();

public partial class Program { }