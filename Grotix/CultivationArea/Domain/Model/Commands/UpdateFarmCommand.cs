namespace GrotixBackend.CultivationArea.Domain.Model.Commands;

public record UpdateFarmCommand(int FarmId, string Name, string Location);
