using GrotixBackend.IAM.Application.Resources;
using GrotixBackend.IAM.Domain.Model.Commands;
using GrotixBackend.IAM.Domain.Model.Services;
using MediatR;

public class IdentityCommandService : IIdentityCommandService
{
    private readonly IMediator _mediator;
    public IdentityCommandService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<int> Handle(RegisterCommand command) => await _mediator.Send(command);
    public async Task<LoginResponse> Handle(LoginCommand command) => await _mediator.Send(command);
}