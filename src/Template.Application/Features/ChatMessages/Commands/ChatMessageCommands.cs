using MediatR;
using MongoDB.Bson;
using Template.Application.Common.Models;
using Template.Domain.Enums;

namespace Template.Application.Features.ChatMessages.Commands
{
    public class CreateChatMessageCommand : IRequest<Result<ChatMessageResponse>>
    {
        public required string ProjectId { get; set; }
        public required string Message { get; set; }
    }

    public class UpdateChatMessageCommand : IRequest<Result<ChatMessageResponse>>
    {
        public required string Id { get; set; }
        public required string Message { get; set; }
    }

    public class DeleteChatMessageCommand : IRequest<Result<bool>>
    {
        public required string Id { get; set; }
    }

    public class ChatMessageResponse
    {
        public string Id { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
