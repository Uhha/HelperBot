﻿using BotApi.Commands;
using BotApi.Interfaces;
using BotApi.Services;

namespace BotApi
{
    public static class DIConfiguration
    {
        internal static void Configure(IServiceCollection services)
        {
            services.AddSingleton<IWebhookService, WebhookService>();

            services.AddSingleton<ICommand, GetCoinsCommand>();
            services.AddSingleton<ICommandFactory, CommandFactory>();
            services.AddSingleton<IQBUrlResolverService, QBUrlResolverService>();
            services.AddSingleton<IMusifyService, MusifyService>();
			services.AddSingleton<CommandInvoker>();

        }
    }
}
