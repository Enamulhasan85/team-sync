using AutoMapper;
using MediatR;
using MongoDB.Bson;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Models;
using Template.Application.Features.Tasks.Commands;

namespace Template.Application.Features.Tasks.Commands
{
    public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<TaskResponse>>
    {
        private readonly IRepository<Domain.Entities.Task, ObjectId> _repository;
        private readonly IMapper _mapper;

        public CreateTaskCommandHandler(IRepository<Domain.Entities.Task, ObjectId> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<TaskResponse>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = _mapper.Map<Domain.Entities.Task>(request);
            var createdTask = await _repository.AddAsync(task, cancellationToken);
            var response = _mapper.Map<TaskResponse>(createdTask);
            return Result<TaskResponse>.Success(response);
        }
    }

    public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, Result<TaskResponse>>
    {
        private readonly IRepository<Domain.Entities.Task, ObjectId> _repository;
        private readonly IMapper _mapper;

        public UpdateTaskCommandHandler(IRepository<Domain.Entities.Task, ObjectId> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
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
            return Result<TaskResponse>.Success(response);
        }
    }

    public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Result<bool>>
    {
        private readonly IRepository<Domain.Entities.Task, ObjectId> _repository;

        public DeleteTaskCommandHandler(IRepository<Domain.Entities.Task, ObjectId> repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            var taskId = ObjectId.Parse(request.Id);
            await _repository.DeleteAsync(taskId, cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
