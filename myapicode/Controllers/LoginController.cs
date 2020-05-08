using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Core.InterFace;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace myapicode.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IUserRepository userRepository;

        public LoginController(IConfiguration configuration,IUserRepository userRepository)
        {
            this.configuration = configuration;
            this.userRepository = userRepository;
        }
        [HttpGet]
        public string GetToken()
        {

            SecurityToken securityToken = new JwtSecurityToken(
                issuer: configuration["Audience:Issuer"],//发行人
                audience: configuration["Audience:audience"],//订阅人
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Audience:Secret"])), SecurityAlgorithms.HmacSha256),//传key和加密算法
                expires: DateTime.Now.AddHours(1),
                claims: new Claim[] {
                new Claim("Name","Admin"),
                new Claim(ClaimTypes.Role,"admin")
                }); 
            string jwtToken = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return jwtToken;
        }
        [HttpPost]
        public async Task<IActionResult>  login(int Id,string pwd)
        {
            var user =await userRepository.Login(Id, pwd);
            if (user == null)
            {
                return Ok(new AjaxResult() { status="err",msg="账号或密码错误"});
            }
            SecurityToken securityToken = new JwtSecurityToken(
               issuer: configuration["Audience:Issuer"],//发行人
               audience: configuration["Audience:audience"],//订阅人
               signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Audience:Secret"])), SecurityAlgorithms.HmacSha256),//传key和加密算法
               expires: DateTime.Now.AddHours(1),
               claims: new Claim[] {
                new Claim("id",user.Id.ToString()),
                new Claim(ClaimTypes.Role,"admin")
               });
            string jwtToken = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return Ok(new AjaxResult() { status="ok",result = new { Id = user.Id, Name = user.Name, Authorization = jwtToken } });
        }
    }
}