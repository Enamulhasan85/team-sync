using AutoMapper;
using MediatR;
using MongoDB.Bson;
using Template.Application.Common.Events;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Models;

namespace Template.Application.Features.Tasks.Commands
{
    public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<TaskResponse>>
    {
        private readonly IRepository<Domain.Entities.Task, ObjectId> _repository;
        private readonly IMapper _mapper;
        private readonly IEventPublisher _eventPublisher;

        public CreateTaskCommandHandler(
            IRepository<Domain.Entities.Task, ObjectId> repository,
            IMapper mapper,
            IEventPublisher eventPublisher)
        {
            _repository = repository;
            _mapper = mapper;
            _eventPublisher = eventPublisher;
        }

        public async Task<Result<TaskResponse>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = _mapper.Map<Domain.Entities.Task>(request);
            var createdTask = await _repository.AddAsync(task, cancellationToken);
            var response = _mapper.Map<TaskResponse>(createdTask);

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
    }

    public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, Result<TaskResponse>>
    {
        private readonly IRepository<Domain.Entities.Task, ObjectId> _repository;
        private readonly IMapper _mapper;
        private readonly IEventPublisher _eventPublisher;

        public UpdateTaskCommandHandler(
            IRepository<Domain.Entities.Task, ObjectId> repository,
            IMapper mapper,
            IEventPublisher eventPublisher)
        {
            _repository = repository;
            _mapper = mapper;
            _eventPublisher = eventPublisher;
        }

        public async Task<Result<TaskResponse>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
        {
            var taskId = ObjectId.Parse(request.Id);
            var existingTask = await _repository.GetByIdAsync(taskId, cancellationToken);

            if (existingTask == null)
                return Result<TaskResponse>.Failure("Task not found");

            _mapper.Map(request, existingTask);
            await _repository.UpdateAsync(existingTask, cancellationToken);
            var response = _mapper.Map<TaskResponse>(existingTask);

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
    }

    public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Result<bool>>
    {
        private readonly IRepository<Domain.Entities.Task, ObjectId> _repository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICurrentUserService _currentUserService;

        public DeleteTaskCommandHandler(
            IRepository<Domain.Entities.Task, ObjectId> repository,
            IEventPublisher eventPublisher,
            ICurrentUserService currentUserService)
        {
            _repository = repository;
            _eventPublisher = eventPublisher;
            _currentUserService = currentUserService;
        }

        public async Task<Result<bool>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            var taskId = ObjectId.Parse(request.Id);
            var existingTask = await _repository.GetByIdAsync(taskId, cancellationToken);

            if (existingTask == null)
                return Result<bool>.Failure("Task not found");

            await _repository.DeleteAsync(taskId, cancellationToken);

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
    }
}
