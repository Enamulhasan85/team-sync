using MediatR;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using Template.Application.Common.Models;
using Template.Domain.Identity;

namespace Template.Application.Features.Authentication.Queries.UserInfo
{
    public class UserInfoQueryHandler : IRequestHandler<UserInfoQuery, Result<UserInfoResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserInfoQueryHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<UserInfoResponse>> Handle(
            UserInfoQuery request,
            CancellationToken cancellationToken)
        {
            if (!ObjectId.TryParse(request.UserId, out var objectId))
            {
                return Result<UserInfoResponse>.Failure("Invalid user ID format");
            }

            var user = await _userManager.FindByIdAsync(objectId.ToString());
            if (user == null)
            {
                return Result<UserInfoResponse>.Failure("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var response = new UserInfoResponse
            {
                Id = user.Id.ToString(),
                Email = user.Email!,
                UserName = user.UserName!,
                FullName = user.FullName ?? string.Empty,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                LastLoginAt = user.LastLoginAt,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedOn
            };

            return Result<UserInfoResponse>.Success(response);
        }
    }
}
