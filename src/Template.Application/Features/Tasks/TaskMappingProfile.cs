using AutoMapper;
using MongoDB.Bson;
using Template.Application.Features.Tasks.Commands;
using Template.Application.Features.Tasks.Queries;

namespace Template.Application.Features.Tasks
{
    public class TaskMappingProfile : Profile
    {
        public TaskMappingProfile()
        {
            // Command to Entity mappings
            CreateMap<CreateTaskCommand, Domain.Entities.Task>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => ObjectId.Parse(src.ProjectId)))
                .ForMember(dest => dest.AssigneeId, opt => opt.MapFrom(src => ParseOptionalObjectId(src.AssigneeId)));

            CreateMap<UpdateTaskCommand, Domain.Entities.Task>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => ObjectId.Parse(src.ProjectId)))
                .ForMember(dest => dest.AssigneeId, opt => opt.MapFrom(src => ParseOptionalObjectId(src.AssigneeId)));

            // Entity to DTO mappings
            CreateMap<Domain.Entities.Task, TaskDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId.ToString()))
                .ForMember(dest => dest.AssigneeId, opt => opt.MapFrom(src =>
                    src.AssigneeId.HasValue ? src.AssigneeId.Value.ToString() : null))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.LastModifiedAt));

            // Entity to Response mappings
            CreateMap<Domain.Entities.Task, TaskResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId.ToString()))
                .ForMember(dest => dest.AssigneeId, opt => opt.MapFrom(src =>
                    src.AssigneeId.HasValue ? src.AssigneeId.Value.ToString() : null));
        }

        private static ObjectId? ParseOptionalObjectId(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return ObjectId.TryParse(id, out var objectId) ? objectId : null;
        }
    }
}
