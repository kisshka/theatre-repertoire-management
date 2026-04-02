using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheatreManagement.Domain.Data;

namespace TheatreManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly DataContext _context;

        public EventsController(DataContext context)
        {
            _context = context;
        }
    }
}
