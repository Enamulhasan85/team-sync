using MediatR;
using Template.Application.Common.Models;

namespace Template.Application.Features.ChatMessages.Queries
{
    public class GetPaginatedChatMessagesQuery : IRequest<Result<PaginatedResult<ChatMessageDto>>>
    {
        public required string ProjectId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = true;
    }

    public class ChatMessageDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
