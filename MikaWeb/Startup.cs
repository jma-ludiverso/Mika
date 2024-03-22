using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MikaWeb.Extensions;
using MikaWeb.Extensions.DB;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MikaWeb
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
            services.Configure<DBConfig>(Configuration.GetSection("ConnectionStrings"));
            services.Configure<MikaConf>(Configuration.GetSection("MikaConf"));
            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                });

            services.AddRazorPages();
            PasswordConfiguration passcfg = this.getPasswordConfiguration();
            services.Configure<Microsoft.AspNetCore.Identity.IdentityOptions>(op =>
            {
                op.Password.RequireDigit = passcfg.RequireDigit;
                op.Password.RequireLowercase = passcfg.RequireLowercase;
                op.Password.RequiredLength = passcfg.RequiredLength;
                op.Password.RequiredUniqueChars = passcfg.RequiredUniqueChars;
                op.Password.RequireNonAlphanumeric = passcfg.RequireNonAlphanumeric;
                op.Password.RequireUppercase = passcfg.RequireUppercase;
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminArea", policy => policy.RequireClaim("role", "admin"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            var cultureInfo = new CultureInfo("es-ES");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }

        protected PasswordConfiguration getPasswordConfiguration()
        {
            PasswordConfiguration ret = new PasswordConfiguration();
            Configuration.GetSection("CustomConfiguration").Bind(ret);
            return ret;
        }
    }
}
