namespace GrotixBackend.CultivationArea.Domain.Model.Commands;

public record CreateFarmCommand(int UserId, string Name, string Location);
