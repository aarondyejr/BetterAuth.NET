using BetterAuth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace BetterAuth.Authorization;

public class BetterAuthHandler(IHttpContextAccessor accessor) : AuthorizationHandler<BetterAuthRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        BetterAuthRequirement requirement)
    {
        if (accessor.HttpContext?.Items["BetterAuth.Session"] is SessionRecord)
            context.Succeed(requirement);
        else
            context.Fail();
        
        return Task.CompletedTask;
    }
}