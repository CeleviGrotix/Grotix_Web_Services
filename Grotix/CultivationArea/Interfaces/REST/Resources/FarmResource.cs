namespace GrotixBackend.CultivationArea.Interfaces.REST.Resources;

public record FarmResource(int Id, int? UserId, int AssociationId, string Name, string Location);
