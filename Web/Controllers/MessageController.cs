using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Logic;
using Telegram.Bot.Types;
using Logic.Handlers;
using NLog;
using Microsoft.Ajax.Utilities;
using System.Data.Entity.Core.Objects;

namespace Web.Controllers
{
    public class MessageController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        [Route(@"api/command")]
        public OkResult Post([FromBody]Update update)
        {
            try
            {
                _logger.Info("api/command called with: " + update?.Message?.Text);
                Task.Run(() => new MessageHandler().Handle(update));
                return Ok();
            }
            catch (System.Exception e)
            {
                _logger.Error("Upper level Exception", e.Message + e.InnerException?.Message);
                return Ok();
            }
        }

        [HttpGet]
        [Route(@"api/comicUpdate")]
        public OkResult ComicUpdate()
        {
            _logger.Info("api/comicUpdate called");
            Task.Run(() => new WorkerHandler().HandleComicAsync());
            return Ok();
        }

        [HttpGet]
        [Route(@"api/coinUpdate")]
        public OkResult CoinUpdate([FromUri]string sendAnyway = "false")
        {
            _logger.Info("api/CoinUpdate called");
            Task.Run(() => new WorkerHandler().HandleCoinAsync(sendAnyway));
            return Ok();
        }
    }
}
