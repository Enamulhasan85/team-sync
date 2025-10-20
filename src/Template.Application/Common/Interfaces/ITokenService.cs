using System.Security.Claims;

namespace Template.Application.Common.Interfaces;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(
        string userId, string email, IEnumerable<string> roles,
        bool rememberMe = false, string? userName = null, string? fullName = null
    );
    Task<string> GenerateRefreshTokenAsync();
    Task<bool> ValidateTokenAsync(string token);
}
