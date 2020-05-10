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
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;//ת��ΪСд��ʽ

               //�����ת��json'Ϊ�Զ���ý������ ��ϲ��
            }).AddFluentValidation();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "my.Core API",
                    Description = "���˵���ĵ�",
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

            #region ʹ��jwt
            services.AddAuthorization(o =>
            {
                o.AddPolicy("AdminRequireMent", o =>
                {
                    o.Requirements.Add(new AdminRequirement() { Name = "Admin" });
                    
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
                        ValidIssuer = Configuration["Audience:Issuer"],

                        ValidateAudience = true,//��֤������
                        ValidAudience = Configuration["Audience:audience"],

                        RequireExpirationTime = true,//����ʱ��
                        ValidateLifetime = true//��֤��������
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
            //ע���������������
            var propertyMappingContainer = new PropertyMappingContainer();
            propertyMappingContainer.Register<PostPropertyMapping>();
            services.AddSingleton<IPropertyMappingContainer>(propertyMappingContainer);

            services.AddTransient<ITypeHelperService, TypeHelperService>();
         
            #region jwt ѧϰ ��ͨ��bearer��֤ ��ע��jwrbearer����
            //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();//����net core��jwt�Ľ�������ӳ�� 

            //var symmetricKeyAsBase64 = Configuration["Audience:Secret"];//����ǩ�� ��Կ ���Ҫ�����ǰ䲼ʱһ��
            //var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            //var signingKey = new SymmetricSecurityKey(keyByteArray);//SymmetricSecurityKeyʹ�öԳ��㷨���ɵ�������Կ�ĳ�����ࡣ

            //var tokenValidationParameters = new TokenValidationParameters()
            //{//���õļ�������  Issuer�䷢��  Signingǩ�� Audience����
            //    ValidateIssuerSigningKey = true,//�Ƿ�����Կ��֤
            //    IssuerSigningKey = signingKey,
            //    ValidateIssuer = true,
            //    ValidIssuer = "http://localhost:5001",//������
            //    ValidateAudience = true,
            //    ValidAudience = "http://localhost:5000",//������
            //    ValidateLifetime = true,
            //    ClockSkew = TimeSpan.FromSeconds(30),//ʱ��ƫб
            //    RequireExpirationTime = true
            //};

            ////ע�����
            //services.AddAuthentication("Bearer").AddJwtBearer(o =>
            //{//ע��jwtBearer����
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
                //app.UseDeveloperExceptionPage(); //��MVC������ api���Ƿ���json�ȽϺ�
                app.UseMyExceptionHandler(loggerFactory);//������õ�Extensions�ļ��е� ʹ��app.UseExceptionHandler()�쳣����
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
