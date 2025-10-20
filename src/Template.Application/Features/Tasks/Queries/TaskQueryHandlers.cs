using AutoMapper;
using MediatR;
using MongoDB.Bson;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Models;

namespace Template.Application.Features.Tasks.Queries
{
    public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, Result<TaskDto>>
    {
        private readonly IRepository<Domain.Entities.Task, ObjectId> _repository;
        private readonly IMapper _mapper;

        public GetTaskByIdQueryHandler(IRepository<Domain.Entities.Task, ObjectId> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<TaskDto>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var taskId = ObjectId.Parse(request.Id);
            var task = await _repository.GetByIdAsync(taskId, cancellationToken);

            if (task == null)
                return Result<TaskDto>.Failure("Task not found");

            var dto = _mapper.Map<TaskDto>(task);
            return Result<TaskDto>.Success(dto);
        }
    }

    public class GetPaginatedTasksQueryHandler : IRequestHandler<GetPaginatedTasksQuery, Result<PaginatedResult<TaskDto>>>
    {
        private readonly IRepository<Domain.Entities.Task, ObjectId> _repository;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetPaginatedTasksQueryHandler(
            IRepository<Domain.Entities.Task, ObjectId> repository,
            IMapper mapper,
            ICacheService cacheService)
        {
            _repository = repository;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Result<PaginatedResult<TaskDto>>> Handle(GetPaginatedTasksQuery request, CancellationToken cancellationToken)
        {
            var version = await GetCacheVersionsAsync(request);
            var cacheKey = GenerateCacheKey(request, version);

            var cachedResult = await _cacheService.GetAsync<PaginatedResult<TaskDto>>(cacheKey);
            if (cachedResult != null)
            {
                return Result<PaginatedResult<TaskDto>>.Success(cachedResult);
            }

            // Build filter predicate
            var tasks = await _repository.FindAsync(predicate: task =>
                (string.IsNullOrEmpty(request.ProjectId) || task.ProjectId == ObjectId.Parse(request.ProjectId)) &&
                (!request.Status.HasValue || task.Status == request.Status.Value) &&
                (string.IsNullOrEmpty(request.AssigneeId) || task.AssigneeId == ObjectId.Parse(request.AssigneeId)),
                cancellationToken);

            var taskList = tasks.ToList();

            // Apply sorting
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                taskList = request.SortBy.ToLower() switch
                {
                    "title" => request.SortDescending ? taskList.OrderByDescending(t => t.Title).ToList() : taskList.OrderBy(t => t.Title).ToList(),
                    "status" => request.SortDescending ? taskList.OrderByDescending(t => t.Status).ToList() : taskList.OrderBy(t => t.Status).ToList(),
                    "duedate" => request.SortDescending ? taskList.OrderByDescending(t => t.DueDate).ToList() : taskList.OrderBy(t => t.DueDate).ToList(),
                    "createdat" => request.SortDescending ? taskList.OrderByDescending(t => t.CreatedAt).ToList() : taskList.OrderBy(t => t.CreatedAt).ToList(),
                    _ => taskList
                };
            }

            // Apply pagination
            var totalCount = taskList.Count;
            var pagedTasks = taskList
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<TaskDto>>(pagedTasks);
            var paginatedResult = new PaginatedResult<TaskDto>(dtos, totalCount, request.PageNumber, request.PageSize);

            await _cacheService.SetAsync(cacheKey, paginatedResult, TimeSpan.FromMinutes(5));

            return Result<PaginatedResult<TaskDto>>.Success(paginatedResult);
        }

        private async Task<Dictionary<string, long>> GetCacheVersionsAsync(GetPaginatedTasksQuery request)
        {
            var versions = new Dictionary<string, long>();

            if (!string.IsNullOrEmpty(request.ProjectId))
                versions["project"] = await GetVersionForKeyAsync($"tasks:v:project:{request.ProjectId}");

            if (request.Status.HasValue)
                versions["status"] = await GetVersionForKeyAsync($"tasks:v:status:{request.Status.Value}");

            if (!string.IsNullOrEmpty(request.AssigneeId))
                versions["assignee"] = await GetVersionForKeyAsync($"tasks:v:assignee:{request.AssigneeId}");

            if (versions.Count == 0)
                versions["global"] = await GetVersionForKeyAsync("tasks:v:global");

            return versions;
        }

        private async Task<long> GetVersionForKeyAsync(string versionKey)
        {
            var version = await _cacheService.GetAsync<long>(versionKey);
            if (version == 0)
            {
                version = 1;
                await _cacheService.SetAsync(versionKey, version, TimeSpan.FromDays(30));
            }
            return version;
        }

        private static string GenerateCacheKey(GetPaginatedTasksQuery request, Dictionary<string, long> versions)
        {
            var parts = new List<string> { "tasks" };

            if (versions.TryGetValue("project", out var pVersion))
                parts.Add($"pv{pVersion}");

            if (versions.TryGetValue("status", out var sVersion))
                parts.Add($"sv{sVersion}");

            if (versions.TryGetValue("assignee", out var aVersion))
                parts.Add($"av{aVersion}");

            if (versions.TryGetValue("global", out var gVersion))
                parts.Add($"gv{gVersion}");

            if (!string.IsNullOrEmpty(request.ProjectId))
                parts.Add($"p:{request.ProjectId}");

            if (request.Status.HasValue)
                parts.Add($"s:{request.Status.Value}");

            if (!string.IsNullOrEmpty(request.AssigneeId))
                parts.Add($"a:{request.AssigneeId}");

            if (!string.IsNullOrEmpty(request.SortBy))
                parts.Add($"sort:{request.SortBy}:{(request.SortDescending ? "d" : "a")}");

            parts.Add($"pg:{request.PageNumber}:{request.PageSize}");

            return string.Join(":", parts);
        }
    }
}
