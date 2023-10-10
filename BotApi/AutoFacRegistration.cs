using Autofac;
using Autofac.Extensions.DependencyInjection;
using BotApi.Interfaces;
using BotApi.Services;

namespace BotApi
{
    public class AutoFacRegistration
    {
        public static void Register(IServiceCollection services)
        {
            //var builder = new ContainerBuilder();

            //builder.RegisterType<Bot>()
            //   .As<IBot>()
            //   .WithParameter("botToken", "YOUR_BOT_TOKEN"); // Replace with your actual bot token


            ////builder.RegisterType<Class>().As<IClass>();
            //builder.Populate(services);
            //builder.Build();
        }

    }
}
