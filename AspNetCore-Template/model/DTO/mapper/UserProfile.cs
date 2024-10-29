using AspNetCore_Template.model.DTO.request.user;
using AspNetCore_Template.model.DTO.response.user;
using AspNetCore_Template.model.entity;
using AutoMapper;

namespace AspNetCore_Template.model.DTO.mapper;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserRequest, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(a => a.Email))
            .ForMember(dest => dest.Timezone, opt => opt.MapFrom(a => TimeZoneInfo.FindSystemTimeZoneById(a.Timezone)));
        CreateMap<RegistrationRequest, User>();

        CreateMap<User, TwoFactorAuthResponse>();
        CreateMap<User, LoginResponse>();
        CreateMap<User, UserResponse>();
        CreateMap<User, EditedUserResponse>();
    }
}