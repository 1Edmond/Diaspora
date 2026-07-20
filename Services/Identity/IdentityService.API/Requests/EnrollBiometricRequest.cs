namespace IdentityService.API.Requests;

public record EnrollBiometricRequest(string DeviceId, string PublicKey);
