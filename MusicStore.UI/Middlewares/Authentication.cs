using Microsoft.Extensions.DependencyInjection;
using System;

namespace MusicStore.UI.Middlewares
{
    public static class Authentication
    {
        public static IServiceCollection SocialMediaAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication().AddFacebook(options =>
            {
                options.AppId = "appId";
                options.AppSecret = "SecretId";
            });

            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = "clientId";
                options.ClientSecret = "clientSecret";
            });

            // null yazan yere kendi secret bilgilerinizi girmeniz gerekmektedir

            return services;
        }
    }
}
