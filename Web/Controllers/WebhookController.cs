using System.Threading.Tasks;
using Logic;
using Telegram.Bot.Types;
using Logic.Handlers;
using Tracer;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using DatabaseInteractions;
using Telegram.Bot;
using QBittorrent.Client;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {

            if (update?.Message?.Text != null && update.Message.Text.StartsWith("/qb"))
            {
                QBittorrentClient qc = new QBittorrentClient(new Uri("http://localhost:8899/"));
                var search = await qc.StartSearchAsync("Inception");
                await Bot.Get().SendTextMessageAsync(update.Message?.Chat?.Id, $"qb search {search} started");
            }


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
                await new MessageHandler().Handle(update);
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
        public OkResult CoinUpdate([FromQuery] string sendAnyway = "false")
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
        public OkResult SendErrorMessageToBot([FromQuery] string errormsg)
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

        [HttpGet]
        [Route(@"api/cuTest")]
        public OkResult CUTest()
        {
            try
            {
                using (var db = new BotDBContext())
                {
                    db.Subscriptions.Add(new DatabaseInteractions.Subscription() { LastPostedKey = "", SubsctiptionType = 7 });
                    db.SaveChanges();
                }

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
