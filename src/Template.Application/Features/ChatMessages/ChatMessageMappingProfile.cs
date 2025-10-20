using AutoMapper;
using Template.Domain.Entities;

namespace Template.Application.Features.ChatMessages
{
    public class ChatMessageMappingProfile : Profile
    {
        public ChatMessageMappingProfile()
        {
            CreateMap<ChatMessage, Queries.ChatMessageDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId.ToString()))
                .ForMember(dest => dest.SenderId, opt => opt.MapFrom(src => src.SenderId.ToString()));
        }
    }
}
