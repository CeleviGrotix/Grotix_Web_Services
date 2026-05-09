namespace GrotixBackend.Profiles.Domain.Model.Queries;

/// <summary>Un perfil por id solo si su rol es de agricultor.</summary>
public sealed record GetFarmerByIdQuery(int UserId);
