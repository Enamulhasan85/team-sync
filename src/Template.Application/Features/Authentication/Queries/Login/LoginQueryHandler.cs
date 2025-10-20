using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Models;
using Template.Application.Common.Settings;
using Template.Domain.Identity;

namespace Template.Application.Features.Authentication.Queries.Login
{
    public class LoginQueryHandler : IRequestHandler<LoginQuery, Result<LoginResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ICacheService _cacheService;
        private readonly JwtSettings _jwtSettings;

        public LoginQueryHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            ICacheService cacheService,
            IOptions<JwtSettings> jwtOptions)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _cacheService = cacheService;
            _jwtSettings = jwtOptions.Value;
        }

        public async Task<Result<LoginResponse>> Handle(
            LoginQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<LoginResponse>.Failure("Invalid email or password");
            }

            if (!user.IsActive)
            {
                return Result<LoginResponse>.Failure("Account is inactive. Please contact support.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                return Result<LoginResponse>.Failure("Invalid email or password");
            }

            var cacheKey = $"users:session:{user.Id}:{user.SecurityStamp}";
            var cachedSession = await _cacheService.GetAsync<LoginResponse>(cacheKey);
            if (cachedSession != null)
            {
                return Result<LoginResponse>.Success(cachedSession);
            }

            var roles = await _userManager.GetRolesAsync(user);

            var token = await _tokenService.GenerateTokenAsync(
                user.Id.ToString(),
                user.Email!,
                roles,
                false,
                user.UserName,
                user.FullName);

            var refreshToken = await _tokenService.GenerateRefreshTokenAsync();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);

            user.SetRefreshToken(refreshToken, refreshTokenExpiry);
            user.UpdateLastLogin();
            await _userManager.UpdateAsync(user);

            var expiryMinutes = _jwtSettings.ExpiryMinutes;
            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var response = new LoginResponse
            {
                UserId = user.Id.ToString(),
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt
            };

            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(expiryMinutes));

            return Result<LoginResponse>.Success(response);
        }
    }
}
