using System.Net;

namespace Notism.Application.Common.Models;

public record AppError(HttpStatusCode StatusCode, IEnumerable<string> Messages);