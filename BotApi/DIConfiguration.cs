using BotApi.Commands;
using BotApi.Database;
using BotApi.Interfaces;
using BotApi.Services;

namespace BotApi
{
    public static class DIConfiguration
    {
        internal static void Configure(IServiceCollection services)
        {
            services.AddSingleton<IWebhookService, WebhookService>();

            //services.AddSingleton<ICommand, GetCoinsCommand>();

            services.AddTransient<IDB, DB>();
            services.AddTransient<IQBitService, QBitService>();
            services.AddTransient<ICommandFactory, CommandFactory>();
            services.AddSingleton<IQBUrlResolverService, QBUrlResolverService>();
            services.AddTransient<IMusifyService, MusifyService>();
            services.AddTransient<IGetCoinsService,  GetCoinsService>();
            services.AddTransient<ISecuritiesService,  SecuritiesService>();

            services.AddSingleton<CommandInvoker>();

        }
    }
}
