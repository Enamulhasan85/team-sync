using MediatR;
using Template.Application.Common.Models;
using Template.Domain.Enums;

namespace Template.Application.Features.Tasks.Queries
{
    public class GetTaskByIdQuery : IRequest<Result<TaskDto>>
    {
        public required string Id { get; set; }
    }

    public class GetPaginatedTasksQuery : IRequest<Result<PaginatedResult<TaskDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? ProjectId { get; set; }
        public TaskWorkflowStatus? Status { get; set; }
        public string? AssigneeId { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
    }

    public class TaskDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskWorkflowStatus Status { get; set; }
        public string? AssigneeId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
