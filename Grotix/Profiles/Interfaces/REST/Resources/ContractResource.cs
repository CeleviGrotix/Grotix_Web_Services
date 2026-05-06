namespace GrotixBackend.Profiles.Interfaces.REST.Resources;

public record ContractResource(
    int ContractId,
    int AssociationId,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    int MaxZones,
    int MaxMicrocontrollers,
    float TotalAmount,
    string Currency,
    string PaymentFrequency,
    bool IsSuspended);
