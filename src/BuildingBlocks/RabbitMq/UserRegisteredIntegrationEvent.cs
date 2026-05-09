namespace GrotixBackend.BuildingBlocks.RabbitMq;

/// <summary>Contrato JSON publicado con routing key <c>user.registered</c>.</summary>
public sealed record UserRegisteredIntegrationEvent(int IdentityId, string Email);
