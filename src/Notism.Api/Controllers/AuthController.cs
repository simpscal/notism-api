using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Notism.Application.Auth.Commands.Register;
using Notism.Application.Auth.Queries.Login;
using Notism.Application.Common.Models;

namespace Notism.Api.Controllers;

[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public Task<JwtToken> Login([FromBody] LoginQuery request)
    {
        return mediator.Send(request);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public Task<JwtToken> Register([FromBody] RegisterCommand request)
    {
        return mediator.Send(request);
    }
}