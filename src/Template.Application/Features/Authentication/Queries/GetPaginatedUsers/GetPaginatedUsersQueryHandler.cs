using MediatR;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Models;
using Template.Domain.Identity;

namespace Template.Application.Features.Authentication.Queries.GetPaginatedUsers
{
    public class GetPaginatedUsersQueryHandler : IRequestHandler<GetPaginatedUsersQuery, Result<PaginatedResult<UserDto>>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetPaginatedUsersQueryHandler(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<PaginatedResult<UserDto>>> Handle(GetPaginatedUsersQuery request, CancellationToken cancellationToken)
        {

            // Get all users
            var usersQuery = _userManager.Users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                usersQuery = usersQuery.Where(u =>
                    u.Email!.ToLower().Contains(searchLower) ||
                    u.UserName!.ToLower().Contains(searchLower) ||
                    (u.FullName != null && u.FullName.ToLower().Contains(searchLower)));
            }

            if (request.IsActive.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.IsActive == request.IsActive.Value);
            }

            // Get total count before pagination
            var totalCount = usersQuery.Count();

            // Apply sorting
            usersQuery = request.SortBy?.ToLower() switch
            {
                "email" => request.SortDescending
                    ? usersQuery.OrderByDescending(u => u.Email)
                    : usersQuery.OrderBy(u => u.Email),
                "username" => request.SortDescending
                    ? usersQuery.OrderByDescending(u => u.UserName)
                    : usersQuery.OrderBy(u => u.UserName),
                "fullname" => request.SortDescending
                    ? usersQuery.OrderByDescending(u => u.FullName)
                    : usersQuery.OrderBy(u => u.FullName),
                "lastlogin" => request.SortDescending
                    ? usersQuery.OrderByDescending(u => u.LastLoginAt)
                    : usersQuery.OrderBy(u => u.LastLoginAt),
                "createdat" => request.SortDescending
                    ? usersQuery.OrderByDescending(u => u.CreatedOn)
                    : usersQuery.OrderBy(u => u.CreatedOn),
                _ => request.SortDescending
                    ? usersQuery.OrderByDescending(u => u.CreatedOn)
                    : usersQuery.OrderBy(u => u.CreatedOn)
            };

            // Apply pagination
            var users = usersQuery
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Map to DTOs and get roles
            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                // Apply role filter if specified
                if (!string.IsNullOrEmpty(request.Role) && !roles.Contains(request.Role))
                {
                    continue;
                }

                userDtos.Add(new UserDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    FullName = user.FullName,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    LastLoginAt = user.LastLoginAt,
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedOn
                });
            }

            // If role filter was applied, adjust total count
            if (!string.IsNullOrEmpty(request.Role))
            {
                totalCount = userDtos.Count;
            }

            var result = new PaginatedResult<UserDto>(
                userDtos,
                totalCount,
                request.PageNumber,
                request.PageSize
            );

            return Result<PaginatedResult<UserDto>>.Success(result);
        }

    }
}
