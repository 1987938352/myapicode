using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Core.Entitites;
using Core.InterFace;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using myapicode.PolicyRequirement;
using Nest;
using StackExchange.Redis;

namespace myapicode.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IUserRepository userRepository;
        private readonly IHttpContextAccessor accessor;
        private readonly IUnitOfWork unitOfWork;
        private readonly IDatabase db;
        public LoginController(IConfiguration configuration,IUserRepository userRepository,IHttpContextAccessor accessor, IConnectionMultiplexer redis,
            IUnitOfWork unitOfWork)
        {
            this.configuration = configuration;
            this.userRepository = userRepository;
            this.accessor = accessor;
            this.unitOfWork = unitOfWork;
            db = redis.GetDatabase();
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
        public async Task<IActionResult>  login([FromForm]MyLogin myLogin)
        {
            var a= Request.RouteValues;
            var user =await userRepository.Login(myLogin.id, myLogin.pwd);
            if (user == null)
            {
                return Ok(new AjaxResult() { status="err",msg="账号或密码错误"});
            }
          IEnumerable< Claim > claims = new Claim[] {
                new Claim("id",user.Id.ToString()),
                new Claim(ClaimTypes.Role,"admin")
               };

            string jwtToken = PushToken.PushMyToken(configuration,claims);

            return Ok(new AjaxResult() { status="ok",result = new { Id = user.Id, Name = user.Name, Authorization = jwtToken } });
        }

        [HttpGet("gettok")]
        //[Authorize(Roles = "admin")]///测试方法
        public  string GetMyToken()
        {
            //var claimsIdentity= this.User.Identity as ClaimsIdentity;
            //var userId= claimsIdentity.FindFirst(ClaimTypes.Name)
            
            var myClaims = this.User.Claims;
            string Authorization = "";
            if (myClaims!=null)
            {
                Authorization = PushToken.PushMyToken(configuration, myClaims);
            }
            return Authorization;
         var myId= myClaims?.FirstOrDefault(c => c.Type.Equals("id", StringComparison.OrdinalIgnoreCase))?.Value;

           
            return myId;
            //var schemeProvider = accessor.HttpContext.RequestServices.GetService(typeof(IAuthenticationSchemeProvider)) as IAuthenticationSchemeProvider;
            //var defaultAuthenticate =  schemeProvider.GetDefaultAuthenticateSchemeAsync();
            //if (defaultAuthenticate != null)
            //{
            //    var result = await accessor.HttpContext.AuthenticateAsync(defaultAuthenticate.Name);
            //    var user = result?.Principal;
            //    if (user != null)
            //    {
            //        account = user.Identity.Name;
            //    }
            //}
        }
        [HttpPost("regist")]
        public async Task<IActionResult> regist([FromForm]Regist regist)
        {
          await  db.ListLeftPushAsync("Email", regist.email);
            string guid = Guid.NewGuid().ToString();
            HashEntry[] hashEntry = { new HashEntry("pwd", regist.pwd), new HashEntry("guid", guid) };
            await db.HashSetAsync($"Email{regist.email}", hashEntry);
            return Ok(new AjaxResult() { status = "ok", result = "查看邮箱获取校验码",msg= guid });
        }
       [HttpPost("registlogin")]
        public async Task<IActionResult> registlogin([FromForm]RegistLogin regist)
        {
            string key = $"Email{ regist.email}";
            if (db.KeyExists(key))
            {
                RegistGetDb registGetDb =RedisHelper.ConvertFromRedis<RegistGetDb>(await db.HashGetAllAsync(key));
                if (registGetDb.guid == regist.guid)
                {
                  User user=  userRepository.Register(registGetDb.pwd);
                    IEnumerable<Claim> claims = new Claim[] {
                new Claim("id",user.Id.ToString()),
                new Claim(ClaimTypes.Role,"admin")
               };
                    string jwtToken = PushToken.PushMyToken(configuration, claims);
                    if (user.Id!=0)
                    {
                        return Ok(new AjaxResult() { status = "ok", result = new { Id = user.Id, Name = user.Name, Authorization = jwtToken } });
                    }
                }
            }
            return BadRequest(new AjaxResult() { status = "err" });
        }
    }
    public class MyLogin
    {
        public int id { get; set; }
        public string pwd { get; set; }
    }
    public class Regist
    {
        
        public string pwd { get; set; }
        public string email { get; set; }
    }
    public class RegistLogin
    {
        public string email { get; set; }
        public string guid { get; set; }
    }
    public class RegistGetDb
    {
        public string pwd { get; set; }
        public string guid { get; set; }
    }

}