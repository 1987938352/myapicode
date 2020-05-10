using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myapicode.PolicyRequirement
{
    public class MustRoleAdminHandler : AuthorizationHandler<AdminRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
        {
           
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
