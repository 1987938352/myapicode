using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace testToken.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IHttpContextAccessor httpContextAccessor;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }
        [HttpGet]
        //[Authorize(Policy = "AdminClaim2")]
        [Authorize(Policy = "AdminRequireMent")]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
        [HttpPost]
        public int Post()
        {
            var a = httpContextAccessor.HttpContext.Request;
            var b = a.Form.Keys;
            return 1;
        }
    }
}
