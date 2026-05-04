using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using GrotixBackend.IAM.Application.Resources;
using GrotixBackend.IAM.Domain.Model.Commands;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.IAM.Application.Internal.OutboundServices;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.IAM.Application.Internal.CommandServices
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly IIdentityRepository _identityRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;

        public LoginCommandHandler(
            IIdentityRepository identityRepository,
            IUnitOfWork unitOfWork,
            ITokenService tokenService)
        {
            _identityRepository = identityRepository;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }

        public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var identity = await _identityRepository.GetByUsernameAsync(command.Username);

            if (identity == null || !identity.VerifyPassword(command.Password))
            {
                return new LoginResponse(0, 0, command.Username, false, "Invalid username or password.");
            }

            await _identityRepository.UpdateAsync(identity);
            await _unitOfWork.CompleteAsync();

            var roles = new List<string> { "User" };

            string token = _tokenService.GenerateToken(identity, roles);

            return new LoginResponse(
                identity.Id,
                identity.UserId,
                identity.UserName,
                true,
                "Login successful.",
                token);
        }
    }
}