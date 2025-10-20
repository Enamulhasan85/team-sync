namespace Template.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    string? UserEmail { get; }
    string? FullName { get; }
    bool IsAuthenticated { get; }
}
