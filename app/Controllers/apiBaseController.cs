using apiBase.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace apiBase.Controllers
{
    [ApiController]
    [Route("api/v1/")]
    [Produces("application/json")]
    [EnableCors("AllowAll")]
    public class apiBaseController(ILogger<apiBaseController> logger) : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> ApiTest()
        {
            logger.LogInformation("apiBase is running");
            return Ok("apiBase is running");
        }
    }
}
