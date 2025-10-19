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

        public GetPaginatedTasksQueryHandler(IRepository<Domain.Entities.Task, ObjectId> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedResult<TaskDto>>> Handle(GetPaginatedTasksQuery request, CancellationToken cancellationToken)
        {
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

            return Result<PaginatedResult<TaskDto>>.Success(paginatedResult);
        }
    }
}
