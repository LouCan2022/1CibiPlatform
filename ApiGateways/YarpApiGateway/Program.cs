using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
	options.AddPolicy("DevCors", policy =>
	{
		policy.WithOrigins(
			 "http://localhost:5134")
			  .AllowCredentials()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});


builder.WebHost.ConfigureKestrel(kestrel =>
{
	if (builder.Environment.IsDevelopment())
	{
		Console.WriteLine("🔧 Development mode — using ASP.NET Core dev certificate.");
		kestrel.ConfigureHttpsDefaults(https =>
		{
			// This ensures dev cert is used for any HTTPS endpoint
		});
	}
	else
	{
		// 🐳 PRODUCTION (Docker/Server): Load from PFX
		kestrel.ListenAnyIP(443, opts =>
		{
			var certPath = "/app/certs/mycert.pfx";
			var certPassword = Environment.GetEnvironmentVariable("CERT_PASSWORD")
				?? throw new InvalidOperationException("CERT_PASSWORD is not set in production.");

			var cert = X509CertificateLoader.LoadPkcs12FromFile(certPath, certPassword);
			var daysUntilExpiry = (cert.NotAfter - DateTime.UtcNow).TotalDays;

			if (daysUntilExpiry < 30)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"⚠️  WARNING: Certificate expires in {daysUntilExpiry:F0} days!");
				Console.ResetColor();
			}

			Console.WriteLine($"✅ Production cert loaded: {cert.Subject} — Expires: {cert.NotAfter:yyyy-MM-dd HH:mm}");
			opts.UseHttps(cert);
		});
	}
});


builder.Services.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseWebSockets();

app.UseCors("DevCors");

app.MapReverseProxy();

app.Run();
