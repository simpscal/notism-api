using MediatR;

using Notism.Shared.Models;

namespace Notism.Application.User.AdminGetUsers;

public record AdminGetUsersRequest : FilterParams, IRequest<AdminGetUsersResponse>;