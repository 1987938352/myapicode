
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace myapicode.PolicyRequirement
{
    public static class GetToken
    {
        
        public static string GetMyToken(IConfiguration configuration ,int id)
        {
            string a = configuration["Audience:Secret"];
            SecurityToken securityToken = new JwtSecurityToken(
                issuer: "issuer",//发行人
                audience: "audience",//订阅人
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Audience:Secret"])), SecurityAlgorithms.HmacSha256),//传key和加密算法
                expires: DateTime.Now.AddHours(1),
                claims: new Claim[] {
                new Claim("Name","laozhang"),
                new Claim(ClaimTypes.Role,"Admin")
                });
            string jwtToken = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return jwtToken;
        }
    }
}
