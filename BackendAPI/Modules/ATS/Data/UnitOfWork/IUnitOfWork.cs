using BuildingBlocks.Data;

namespace ATS.Data.UnitOfWork;

/// <summary>
/// The ATS transaction boundary.
/// </summary>
/// <remarks>
/// The four members come from <see cref="ITransactionScope"/> and are unchanged - they are
/// declared there rather than here so a service can hand this to
/// <c>TransactionRunner.RunAsync</c> instead of hand-rolling begin / SaveChanges / commit /
/// rollback, which is where a forgotten rollback used to be able to hide.
/// </remarks>
public interface IUnitOfWork : ITransactionScope
{
}
