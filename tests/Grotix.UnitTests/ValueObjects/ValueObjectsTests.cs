using Xunit;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;

namespace Grotix.UnitTests.ValueObjects;

public class UserEmailTests
{
    [Fact]
    public void Create_ValidEmail_ReturnsUserEmail()
    {
        // Arrange
        var raw = "agricultor@grotix.pe";

        // Act
        var email = UserEmail.Create(raw);

        // Assert
        Assert.Equal(raw, email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("noesuncorreo")]
    [InlineData("@dominio.com")]
    [InlineData("usuario@x")]
    public void Create_InvalidEmail_ThrowsArgumentException(string raw)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => UserEmail.Create(raw));
    }
}

public class UserPhoneTests
{
    [Theory]
    [InlineData("+51999888777", "+51999888777")]
    [InlineData("+51 999 888 777", "+51999888777")]   // espacios se eliminan
    [InlineData("999888777", "999888777")]             // sin prefijo, 9 dígitos válido
    public void Create_ValidPhone_NormalizesCorrectly(string raw, string expected)
    {
        // Act
        var phone = UserPhone.Create(raw);

        // Assert
        Assert.Equal(expected, phone.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]          // muy corto
    [InlineData("abcdefghij")]   // letras
    public void Create_InvalidPhone_ThrowsArgumentException(string raw)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => UserPhone.Create(raw));
    }
}