using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VariantBot.Services;
using VariantBot.Middleware;
using VariantBot.Slack;

namespace VariantBot
{
    public class Startup
    {
        private readonly IConfiguration Configuration;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            services.AddApplicationInsightsTelemetry();

            services
                .AddControllers()
                .AddNewtonsoftJson();

            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            services.Configure<List<SlackChannel>>(Configuration.GetSection("SlackChannels"));
            services.Configure<MusicRecommendationAppConfig>(Configuration.GetSection("MusicRecommendationAppConfig"));
            
            services.AddTransient<SlackAuthenticator>();
            services.AddTransient<SlackMessageHandler, SlackMessageHandler>();
            services.AddSingleton<MusicRecommendationService>();
            services.AddHostedService<SlackMessageHistoryService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"),
                    app => app.UseMiddleware<SlackAuthenticator>())
                .UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}