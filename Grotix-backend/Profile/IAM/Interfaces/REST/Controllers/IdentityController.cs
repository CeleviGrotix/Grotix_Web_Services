using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GrotixBackend.IAM.Domain.Model.Commands;
using GrotixBackend.IAM.Domain.Model.Services;
using GrotixBackend.IAM.Application.Resources;
using Microsoft.AspNetCore.Authorization;

namespace GrotixBackend.IAM.Interfaces.REST.Controllers
{
    [ApiController]
    [Route("api/v1/authentication")]
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityCommandService _identityCommandService;

        public IdentityController(IIdentityCommandService identityCommandService)
        {
            _identityCommandService = identityCommandService;
        }

        public record RegisterRequest(string Username, string Password);

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest request)
        {
            var command = new RegisterCommand(0, request.Username, request.Password);

            var result = await _identityCommandService.Handle(command);

            if (result == 0)
            {
                return BadRequest(new { message = "Registration failed." });
            }

            return StatusCode(201, new { message = "Registration successful.", id = result });
        }

        [HttpPost("sign-in")]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn([FromBody] LoginCommand command)
        {
            var response = await _identityCommandService.Handle(command);

            if (!response.Success)
            {
                return Unauthorized(new { message = response.Message });
            }

            return Ok(response);
        }
    }
}