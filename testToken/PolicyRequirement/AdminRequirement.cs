using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace testToken.PolicyRequirement
{
    public class AdminRequirement:IAuthorizationRequirement
    {
            public String Role { get; set; }

    }
}
