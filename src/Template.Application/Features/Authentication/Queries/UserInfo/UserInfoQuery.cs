using MediatR;
using Template.Application.Common.Models;

namespace Template.Application.Features.Authentication.Queries.UserInfo
{
    public class UserInfoQuery : IRequest<Result<UserInfoResponse>>
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class UserInfoResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
