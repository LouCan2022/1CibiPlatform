namespace BuildingBlocks.Exceptions.Handler;

/// <summary>
/// Runs a best-effort side effect that must never fail the operation that triggered it.
/// </summary>
/// <remarks>
/// The counterpart to <see cref="CustomExceptionHandler"/>, and the reason it is a
/// separate thing rather than more cases in that switch.
///
/// CustomExceptionHandler exists for exceptions that SHOULD fail the request: it turns a
/// thrown NotFoundException or ValidationException into the right status code and a
/// ProblemDetails body. That is the correct destination for almost everything, and feature
/// code should throw and let it handle the result rather than catching.
///
/// A small number of operations are the opposite case. Raising a notification after a
/// candidate submits an application form, or after a bulk file finishes parsing, is a
/// follow-up to work that has already committed. If that follow-up throws and is allowed
/// to bubble, CustomExceptionHandler does exactly its job and returns a 500 - and the
/// candidate is told their submission failed when it actually succeeded, so they submit
/// again. The exception has to stop before it reaches the pipeline.
///
/// This is that stopping point, in one place, so no caller hand-rolls its own try/catch.
/// A failure is logged and swallowed; nothing else is.
///
/// Use it ONLY where all three are true:
///   1. The primary work is already durable (committed).
///   2. The side effect is best-effort - the user is not waiting on its result.
///   3. Losing it degrades the experience rather than the data.
/// Anywhere else, throw and let CustomExceptionHandler answer.
/// </remarks>
public static class SideEffectGuard
{
	public static async Task RunAsync(
		Func<Task> sideEffect,
		ILogger logger,
		string description,
		CancellationToken cancellationToken = default)
	{
		try
		{
			await sideEffect();
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			// Shutdown or a cancelled request, not a fault. The primary work is already
			// committed, so there is nothing to report and nothing to undo.
		}
		catch (Exception exception)
		{
			logger.LogError(
				exception,
				"Best-effort side effect failed and was suppressed: {Description}",
				description);
		}
	}

	/// <summary>
	/// As <see cref="RunAsync(Func{Task}, ILogger, string, CancellationToken)"/>, but for a
	/// side effect that produces a value. Returns <paramref name="fallback"/> on failure.
	/// </summary>
	public static async Task<T?> RunAsync<T>(
		Func<Task<T?>> sideEffect,
		ILogger logger,
		string description,
		T? fallback = default,
		CancellationToken cancellationToken = default)
	{
		try
		{
			return await sideEffect();
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			return fallback;
		}
		catch (Exception exception)
		{
			logger.LogError(
				exception,
				"Best-effort side effect failed and was suppressed: {Description}",
				description);

			return fallback;
		}
	}
}
