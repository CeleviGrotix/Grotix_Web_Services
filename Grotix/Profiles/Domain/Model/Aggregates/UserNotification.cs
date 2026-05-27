namespace GrotixBackend.Profiles.Domain.Model.Aggregates;

/// <summary>
/// Notificación interna asociada a un usuario de Profiles.
/// </summary>
public class UserNotification
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string Type { get; private set; } = "info";
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    protected UserNotification() { }

    public UserNotification(int userId, string title, string message, string? type = null)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId invalido.");
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title es requerido.");
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message es requerido.");

        UserId = userId;
        Title = title.Trim();
        Message = message.Trim();
        Type = NormalizeType(type);
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }

    private static string NormalizeType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return "info";

        var normalized = type.Trim().ToLowerInvariant();
        return normalized switch
        {
            "info" => "info",
            "warning" => "warning",
            "alert" => "alert",
            "success" => "success",
            _ => "info"
        };
    }
}

