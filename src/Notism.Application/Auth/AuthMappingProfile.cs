using AutoMapper;
using Notism.Application.Auth.Login;
using Notism.Application.Auth.Register;
using Notism.Domain.User;

namespace Notism.Application.Auth;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<User, LoginResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Token, opt => opt.Ignore()) // Set manually in handler
            .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore()); // Set manually in handler

        CreateMap<User, RegisterResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Token, opt => opt.Ignore()) // Set manually in handler
            .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore()); // Set manually in handler
    }
}