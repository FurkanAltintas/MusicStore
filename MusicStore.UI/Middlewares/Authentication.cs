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
                options.AppId = "341606417617781";
                options.AppSecret = "88c6543e7395b1278de0985ff60abfe9";
            });

            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = "1077506827939-g05arfjl09pakiitmiffifes7ocsj7lg.apps.googleusercontent.com";
                options.ClientSecret = "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";
            });

            // null yazan yere kendi secret bilgilerinizi girmeniz gerekmektedir

            return services;
        }
    }
}
