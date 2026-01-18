using Microsoft.AspNetCore.Authorization;

namespace Notism.Api.Attributes;

public class RequireAdminAttribute : AuthorizeAttribute
{
    public RequireAdminAttribute()
    {
        Roles = "admin";
    }
}