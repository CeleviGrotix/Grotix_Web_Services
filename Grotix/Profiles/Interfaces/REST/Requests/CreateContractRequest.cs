using GrotixBackend.Profiles.Domain.Model.Enums;

namespace GrotixBackend.Profiles.Interfaces.REST.Requests;

/// <summary>Cuerpo de <c>POST /api/v1/contracts</c>. Requiere rol <c>admin</c> o <c>staff</c>.</summary>
public sealed record CreateContractRequest(
    int AssociationId,
    DateTime StartDate,
    DateTime EndDate,
    ContractStatus Status,
    int MaxZones,
    int MaxMicrocontrollers,
    float TotalAmount,
    ContractCurrency Currency,
    ContractPaymentFrequency PaymentFrequency,
    bool IsSuspended,
    /// <summary>Correo al que se enviará la invitación para registrarse como <c>user_admin</c> de la asociación.</summary>
    string OrgAdminEmail);
