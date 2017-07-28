using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Logic;
using Telegram.Bot.Types;
using Logic.Handlers;

namespace Web.Controllers
{
    public class MessageController : ApiController
    {
        [Route(@"api/command")]
        public OkResult Post([FromBody]Update update)
        {
            Task.Run(() => new MessageHandler().Handle(update));
            return Ok();
        }

        [HttpGet]
        [Route(@"api/comicUpdate")]
        public OkResult ComicUpdate()
        {
            Task.Run(() => new WorkerHandler().HandleComicAsync());
            return Ok();
        }

        [HttpGet]
        [Route(@"api/coinUpdate")]
        public OkResult CoinUpdate([FromUri]string sendAnyway = "false")
        {
            bool.TryParse(sendAnyway, out bool result);
            Task.Run(() => new WorkerHandler().HandleCoinAsync(result));
            return Ok();
        }
    }
}
