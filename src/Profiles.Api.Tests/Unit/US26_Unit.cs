using FluentAssertions;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using Xunit;

namespace Profiles.Api.Tests.Unit;

/// <summary>
/// US26 — Configuración y gestión de alertas de usuario
/// </summary>
public class US26_NotificationPreferencesUnitTests
{
    private static User BuildUser() =>
        new User(100, UserEmail.Create("test@grotix.pe"), 4, "Test User");

    // Escenario 1 — Activar/Desactivar notificaciones

    [Fact]
    public void User_GetPreferences_ReturnsDefaultPreferences_WhenNoneSet()
    {
        var user = BuildUser();
        var prefs = user.GetPreferences();
        prefs.Should().NotBeNull();
    }

    [Fact]
    public void User_UpdatePreferences_PersistsPushEnabled()
    {
        var user = BuildUser();
        var prefs = new UserPreferences(push: true, email: false);

        user.UpdatePreferences(prefs);

        user.GetPreferences().Push.Should().BeTrue();
        user.GetPreferences().Email.Should().BeFalse();
    }

    [Fact]
    public void User_UpdatePreferences_PersistsEmailEnabled()
    {
        var user = BuildUser();
        var prefs = new UserPreferences(push: false, email: true);

        user.UpdatePreferences(prefs);

        user.GetPreferences().Push.Should().BeFalse();
        user.GetPreferences().Email.Should().BeTrue();
    }

    [Fact]
    public void User_UpdatePreferences_CanDisableBothChannels()
    {
        var user = BuildUser();
        var prefs = new UserPreferences(push: false, email: false);

        user.UpdatePreferences(prefs);

        user.GetPreferences().Push.Should().BeFalse();
        user.GetPreferences().Email.Should().BeFalse();
    }

    [Fact]
    public void User_UpdatePreferences_CanEnableBothChannels()
    {
        var user = BuildUser();
        var prefs = new UserPreferences(push: true, email: true);

        user.UpdatePreferences(prefs);

        user.GetPreferences().Push.Should().BeTrue();
        user.GetPreferences().Email.Should().BeTrue();
    }

    // Escenario 3 — Notificaciones del sistema

    [Fact]
    public void UserNotification_Constructor_SetsAllFields()
    {
        var notification = new UserNotification(1, "Alerta Crítica", "Humedad baja en zona 1", "alert");

        notification.UserId.Should().Be(1);
        notification.Title.Should().Be("Alerta Crítica");
        notification.Message.Should().Be("Humedad baja en zona 1");
        notification.Type.Should().Be("alert");
        notification.IsRead.Should().BeFalse();
    }

    [Fact]
    public void UserNotification_MarkAsRead_SetsIsReadAndReadAt()
    {
        var notification = new UserNotification(1, "Test", "Mensaje", "info");

        notification.MarkAsRead();

        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().NotBeNull();
        notification.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UserNotification_MarkAsRead_IsIdempotent()
    {
        var notification = new UserNotification(1, "Test", "Mensaje", "info");
        notification.MarkAsRead();
        var firstReadAt = notification.ReadAt;

        notification.MarkAsRead();

        notification.ReadAt.Should().Be(firstReadAt);
    }

    [Fact]
    public void UserNotification_Constructor_Throws_WhenTitleIsEmpty()
    {
        Action act = () => new UserNotification(1, "", "Mensaje");
        act.Should().Throw<ArgumentException>().WithMessage("Title es requerido.");
    }

    [Fact]
    public void UserNotification_Constructor_Throws_WhenUserIdIsInvalid()
    {
        Action act = () => new UserNotification(0, "Título", "Mensaje");
        act.Should().Throw<ArgumentException>().WithMessage("UserId invalido.");
    }

    [Theory]
    [InlineData("alert", "alert")]
    [InlineData("warning", "warning")]
    [InlineData("info", "info")]
    [InlineData("success", "success")]
    [InlineData(null, "info")]
    [InlineData("unknown", "info")]
    public void UserNotification_NormalizesType_Correctly(string? input, string expected)
    {
        var notification = new UserNotification(1, "Título", "Mensaje", input);
        notification.Type.Should().Be(expected);
    }
}