using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheatreManagement.Domain.Data;
using TheatreManagement.Shared.DTOs;

namespace TheatreManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CastsController : ControllerBase
    {

        private readonly DataContext _context;

        public CastsController(DataContext context)
        {
            _context = context;
        }

        //[HttpGet("{id}")]
        //public async Task<ActionResult<PlayDto>> GetCasts(int id)
        //{
        //    var play = await _context.Plays.Include(p => p.RoleInPlays)
        //                                   .FirstOrDefaultAsync(p => p.PlayId == id);

        //    if (play == null)
        //    {
        //        return NotFound();
        //    }

        //    var playDto = new PlayDto
        //    {
        //        PlayId = play.PlayId,
        //        Name = play.Name,
        //        Duration = play.Duration,
        //        AgeCategory = play.AgeCategory,
        //        IsActive = play.IsActive,
        //        LastEditTime = play.LastEditTime,
        //    };

        //    var roles = play.RoleInPlays.Select(r =>
        //    new RoleDto
        //    {
        //        RoleInPlayId = r.RoleInPlayId,
        //        RoleType = r.Type,
        //        //Name = r.Name,
        //        LastEditTime = r.LastEditTime,
        //    }
        //    ).ToList();

        //    playDto.RoleDtos = roles;

        //    return playDto;
        //}
    }
}
