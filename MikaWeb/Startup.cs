using System.Globalization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MikaWeb.Extensions;
using MikaWeb.Extensions.DB;
using Newtonsoft.Json.Converters;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using MikaWeb.Areas.Identity.Data;
using MikaWeb.Data;
using MikaWeb.Areas.Identity;
using Microsoft.OpenApi.Models;

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
            services.Configure<ApiConfiguration>(Configuration.GetSection("APIConfig"));
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
            services.AddAuthentication(IdentityConstants.ApplicationScheme);
            ApiConfiguration apiConf = this.getApiConfiguration();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = apiConf.JwtSettings.Issuer,
                        ValidAudience = apiConf.JwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiConf.JwtSettings.Key))
                    };
                });
            services.AddScoped<ITokenClaimsService, IdentityTokenClaimService>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Mica peluqueros API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Introduce un token válido",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        System.Array.Empty<string>()
                    }
                });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
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

        protected ApiConfiguration getApiConfiguration()
        {
            ApiConfiguration ret = new ApiConfiguration();
            Configuration.GetSection("APIConfig").Bind(ret);
            return ret; 
        }

        protected PasswordConfiguration getPasswordConfiguration()
        {
            PasswordConfiguration ret = new PasswordConfiguration();
            Configuration.GetSection("CustomConfiguration").Bind(ret);
            return ret;
        }
    }
}
