namespace FrontendWebassembly.Services.CNX.Interface;

public interface ICandidateService
{
	Task<List<CandidateResponseDTO>> GetCandidates(string gmail, CancellationToken cancellationToken = default);
}
