using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicStore.DataAccess.Data;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.DataAccess.MainRepository;
using MusicStore.Utility;

namespace MusicStore.UI.Middlewares
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection Service(this IServiceCollection services, IConfiguration configuration, string connection)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connection).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            /*Tracking işlemini engelleyerek performansı arttırmış oluyoruz.
              Default hali : TrackAll
            */

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<IdentityUser, IdentityRole>().
                AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            //DefaultIdentityUser yazıyordu onu IdentityUser yapıyoruz cunku bız sadece User kısmını kullanmıyoruz o yüzden default halini kaldırmamız gerekiyor

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            // biri senden IUnitOfWork isterse sen ona UnitOfWork ver.
            // Scoped: Her request için bir tane oluşturur, her request için yeni bir tane oluşturmadan önce eski olanı atar.
            services.AddSingleton<IEmailSender, EmailSender>();
            services.Configure<EmailOptions>(configuration); // dependency injection için hazırlamış olduk

            return services;
        }
    }
}
