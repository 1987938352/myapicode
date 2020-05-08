using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace myapicode.Controllers
{
    [Route("[controller]")]
    
    [ApiController]
    public class ValueController : ControllerBase
    {
        private readonly IHttpContextAccessor accessor;

        public ValueController(IHttpContextAccessor accessor)
        {
            this.accessor = accessor;
        }
        [HttpGet]
        //[Authorize(Policy = "AdminRequireMent")]
        //[Authorize]
        //[Authorize(Roles = "admin")]
        public IActionResult Get()
        {
            return Ok("nihao");
        }
    }
}