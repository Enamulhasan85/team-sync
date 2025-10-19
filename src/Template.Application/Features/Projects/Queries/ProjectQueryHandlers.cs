using AutoMapper;
using MediatR;
using MongoDB.Bson;
using Template.Application.Common.Interfaces;
using Template.Application.Common.Models;
using Template.Domain.Entities;

namespace Template.Application.Features.Projects.Queries
{
    public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
    {
        private readonly IRepository<Project, ObjectId> _repository;
        private readonly IMapper _mapper;

        public GetProjectByIdQueryHandler(IRepository<Project, ObjectId> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
        {
            if (!ObjectId.TryParse(request.Id, out var objectId))
            {
                return Result<ProjectDto>.Failure("Invalid project ID format");
            }

            var project = await _repository.GetByIdAsync(objectId, cancellationToken);
            if (project == null)
            {
                return Result<ProjectDto>.Failure("Project not found");
            }

            var dto = _mapper.Map<ProjectDto>(project);
            return Result<ProjectDto>.Success(dto);
        }
    }

    public class GetPaginatedProjectsQueryHandler : IRequestHandler<GetPaginatedProjectsQuery, Result<PaginatedResult<ProjectDto>>>
    {
        private readonly IRepository<Project, ObjectId> _repository;
        private readonly IMapper _mapper;

        public GetPaginatedProjectsQueryHandler(IRepository<Project, ObjectId> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedResult<ProjectDto>>> Handle(GetPaginatedProjectsQuery request, CancellationToken cancellationToken)
        {
            var paginatedResult = await _repository.GetPaginatedAsync(
                page: request.PageNumber,
                pageSize: request.PageSize,
                predicate: request.Status.HasValue ? p => p.Status == request.Status.Value : null,
                orderBy: request.SortBy switch
                {
                    "name" => p => p.Name,
                    "startDate" => p => (object)(p.StartDate ?? DateTime.MinValue),
                    "endDate" => p => (object)(p.EndDate ?? DateTime.MinValue),
                    "status" => p => p.Status,
                    _ => p => p.CreatedAt
                },
                orderByDescending: request.SortDescending,
                cancellationToken: cancellationToken
            );

            var projectDtos = _mapper.Map<List<ProjectDto>>(paginatedResult.Items);

            var result = new PaginatedResult<ProjectDto>(
                projectDtos,
                paginatedResult.TotalCount,
                paginatedResult.PageNumber,
                paginatedResult.PageSize
            );

            return Result<PaginatedResult<ProjectDto>>.Success(result);
        }
    }
}
