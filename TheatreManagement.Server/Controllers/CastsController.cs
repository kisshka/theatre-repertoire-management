using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheatreManagement.Domain.Data;
using TheatreManagement.Domain.Entities;
using TheatreManagement.Shared.DTOs;
using TheatreManagement.Shared.DTOs.Employees;

namespace TheatreManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CastsController : ControllerBase
    {

        private readonly UserManager<User> _userManager;
        private readonly DataContext _context;


        public CastsController(UserManager<User> userManager, DataContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCast(CastDto castDto)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var cast = new Cast
            {
                Name = castDto.Name,
                PlayId = castDto.PlayId,
                LastEditTime = DateTime.UtcNow,
                DeletionTime = null,
                User = currentUser
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
                .Include(c => c.User)
                .Select(c => new CastWithRolesDto
                {
                    CastId = c.CastId,
                    Name = c.Name,
                    PlayId = c.PlayId,
                    LastEditTime = c.LastEditTime,
                    UserFullName = (c.User != null ? c.User.Surname + " " + c.User.Name + " " + c.User.FatherName : ""),
                    Roles = new List<RoleGroupDto>()
                })
                .FirstOrDefaultAsync();

            if (cast == null) return NotFound();

            // Получаем все роли спектакля
            var allRoles = await _context.RoleInPlays
                .Where(r => r.PlayId == cast.PlayId) // Нужно добавить PlayId в CastWithRolesDto
                .ToListAsync();

            // Получаем назначенных сотрудников для этого состава
            var employeeRoles = await _context.EmployeeRoles
                .Where(er => er.CastId == castId && er.EventId == null)
                .Include(er => er.Employee)
                .Include(er => er.RoleInPlay)
                .ToListAsync();

            // Формируем результат для всех ролей
            cast.Roles = allRoles
                .Select(role => new RoleGroupDto
                {
                    RoleInPlayId = role.RoleInPlayId,
                    RoleName = role.Name,
                    RoleType = role.Type,
                    Employees = employeeRoles
                        .Where(er => er.RoleInPlayId == role.RoleInPlayId)
                        .Select(er => new EmployeeDto
                        {
                            EmployeeId = er.EmployeeId,
                            Surname = er.Employee.Surname,
                            Name = er.Employee.Name,
                            FatherName = er.Employee.FatherName,
                            Post = er.Employee.Post
                        })
                        .ToList()
                })
                .ToList();

            return Ok(cast);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateCast(CastDto updateCastDto)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var cast = await _context.Castes
                .Include(c => c.EmployeeRoles)
                .FirstOrDefaultAsync(c => c.CastId == updateCastDto.CastId);

            if (cast == null)
                return NotFound($"Каст {updateCastDto.CastId} не найден");

            // Изменение полей каста
            cast.Name = updateCastDto.Name;
            cast.LastEditTime = DateTime.Now;
            cast.User = currentUser;


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
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }
            var cast = await _context.Castes
                .FirstOrDefaultAsync(c => c.CastId == castId);

            cast.DeletionTime = DateTime.Now;
            cast.User = currentUser;

            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
