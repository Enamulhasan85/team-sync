using AutoMapper;
using MongoDB.Bson;
using Template.Application.Features.Projects.Commands;
using Template.Application.Features.Projects.Queries;
using Template.Domain.Entities;

namespace Template.Application.Features.Projects
{
    public class ProjectMappingProfile : Profile
    {
        public ProjectMappingProfile()
        {
            // Command to Entity mappings
            CreateMap<CreateProjectCommand, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.MemberIds, opt => opt.MapFrom(src => ParseObjectIds(src.MemberIds)));

            CreateMap<UpdateProjectCommand, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.MemberIds, opt => opt.MapFrom(src => ParseObjectIds(src.MemberIds)));

            // Entity to DTO mappings
            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.MemberIds, opt => opt.MapFrom(src =>
                    src.MemberIds.Select(id => id.ToString()).ToList()
                ));

            // Entity to Response mappings
            CreateMap<Project, ProjectResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.MemberIds, opt => opt.MapFrom(src => src.MemberIds.Select(id => id.ToString()).ToList()));
        }

        private static List<ObjectId> ParseObjectIds(List<string>? memberIds)
        {
            if (memberIds == null || !memberIds.Any())
                return new List<ObjectId>();

            return memberIds
                .Where(id => !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _))
                .Select(ObjectId.Parse)
                .ToList();
        }
    }
}
