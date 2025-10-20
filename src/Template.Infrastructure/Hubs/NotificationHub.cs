using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Template.Application.Common.Interfaces;
using Template.Domain.Entities;
using SystemTask = System.Threading.Tasks.Task;
using TaskEntity = Template.Domain.Entities.Task;

namespace Template.Infrastructure.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications
/// Clients can connect to this hub to receive task updates in real-time
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;
    private readonly IRepository<Project, ObjectId> _projectRepository;
    private readonly ICurrentUserService _currentUserService;

    public NotificationHub(
        ILogger<NotificationHub> logger,
        IRepository<Project, ObjectId> projectRepository,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
    }


    public override async SystemTask OnConnectedAsync()
    {
        var userId = _currentUserService.UserId;
        _logger.LogInformation("Client connected: {ConnectionId}, User: {UserId}",
            Context.ConnectionId, userId);

        if (!string.IsNullOrEmpty(userId) && ObjectId.TryParse(userId, out var userObjectId))
        {
            var projects = await _projectRepository.FindAsync(p => p.MemberIds.Contains(userObjectId));
            var projectList = projects.ToList();
            _logger.LogInformation("User {UserId} is a member of {ProjectCount} projects",
                userId, projectList.Count);
            foreach (var project in projectList)
            {
                var groupName = $"project_{project.Id}";
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                _logger.LogInformation("Client {ConnectionId} added to group {GroupName}",
                    Context.ConnectionId, groupName);
            }

            _logger.LogInformation("Client {ConnectionId} added to {GroupCount} project groups",
                Context.ConnectionId, projectList.Count);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub
    /// </summary>
    public override async SystemTask OnDisconnectedAsync(Exception? exception)
    {
        var userId = _currentUserService.UserId;
        _logger.LogInformation("Client disconnected: {ConnectionId}, User: {UserId}",
            Context.ConnectionId, userId);

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to project notifications
    /// Clients join a group named "project_{projectId}" to receive updates for that project
    /// </summary>
    public async SystemTask JoinProjectGroup(string projectId)
    {
        var groupName = $"project_{projectId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        _logger.LogInformation("Client {ConnectionId} joined group {GroupName}",
            Context.ConnectionId, groupName);

        await Clients.Caller.SendAsync("JoinedGroup", groupName);
    }

    /// <summary>
    /// Unsubscribe from project notifications
    /// </summary>
    public async SystemTask LeaveProjectGroup(string projectId)
    {
        var groupName = $"project_{projectId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        _logger.LogInformation("Client {ConnectionId} left group {GroupName}",
            Context.ConnectionId, groupName);

        await Clients.Caller.SendAsync("LeftGroup", groupName);
    }
}
