using MediatR;
using MongoDB.Bson;
using Template.Application.Common.Models;
using Template.Domain.Entities;

namespace Template.Application.Features.Projects.Commands
{
    public class CreateProjectCommand : IRequest<Result<ProjectResponse>>
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Planned;
        public List<string>? MemberIds { get; set; }
    }

    public class UpdateProjectCommand : IRequest<Result<ProjectResponse>>
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ProjectStatus Status { get; set; }
        public List<string>? MemberIds { get; set; }
    }

    public class DeleteProjectCommand : IRequest<Result<bool>>
    {
        public required string Id { get; set; }
    }

    public class ProjectResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ProjectStatus Status { get; set; }
        public List<string> MemberIds { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
