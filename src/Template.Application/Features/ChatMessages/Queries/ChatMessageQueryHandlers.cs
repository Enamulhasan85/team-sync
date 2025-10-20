using AutoMapper;
using MediatR;
using MongoDB.Bson;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Models;
using Template.Domain.Entities;

namespace Template.Application.Features.ChatMessages.Queries
{
    public class GetPaginatedChatMessagesQueryHandler : IRequestHandler<GetPaginatedChatMessagesQuery, Result<PaginatedResult<ChatMessageDto>>>
    {
        private readonly IRepository<ChatMessage, ObjectId> _repository;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetPaginatedChatMessagesQueryHandler(
            IRepository<ChatMessage, ObjectId> repository,
            IMapper mapper,
            ICacheService cacheService)
        {
            _repository = repository;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Result<PaginatedResult<ChatMessageDto>>> Handle(
            GetPaginatedChatMessagesQuery request,
            CancellationToken cancellationToken)
        {

            ObjectId.TryParse(request.ProjectId, out var projectObjectId);

            if (request.PageNumber == 1 && request.PageSize == 20)
            {
                var cacheKey = $"chatmessages:project:{request.ProjectId}:latest20";
                var cachedMessages = await _cacheService.GetAsync<PaginatedResult<ChatMessageDto>>(cacheKey);

                if (cachedMessages != null)
                {
                    return Result<PaginatedResult<ChatMessageDto>>.Success(cachedMessages);
                }
            }

            var messages = await _repository.GetPaginatedAsync(
                page: request.PageNumber,
                pageSize: request.PageSize,
                predicate: msg => msg.ProjectId == projectObjectId,
                orderBy: m => m.CreatedAt,
                orderByDescending: request.SortDescending
            );

            var dtos = _mapper.Map<List<ChatMessageDto>>(messages.Items);
            var result = new PaginatedResult<ChatMessageDto>(dtos, messages.TotalCount, messages.PageNumber, messages.PageSize);

            if (request.PageNumber == 1 && request.PageSize == 20)
            {
                var cacheKey = $"chatmessages:project:{request.ProjectId}:latest20";
                await _cacheService.SetAsync(cacheKey, result);
            }

            return Result<PaginatedResult<ChatMessageDto>>.Success(result);
        }
    }
}
