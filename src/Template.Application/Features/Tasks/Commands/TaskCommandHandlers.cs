using AutoMapper;
using MediatR;
using MongoDB.Bson;
using Template.Application.Common.Events;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Models;
using Template.Domain.Enums;

namespace Template.Application.Features.Tasks.Commands
{
    public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<TaskResponse>>
    {
        private readonly IRepository<Domain.Entities.Task, ObjectId> _repository;
        private readonly IMapper _mapper;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheService _cacheService;

        public CreateTaskCommandHandler(
            IRepository<Domain.Entities.Task, ObjectId> repository,
            IMapper mapper,
            IEventPublisher eventPublisher,
            ICacheService cacheService)
        {
            _repository = repository;
            _mapper = mapper;
            _eventPublisher = eventPublisher;
            _cacheService = cacheService;
        }

        public async Task<Result<TaskResponse>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = _mapper.Map<Domain.Entities.Task>(request);
            var createdTask = await _repository.AddAsync(task, cancellationToken);
            var response = _mapper.Map<TaskResponse>(createdTask);

            await IncrementCacheVersions(
                createdTask.ProjectId.ToString(),
                createdTask.Status,
                createdTask.AssigneeId?.ToString());

            // Publish TaskCreatedEvent to RabbitMQ
            var taskCreatedEvent = new TaskCreatedEvent
            {
                TaskId = createdTask.Id.ToString(),
                ProjectId = createdTask.ProjectId.ToString(),
                Title = createdTask.Title,
                Description = createdTask.Description,
                Status = createdTask.Status,
                AssigneeId = createdTask.AssigneeId?.ToString(),
                DueDate = createdTask.DueDate,
                CreatedBy = createdTask.CreatedBy
            };

            await _eventPublisher.PublishAsync("task.created", taskCreatedEvent);

            return Result<TaskResponse>.Success(response);
        }

        private async Task IncrementCacheVersions(string projectId, TaskWorkflowStatus status, string? assigneeId)
        {
            var tasks = new List<System.Threading.Tasks.Task>
            {
                _cacheService.IncrementAsync($"tasks:v:project:{projectId}", 1, TimeSpan.FromDays(30)),
                _cacheService.IncrementAsync($"tasks:v:status:{status}", 1, TimeSpan.FromDays(30)),
                _cacheService.IncrementAsync("tasks:v:global", 1, TimeSpan.FromDays(30))
            };

            if (!string.IsNullOrEmpty(assigneeId))
            {
                tasks.Add(_cacheService.IncrementAsync($"tasks:v:assignee:{assigneeId}", 1, TimeSpan.FromDays(30)));
            }

            await System.Threading.Tasks.Task.WhenAll(tasks);
        }
    }

    public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, Result<TaskResponse>>
    {
        private readonly IRepository<Domain.Entities.Task, ObjectId> _repository;
        private readonly IMapper _mapper;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheService _cacheService;

        public UpdateTaskCommandHandler(
            IRepository<Domain.Entities.Task, ObjectId> repository,
            IMapper mapper,
            IEventPublisher eventPublisher,
            ICacheService cacheService)
        {
            _repository = repository;
            _mapper = mapper;
            _eventPublisher = eventPublisher;
            _cacheService = cacheService;
        }

        public async Task<Result<TaskResponse>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
        {
            var taskId = ObjectId.Parse(request.Id);
            var existingTask = await _repository.GetByIdAsync(taskId, cancellationToken);

            if (existingTask == null)
                return Result<TaskResponse>.Failure("Task not found");

            var oldStatus = existingTask.Status;
            var oldAssigneeId = existingTask.AssigneeId?.ToString();

            _mapper.Map(request, existingTask);
            await _repository.UpdateAsync(existingTask, cancellationToken);
            var response = _mapper.Map<TaskResponse>(existingTask);

            await IncrementCacheVersionsForUpdate(
                existingTask.ProjectId.ToString(),
                oldStatus,
                existingTask.Status,
                oldAssigneeId,
                existingTask.AssigneeId?.ToString());

            // Publish TaskUpdatedEvent to RabbitMQ
            var taskUpdatedEvent = new TaskUpdatedEvent
            {
                TaskId = existingTask.Id.ToString(),
                ProjectId = existingTask.ProjectId.ToString(),
                Title = existingTask.Title,
                Description = existingTask.Description,
                Status = existingTask.Status,
                AssigneeId = existingTask.AssigneeId?.ToString(),
                DueDate = existingTask.DueDate,
                LastModifiedBy = existingTask.LastModifiedBy,
                ChangedFields = new List<string> { "Title", "Description", "Status", "AssigneeId", "DueDate" }
            };

            await _eventPublisher.PublishAsync("task.updated", taskUpdatedEvent);

            return Result<TaskResponse>.Success(response);
        }

        private async Task IncrementCacheVersionsForUpdate(
            string projectId,
            TaskWorkflowStatus oldStatus,
            TaskWorkflowStatus newStatus,
            string? oldAssigneeId,
            string? newAssigneeId)
        {
            var tasks = new List<System.Threading.Tasks.Task>
            {
                _cacheService.IncrementAsync($"tasks:v:project:{projectId}", 1, TimeSpan.FromDays(30)),
                _cacheService.IncrementAsync("tasks:v:global", 1, TimeSpan.FromDays(30))
            };

            tasks.Add(_cacheService.IncrementAsync($"tasks:v:status:{oldStatus}", 1, TimeSpan.FromDays(30)));
            if (oldStatus != newStatus)
            {
                tasks.Add(_cacheService.IncrementAsync($"tasks:v:status:{newStatus}", 1, TimeSpan.FromDays(30)));
            }

            if (!string.IsNullOrEmpty(oldAssigneeId))
            {
                tasks.Add(_cacheService.IncrementAsync($"tasks:v:assignee:{oldAssigneeId}", 1, TimeSpan.FromDays(30)));
            }
            if (!string.IsNullOrEmpty(newAssigneeId) && oldAssigneeId != newAssigneeId)
            {
                tasks.Add(_cacheService.IncrementAsync($"tasks:v:assignee:{newAssigneeId}", 1, TimeSpan.FromDays(30)));
            }

            await System.Threading.Tasks.Task.WhenAll(tasks);
        }
    }

    public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Result<bool>>
    {
        private readonly IRepository<Domain.Entities.Task, ObjectId> _repository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICacheService _cacheService;

        public DeleteTaskCommandHandler(
            IRepository<Domain.Entities.Task, ObjectId> repository,
            IEventPublisher eventPublisher,
            ICurrentUserService currentUserService,
            ICacheService cacheService)
        {
            _repository = repository;
            _eventPublisher = eventPublisher;
            _currentUserService = currentUserService;
            _cacheService = cacheService;
        }

        public async Task<Result<bool>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            var taskId = ObjectId.Parse(request.Id);
            var existingTask = await _repository.GetByIdAsync(taskId, cancellationToken);

            if (existingTask == null)
                return Result<bool>.Failure("Task not found");

            var projectId = existingTask.ProjectId.ToString();
            var status = existingTask.Status;
            var assigneeId = existingTask.AssigneeId?.ToString();

            await _repository.DeleteAsync(taskId, cancellationToken);

            await IncrementCacheVersions(projectId, status, assigneeId);

            // Publish TaskDeletedEvent to RabbitMQ
            var taskDeletedEvent = new TaskDeletedEvent
            {
                TaskId = existingTask.Id.ToString(),
                ProjectId = existingTask.ProjectId.ToString(),
                Title = existingTask.Title,
                DeletedBy = _currentUserService.UserId
            };

            await _eventPublisher.PublishAsync("task.deleted", taskDeletedEvent);

            return Result<bool>.Success(true);
        }

        private async Task IncrementCacheVersions(string projectId, TaskWorkflowStatus status, string? assigneeId)
        {
            var tasks = new List<System.Threading.Tasks.Task>
            {
                _cacheService.IncrementAsync($"tasks:v:project:{projectId}", 1, TimeSpan.FromDays(30)),
                _cacheService.IncrementAsync($"tasks:v:status:{status}", 1, TimeSpan.FromDays(30)),
                _cacheService.IncrementAsync("tasks:v:global", 1, TimeSpan.FromDays(30))
            };

            if (!string.IsNullOrEmpty(assigneeId))
            {
                tasks.Add(_cacheService.IncrementAsync($"tasks:v:assignee:{assigneeId}", 1, TimeSpan.FromDays(30)));
            }

            await System.Threading.Tasks.Task.WhenAll(tasks);
        }
    }
}
