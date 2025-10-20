using AutoMapper;
using MongoDB.Bson;
using Template.Application.Features.ChatMessages.Commands;
using Template.Domain.Entities;

namespace Template.Application.Features.ChatMessages
{
    public class ChatMessageMappingProfile : Profile
    {
        public ChatMessageMappingProfile()
        {
            // Command to Entity mappings
            CreateMap<CreateChatMessageCommand, ChatMessage>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.SenderId, opt => opt.Ignore())
                .ForMember(dest => dest.SenderName, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => ObjectId.Parse(src.ProjectId)));

            CreateMap<UpdateChatMessageCommand, ChatMessage>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.SenderId, opt => opt.Ignore())
                .ForMember(dest => dest.SenderName, opt => opt.Ignore());

            // Entity to DTO mappings
            CreateMap<ChatMessage, Queries.ChatMessageDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId.ToString()))
                .ForMember(dest => dest.SenderId, opt => opt.MapFrom(src => src.SenderId.ToString()));

            // Entity to Response mappings
            CreateMap<ChatMessage, ChatMessageResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId.ToString()))
                .ForMember(dest => dest.SenderId, opt => opt.MapFrom(src => src.SenderId.ToString()));
        }
    }
}
