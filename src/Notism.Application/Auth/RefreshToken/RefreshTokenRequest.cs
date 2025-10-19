using System.ComponentModel.DataAnnotations;

using MediatR;

using Notism.Application.Common.Interfaces;

using Notism.Shared.Models;

namespace Notism.Application.Auth.RefreshToken;

public class RefreshTokenRequest : IRequest<Result<TokenResult>>
{
    public string RefreshToken { get; set; } = string.Empty;
}