using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Logic;
using Telegram.Bot.Types;
using Logic.Handlers;
using Microsoft.Ajax.Utilities;
using System.Data.Entity.Core.Objects;
using Tracer;
using System;

namespace Web.Controllers
{
    public class MessageController : ApiController
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
        public OkResult CoinUpdate([FromUri]string sendAnyway = "false")
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
      
    }
}
