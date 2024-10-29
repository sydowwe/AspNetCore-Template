using System.Security.Claims;
using AspNetCore_Template.model.entity;
using AspNetCore_Template.security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AspNetCore_Template.config;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<User, UserRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 4;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                // options.Stores.ProtectPersonalData = true;
                options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Email;
            })
            .AddEntityFrameworkStores<AppDbContext>() // Use the custom DbContext
            .AddDefaultTokenProviders()
            // .AddPersonalDataProtection<>()
            .AddApiEndpoints();
        // .AddIdentityCookies(options =>
        // {
        //     options.ApplicationCookie?.Configure(opt =>
        //     {
        //         opt.Cookie.IsEssential = true;
        //         opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        //         opt.Cookie.SameSite = SameSiteMode.Strict;
        //         opt.Cookie.MaxAge = TimeSpan.FromHours(3);
        //     });
        // });


        services.AddAuthorizationBuilder().SetFallbackPolicy(
            new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(ClaimTypes.NameIdentifier)
                .Build()
        );
        services.ConfigureApplicationCookie(options =>
        {
            // options.Cookie.SameSite = SameSiteMode.None;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Cookie expiration time
            options.Cookie.Path = "/";
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });
        return services;
    }
}