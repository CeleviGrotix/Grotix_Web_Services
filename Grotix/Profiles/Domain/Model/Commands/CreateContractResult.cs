using GrotixBackend.Profiles.Domain.Model.Aggregates;

namespace GrotixBackend.Profiles.Domain.Model.Commands;

/// <summary>Resultado de crear contrato + usuario administrador de la organización.</summary>
public sealed record CreateContractResult(Contract Contract, int OrganizationAdminUserId);
