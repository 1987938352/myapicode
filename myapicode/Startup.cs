using AutoMapper;
using Core.InterFace;
using FluentValidation;
using FluentValidation.AspNetCore;
using infrastructure.DataBase;
using infrastructure.DTO;
using infrastructure.Repositories;
using infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using myapicode.ES;
using myapicode.Extensions;
using myapicode.PolicyRequirement;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace myapicode
{
    public class Startup
    {
        private static IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers().AddJsonOptions(options=>
            {
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;//转换为小写格式

               //这里的转换json'为自定义媒体类型 不喜欢
            }).AddFluentValidation();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "my.Core API",
                    Description = "框架说明文档",
                    Version = "v0.1.0",
                });
            });

            services.AddDbContext<MyContext>(options =>
            {
                var connectionString = Configuration["ConnectionStrings:DefaultConnection"];
                //var connectionString = Configuration.GetConnectionString("DefaultConnection");
                options.UseLazyLoadingProxies().UseMySql(connectionString);
            });
            services.AddHttpContextAccessor();

            #region 使用jwt
            services.AddAuthorization(o =>
            {
                o.AddPolicy("AdminRequireMent", o =>
                {
                    o.Requirements.Add(new AdminRequirement() { Name = "Admin" });
                    
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
                        ValidIssuer = Configuration["Audience:Issuer"],

                        ValidateAudience = true,//验证订阅人
                        ValidAudience = Configuration["Audience:audience"],

                        RequireExpirationTime = true,//过期时间
                        ValidateLifetime = true//验证生命周期
                    };
                });
            #endregion

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                options.HttpsPort = 5001;
            });

            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepositories>();

            services.AddSingleton<IESSever, ESSever>();

            services.AddAutoMapper(Assembly.Load("myapicode").GetTypes().Where(type => type.IsSubclassOf(typeof(Profile)) && !type.IsAbstract).ToArray());

            services.AddTransient<IValidator<PostAddResource>, PostAddOrUpdateResourceValidator<PostAddResource>>();
            services.AddTransient<IValidator<PostUpdateResource>, PostAddOrUpdateResourceValidator<PostUpdateResource>>();
            //注册我们排序的容器
            var propertyMappingContainer = new PropertyMappingContainer();
            propertyMappingContainer.Register<PostPropertyMapping>();
            services.AddSingleton<IPropertyMappingContainer>(propertyMappingContainer);

            services.AddTransient<ITypeHelperService, TypeHelperService>();
         
            #region jwt 学习 先通过bearer认证 再注册jwrbearer服务
            //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();//紧张net core对jwt的进行配置映射 

            //var symmetricKeyAsBase64 = Configuration["Audience:Secret"];//数字签名 密钥 这个要和我们颁布时一样
            //var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            //var signingKey = new SymmetricSecurityKey(keyByteArray);//SymmetricSecurityKey使用对称算法生成的所有密钥的抽象基类。

            //var tokenValidationParameters = new TokenValidationParameters()
            //{//常用的几个单词  Issuer颁发者  Signing签名 Audience观众
            //    ValidateIssuerSigningKey = true,//是否开启密钥认证
            //    IssuerSigningKey = signingKey,
            //    ValidateIssuer = true,
            //    ValidIssuer = "http://localhost:5001",//发行人
            //    ValidateAudience = true,
            //    ValidAudience = "http://localhost:5000",//订阅人
            //    ValidateLifetime = true,
            //    ClockSkew = TimeSpan.FromSeconds(30),//时钟偏斜
            //    RequireExpirationTime = true
            //};

            ////注入服务
            //services.AddAuthentication("Bearer").AddJwtBearer(o =>
            //{//注册jwtBearer服务
            //    o.TokenValidationParameters = tokenValidationParameters;
            //});
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,ILoggerFactory loggerFactory)
        {
            app.UseCors(o =>
            {
                o.WithOrigins("http://localhost:8081");
                //o.AllowAnyOrigin();
                o.AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            });
            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage(); //在MVC里面用 api还是返回json比较好
                app.UseMyExceptionHandler(loggerFactory);//这个是用的Extensions文件夹的 使用app.UseExceptionHandler()异常处理
                //app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            }
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUI(m =>
            {
                m.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiHelp V1");
            });
            app.UseAuthentication();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }
    }
}
