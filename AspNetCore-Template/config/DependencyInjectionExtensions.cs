using AspNetCore_Template.helper;
using AspNetCore_Template.helper.Sessions;
using AspNetCore_Template.model.DTO.mapper;
using AspNetCore_Template.model.entity;
using AspNetCore_Template.repository;
using AspNetCore_Template.security;
using AspNetCore_Template.service;

namespace AspNetCore_Template.config;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
    {
        //Repository


        //User Service
        services.AddScoped<ILoggedUserService, LoggedUserService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserSessionService, UserSessionService>();
        services.AddHttpClient<IGoogleRecaptchaService, GoogleRecaptchaService>();
        // Configure mail settings
        // services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
        services.AddTransient<IMyEmailSender<User>, EmailSender>();

        //Service


        //MAPPER profiles
        services.AddAutoMapper(typeof(UserProfile).Assembly);
        return services;
    }
}