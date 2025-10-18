using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.API.Controllers.Common;
using Template.Application.Features.Authentication.Queries.UserInfo;

namespace Template.API.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var query = new UserInfoQuery { UserId = id };
            var result = await _mediator.Send(query);
            return result.Succeeded ? SuccessResponse(result.Value) : FailureResponse(result.Errors);
        }

    }
}
