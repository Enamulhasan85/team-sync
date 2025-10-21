using Template.Application.Features.ChatMessages.Commands;

namespace Template.Application.Common.Interfaces
{
    public interface IChatNotificationService
    {
        Task SendMessageToProjectAsync(string projectId, ChatMessageResponse message);
        Task SendUpdatedMessageToProjectAsync(string projectId, ChatMessageResponse message);
        Task SendDeletedMessageToProjectAsync(string projectId, string messageId);
    }
}
