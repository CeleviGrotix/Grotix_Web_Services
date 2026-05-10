using GrotixBackend.Profiles.Interfaces.REST.Resources;
using MediatR;

namespace GrotixBackend.Profiles.Domain.Model.Queries;

public record GetGlobalSearchQuery(string Query, int? AssociationId) 
    : IRequest<IEnumerable<GlobalSearchResultResource>>;