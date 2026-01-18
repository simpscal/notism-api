using AutoMapper;

using Notism.Application.Auth.Login;
using Notism.Application.Auth.Register;
using Notism.Application.Common.Models;
using Notism.Domain.User;

namespace Notism.Application.Auth;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<Domain.User.User, AuthenticationResponse>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => new AuthenticationUserInfoResponse
            {
                Id = src.Id,
                Email = src.Email,
                FirstName = src.FirstName,
                LastName = src.LastName,
                AvatarUrl = src.AvatarUrl,
            }))
            .ForMember(dest => dest.Token, opt => opt.Ignore()) // Set manually in handler
            .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore()); // Set manually in handler

        CreateMap<Domain.User.User, RegisterResponse>()
             .ForMember(dest => dest.User, opt => opt.MapFrom(src => new RegisterUserInfoResponse
             {
                 Id = src.Id,
                 Email = src.Email,
                 FirstName = src.FirstName,
                 LastName = src.LastName,
                 AvatarUrl = src.AvatarUrl,
             }))
            .ForMember(dest => dest.Token, opt => opt.Ignore()) // Set manually in handler
            .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore()); // Set manually in handler
    }
}