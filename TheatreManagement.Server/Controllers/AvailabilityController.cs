using Microsoft.AspNetCore.Mvc;
using TheatreManagement.Server.Services;
using TheatreManagement.Shared;
using TheatreManagement.Shared.DTOs.Availability;

namespace TheatreManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly AvailabilityService _availabilityService;

        public AvailabilityController(AvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        [HttpGet("plays")]
        public async Task<ActionResult<PagedResult<PlayAvailabilityDto>>> GetPlaysAvailability(
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchText = null)
        {
            var result = await _availabilityService.GetPlaysAvailabilityAsync(
                startTime, endTime, page, pageSize, searchText);
            return Ok(result);
        }
    }
}