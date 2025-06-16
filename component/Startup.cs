using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyHybridApp.Services.PeerServices;

namespace MyHybridApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSignalR().AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.MaxDepth = 0;
            });

            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 104857600; // 100 MB
            });

        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();
            app.UseEndpoints(endpoints => { _ = endpoints.MapHub<PeerHub>("/peerhub");});
        }
    }
}