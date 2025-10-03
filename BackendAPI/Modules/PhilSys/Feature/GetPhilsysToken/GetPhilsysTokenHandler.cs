using PhilSys.DTO;

namespace PhilSys.Feature.GetPhilsysToken;

public record GetPhilsysTokenCommand(GetCredential credential) : ICommand<CredentialResponseDTO>;
