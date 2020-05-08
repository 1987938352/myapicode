using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myapicode.PolicyRequirement
{
    public class AdminRequirement : IAuthorizationRequirement
    {
        public String Name { get; set; }
    }
}
