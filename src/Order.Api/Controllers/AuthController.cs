using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Order.Application.Auth.Commands.Register;
using Order.Application.Auth.Queries.Login;
using Order.Application.Common.Models;

namespace Order.Api.Controllers;

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