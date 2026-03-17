using AutoMapper;

using Notism.Application.Auth.Models;
using Notism.Domain.User;
using Notism.Shared.Extensions;

namespace Notism.Application.Auth.Mappers;

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
                Role = src.Role.ToCamelCase(),
            }))
            .ForMember(dest => dest.Token, opt => opt.Ignore()) // Set manually in handler
            .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore()); // Set manually in handler
    }
}