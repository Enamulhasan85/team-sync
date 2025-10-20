using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.API.Controllers.Common;
using Template.Application.Features.Projects.Commands;
using Template.Application.Features.Projects.Queries;
using Template.Domain.Entities;

namespace Template.API.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class ProjectsController : BaseController
    {
        private readonly IMediator _mediator;

        public ProjectsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetPaginated(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? status = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false,
            CancellationToken cancellationToken = default)
        {
            var query = new GetPaginatedProjectsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Status = status.HasValue ? (ProjectStatus)status.Value : null,
                SortBy = sortBy,
                SortDescending = sortDescending
            };
            var result = await _mediator.Send(query, cancellationToken);
            return result.Succeeded ? SuccessResponse(result.Value) : FailureResponse(result.Errors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            var query = new GetProjectByIdQuery { Id = id };
            var result = await _mediator.Send(query, cancellationToken);
            return result.Succeeded ? SuccessResponse(result.Value) : FailureResponse(result.Errors);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Succeeded ? SuccessResponse(result.Value) : FailureResponse(result.Errors);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateProjectCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return FailureResponse("Project ID in URL does not match ID in body");

            var result = await _mediator.Send(command, cancellationToken);
            return result.Succeeded ? SuccessResponse(result.Value) : FailureResponse(result.Errors);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            var command = new DeleteProjectCommand { Id = id };
            var result = await _mediator.Send(command, cancellationToken);
            return result.Succeeded ? SuccessResponse(result.Value) : FailureResponse(result.Errors);
        }
    }
}