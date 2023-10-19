
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using BotApi.Services;
using BotApi.Interfaces;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using BotApi.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace BotApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseUrls("http://*:9005");
            // Create a ConfigurationBuilder
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            // Build the configuration
            var configuration = configurationBuilder.Build();

            // Configure services
            builder.Services.Configure<APIConfig>(configuration.GetSection("APIConfig"));

            // Logging
            builder.Logging.AddConsole();

            // Configure common services
            builder.Services.AddControllers().AddNewtonsoftJson();
            builder.Services.AddMvc();
            builder.Services.AddRouting();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            builder.Services.AddHostedService<TorrentStatusCheckService>();

            //services.AddHttpContextAccessor();


            builder.Services.AddSingleton<ITelegramBotService>(provider =>
            {
                var botToken = configuration["APIConfig:BotApiKey"]; 
                var botService = new TelegramBotService(botToken);
                return botService;
            });


            

            builder.Services.AddSingleton<IQBitService, QBitService>();
            builder.Services.AddSingleton<ITorrentStatusCheckService, TorrentStatusCheckService>();


            //builder.Services.AddSingleton<IQBitService>(provider =>
            //{
            //    var qBUrl = configuration["APIConfig:QBUrl"];
            //    return new QBitService(qBUrl);
            //});

            DIConfiguration.Configure(builder.Services);
            RegisterCommands.Register(builder.Services);

            var app = builder.Build();

            //var torrentStatusCheckService = app.Services.GetRequiredService<ITorrentStatusCheckService>();
            //await torrentStatusCheckService.ExecuteAsync(CancellationToken.None);

            var qbitService = app.Services.GetRequiredService<IQBitService>();
            await qbitService.Auth();

            var webHookUrl = configuration["APIConfig:WebHookUrl"];
            var telegramService = app.Services.GetRequiredService<ITelegramBotService>();
            await telegramService.SetWebhookAsync(webHookUrl);

            if (app.Environment.IsDevelopment())
            {
                // Development-specific middleware and configuration
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                // Production-specific middleware and configuration
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Common middleware
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            // Define your routes and controllers
            app.MapControllers();

       
            //bot.SendTextMessageAsync(182328439, $"Webhook set to {webHookUrl}");

            //This doesn't get initiated
            //app.Use(async (context, next) =>
            //{
            //    var botService = context.RequestServices.GetRequiredService<ITelegramBotService>();
            //    await botService.SetWebhookAsync(webHookUrl);

            //    await next.Invoke();
            //});

            app.Run();
        }
    }
}