using Xunit;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Enums;
using GrotixBackend.Profiles.Domain.Security;

namespace Grotix.UnitTests.Domain;

public class FarmTests
{
    [Fact]
    public void Constructor_ValidData_AssignsPropertiesCorrectly()
    {
        // Arrange & Act
        var farm = new Farm(1, "Granja Los Andes", "Cusco, Perú");

        // Assert
        Assert.Equal(1, farm.UserId);
        Assert.Equal("Granja Los Andes", farm.Name);
        Assert.Equal("Cusco, Perú", farm.Location);
    }

    [Fact]
    public void Constructor_EmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Farm(1, "   ", "Cusco, Perú"));
    }

    [Fact]
    public void Constructor_InvalidUserId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Farm(0, "Granja Los Andes", "Cusco, Perú"));
    }

    [Fact]
    public void Update_ValidData_UpdatesNameAndLocation()
    {
        // Arrange
        var farm = new Farm(1, "Granja Vieja", "Lima");

        // Act
        farm.Update("Granja Nueva", "Arequipa");

        // Assert
        Assert.Equal("Granja Nueva", farm.Name);
        Assert.Equal("Arequipa", farm.Location);
    }
}

public class ContractTests
{
    private static Contract BuildValidContract(DateTime? start = null, DateTime? end = null)
    {
        var s = start ?? DateTime.Today;
        var e = end ?? DateTime.Today.AddYears(1);
        return new Contract(1, s, e, ContractStatus.Active, 10, 5, 1500f,
            ContractCurrency.USD, ContractPaymentFrequency.Monthly, false);
    }

    [Fact]
    public void Constructor_EndDateBeforeStartDate_ThrowsArgumentException()
    {
        // Arrange
        var start = DateTime.Today;
        var end = DateTime.Today.AddDays(-1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Contract(1, start, end, ContractStatus.Active, 10, 5, 1500f,
                ContractCurrency.USD, ContractPaymentFrequency.Monthly, false));
    }

    [Fact]
    public void Constructor_NegativeTotalAmount_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Contract(1, DateTime.Today, DateTime.Today.AddYears(1),
                ContractStatus.Active, 10, 5, -100f,
                ContractCurrency.USD, ContractPaymentFrequency.Monthly, false));
    }

    [Fact]
    public void Update_SuspendsContract_SetsSuspendedTrue()
    {
        // Arrange
        var contract = BuildValidContract();

        // Act
        contract.Update(null, null, null, null, true, null, null);

        // Assert
        Assert.True(contract.IsSuspended);
    }
}

public class CropTests
{
    [Fact]
    public void Constructor_NegativeMaxStressTime_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Crop("Papa", "Solanum tuberosum", 18.0, 75.0, 6.0, -1));
    }

    [Fact]
    public void UpdateBiologicalProfile_ValidData_UpdatesCorrectly()
    {
        // Arrange
        var crop = new Crop("Papa", "Solanum tuberosum", 18.0, 75.0, 6.0, 48);

        // Act
        crop.UpdateBiologicalProfile(20.0, 80.0, 7.0, 24);

        // Assert
        Assert.Equal(20.0, crop.OptimalTemperature);
        Assert.Equal(80.0, crop.OptimalHumidity);
        Assert.Equal(24, crop.MaxStressTime);
    }
}

public class ZoneTests
{
    [Fact]
    public void Constructor_InvalidFarmId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Zone(0, 1, -12.046, -77.042));
    }

    [Fact]
    public void UpdatePhase_ValidPhase_UpdatesCurrentPhase()
    {
        // Arrange
        var zone = new Zone(1, 1, -12.046, -77.042, "Germinación");
        var newDate = DateTime.Today;

        // Act
        zone.UpdatePhase("Floración", newDate);

        // Assert
        Assert.Equal("Floración", zone.CurrentPhase);
        Assert.Equal(newDate, zone.PhaseStartDate);
    }
}

public class InviteTokenHasherTests
{
    [Fact]
    public void Hash_SameInput_AlwaysReturnsSameHash()
    {
        // Arrange
        var token = InviteTokenHasher.GenerateToken();

        // Act
        var hash1 = InviteTokenHasher.Hash(token);
        var hash2 = InviteTokenHasher.Hash(token);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GenerateToken_TwoCalls_ReturnsDifferentTokens()
    {
        // Act
        var token1 = InviteTokenHasher.GenerateToken();
        var token2 = InviteTokenHasher.GenerateToken();

        // Assert
        Assert.NotEqual(token1, token2);
    }
}