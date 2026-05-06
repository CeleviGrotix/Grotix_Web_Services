using GrotixBackend.Profiles.Domain.Model.Enums;

namespace GrotixBackend.Profiles.Domain.Model.Commands;

public record CreateContractCommand(
    int AssociationId,
    DateTime StartDate,
    DateTime EndDate,
    ContractStatus Status,
    int MaxZones,
    int MaxMicrocontrollers,
    float TotalAmount,
    ContractCurrency Currency,
    ContractPaymentFrequency PaymentFrequency,
    bool IsSuspended);
