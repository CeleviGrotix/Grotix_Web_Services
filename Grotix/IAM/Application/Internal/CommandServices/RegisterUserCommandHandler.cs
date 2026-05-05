using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using GrotixBackend.IAM.Domain.Model.Commands;
using GrotixBackend.IAM.Domain.Model.Aggregates;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.IAM.Application.Internal.CommandServices
{
    
    public class RegisterUserCommandHandler : IRequestHandler<RegisterCommand, int>
    {
        private readonly IIdentityRepository _identityRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterUserCommandHandler(
            IIdentityRepository identityRepository,
            IUnitOfWork unitOfWork
           )
        {
            _identityRepository = identityRepository;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<int> Handle(RegisterCommand command, CancellationToken cancellationToken)
        {
            var existingIdentity = await _identityRepository.GetByEmailAsync(command.Email);
            if (existingIdentity != null)
            {
                throw new ApplicationException($"Email '{command.Email}' is already taken.");
            }
            
            var identity = new Identity(
                command.UserId,
                command.Email,
                command.Password
            );
            
            await _identityRepository.AddAsync(identity);
            await _unitOfWork.CompleteAsync();
            
            return identity.Id;
        }
    }
}