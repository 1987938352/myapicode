using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace testToken.PolicyRequirement
{
    //public class MustRoleAdminHandler : IAuthorizationHandler
    //{
    //    public Task HandleAsync(AuthorizationHandlerContext context)
    //    {
    //        var requirement = context.Requirements.FirstOrDefault();//我们注册在AddAuthorization里面的requireRole这些策略实体

    //        context.Succeed(requirement);
    //        return Task.CompletedTask;
    //    }

    //}

    public class MustRoleAdminHandler : AuthorizationHandler<AdminRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
