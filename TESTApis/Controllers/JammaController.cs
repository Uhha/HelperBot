using Microsoft.AspNetCore.Mvc;
using TESTApis.Interfaces;

namespace TESTApis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JammaController : ControllerBase
    {
        private readonly IJamma _jamma;

        public JammaController(IJamma jamma)
        {
            _jamma = jamma;
        }

        [HttpGet(Name = "GetJamma")]
        public ActionResult Get()
        { 
            _jamma.Do();
            return Ok();
        }
    }
}