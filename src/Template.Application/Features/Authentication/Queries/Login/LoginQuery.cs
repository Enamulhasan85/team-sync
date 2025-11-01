using System.ComponentModel;
using MediatR;
using Template.Application.Common.Models;

namespace Template.Application.Features.Authentication.Queries.Login
{
    public class LoginQuery : IRequest<Result<LoginResponse>>
    {
        [DefaultValue("admin@template.com")]
        public string Email { get; set; } = string.Empty;
        [DefaultValue("Admin@123")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
