using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Notism.Api.Controllers;

[ApiController]
[Authorize]
public class ApiController : ControllerBase
{
}