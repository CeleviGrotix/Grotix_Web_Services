namespace GrotixBackend.Profiles.Interfaces.REST.Resources;

public record GlobalSearchResultResource(
    int Id,
    string Type, // "agriculturist", "association", "device"
    string Title,
    string Subtitle,
    string? ImageUrl,
    string? Status, // Para contratos/dispositivos
    bool? IsActive  // Para el toggle del agricultor
);