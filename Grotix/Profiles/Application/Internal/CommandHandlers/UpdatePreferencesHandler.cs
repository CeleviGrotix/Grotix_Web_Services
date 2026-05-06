using MediatR;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.Internal.CommandHandlers;

/// <summary>Informe Profile: activar/desactivar canales de notificación en preferencias.</summary>
public sealed class UpdatePreferencesHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateUserPreferencesCommand, User>
{
    public async Task<User> Handle(UpdateUserPreferencesCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId)
                   ?? throw new KeyNotFoundException($"Usuario {request.UserId} no encontrado.");
        user.UpdatePreferences(new UserPreferences(request.Push, request.Email));
        await unitOfWork.CompleteAsync();
        return user;
    }
}
