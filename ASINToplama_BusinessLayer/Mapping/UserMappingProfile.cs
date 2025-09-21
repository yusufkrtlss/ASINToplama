using ASINToplama_EntityLayer.Concrete;
using ASINToplama_EntityLayer.Dtos;
using AutoMapper;

namespace ASINToplama_BusinessLayer.Mapping
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            // Entity -> DTO
            CreateMap<User, UserDto>();

            // Create DTO -> Entity (Password PLAIN -> PasswordHash alanına aktar)
            CreateMap<UserCreateRequest, User>()
                .ForMember(d => d.PasswordHash, o => o.MapFrom(s => s.Password))
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.CreatedAtUtc, o => o.Ignore());

            // Update DTO -> Entity (yalnızca gelen alanları güncelle)
            CreateMap<UserUpdateRequest, User>()
                .ForMember(d => d.PasswordHash, o => o.MapFrom(s => s.Password))
                .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
