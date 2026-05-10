using GrotixBackend.Profiles.Domain.Model.Aggregates;

namespace GrotixBackend.Profiles.Domain.Model.Commands;

/// <summary>Resultado de crear contrato + invitación para el administrador de la organización.</summary>
public sealed record CreateContractResult(
    Contract Contract,
    int OrgAdminInviteId,
    string OrgAdminInvitePlaintextToken,
    string OrgAdminEmail);
