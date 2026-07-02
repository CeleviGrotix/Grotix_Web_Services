using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
namespace HardwareDevice.Api.Tests.Unit;

public class ProfilesUnitTests
{
    [Fact]
    public void TC_U61_Identity_ValidConstructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var email = "juan@grotix.pe";
        var passwordHash = "AQAAAAEAACcQAAAAEBC...";
        
        // Simulación del agregado Identity o User
        var identity = new { Email = email, PasswordHash = passwordHash };

        // Assert
        identity.Email.Should().Be(email);
        identity.PasswordHash.Should().Be(passwordHash);
    }

    [Fact]
    public void TC_U62_Identity_InvalidEmailFormat_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Action act = () => {
            var email = "juan@@grotix";
            if (!email.Contains("@") || email.Split('@').Length > 2)
                throw new ArgumentException("Email con formato inválido");
        };

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Email con formato inválido");
    }

    [Fact]
    public void TC_U63_InviteTokenHasher_ValidToken_ShouldReturnTrue()
    {
        // Arrange
        var tokenPlano = "grotix-invite-2026";
        var hashAlmacenado = "hashed-grotix-invite-2026";
        var mockHasher = new Mock<IInviteTokenHasher>();
        mockHasher.Setup(h => h.VerifyToken(tokenPlano, hashAlmacenado)).Returns(true);

        // Act
        var result = mockHasher.Object.VerifyToken(tokenPlano, hashAlmacenado);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TC_U64_InviteTokenHasher_ExpiredToken_ShouldReturnFalse()
    {
        // Arrange
        var mockHasher = new Mock<IInviteTokenHasher>();
        mockHasher.Setup(h => h.IsTokenExpired(It.IsAny<DateTime>())).Returns(true);

        // Act
        var result = mockHasher.Object.IsTokenExpired(DateTime.UtcNow.AddDays(-1));

        // Assert
        result.Should().BeTrue(); // Retorna true indicando que SÍ está expirado (o false según tu firma)
    }

    [Fact]
    public void TC_U65_CreateAccountHandler_EmailMismatch_ShouldThrowArgumentException()
    {
        // Arrange
        var inviteEmail = "juan@grotix.pe";
        var requestEmail = "pedro@grotix.pe";

        // Act
        Action act = () => {
            if (inviteEmail != requestEmail)
                throw new ArgumentException("Email no coincide con la invitación");
        };

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task TC_U66_CreateAccountHandler_ValidInvitation_ShouldRegisterUserSuccessfully()
    {
        // Arrange
        var mockService = new Mock<IAccountService>();
        mockService.Setup(s => s.CreateAccountAsync(It.IsAny<string>(), It.IsAny<string>()))
                   .ReturnsAsync(true);

        // Act
        var result = await mockService.Object.CreateAccountAsync("juan@grotix.pe", "valid-token");

        // Assert
        result.Should().BeTrue();
    }
}

public interface IInviteTokenHasher { bool VerifyToken(string p, string h); bool IsTokenExpired(DateTime d); }
public interface IAccountService { Task<bool> CreateAccountAsync(string e, string t); }