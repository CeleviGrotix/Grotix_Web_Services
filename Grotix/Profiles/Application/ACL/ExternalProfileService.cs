using GrotixBackend.IAM.Application.Internal.OutboundServices.ACL;
using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Domain.Model.Commands;
namespace GrotixBackend.Profiles.Application.ACL
{
    public class ExternalProfileService : IExternalProfileService // Asegúrate de tener la interfaz
    {
        private readonly IUserCommandService _userCommandService;

        // El constructor es lo que falta para que '_userCommandService' exista
        public ExternalProfileService(IUserCommandService userCommandService)
        {
            _userCommandService = userCommandService;
        }

        public async Task<int> CreateUserAndReturnId(int identityId, string email)
        {
            // Ahora la variable ya existe y no dará error
            var createUserCommand = new CreateUserCommand(identityId, null, email, null, null);
            var user = await _userCommandService.Handle(createUserCommand);

            return user?.Id ?? 0;
        }
    }
}