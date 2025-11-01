using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Template.Application.Common.Interfaces;
using Template.Application.Features.ChatMessages.Commands;
using Template.Infrastructure.Hubs;

namespace Template.Infrastructure.Services
{
    public class SignalRChatNotificationService : IChatNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<SignalRChatNotificationService> _logger;

        public SignalRChatNotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<SignalRChatNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendMessageToProjectAsync(string projectId, string title, ChatMessageResponse message)
        {
            try
            {
                var groupName = $"project_{projectId}";
                await _hubContext.Clients.Group(groupName).SendAsync(title, message);

                _logger.LogInformation(
                    "Chat message {MessageId} sent to project group {GroupName}",
                    message.Id, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending chat message notification to project {ProjectId}", projectId);
            }
        }

        public async Task SendUpdatedMessageToProjectAsync(string projectId, ChatMessageResponse message)
        {
            try
            {
                var groupName = $"project_{projectId}";

                await _hubContext.Clients.Group(groupName)
                    .SendAsync("MessageUpdated", message);

                _logger.LogInformation(
                    "Chat message {MessageId} update sent to project group {GroupName}",
                    message.Id, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending chat message update notification to project {ProjectId}", projectId);
            }
        }

        public async Task SendDeletedMessageToProjectAsync(string projectId, string messageId)
        {
            try
            {
                var groupName = $"project_{projectId}";

                await _hubContext.Clients.Group(groupName)
                    .SendAsync("MessageDeleted", new { messageId, projectId });

                _logger.LogInformation(
                    "Chat message {MessageId} deletion sent to project group {GroupName}",
                    messageId, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending chat message deletion notification to project {ProjectId}", projectId);
            }
        }
    }
}
