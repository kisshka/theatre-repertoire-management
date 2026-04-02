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

        [HttpPost]
        public async Task<IActionResult> CreateCast(CastDto castDto)
        {
            var cast = new Cast
            {
                Name = castDto.Name,
                PlayId = castDto.PlayId,
                LastEditTime = DateTime.UtcNow,
                DeletionTime = null,
            };

            var employeeRoles = new List<EmployeeRole>();

            // Создание связей с ассоциативной таблицей
            foreach (var employeeRoleDto in castDto.EmployeeRolesCreate)
            {
                var employee = await _context.Employees
                    .FindAsync(employeeRoleDto.EmployeeId);
                if (employee == null)
                    return BadRequest($"Сотрудник {employeeRoleDto.EmployeeId} не найден");

                var roleInPlay = await _context.RoleInPlays
                    .FindAsync(employeeRoleDto.RoleInPlayId);
                if (roleInPlay == null)
                    return BadRequest($"Роль {employeeRoleDto.RoleInPlayId} не найдена");


                var employeeRole = new EmployeeRole
                {
                    Cast = cast,
                    Employee = employee,
                    RoleInPlay = roleInPlay
                };

                employeeRoles.Add(employeeRole);
            }

            cast.EmployeeRoles = employeeRoles;

            _context.Castes.Add(cast);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{playId}")]
        public async Task<ActionResult<List<CastDto>>> GetCasts(int playId)
        {
            var casts = await _context.Castes.Where(c => c.PlayId == playId)
                                             .Select(c => new CastDto
                                             {
                                                 CastId = c.CastId,
                                                 Name = c.Name,
                                             })
                                             .ToListAsync();
            return casts;
        }
         

        [HttpGet("{castId}/employeeroles")]
        public async Task<ActionResult<CastWithRolesDto>> GetCastEmployeeRoles(int castId)
        {
            var cast = await _context.Castes
                .Where(c => c.CastId == castId)
                .Select(c => new CastWithRolesDto
                {
                    CastId = c.CastId,
                    Name = c.Name,
                    LastEditTime = c.LastEditTime,
                    Roles = new List<RoleGroupDto>()
                })
                .FirstOrDefaultAsync();

            if (cast == null) return NotFound();


            var employeeRoles = await _context.EmployeeRoles
                .Where(er => er.CastId == castId)
                .Include(er => er.Employee)
                .Include(er => er.RoleInPlay)
                .ToListAsync();

            // Сотрудники сгруппированы по ролям
            cast.Roles = employeeRoles
                .GroupBy(er => er.RoleInPlayId)
                .Select(g => new RoleGroupDto
                {
                    RoleInPlayId = g.Key,
                    RoleName = g.First().RoleInPlay.Name,
                    RoleType = g.First().RoleInPlay.Type,
                    Employees = g.Select(er => new EmployeeDto
                    {
                        EmployeeId = er.EmployeeId,
                        Surname = er.Employee.Surname,
                        Name = er.Employee.Name,
                        FatherName = er.Employee.FatherName,
                        Post = er.Employee.Post
                    }).ToList()
                })
                .ToList();

            return cast;
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCast(CastDto updateCastDto)
        {
            var cast = await _context.Castes
                .Include(c => c.EmployeeRoles)
                .FirstOrDefaultAsync(c => c.CastId == updateCastDto.CastId);

            if (cast == null)
                return NotFound($"Каст {updateCastDto.CastId} не найден");

            // Изменение полей каста
            cast.Name = updateCastDto.Name;
            cast.LastEditTime = DateTime.Now;


            _context.EmployeeRoles.RemoveRange(cast.EmployeeRoles);

            // Создание связей сотрудников и ролей
            var newEmployeeRoles = new List<EmployeeRole>();

            foreach (var employeeRoleDto in updateCastDto.EmployeeRolesCreate)
            {
                var employee = await _context.Employees
                    .FindAsync(employeeRoleDto.EmployeeId);
                if (employee == null)
                    return BadRequest($"Сотрудник {employeeRoleDto.EmployeeId} не найден");

                var roleInPlay = await _context.RoleInPlays
                    .FindAsync(employeeRoleDto.RoleInPlayId);
                if (roleInPlay == null)
                    return BadRequest($"Роль {employeeRoleDto.RoleInPlayId} не найдена");

                var employeeRole = new EmployeeRole
                {
                    Cast = cast,
                    Employee = employee,
                    RoleInPlay = roleInPlay
                };

                newEmployeeRoles.Add(employeeRole);
            }

            cast.EmployeeRoles = newEmployeeRoles;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{castId}/soft-delete")]
        public async Task<IActionResult> SoftDeleteCast(int castId)
        {
            var cast = await _context.Castes
                .FirstOrDefaultAsync(c => c.CastId == castId);

            cast.DeletionTime = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
