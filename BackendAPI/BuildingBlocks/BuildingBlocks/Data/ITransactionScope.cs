namespace BuildingBlocks.Data;

/// <summary>
/// The transaction primitives <see cref="TransactionRunner"/> drives.
/// </summary>
/// <remarks>
/// Each module already has its own <c>IUnitOfWork</c> with exactly these four members
/// (ATS, PhilSys). Rather than move those to a shared type - which would touch every
/// service that injects one - each module's interface simply declares that it implements
/// this, and the shared runner works against it.
/// </remarks>
public interface ITransactionScope
{
	Task BeginTransactionAsync(CancellationToken ct = default);

	Task SaveChangesAsync(CancellationToken ct = default);

	Task CommitAsync(CancellationToken ct = default);

	Task RollbackAsync(CancellationToken ct = default);
}
