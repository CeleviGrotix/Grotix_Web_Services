namespace GrotixBackend.CultivationArea.Domain.Model.Commands;

public record CreateFarmCommand(int AssociationId, string Name, string Location);
