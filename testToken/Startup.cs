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
            {//���ڽ�ɫ
                o.AddPolicy("AdminOrUser", o =>
                {
                    o.RequireRole("Admin").Build();//��AdminOrUser��ǩ�Ŀ����� ��AdminȨ�޻�User
                });
                //��������
                o.AddPolicy("AdminClaim2", o =>
                {
                    o.RequireClaim("Name", "laozhang", "abc@qq.com");
                });
                //������Ҫ��Requirement\
                o.AddPolicy("AdminRequireMent", o =>
                {
                    o.Requirements.Add(new AdminRequirement() { Role = "Admin" });
                });
            });
            services.AddSingleton<IAuthorizationHandler, MustRoleAdminHandler>();
          
            var symmetricKeyAsBase64 = Configuration["Audience:Secret"];//����ǩ�� ��Կ ���Ҫ�����ǰ䲼ʱһ��
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);//SymmetricSecurityKeyʹ�öԳ��㷨���ɵ�������Կ�ĳ�����ࡣ
            services.AddAuthentication("Bearer")
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,//��֤��Կ
                        IssuerSigningKey = signingKey,

                        ValidateIssuer = true,//��֤������
                        ValidIssuer = "issuer",

                        ValidateAudience = true,//��֤������
                        ValidAudience = "audience",

                        RequireExpirationTime = true,//����ʱ��
                        ValidateLifetime = true//��֤��������
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
