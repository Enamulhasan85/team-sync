using AutoMapper;
using MediatR;
using MongoDB.Bson;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Models;
using Template.Domain.Entities;

namespace Template.Application.Features.Projects.Commands
{
    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<ProjectResponse>>
    {
        private readonly IRepository<Project, ObjectId> _repository;
        private readonly IMapper _mapper;

        public CreateProjectCommandHandler(
            IRepository<Project, ObjectId> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProjectResponse>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            var project = _mapper.Map<Project>(request);

            var createdProject = await _repository.AddAsync(project, cancellationToken);

            var response = _mapper.Map<ProjectResponse>(createdProject);
            return Result<ProjectResponse>.Success(response);
        }
    }

    public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, Result<ProjectResponse>>
    {
        private readonly IRepository<Project, ObjectId> _repository;
        private readonly IMapper _mapper;

        public UpdateProjectCommandHandler(
            IRepository<Project, ObjectId> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProjectResponse>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            if (!ObjectId.TryParse(request.Id, out var objectId))
            {
                return Result<ProjectResponse>.Failure("Invalid project ID format");
            }

            var project = await _repository.GetByIdAsync(objectId, cancellationToken);
            if (project == null)
            {
                return Result<ProjectResponse>.Failure("Project not found");
            }

            _mapper.Map(request, project);

            await _repository.UpdateAsync(project, cancellationToken);

            var response = _mapper.Map<ProjectResponse>(project);
            return Result<ProjectResponse>.Success(response);
        }
    }

    public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Result<bool>>
    {
        private readonly IRepository<Project, ObjectId> _repository;

        public DeleteProjectCommandHandler(IRepository<Project, ObjectId> repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
        {
            if (!ObjectId.TryParse(request.Id, out var objectId))
            {
                return Result<bool>.Failure("Invalid project ID format");
            }

            var project = await _repository.GetByIdAsync(objectId, cancellationToken);
            if (project == null)
            {
                return Result<bool>.Failure("Project not found");
            }

            await _repository.DeleteAsync(objectId, cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
