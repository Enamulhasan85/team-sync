using MediatR;
using MongoDB.Bson;
using Template.Application.Common.Models;
using Template.Domain.Enums;

namespace Template.Application.Features.Tasks.Commands
{
    public class CreateTaskCommand : IRequest<Result<TaskResponse>>
    {
        public required string ProjectId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public TaskWorkflowStatus Status { get; set; } = TaskWorkflowStatus.ToDo;
        public string? AssigneeId { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class UpdateTaskCommand : IRequest<Result<TaskResponse>>
    {
        public required string Id { get; set; }
        public required string ProjectId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public TaskWorkflowStatus Status { get; set; }
        public string? AssigneeId { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class DeleteTaskCommand : IRequest<Result<bool>>
    {
        public required string Id { get; set; }
    }

    public class TaskResponse
    {
        public string Id { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskWorkflowStatus Status { get; set; }
        public string? AssigneeId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
