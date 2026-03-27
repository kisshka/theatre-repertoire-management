using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheatreManagement.Domain.Data;
using TheatreManagement.Domain.Entities;
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

        //[HttpGet("{Playid}")]
        //public async Task<ActionResult<List<CastDto>> GetCasts(int Playid)
        //{
        //    var play = await _context.Plays.Include(p => p.)
        //                                   .FirstOrDefaultAsync(p => p.PlayId == Playid);

        //    if (play == null)
        //    {
        //        return NotFound();
        //    }

        //    var casts 

        //    return playDto;
        //}

        [HttpPost("plays/{playId}/casts")]
        public async Task<IActionResult> CreateCast(int playId, CastDto castDto)
        {
            var play = await _context.Plays
                .Include(p => p.RoleInPlays)
                .FirstOrDefaultAsync(p => p.PlayId == playId);

            if (play == null)
                return NotFound($"Play with ID {playId} not found");

            var cast = new Cast
            {
                Name = castDto.Name,
                LastEditTime = DateTime.UtcNow,
                DeletionTime = null,
            };

            var employeeRoles = new List<EmployeeRole>();

            foreach (var employeeRoleDto in castDto.EmployeeRolesCreate)
            {
                var employee = await _context.Employees
                    .FindAsync(employeeRoleDto.EmployeeId);
                if (employee == null)
                    return BadRequest($"Employee with ID {employeeRoleDto.EmployeeId} not found");

                var roleInPlay = await _context.RoleInPlays
                    .FindAsync(employeeRoleDto.RoleInPlayId);
                if (roleInPlay == null)
                    return BadRequest($"RoleInPlay with ID {employeeRoleDto.RoleInPlayId} not found");


                var employeeRole = new EmployeeRole
                {
                    Cast = cast,
                    EmployeeId = employeeRoleDto.EmployeeId,
                    RoleInPlayId = employeeRoleDto.RoleInPlayId,
                    UserId = employeeRoleDto.UserId,
                    LastEditTime = DateTime.UtcNow
                };

                employeeRoles.Add(employeeRole);
            }

            // Сохраняем в базу
            cast.EmployeeRoles = employeeRoles;
            _context.Castes.Add(cast);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
