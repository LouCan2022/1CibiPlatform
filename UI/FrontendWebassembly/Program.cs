var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Print environment and base address for debugging (appears in browser console for Blazor WebAssembly)
Console.WriteLine($"Environment: {builder.HostEnvironment.Environment}");
Console.WriteLine($"Host BaseAddress: {builder.HostEnvironment.BaseAddress}");

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMudServices();
builder.Services.AddFrontEndServices(builder.Configuration, builder.HostEnvironment);

await builder.Build().RunAsync();