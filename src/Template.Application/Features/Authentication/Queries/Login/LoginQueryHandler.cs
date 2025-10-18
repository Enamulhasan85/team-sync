using MediatR;
using Microsoft.AspNetCore.Identity;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Models;
using Template.Domain.Identity;

namespace Template.Application.Features.Authentication.Queries.Login
{
    public class LoginQueryHandler : IRequestHandler<LoginQuery, Result<LoginResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ICacheService _cacheService;

        public LoginQueryHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            ICacheService cacheService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _cacheService = cacheService;
        }

        public async Task<Result<LoginResponse>> Handle(
            LoginQuery request,
            CancellationToken cancellationToken)
        {
            var cacheKey = $"user:session:{request.Email}";
            var cachedSession = await _cacheService.GetAsync<LoginResponse>(cacheKey);
            if (cachedSession != null)
            {
                return Result<LoginResponse>.Success(cachedSession);
            }

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

            var roles = await _userManager.GetRolesAsync(user);

            var token = await _tokenService.GenerateTokenAsync(
                user.Id.ToString(),
                user.Email!,
                roles);

            var refreshToken = await _tokenService.GenerateRefreshTokenAsync();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            user.SetRefreshToken(refreshToken, refreshTokenExpiry);
            user.UpdateLastLogin();
            await _userManager.UpdateAsync(user);

            var expiresAt = DateTime.UtcNow.AddHours(24);

            var response = new LoginResponse
            {
                UserId = user.Id.ToString(),
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt
            };

            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromHours(24));

            return Result<LoginResponse>.Success(response);
        }
    }
}
