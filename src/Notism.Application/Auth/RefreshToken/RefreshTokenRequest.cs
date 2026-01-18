using System.ComponentModel.DataAnnotations;

using MediatR;

using Notism.Application.Common.Interfaces;

namespace Notism.Application.Auth.RefreshToken;

public class RefreshTokenRequest : IRequest<TokenResult>
{
    public string RefreshToken { get; set; } = string.Empty;
}