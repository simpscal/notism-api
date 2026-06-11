using MediatR;

using Notism.Application.Common.Services;

namespace Notism.Application.Auth.GoogleOAuth;

public class GoogleOAuthRedirectHandler : IRequestHandler<GoogleOAuthRedirectRequest, GoogleOAuthRedirectResponse>
{
    private readonly IGoogleOAuthService _googleOAuthService;

    public GoogleOAuthRedirectHandler(IGoogleOAuthService googleOAuthService)
    {
        _googleOAuthService = googleOAuthService;
    }

    public Task<GoogleOAuthRedirectResponse> Handle(GoogleOAuthRedirectRequest request, CancellationToken cancellationToken)
    {
        var result = _googleOAuthService.GetRedirectUrl();

        var response = GoogleOAuthRedirectResponse.FromDomain(result);

        return Task.FromResult(response);
    }
}