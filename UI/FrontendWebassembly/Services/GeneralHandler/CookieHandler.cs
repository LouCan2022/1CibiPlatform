namespace FrontendWebassembly.Services.GeneralHandler;

public class CookieHandler : DelegatingHandler
{
	protected override Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
		request.SetBrowserRequestMode(BrowserRequestMode.Cors);

		return base.SendAsync(request, cancellationToken);
	}
}
