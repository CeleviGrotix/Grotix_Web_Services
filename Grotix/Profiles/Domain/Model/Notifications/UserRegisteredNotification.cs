using MediatR;

namespace GrotixBackend.Profiles.Domain.Model.Notifications;

/// <summary>Evento de integración tras registro (informe: disparar bienvenida vía notificaciones).</summary>
public record UserRegisteredNotification(int IdentityId, string Email) : INotification;
