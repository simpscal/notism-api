using MediatR;

using Notism.Application.Common.Interfaces;
using Notism.Shared.Exceptions;

namespace Notism.Application.Auth.RefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenRequest, TokenResult>
{
    private readonly ITokenService _tokenService;

    public RefreshTokenHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task<TokenResult> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var tokenResult = await _tokenService.RefreshTokenAsync(request.RefreshToken);

            return tokenResult;
        }
        catch (InvalidRefreshTokenException ex)
        {
            throw new ResultFailureException(ex.Message);
        }
    }
}