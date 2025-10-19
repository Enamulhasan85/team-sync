using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.API.Controllers.Common;
using Template.Application.Features.Tasks.Commands;
using Template.Application.Features.Tasks.Queries;
using Template.Domain.Enums;

namespace Template.API.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class TasksController : BaseController
    {
        private readonly IMediator _mediator;

        public TasksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetPaginated(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? projectId = null,
            [FromQuery] int? status = null,
            [FromQuery] string? assigneeId = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false,
            CancellationToken cancellationToken = default)
        {
            var query = new GetPaginatedTasksQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                ProjectId = projectId,
                Status = status.HasValue ? (TaskWorkflowStatus)status.Value : null,
                AssigneeId = assigneeId,
                SortBy = sortBy,
                SortDescending = sortDescending
            };
            var result = await _mediator.Send(query, cancellationToken);
            return result.Succeeded ? SuccessResponse(result.Value) : FailureResponse(result.Errors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            var query = new GetTaskByIdQuery { Id = id };
            var result = await _mediator.Send(query, cancellationToken);
            return result.Succeeded ? SuccessResponse(result.Value) : FailureResponse(result.Errors);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Succeeded ? SuccessResponse(result.Value) : FailureResponse(result.Errors);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateTaskCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return FailureResponse("Task ID in URL does not match ID in body");

            var result = await _mediator.Send(command, cancellationToken);
            return result.Succeeded ? SuccessResponse(result.Value) : FailureResponse(result.Errors);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            var command = new DeleteTaskCommand { Id = id };
            var result = await _mediator.Send(command, cancellationToken);
            return result.Succeeded ? SuccessResponse(result.Value) : FailureResponse(result.Errors);
        }
    }
}
