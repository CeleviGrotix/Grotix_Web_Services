using GrotixBackend.Profiles.Domain.Model.Commands;

namespace GrotixBackend.Profiles.Application.Internal.CommandServices;

public interface IAssociationInviteCommandService
{
    Task<CreateAssociationInviteResult> Handle(CreateAssociationInviteCommand command);
}
