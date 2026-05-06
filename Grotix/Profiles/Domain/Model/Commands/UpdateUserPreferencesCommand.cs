using GrotixBackend.Profiles.Domain.Model.Aggregates;
using MediatR;

namespace GrotixBackend.Profiles.Domain.Model.Commands;

public record UpdateUserPreferencesCommand(int UserId, bool Push, bool Email) : IRequest<User>;
