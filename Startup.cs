using System;
using System.Text;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VariantBot.Middleware;
using VariantBot.Slack;

namespace VariantBot
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            services.AddApplicationInsightsTelemetry();

            services
                .AddControllers()
                .AddNewtonsoftJson();

            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            services.AddTransient<SlackAuthenticator>();
            services.AddTransient<SlackMessageHandler, SlackMessageHandler>();

            services.AddHangfire(config =>
            {
                config.UseInMemoryStorage();
            });
            services.AddHangfireServer();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseHangfireDashboard();
            app.UseMiddleware<SlackAuthenticator>()
                .UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints => { endpoints.MapControllers(); });

            BackgroundJob.Enqueue(() => KeeperAliver.DontDie());
            RecurringJob.AddOrUpdate(() => KeeperAliver.DontDie(), Cron.Minutely());
            RecurringJob.AddOrUpdate(() => DeskBooking.PostDeskBookingDays(), Cron.Weekly(DayOfWeek.Friday, 13));
        }
    }
}