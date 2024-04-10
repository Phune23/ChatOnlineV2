using ChatOnlineV2.Data;
using ChatOnlineV2.Data.Entities;
using ChatOnlineV2.IdentityServer;
using ChatOnlineV2.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

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



            services.AddRazorPages();
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

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });

        }

    }
}
