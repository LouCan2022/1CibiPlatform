namespace FrontendWebassembly.Services.Shared.Extensions;

/// <summary>
/// The one place a UI service's HTTP call is wrapped in a try/catch.
/// </summary>
/// <remarks>
/// Every typed service used to repeat the same block: send, check IsSuccessStatusCode,
/// read ApiErrorResponse on failure, deserialize on success, then catch
/// HttpRequestException/JsonException/NotSupportedException and turn it into a failure
/// message. That is transport plumbing, not feature code, and copying it per method is
/// how one of the copies quietly ends up missing a case.
///
/// These helpers own it once so a service method reads as the request it makes and
/// nothing else. Two rules are preserved exactly as the hand-written copies had them:
///
///   - OperationCanceledException is rethrown, never converted to a failure. A cancelled
///     request is the caller navigating away, not a server problem, and swallowing it
///     turns a normal cancellation into a spurious error toast.
///   - Only transport-shaped exceptions are converted. A NullReferenceException in a
///     service is a bug and must still surface as one rather than being reported to the
///     user as "unable to reach the server".
///
/// The backend has its own global handler for this (BuildingBlocks CustomExceptionHandler,
/// which turns a thrown domain exception into a ProblemDetails response). This is the
/// client-side counterpart: it reads that response back out.
/// </remarks>
public static class ApiRequestExtensions
{
	/// <summary>
	/// Sends a request and deserializes the body into <typeparamref name="TResponse"/>.
	/// </summary>
	public static async Task<ServiceResponse<TResponse>> SendAsync<TResponse>(
		Func<Task<HttpResponseMessage>> send,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var response = await send();

			if (!response.IsSuccessStatusCode)
			{
				return ServiceResponse<TResponse>.Failure(
					await response.ReadErrorDetailAsync(cancellationToken));
			}

			var payload = await response.Content
				.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);

			return payload is null
				? ServiceResponse<TResponse>.Failure("The server returned an empty response.")
				: ServiceResponse<TResponse>.Success(payload);
		}
		catch (OperationCanceledException) { throw; }
		catch (Exception ex) when (ex is HttpRequestException or JsonException or NotSupportedException)
		{
			return ServiceResponse<TResponse>.Failure($"Unable to reach the server. {ex.Message}");
		}
	}

	/// <summary>
	/// Sends a request, deserializes the body, then projects it. Use when the endpoint
	/// wraps its payload in a response envelope the caller does not want to expose.
	/// </summary>
	/// <remarks>
	/// A null projection result means the envelope arrived but the part that matters was
	/// missing, which is a server-side problem rather than a transport one - hence the
	/// distinct message.
	/// </remarks>
	public static async Task<ServiceResponse<TResult>> SendAsync<TResponse, TResult>(
		Func<Task<HttpResponseMessage>> send,
		Func<TResponse, TResult?> select,
		CancellationToken cancellationToken = default)
	{
		var response = await SendAsync<TResponse>(send, cancellationToken);

		if (!response.IsSuccess)
		{
			return ServiceResponse<TResult>.Failure(response.ErrorDetail);
		}

		var projected = select(response.Data!);

		return projected is null
			? ServiceResponse<TResult>.Failure("The server returned an empty response.")
			: ServiceResponse<TResult>.Success(projected);
	}
}
