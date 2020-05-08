using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using testToken.PolicyRequirement;

namespace testToken
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddAuthorization(o =>
            {//基于角色
                o.AddPolicy("AdminOrUser", o =>
                {
                    o.RequireRole("Admin").Build();//带AdminOrUser标签的控制器 有Admin权限或User
                });
                //基于声明
                o.AddPolicy("AdminClaim2", o =>
                {
                    o.RequireClaim("Name", "laozhang", "abc@qq.com");
                });
                //基于需要的Requirement\
                o.AddPolicy("AdminRequireMent", o =>
                {
                    o.Requirements.Add(new AdminRequirement() { Role = "Admin" });
                });
            });
            services.AddSingleton<IAuthorizationHandler, MustRoleAdminHandler>();
          
            var symmetricKeyAsBase64 = Configuration["Audience:Secret"];//数字签名 密钥 这个要和我们颁布时一样
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);//SymmetricSecurityKey使用对称算法生成的所有密钥的抽象基类。
            services.AddAuthentication("Bearer")
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,//验证密钥
                        IssuerSigningKey = signingKey,

                        ValidateIssuer = true,//验证发行人
                        ValidIssuer = "issuer",

                        ValidateAudience = true,//验证订阅人
                        ValidAudience = "audience",

                        RequireExpirationTime = true,//过期时间
                        ValidateLifetime = true//验证生命周期
                    };
                });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
