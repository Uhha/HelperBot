using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Logic.Modules;
using System.Reflection;
using Logic;

namespace DBCallsNetTest
{
    [TestClass]
    public class LogicOglafTest
    {
        [TestMethod]
        public void OglafTest()
        {


            //var bot = Bot.Get();
        
            var module = new ComicModule();
            var dddd = module.GenerateAndSendWorkerAsync(null);

            ////PrivateObject obj = new PrivateObject(module);
            ////var retVal = obj.Invoke("GetOglafPicture");

            //MethodInfo methodInfo = typeof(ComicModule).GetMethod("GetOglafPicture", BindingFlags.NonPublic | BindingFlags.Instance);
            //object[] parameters = { "1" };
            //methodInfo.Invoke(module, parameters);

        }
    }
}
