using GrotixBackend.Profiles.Domain.Model.Enums;

namespace GrotixBackend.Profiles.Domain.Model.Commands;

public record UpdateContractCommand(
    int ContractId,
    DateTime? EndDate,
    ContractStatus? Status,
    int? MaxZones,
    int? MaxMicrocontrollers,
    bool? IsSuspended,
    float? TotalAmount,
    ContractPaymentFrequency? PaymentFrequency
);