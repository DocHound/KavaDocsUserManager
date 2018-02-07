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
                var ctx = KavaDocsContext.GetWeblogContext(config.ConnectionString);
                DatabaseCreator.EnsureKavaDocsData(ctx);
                ctx.Users.Any(p => p.Id == Guid.NewGuid());
            });

            // set up and configure Authentication - make sure to call .UseAuthentication()
            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o =>
                {
                    o.LoginPath = "/account/login";
                    o.LogoutPath = "/account/logout";
                });

            services.AddMvc();
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
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseAuthentication();

            app.UseDatabaseErrorPage();
            app.UseStatusCodePages();
            app.UseStaticFiles();

            app.UseMvcWithDefaultRoute();
        }
    }
}
