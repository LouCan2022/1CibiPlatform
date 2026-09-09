namespace BuildingBlocks.Data;

/// <summary>
/// Runs a unit of work inside a transaction, rolling back if it throws.
/// </summary>
/// <remarks>
/// Every service that writes more than one row was repeating the same seven lines:
/// begin, do the work, SaveChanges, Commit, catch, Rollback, rethrow. That block is
/// transaction plumbing, not feature code, and each copy is a chance to forget the
/// rollback - or to swallow the exception on the way out and turn a failed write into a
/// silent success.
///
/// This owns it once. The <c>catch</c> here is deliberately NOT error handling: it exists
/// only to release the transaction, and the exception is rethrown untouched so
/// <see cref="Exceptions.Handler.CustomExceptionHandler"/> still maps it to the right
/// status code. A handler that throws <c>NotFoundException</c> inside a transaction still
/// produces a 404, not a 500.
///
/// Contrast with <see cref="Exceptions.Handler.SideEffectGuard"/>, which suppresses. Use
/// that for best-effort work after a commit; use this for the commit itself.
/// </remarks>
public static class TransactionRunner
{
	public static async Task RunAsync(
		ITransactionScope unitOfWork,
		Func<Task> work,
		CancellationToken cancellationToken = default)
	{
		await unitOfWork.BeginTransactionAsync(cancellationToken);

		try
		{
			await work();

			await unitOfWork.SaveChangesAsync(cancellationToken);

			await unitOfWork.CommitAsync(cancellationToken);
		}
		catch
		{
			// Release the transaction, then let the original exception continue to the
			// global handler untouched. Nothing is logged or wrapped here - doing either
			// would hide which exception type was thrown and cost the caller its status
			// code.
			await unitOfWork.RollbackAsync(cancellationToken);

			throw;
		}
	}

	/// <summary>
	/// As <see cref="RunAsync(ITransactionScope, Func{Task}, CancellationToken)"/>, for a
	/// unit of work that produces a value. The value is returned only after the commit
	/// succeeds.
	/// </summary>
	public static async Task<T> RunAsync<T>(
		ITransactionScope unitOfWork,
		Func<Task<T>> work,
		CancellationToken cancellationToken = default)
	{
		await unitOfWork.BeginTransactionAsync(cancellationToken);

		try
		{
			var result = await work();

			await unitOfWork.SaveChangesAsync(cancellationToken);

			await unitOfWork.CommitAsync(cancellationToken);

			return result;
		}
		catch
		{
			await unitOfWork.RollbackAsync(cancellationToken);

			throw;
		}
	}

	/// <summary>
	/// Runs work that has already touched something a database transaction cannot undo -
	/// an uploaded blob, a created remote record - and undoes it if the work throws.
	/// </summary>
	/// <remarks>
	/// Object storage and third-party APIs are not enlisted in the transaction, so a failed
	/// write leaves them holding an orphan. <paramref name="compensate"/> is the manual
	/// rollback for exactly that, and runs only on failure.
	///
	/// The original exception always wins: if the compensation itself throws, that failure
	/// is passed to <paramref name="onCompensationFailed"/> rather than replacing the real
	/// error, because "could not delete the blob" is far less useful to the caller than the
	/// insert failure that caused it.
	/// </remarks>
	public static Task RunWithCompensationAsync(
		Func<Task> work,
		Func<Task> compensate,
		Action<Exception>? onCompensationFailed = null) =>
		RunWithCompensationAsync(work, [compensate], onCompensationFailed);

	/// <summary>
	/// As above, for work that has to undo several things.
	/// </summary>
	/// <remarks>
	/// Each compensation is awaited in its own try, so one that throws does not stop the
	/// rest from running - the whole point of passing several is that they are independent,
	/// and losing the second cleanup because the first failed is the bug this prevents.
	///
	/// They run in the order given. Pass them in reverse order of acquisition (undo the
	/// last thing first) when one depends on another.
	/// </remarks>
	public static async Task RunWithCompensationAsync(
		Func<Task> work,
		IReadOnlyList<Func<Task>> compensations,
		Action<Exception>? onCompensationFailed = null)
	{
		try
		{
			await work();
		}
		catch
		{
			foreach (var compensate in compensations)
			{
				try
				{
					await compensate();
				}
				catch (Exception compensationException)
				{
					// Reported, never rethrown: the original failure is the one the caller
					// needs, and a broken cleanup must not stop the cleanups after it.
					onCompensationFailed?.Invoke(compensationException);
				}
			}

			throw;
		}
	}
}
