namespace GrotixBackend.Contracts.Integration.Users;

/// <summary>Contrato de integración publicado cuando se registra un usuario.</summary>
public sealed record UserRegisteredIntegrationEvent(int IdentityId, string Email);
