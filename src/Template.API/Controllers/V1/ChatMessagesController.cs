using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.API.Controllers.Common;
using Template.Application.Features.ChatMessages.Commands;
using Template.Application.Features.ChatMessages.Queries;

namespace Template.API.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class ChatMessagesController : BaseController
    {
        private readonly IMediator _mediator;

        public ChatMessagesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetPaginated(
            [FromQuery] string projectId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = true,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return FailureResponse("ProjectId is required");
            }

            var query = new GetPaginatedChatMessagesQuery
            {
                ProjectId = projectId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _mediator.Send(query, cancellationToken);
            return result.Succeeded ? SuccessResponse(result.Value) : FailureResponse(result.Errors);
        }


        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateChatMessageCommand command,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Succeeded ? SuccessResponse(result.Value) : FailureResponse(result.Errors);
        }


        [HttpPut]
        public async Task<IActionResult> Update(
            [FromBody] UpdateChatMessageCommand command,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Succeeded ? SuccessResponse(result.Value) : FailureResponse(result.Errors);
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(
            [FromBody] DeleteChatMessageCommand command,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Succeeded ? SuccessResponse(result.Value) : FailureResponse(result.Errors);
        }
    }
}
