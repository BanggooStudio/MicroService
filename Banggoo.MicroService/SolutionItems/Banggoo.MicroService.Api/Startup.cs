using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Exceptionless;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;

namespace Banggoo.MicroService.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            #region 配置MVC

            services.AddMvc(options =>
                    {
                        //options.Filters.Add(typeof(HttpGlobalExceptionFilter));
                    })
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                        options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";//格式化DateTime
                        options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                        options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                    }).AddControllersAsServices();

            #endregion

            #region 配置应用程序一般设置
            //services.Configure<AppSettings>(Configuration);
            #endregion

            #region 配置AutoMapper
            //services.AddAutoMapper(typeof(SettlementAutoMapperProfile).Assembly);
            #endregion

            #region 配置DbContext
            /*
            services.AddDbContext<SettlementContext>(options =>
            {
                options.UseMySql(Configuration.GetConnectionString("Master"),
                    mySqlOptionsAction: sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    });
            });
            */

            #endregion

            #region 配置Swagger

            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1", new Info
                {
                    Title = "越海运输管理系统服务 - 结算模块 HTTP API",
                    Version = "v1",
                    Description = "越海运输管理系统-结算模块微服务",
                    TermsOfService = "Terms Of Service"
                });
                options.DocInclusionPredicate((docName, description) => true);
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, "Banggoo.MicroService.Api.xml");
                options.IncludeXmlComments(xmlPath);
            });

            #endregion

            #region 配置跨域

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                                      .AllowAnyMethod()
                                      .AllowAnyHeader()
                                      .AllowCredentials());
            });

            #endregion

            #region 配置Ioc

            var container = new ContainerBuilder();
            container.Populate(services);

            //container.RegisterModule(new MediatorModule());
            //container.RegisterModule(new ApplicationModule(Configuration.GetConnectionString("Slave")));

            return new AutofacServiceProvider(container.Build());

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            #region 日志
            app.UseExceptionless(Configuration);
            #endregion

            var pathBase = Configuration["PATH_BASE"];
            if (!string.IsNullOrEmpty(pathBase))
            {
                loggerFactory.CreateLogger("init").LogDebug($"Using PATH BASE '{pathBase}'");
                app.UsePathBase(pathBase);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("CorsPolicy");
            app.UseMvcWithDefaultRoute();
            app.UseSwagger()
               .UseHSwaggerUI(c =>
               {
                   c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/v1/swagger.json", "Catalog.API V1");
                   c.DocExpansion("none");
               });
        }
    }
}
