using Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace Web.Controllers
{
    public class OglafController : ApiController
    {
        [HttpGet]
        [Route(@"api/oglaf")]
        public OkResult Check()
        {
            Task.Run(() => new Handler().Handle());
            return Ok();
        }

    }
}
