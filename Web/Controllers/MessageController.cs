using System.Threading.Tasks;
using Logic;
using Telegram.Bot.Types;
using Logic.Handlers;
using Tracer;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Web.Services;

namespace Web.Controllers
{
    
    public class MessageController : Controller
    {
        private IBotService _botService;
        public MessageController(IBotService botservice)
        {
            _botService = botservice;
        }



        [HttpPost]
        [Route(@"api/command")]
        public async Task<IActionResult> Post([FromBody]Update update)
        {
            TraceError.Info("Called Trace Error from api/command");

            if (update == null)
            {
                TraceError.Info($"Update is null");
            }
            if (update?.Message == null)
            {
                TraceError.Info($"Message is null");
            }
            if (update?.Message?.Text == null)
            {
                TraceError.Info($"Text is null");
            }
            TraceError.Info("Update text - " + update?.Message?.Text);

            try
            {
                if (update?.Message?.Text != null && update.Message.Text.StartsWith("/"))
                {
                    TraceError.Info("api/command called with: " + update?.Message?.Text);
                }
                await new MessageHandler().Handle(_botService.Bot, update);
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
