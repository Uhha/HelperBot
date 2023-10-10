
using Autofac.Extensions.DependencyInjection;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using BotApi.Services;
using BotApi.Interfaces;

namespace BotApi
{
    public class Program
    {
       // public static void Main(string[] args)
       // {
       //     CreateHostBuilder(args).Build().Run();
       // }

       // public static IHostBuilder CreateHostBuilder(string[] args) =>
       //Host.CreateDefaultBuilder(args)
       //    .ConfigureAppConfiguration((hostingContext, config) =>
       //    {
       //        config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
       //              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
       //              .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true)
       //              .AddEnvironmentVariables()
       //              ;

               
       //    })
       //    .ConfigureServices((hostContext, services) =>
       //    {
       //        // Add configuration settings to the services
       //        services.Configure<AppSettings>(hostContext.Configuration.GetSection("TelegramBot"));
       //        services.AddSingleton(sp => sp.GetRequiredService<IOptions<AppSettings>>().Value);

       //        // Add other services
       //        services.AddHttpClient();
       //        services.AddControllers();
       //        services.AddEndpointsApiExplorer();
       //        services.AddSwaggerGen();

       //        if (hostContext.HostingEnvironment.IsDevelopment())
       //        {
       //            hostContext.Configuration.UseSwagger();
       //            app.UseSwaggerUI();
       //        }
       //    });

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

           // AutoFacRegistration.Register(builder.Services);


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);


            // Register your bot class with Autofac
            var builder = new ContainerBuilder();

            // Retrieve the bot token from configuration
            string botToken = Configuration["BotToken"];
            string webHookUrl = Configuration["WebHookUrl"];

            builder.RegisterType<Bot>()
                   .As<IBot>()
                   .WithParameter("botToken", botToken)
                   .WithParameter("webHookUrl", webHookUrl);

            builder.RegisterType<QBitService>()
                  .As<IQBitService>()
                  .WithParameter("url", Configuration["QBUrl"]);

            builder.RegisterType<WebhookService>().As<IWebhookService>();
            builder.RegisterType<CommandProcessingService>().As<ICommandProcessingService>();
            

            builder.Populate(services);

            var container = builder.Build();

            return new AutofacServiceProvider(container);
        }

    }
}