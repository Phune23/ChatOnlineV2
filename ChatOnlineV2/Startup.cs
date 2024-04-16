using ChatOnlineV2.Data;
using ChatOnlineV2.Data.Entities;
using ChatOnlineV2.IdentityServer;
using ChatOnlineV2.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace ChatOnlineV2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ManageChatDbContext>(options =>
            options.UseSqlServer(
                Configuration.GetConnectionString("DefaultConnection"))
            );

            services.AddIdentity<ManageUser, IdentityRole>() // để cho nó dùng được UserManger và roleManager
                .AddEntityFrameworkStores<ManageChatDbContext>()
                .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

            }) //https://nhatkyhoctap.blogspot.com/2017/09/identity-server-4-su-dung-sigining.html
            .AddInMemoryApiResources(Config.Apis) // bên folder IdentityServer thêm Config
                                                  // .AddInMemoryClients(Configuration.GetSection("IdentityServer:Clients"))
            .AddInMemoryClients(Config.Clients) // lấy ra các client
            .AddInMemoryIdentityResources(Config.Ids)

            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddAspNetIdentity<ManageUser>()
            .AddDeveloperSigningCredential();

            services.AddTransient<IEmailSender, EmailSenderService>();


            services.AddAutoMapper(typeof(Startup));

            services.AddAuthentication()
                .AddGoogle(googleOptions =>
                {
                // Đọc thông tin Authentication:Google từ appsettings.json
                IConfigurationSection googleAuthNSection = Configuration.GetSection("Authentication:Google");
                    // Thiết lập ClientID và ClientSecret để truy cập API google
                    googleOptions.ClientId = googleAuthNSection["ClientId"];
                    googleOptions.ClientSecret = googleAuthNSection["ClientSecret"];
                })
               .AddFacebook(facebookOptions => {
                   // Đọc cấu hình
                   IConfigurationSection facebookAuthNSection = Configuration.GetSection("Authentication:Facebook");
                   facebookOptions.AppId = facebookAuthNSection["AppId"];
                   facebookOptions.AppSecret = facebookAuthNSection["AppSecret"];
                   // Thiết lập đường dẫn Facebook chuyển hướng đến
                   facebookOptions.CallbackPath = "/sign-facebook";
               })

                .AddLocalApi("Bearer", option =>
                {
                    option.ExpectedScope = "api.WebChat";
                });


            services.AddAuthorization(options =>
            {
                options.AddPolicy("Bearer", policy =>  // thêm một cái chính sách
                {
                    policy.AddAuthenticationSchemes("Bearer");
                    policy.RequireAuthenticatedUser();
                });
            });

                
            IMvcBuilder build = services.AddRazorPages(options =>
            {
                options.Conventions.AddAreaFolderRouteModelConvention("Identity", "/Account/", model =>
                {
                    foreach (var selector in model.Selectors)
                    {
                        var attributeRouteModel = selector.AttributeRouteModel;
                        attributeRouteModel.Order = -1;
                        attributeRouteModel.Template = attributeRouteModel.Template.Remove(0, "Identity".Length);
                    }
                });
            });


#if DEBUG
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"); if (environment == Environments.Development)
            {
                build.AddRazorRuntimeCompilation();
            }
#endif
            services.AddControllersWithViews();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebChat Space Api", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            //địa chỉ chứng thực tới localhost:5000
                            AuthorizationUrl = new Uri(Configuration["AuthorityUrl"] + "/connect/authorize"),
                            Scopes = new Dictionary<string, string> { { "api.WebChat", "WebChat API" } }
                        },
                    },
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new List<string>{ "api.WebChat" }
                    }
                });
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseIdentityServer();

            ////app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });
            //tư động sử dụng và gen ra file swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                //trỏ tới swagger
                c.OAuthClientId("swagger");
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebChat Space Api V1");
            });

        }

    }
}
