using System.Net;

namespace Order.Application.Common.Models;

public record AppError(HttpStatusCode StatusCode, IEnumerable<string> Messages);