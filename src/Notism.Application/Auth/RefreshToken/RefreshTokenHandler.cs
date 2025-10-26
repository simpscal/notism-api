using MediatR;

using Notism.Application.Common.Interfaces;
using Notism.Shared.Exceptions;
using Notism.Shared.Models;

namespace Notism.Application.Auth.RefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenRequest, Result<TokenResult>>
{
    private readonly ITokenService _tokenService;

    public RefreshTokenHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task<Result<TokenResult>> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var tokenResult = await _tokenService.RefreshTokenAsync(request.RefreshToken);

            return Result<TokenResult>.Success(tokenResult);
        }
        catch (InvalidRefreshTokenException ex)
        {
            throw new ResultFailureException(ex.Message);
        }
    }
}