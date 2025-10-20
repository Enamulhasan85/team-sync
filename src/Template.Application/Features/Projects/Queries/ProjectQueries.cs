using MediatR;
using Template.Application.Common.Models;
using Template.Domain.Entities;

namespace Template.Application.Features.Projects.Queries
{
    public class GetProjectByIdQuery : IRequest<Result<ProjectDto>>
    {
        public required string Id { get; set; }
    }


    public class GetPaginatedProjectsQuery : IRequest<Result<PaginatedResult<ProjectDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public ProjectStatus? Status { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
    }

    public class ProjectDto
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
