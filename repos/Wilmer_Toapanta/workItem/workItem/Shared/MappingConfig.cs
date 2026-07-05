using AutoMapper;
using workItem.DTO;
using workItem.Models;

namespace workItem.Shared
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            // WorkItem entity <-> DTOs
            CreateMap<WorkItem, WorkItemResponseDTO>()
                .ForMember(dest => dest.WorkItemId, opt => opt.MapFrom(src => src.Id)); // resolved at service level

            CreateMap<WorkItemRequestDTO, WorkItem>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.WorkItemId))
                .ForMember(dest => dest.AssignedUserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());
        }
    }
}
