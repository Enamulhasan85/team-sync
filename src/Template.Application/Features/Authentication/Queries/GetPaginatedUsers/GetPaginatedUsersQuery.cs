using MediatR;
using Template.Application.Common.Models;

namespace Template.Application.Features.Authentication.Queries.GetPaginatedUsers
{
    public class GetPaginatedUsersQuery : IRequest<Result<PaginatedResult<UserDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public string? Role { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
