using GrotixBackend.Profiles.Domain.Model.Aggregates;

namespace GrotixBackend.Profiles.Domain.Model.Commands;

/// <summary>Resultado de crear contrato y, si aplica, invitación o asignación del administrador de organización.</summary>
public sealed record CreateContractResult(
    Contract Contract,
    string OrgAdminEmail,
    int? OrgAdminInviteId = null,
    string? OrgAdminInvitePlaintextToken = null,
    int? AssignedOrgAdminUserId = null,
    bool InviteSkipped = false);
