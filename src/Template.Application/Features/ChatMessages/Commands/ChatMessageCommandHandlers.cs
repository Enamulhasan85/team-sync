using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Models;
using Template.Domain.Identity;

namespace Template.Application.Features.ChatMessages.Commands
{
    public class CreateChatMessageCommandHandler : IRequestHandler<CreateChatMessageCommand, Result<ChatMessageResponse>>
    {
        private readonly IRepository<Domain.Entities.ChatMessage, ObjectId> _repository;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ICurrentUserService _currentUserService;

        public CreateChatMessageCommandHandler(
            IRepository<Domain.Entities.ChatMessage, ObjectId> repository,
            IMapper mapper,
            ICacheService cacheService,
            ICurrentUserService currentUserService)
        {
            _repository = repository;
            _mapper = mapper;
            _cacheService = cacheService;
            _currentUserService = currentUserService;
        }

        public async Task<Result<ChatMessageResponse>> Handle(CreateChatMessageCommand request, CancellationToken cancellationToken)
        {
            var chatMessage = _mapper.Map<Domain.Entities.ChatMessage>(request);

            chatMessage.SenderId = ObjectId.Parse(_currentUserService.UserId);
            chatMessage.SenderName = _currentUserService.FullName ?? _currentUserService.UserName!;

            var createdMessage = await _repository.AddAsync(chatMessage, cancellationToken);
            var response = _mapper.Map<ChatMessageResponse>(createdMessage);

            await InvalidateProjectCache(createdMessage.ProjectId.ToString());

            return Result<ChatMessageResponse>.Success(response);
        }

        private async Task InvalidateProjectCache(string projectId)
        {
            await _cacheService.IncrementAsync($"chatmessages:v:project:{projectId}", 1);
        }
    }

    public class UpdateChatMessageCommandHandler : IRequestHandler<UpdateChatMessageCommand, Result<ChatMessageResponse>>
    {
        private readonly IRepository<Domain.Entities.ChatMessage, ObjectId> _repository;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ICurrentUserService _currentUserService;

        public UpdateChatMessageCommandHandler(
            IRepository<Domain.Entities.ChatMessage, ObjectId> repository,
            IMapper mapper,
            ICacheService cacheService,
            ICurrentUserService currentUserService)
        {
            _repository = repository;
            _mapper = mapper;
            _cacheService = cacheService;
            _currentUserService = currentUserService;
        }

        public async Task<Result<ChatMessageResponse>> Handle(UpdateChatMessageCommand request, CancellationToken cancellationToken)
        {
            var messageId = ObjectId.Parse(request.Id);
            var existingMessage = await _repository.GetByIdAsync(messageId, cancellationToken);

            if (existingMessage == null)
                return Result<ChatMessageResponse>.Failure("Chat message not found");

            var currentUserId = _currentUserService.UserId;
            if (existingMessage.SenderId.ToString() != currentUserId)
                return Result<ChatMessageResponse>.Failure("You can only edit your own messages");

            _mapper.Map(request, existingMessage);
            await _repository.UpdateAsync(existingMessage, cancellationToken);
            var response = _mapper.Map<ChatMessageResponse>(existingMessage);

            await InvalidateProjectCache(existingMessage.ProjectId.ToString());

            return Result<ChatMessageResponse>.Success(response);
        }

        private async Task InvalidateProjectCache(string projectId)
        {
            await _cacheService.IncrementAsync($"chatmessages:v:project:{projectId}", 1);
        }
    }

    public class DeleteChatMessageCommandHandler : IRequestHandler<DeleteChatMessageCommand, Result<bool>>
    {
        private readonly IRepository<Domain.Entities.ChatMessage, ObjectId> _repository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICacheService _cacheService;

        public DeleteChatMessageCommandHandler(
            IRepository<Domain.Entities.ChatMessage, ObjectId> repository,
            ICurrentUserService currentUserService,
            ICacheService cacheService)
        {
            _repository = repository;
            _currentUserService = currentUserService;
            _cacheService = cacheService;
        }

        public async Task<Result<bool>> Handle(DeleteChatMessageCommand request, CancellationToken cancellationToken)
        {
            var messageId = ObjectId.Parse(request.Id);
            var existingMessage = await _repository.GetByIdAsync(messageId, cancellationToken);

            if (existingMessage == null)
                return Result<bool>.Failure("Chat message not found");

            var currentUserId = _currentUserService.UserId;
            if (existingMessage.SenderId.ToString() != currentUserId)
                return Result<bool>.Failure("You can only delete your own messages");

            var projectId = existingMessage.ProjectId.ToString();

            await _repository.DeleteAsync(messageId, cancellationToken);

            await InvalidateProjectCache(projectId);

            return Result<bool>.Success(true);
        }

        private async Task InvalidateProjectCache(string projectId)
        {
            await _cacheService.IncrementAsync($"chatmessages:v:project:{projectId}", 1);
        }
    }
}
