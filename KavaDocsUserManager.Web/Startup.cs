using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KavaDocsUserManager.Business;
using KavaDocsUserManager.Business.Configuration;
using KavaDocsUserManager.Business.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace KavaDocsUserManager
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IServiceProvider ServiceProvider { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

        }

        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var config = new KavaDocsConfiguration();
            Configuration.Bind("KavaDocs", config);
            services.AddSingleton(config);

            KavaDocsConfiguration.Current = config;

            services.AddDbContext<KavaDocsContext>(builder =>
            {
                var connStr = config.ConnectionString;
                if (string.IsNullOrEmpty(connStr))
                    connStr = "server=.;database=KavaDocs; integrated security=true;MultipleActiveResultSets=true";

                builder.UseSqlServer(connStr, opt =>
                {
                    opt.EnableRetryOnFailure();
                    opt.CommandTimeout(15);
                });
            });

            services.AddTransient<UserBusiness>();
            services.AddTransient<RepositoryBusiness>();
            services.AddTransient<OrganizationBusiness>();

            Task.Run(() =>
            {
                // preload data on separate thread if possible
                var ctx = KavaDocsContext.GetKavaDocsContext(config.ConnectionString);
                DatabaseCreator.EnsureKavaDocsData(ctx);
                ctx.Users.Any(p => p.Id == Guid.NewGuid());
            });

            // set up and configure Authentication - make sure to call .UseAuthentication()
            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                
                .AddCookie(o =>
                {
                    o.LoginPath = "/account/signin";
                    o.LogoutPath = "/account/signout";
                    o.SlidingExpiration = true;
                    o.ExpireTimeSpan = new TimeSpan(2, 0, 0, 0);                    
                });

            services.AddMvc()
                .AddJsonOptions(opt =>
                {
                    opt.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                    opt.SerializerSettings.Converters.Add(new StringEnumConverter()
                        { NamingStrategy = new CamelCaseNamingStrategy() });
                })
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
            IHostingEnvironment env, 
            KavaDocsConfiguration config)

        {            

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseDeveloperExceptionPage();
                app.UseExceptionHandler("/Home/Error");
            }

            Console.WriteLine("\r\nPlatform: " + System.Runtime.InteropServices.RuntimeInformation.OSDescription);
            Console.WriteLine("Connection: " + KavaDocsConfiguration.Current.ConnectionString);
            
            app.UseAuthentication();

            app.UseDatabaseErrorPage();
            app.UseStatusCodePages();
            app.UseStaticFiles();

            app.UseMvcWithDefaultRoute();   
            

        }
    }
}
