using Microsoft.AspNetCore.Builder;

namespace MusicStore.UI.Middlewares
{
    public static class Endpoint
    {
        public static IApplicationBuilder Route(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();
            });

            return app;
        }
    }
}
