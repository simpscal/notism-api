using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Order.Api.Controllers;

[ApiController]
[Authorize]
public class ApiController : ControllerBase
{
}