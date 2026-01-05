var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Print environment and base address for debugging (appears in browser console for Blazor WebAssembly)
#if DEBUG
Console.WriteLine($"Environment: {builder.HostEnvironment.Environment}");
Console.WriteLine($"Host BaseAddress: {builder.HostEnvironment.BaseAddress}");
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
#else
// In production limit logs to warnings and above to prevent debug/info from appearing in browser console
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Warning);
#endif

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMudServices();
builder.Services.AddFrontEndServices(builder.Configuration, builder.HostEnvironment);

await builder.Build().RunAsync();