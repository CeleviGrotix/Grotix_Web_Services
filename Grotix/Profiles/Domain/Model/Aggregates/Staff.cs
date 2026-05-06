using GrotixBackend.Profiles.Domain.Model.Enums;

namespace GrotixBackend.Profiles.Domain.Model.Aggregates;

/// <summary>Personal técnico vinculado a un usuario del perfil (tabla <c>staff</c>).</summary>
public class Staff
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public TechnicalRole TechnicalRole { get; private set; }
    public bool IsActive { get; private set; }

    protected Staff() { }

    public Staff(int userId, TechnicalRole technicalRole, bool isActive = true)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId inválido.");
        UserId = userId;
        TechnicalRole = technicalRole;
        IsActive = isActive;
    }

    public void UpdateTechnicalRole(TechnicalRole technicalRole) => TechnicalRole = technicalRole;

    public void SetActive(bool isActive) => IsActive = isActive;
}
