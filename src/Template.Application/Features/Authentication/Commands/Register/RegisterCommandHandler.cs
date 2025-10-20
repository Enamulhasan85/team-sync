using MediatR;
using Microsoft.AspNetCore.Identity;
using Template.Application.Common.Models;
using Template.Domain.Identity;

namespace Template.Application.Features.Authentication.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RegisterCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<RegisterResponse>> Handle(
            RegisterCommand request,
            CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Result<RegisterResponse>.Failure("User with this email already exists");
            }

            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                FullName = $"{request.FirstName} {request.LastName}",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return Result<RegisterResponse>.Failure(errors);
            }

            await _userManager.AddToRoleAsync(user, "User");

            var response = new RegisterResponse
            {
                UserId = user.Id.ToString(),
                Email = user.Email!,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            return Result<RegisterResponse>.Success(response);
        }
    }
}
