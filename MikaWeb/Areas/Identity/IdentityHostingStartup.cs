using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MikaWeb.Areas.Identity.Data;
using MikaWeb.Data;

[assembly: HostingStartup(typeof(MikaWeb.Areas.Identity.IdentityHostingStartup))]
namespace MikaWeb.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<MikaWebContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("MikaWebContextConnection")));

                services.AddDefaultIdentity<MikaWebUser>(options => options.SignIn.RequireConfirmedAccount = false)
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<MikaWebContext>();

                services.AddScoped<IUserClaimsPrincipalFactory<MikaWebUser>, AdditionalUserClaimsPrincipalFactory>();
            });
        }
    }
}