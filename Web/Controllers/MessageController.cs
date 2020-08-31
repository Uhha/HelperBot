using System.Threading.Tasks;
using Logic;
using Telegram.Bot.Types;
using Logic.Handlers;
using Tracer;
using System;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class MessageController : Controller
    {
        [Route(@"api/command")]
        public OkResult Post([FromBody]Update update)
        {
            try
            {
                if (update?.Message?.Text != null && update.Message.Text.StartsWith("/"))
                {
                    TraceError.Info("api/command called with: " + update?.Message?.Text);
                }
                Task.Run(() => new MessageHandler().Handle(update));
                return Ok();
            }
            catch (System.Exception e)
            {
                TraceError.Error(e, "Upper level Exception");
                return Ok();
            }
        }

        [HttpGet]
        [Route(@"api/comicUpdate")]
        public OkResult ComicUpdate()
        {
            TraceError.Info("api/comicUpdate called");
            try
            {
                Task.Run(() => new WorkerHandler().HandleComicAsync());
            }
            catch (Exception e)
            {
                TraceError.Error(e, "Upper level Exception");
                return Ok();
            }
            return Ok();
        }

        [HttpGet]
        [Route(@"api/coinUpdate")]
        public OkResult CoinUpdate([FromRoute] string sendAnyway = "false")
        {
            TraceError.Info("api/CoinUpdate called");
            try
            {
                Task.Run(() => new WorkerHandler().HandleCoinAsync(sendAnyway));
            }
            catch (Exception e)
            {
                TraceError.Error(e, "Upper level Exception");
                return Ok();
            }
            return Ok();
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        [HttpPost]
        [Route(@"api/sendErrorMessageToBot")]
        public OkResult SendErrorMessageToBot([FromRoute] string errormsg)
        {
            TraceError.Info("api/SendErrorMessageToBot");
            try
            {
                Task.Run(() => new WorkerHandler().SendErrorMessageToBot(errormsg));
            }
            catch (Exception e)
            {
                TraceError.Error(e, "Upper level Exception");
                return Ok();
            }
            return Ok();
        }
    }
}
