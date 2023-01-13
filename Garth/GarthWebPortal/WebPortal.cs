using System.Globalization;
using GarthWebPortal.Controllers;
using GarthWebPortal.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Shared.Helpers;

namespace GarthWebPortal;

public class WebPortal
{
    public WebApplicationBuilder Builder { get; private set; }
    public WebApplication App { get; private set; }
    
    public WebPortal()
    {
        string? wwwrootDirectory = EnvironmentVariables.Get("wwwroot_dir", true);
        
        Builder = WebApplication.CreateBuilder(new WebApplicationOptions()
        {
            WebRootPath = wwwrootDirectory
        });

        Builder.Services.AddControllersWithViews().AddApplicationPart(typeof(HomeController).Assembly);
        Builder.Services.AddHttpContextAccessor();
        Builder.Services.AddScoped<AuthHelper>();
        
        Builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/signin";
                options.LogoutPath = "/signout";
            })
            .AddDiscord(options =>
            {
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.ClientId = EnvironmentVariables.Get("GARTH_CLIENT_ID", true)!;
                options.ClientSecret = EnvironmentVariables.Get("GARTH_CLIENT_SECRET", true)!;
                options.ClaimActions.MapCustomJson("urn:discord:avatar:url", user => {
                    var result = string.Format(
                        CultureInfo.InvariantCulture,
                        "https://cdn.discordapp.com/avatars/{0}/{1}.{2}",
                        user.GetString("id"),
                        user.GetString("avatar"),
                        user.GetString("avatar").StartsWith("a_") ? "gif" : "png");
                    Console.WriteLine("Got avatar url: " + result);
                    return result;
                });
                options.Scope.Add("guilds");
                options.SaveTokens = true;
                
                options.Events.OnCreatingTicket = ctx =>
                {
                    //Through intellisense I randomly stumbled upon being able to get accesToken here... but is there a more
                    //elegant way to access it _after this step_ ? 
                    Console.WriteLine("Got access token: " + ctx.AccessToken.ToString());
                    //do I have to somehow manually attach this ctx.AccessToken to a user object here? Does the AspNet.Security.Oauth.Providers framework provide some mechanism to access it other than this?
                    return Task.CompletedTask;
                };
            });
    }

    public WebApplication Start()
    {
        App = Builder.Build();
        
        // Configure the HTTP request pipeline.
        if (!App.Environment.IsDevelopment())
        {
            App.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            App.UseHsts();
        }
    
        App.UseStaticFiles();

        App.UseRouting();

        App.UseAuthentication();
        App.UseAuthorization();

        App.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        App.Run("http://localhost:5777");

        return App;
    }
}