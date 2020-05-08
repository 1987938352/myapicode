using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace testToken.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public LoginController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
       
        [HttpGet]
        public string GetToken()
        {
            
            SecurityToken securityToken = new JwtSecurityToken(
                issuer: "issuer",//发行人
                audience: "audience",//订阅人
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Audience:Secret"])), SecurityAlgorithms.HmacSha256),//传key和加密算法
                expires: DateTime.Now.AddHours(1),
                claims: new Claim[] {
                new Claim("Name","laozhang"),
                new Claim(ClaimTypes.Role,"Admin")
                }) ;
            string jwtToken = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return jwtToken;
        }
    }
}