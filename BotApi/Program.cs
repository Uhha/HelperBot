
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
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseUrls("http://*:9005");
            // Create a ConfigurationBuilder
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

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

            //services.AddHttpContextAccessor();

            builder.Services.AddSingleton<ITelegramBotService>(provider =>
            {
                var botToken = configuration["APIConfig:BotApiKey"]; 
                var botService = new TelegramBotService(botToken);
                return botService;
            });

            builder.Services.AddSingleton<IQBitService>(provider =>
            {
                //var configuration = provider.GetRequiredService<IConfiguration>();
                var qBUrl = configuration["APIConfig:QBUrl"];
                return new QBitService(qBUrl);
            });

            builder.Services.AddSingleton<IWebhookService, WebhookService>();
            builder.Services.AddSingleton<ICommandProcessingService, CommandProcessingService>();

            builder.Services.AddSingleton<ICommand, GetCoinsCommand>();
            builder.Services.AddSingleton<ICommandFactory, CommandFactory>();


            builder.Services.AddSingleton<GetCoinsCommand>();


            var commandDictionary = new Dictionary<string, ICommand>
            {
                { "/coins", null }, // The factory will provide the implementation
            };

            builder.Services.AddSingleton(commandDictionary);

            // Register the CommandInvoker
            builder.Services.AddSingleton<CommandInvoker>();


            //builder.Services.AddSingleton(serviceProvider =>
            //{
            //    var commandDictionary = new Dictionary<string, ICommand>
            //    {
            //        { "/coins", serviceProvider.GetRequiredService<GetCoinsCommand>() },
            //    };
            //    return new CommandInvoker(commandDictionary);
            //});


            var app = builder.Build();

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

       
            //SET THE FUCKING WEBHOOK!
            var webHookUrl = configuration["APIConfig:WebHookUrl"];
            var botToken = configuration["APIConfig:BotApiKey"];
            var bot = new TelegramBotClient(botToken);
            bot.SetWebhookAsync(webHookUrl);
            bot.SendTextMessageAsync(182328439, $"Webhook set to {webHookUrl}");

            //This doesn't get initiated
            //app.Use(async (context, next) =>
            //{
            //    var botService = context.RequestServices.GetRequiredService<ITelegramBotService>();
            //    await botService.SetWebhookAsync(webHookUrl);

            //    await next.Invoke();
            //});

            app.Run();
        }


        public interface ICommandFactory
        {
            ICommand Create(string name);
        }

        public class CommandFactory : ICommandFactory
        {
            private readonly IServiceProvider _serviceProvider;

            public CommandFactory(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public ICommand Create(string name)
            {
                switch (name)
                {
                    case "/coins":
                        return _serviceProvider.GetRequiredService<GetCoinsCommand>();
                    default:
                        throw new ArgumentException("Unknown command name");
                }
            }
        }

    }
}