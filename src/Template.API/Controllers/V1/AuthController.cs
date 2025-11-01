using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Template.API.Controllers.Common;
using Template.Application.Features.Authentication.Commands.Register;
using Template.Application.Features.Authentication.Queries.Login;

namespace Template.API.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        [EnableRateLimiting("Authentication")]
        public async Task<IActionResult> Register(RegisterCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Succeeded ? SuccessResponse(result.Value) : FailureResponse(result.Errors);
        }

        [HttpPost("login")]
        [EnableRateLimiting("Authentication")]
        public async Task<IActionResult> Login(LoginQuery query)
        {
            var result = await _mediator.Send(query);
            return result.Succeeded ? SuccessResponse(result.Value) : FailureResponse(result.Errors);
        }

    }
}
