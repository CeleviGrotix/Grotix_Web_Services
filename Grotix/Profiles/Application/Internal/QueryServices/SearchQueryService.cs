using GrotixBackend.Profiles.Domain.Model.Queries;
using GrotixBackend.Profiles.Interfaces.REST.Resources;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Profiles.Domain.Model.Aggregates; // <--- AGREGADO
using GrotixBackend.Profiles.Domain.Model.Enums;      // <--- AGREGADO
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Profiles.Application.Internal.QueryServices;

public class SearchQueryHandler(AppDbContext context) : IRequestHandler<GetGlobalSearchQuery, IEnumerable<GlobalSearchResultResource>>
{
    public async Task<IEnumerable<GlobalSearchResultResource>> Handle(GetGlobalSearchQuery request, CancellationToken ct)
    {
        var searchTerm = (request.Query ?? string.Empty).Trim();
        var results = new List<GlobalSearchResultResource>();

        // 1. Buscar Agricultores (Users)
        var usersQuery = context.Users
        .Where(u => u.Name != null && u.Name != string.Empty)
        .AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchTerm)) 
        {
            usersQuery = usersQuery.Where(u => EF.Functions.Like(u.Name, $"%{searchTerm}%") || 
                                               EF.Functions.Like((string)(object)u.Email, $"%{searchTerm}%"));
        }

        var users = await usersQuery
            .Where(u => !request.AssociationId.HasValue || u.AssociationId == request.AssociationId)
            .Take(6)
            .ToListAsync(ct);

        results.AddRange(users.Select(u => new GlobalSearchResultResource(
        u.Id, "agriculturist", u.Name!, "Agriculturist", u.ProfilePicture, null, u.IsActive)));

        // 2. Buscar Asociaciones y el estado de su contrato actual
        var assocsData = await context.Associations
            .Where(a => string.IsNullOrWhiteSpace(searchTerm) || EF.Functions.Like(a.Name, $"%{searchTerm}%"))
            .Select(a => new
            {
                a.Id,
                a.Name,
                // Buscamos el estado del contrato más reciente para esta asociación
                ContractStatus = context.Set<Contract>()
                    .Where(c => c.AssociationId == a.Id)
                    .OrderByDescending(c => c.Id)
                    .Select(c => c.Status)
                    .Cast<ContractStatus?>() 
                    .FirstOrDefault()
            })
            .Take(6)
            .ToListAsync(ct);

        results.AddRange(assocsData.Select(data => new GlobalSearchResultResource(
            data.Id,
            "association",
            data.Name,
            "Desde 02/04/2026",
            null,
            data.ContractStatus?.ToString() ?? "NoContract", 
            null)));

        // 3. Mock de Dispositivos (Simulación de Online/Offline)
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.ToLower().Contains("device") || searchTerm.Contains("#"))
        {
            results.Add(new GlobalSearchResultResource(99, "device", "#HF32A1", "Microcontroller", null, "ONLINE", null));
            results.Add(new GlobalSearchResultResource(100, "device", "#S23W1D", "Microcontroller", null, "OFFLINE", null));
        }

        return results;
    }
}