using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.Sources.Clear();

                    var env = hostingContext.HostingEnvironment;

                    config
                          //.Add(new WebConfigSource() { Path = "web.config", Optional = false, ReloadOnChange = true, })
                          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                          //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                          ;


                    //var builder = new ConfigurationBuilder()
                    //    .SetBasePath(env.ContentRootPath)
                    //    .Add(new WebConfigSource() { Path = "web.config", Optional = false, ReloadOnChange = true, })
                    //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    //    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                    //    .AddEnvironmentVariables();
                    //                Configuration = builder.Build();



                    //config.AddJsonFile("MyConfig.json", optional: true, reloadOnChange: true)
                    //      .AddJsonFile($"MyConfig.{env.EnvironmentName}.json",
                    //                     optional: true, reloadOnChange: true);

                    config.AddEnvironmentVariables();

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });


        
    }

    //public class WebConfigSource : FileConfigurationSource
    //{
    //    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    //    {
    //        FileProvider = FileProvider ?? builder.GetFileProvider();
    //        return new WebConfigConfigurationProvider(this);
    //    }

    //    public class WebConfigConfigurationProvider : FileConfigurationProvider
    //    {
    //        public WebConfigConfigurationProvider(WebConfigSource source) : base(source) { }

    //        public override void Load(Stream stream)
    //        {
    //            Data = XDocument.Load(stream).Element("configuration").Element("appSettings")
    //                .Elements("add").ToDictionary(_ => "webconfig:" + _.Attribute("key").Value.Replace(".", string.Empty), _ => _.Attribute("value").Value);
    //        }
    //    }
    //}
}
