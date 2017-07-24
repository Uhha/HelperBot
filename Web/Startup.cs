using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using System.Threading;

[assembly: OwinStartup(typeof(Web.Startup))]

namespace Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            if (Logic.Config.Environment == "Debug")
            {
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    NoHookLoop.StartAsync();
                }).Start();
            }
        }

       
    }
}
