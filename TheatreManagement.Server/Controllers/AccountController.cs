using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TheatreManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        [HttpGet]
        public IActionResult Welcome()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Ok("ЧТО ЭТОТ ДИДИБЛАД ДЕЛАЕТ НА КАЛЬКУЛЯТОРК");
            }
      
            return Ok("Вы вошли");
        }
    }
}
