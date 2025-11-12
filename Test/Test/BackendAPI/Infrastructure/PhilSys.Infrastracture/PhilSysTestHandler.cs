namespace Test.BackendAPI.Infrastructure.PhilSys.Infrastracture;

public class PhilSysTestHandler : DelegatingHandler
{
	private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _responder;

	public PhilSysTestHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
	{
		_responder = responder ?? throw new ArgumentNullException(nameof(responder));
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		=> _responder(request, cancellationToken);
}
