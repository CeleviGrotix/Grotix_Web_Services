using GrotixBackend.Profiles.Domain.Model.Enums;

namespace GrotixBackend.Profiles.Domain.Model.Aggregates;

/// <summary>Contrato de asociación (límites operativos y condiciones comerciales).</summary>
public class Contract
{
    public int Id { get; private set; }
    public int AssociationId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public ContractStatus Status { get; private set; }
    public int MaxZones { get; private set; }
    public int MaxMicrocontrollers { get; private set; }
    public float TotalAmount { get; private set; }
    public ContractCurrency Currency { get; private set; }
    public ContractPaymentFrequency PaymentFrequency { get; private set; }
    public bool IsSuspended { get; private set; }

    protected Contract() { }

    public Contract(
        int associationId,
        DateTime startDate,
        DateTime endDate,
        ContractStatus status,
        int maxZones,
        int maxMicrocontrollers,
        float totalAmount,
        ContractCurrency currency,
        ContractPaymentFrequency paymentFrequency,
        bool isSuspended)
    {
        if (associationId <= 0)
            throw new ArgumentException("AssociationId inválido.");
        if (endDate < startDate)
            throw new ArgumentException("EndDate no puede ser anterior a StartDate.");
        if (maxZones < 0)
            throw new ArgumentException("MaxZones no puede ser negativo.");
        if (maxMicrocontrollers < 0)
            throw new ArgumentException("MaxMicrocontrollers no puede ser negativo.");
        if (totalAmount < 0)
            throw new ArgumentException("TotalAmount no puede ser negativo.");

        AssociationId = associationId;
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
        MaxZones = maxZones;
        MaxMicrocontrollers = maxMicrocontrollers;
        TotalAmount = totalAmount;
        Currency = currency;
        PaymentFrequency = paymentFrequency;
        IsSuspended = isSuspended;
    }
}
