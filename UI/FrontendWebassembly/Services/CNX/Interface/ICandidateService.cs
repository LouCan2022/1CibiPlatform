namespace FrontendWebassembly.Services.CNX.Interface;

public interface ICandidateService
{
	Task<PaginatedCNX> GetCandidates(string gmail, string page, CancellationToken cancellationToken = default);
}
