using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TheatreManagement.Domain.Data;
using TheatreManagement.Domain.Entities;
using TheatreManagement.Server.Mappings;
using TheatreManagement.Shared.DTOs;
using TheatreManagement.Shared.DTOs.Employees;
using TheatreManagement.Shared.DTOs.Events;
using TheatreManagement.Shared.Helpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TheatreManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly DataContext _context;


        public EmployeesController(UserManager<User> userManager, DataContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("Actors")]
        public async Task<ActionResult<List<EmployeeDto>>> GetActors()
        {
            var actors =  await _context.Employees.Where(e => e.Post == "Актер")
                                           .Select(e => new EmployeeDto
                                           {
                                               EmployeeId = e.EmployeeId,
                                               Surname = e.Surname,
                                               Name = e.Name,
                                               FatherName = e.FatherName,
                                               Post = e.Post,
                                           })
                                           .ToListAsync();
            return actors;
        }

        [HttpGet("Technics")]
        public async Task<ActionResult<List<EmployeeDto>>> GetTechics()
        {
            var techics = await _context.Employees.Where(e => e.Post == "Технический сотрудник")
                                           .Select(e => new EmployeeDto
                                           {
                                               EmployeeId = e.EmployeeId,
                                               Surname = e.Surname,
                                               Name = e.Name,
                                               FatherName = e.FatherName,
                                               Post = e.Post,
                                           })
                                           .ToListAsync();
            return techics;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<EmployeeDto>>> GetEmployees(
            [FromQuery] string searchText = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool isArchive = false)
        {

            var query = _context.Employees.AsQueryable();

            if (isArchive == true)
            {
                query = query.IgnoreQueryFilters().Where(e => e.DeletionTime != null);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var normalizeSearchText = searchText.Trim().ToLower();

                query = query.Where(e =>
                    DataContext.CustomLike(e.Name, normalizeSearchText));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(e => e.EmployeeId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EmployeeDto
                {
                    EmployeeId = e.EmployeeId,
                    Name = e.Name,
                    Surname = e.Surname,
                    FatherName = e.FatherName,
                    Post = e.Post,
                    IsActive = e.IsActive,
                    LastEditTime = e.LastEditTime,
                    DeletionTime = e.DeletionTime,
                    UserFullName = (e.User != null ? e.User.Surname + " " + e.User.Name + " " + e.User.FatherName : ""),
                })
                .ToListAsync();

            return Ok(new PagedResult<EmployeeDto>
            {
                Items = items,
                TotalCount = totalCount
            });
        }


        [HttpGet("{employeeId}")]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(int employeeId)
        {

            var employee = await _context.Employees.Include(e => e.User)
                                                    .FirstOrDefaultAsync(p => p.EmployeeId == employeeId);

            if (employee == null)
            {
                return NotFound();
            }

            var employeeDto = new EmployeeDto
            {
                EmployeeId = employee.EmployeeId,
                Name = employee.Name,
                Surname = employee.Surname,
                FatherName = employee.FatherName,
                Post = employee.Post,
                IsActive = employee.IsActive,
                LastEditTime = employee.LastEditTime,
                UserFullName = (employee.User != null ? employee.User.Surname + " " + employee.User.Name + " " + employee.User.FatherName : ""),
                IsUsed = await _context.EmployeeRoles.AnyAsync(er => er.EmployeeId == employeeId &&
                                                      ((er.Event != null && er.Event.DeletionTime == null) ||
                                                      (er.Cast != null && er.Cast.DeletionTime == null)))
            };

            return employeeDto;
        }


        [HttpGet("{employeeId}/casts")]
        public async Task<ActionResult<PagedResult<EmployeePlays>>> GetEmployeeCasts(int employeeId,
            [FromQuery] string searchText = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {

            var allPlayIdsQuery = _context.EmployeeRoles
                .Where(er => er.EmployeeId == employeeId && er.Cast != null && er.EventId == null)
                .Select(er => er.RoleInPlay.PlayId)
                .Distinct();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var normalizeSearchText = searchText.Trim().ToLower();
                var matchingPlayIds = _context.EmployeeRoles
                    .Where(er => er.EmployeeId == employeeId && er.Cast != null && er.EventId == null)
                    .Where(er =>
                        DataContext.CustomLike(er.Cast.Name, normalizeSearchText) ||
                        DataContext.CustomLike(er.Cast.Play.Name, normalizeSearchText) ||
                        DataContext.CustomLike(er.RoleInPlay.Name, normalizeSearchText))
                    .Select(er => er.RoleInPlay.PlayId)
                    .Distinct();

                // Выбираются подходящие под поиск PlayId
                allPlayIdsQuery = allPlayIdsQuery.Where(id => matchingPlayIds.Contains(id));
            }

            var totalCount = await allPlayIdsQuery.CountAsync();

            var pagedPlayIds = await allPlayIdsQuery
                .OrderBy(id => id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!pagedPlayIds.Any())
            {
                return Ok(new PagedResult<EmployeePlays>
                {
                    Items = new List<EmployeePlays>(),
                    TotalCount = totalCount
                });
            }

            var allData = await _context.EmployeeRoles
                .Where(er => er.EmployeeId == employeeId && er.Cast != null && er.EventId == null &&
                             pagedPlayIds.Contains(er.RoleInPlay.PlayId))
                .Select(er => new
                {
                    er.RoleInPlay.PlayId,
                    PlayName = er.RoleInPlay.Play.Name,
                    er.CastId,
                    CastName = er.Cast.Name,
                    RoleName = er.RoleInPlay.Name
                })
                .ToListAsync();

            var result = allData
                .GroupBy(x => x.PlayId)
                .Select(g => new EmployeePlays
                {
                    PlayId = g.Key,
                    PlayName = g.First().PlayName,
                    Casts = g
                        .GroupBy(x => new { x.CastId, x.CastName })
                        .Select(cg => new EmployeeCasts
                        {
                            CastId = cg.Key.CastId,
                            CastName = cg.Key.CastName,
                            Roles = cg.Select(x => x.RoleName).Distinct().ToList()
                        })
                        .ToList()
                })
                .ToList();

            return Ok(new PagedResult<EmployeePlays>
            {
                Items = result,
                TotalCount = totalCount
            });
        }


        [HttpGet("{employeeId}/events")]
        public async Task<ActionResult<List<EventGetModel>>> GetEmployeeEvents(int employeeId, [FromQuery] string start, [FromQuery] string end)
        {
            if (!DateTime.TryParseExact(start, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate) ||
                !DateTime.TryParseExact(end, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
            {
                return BadRequest("Неверный формат даты");
            }

            var employeeRoles = _context.EmployeeRoles
                          .Where(er => er.EmployeeId == employeeId && er.EventId != null).AsQueryable();

            var events = employeeRoles.Where(e => e.Event.StartTime < endDate.AddDays(1) && e.Event.EndTime > startDate)
                                      .Select(e => new EventGetModel
            {
                EventId = e.Event.EventId,
                StartTime = e.Event.StartTime,
                EndTime = e.Event.EndTime,
                Type = e.Event.Type,
                LastEditTime = e.Event.LastEditTime,
                CancellationReason = e.Event.CancellationReason,
                Plays = e.Event.PlayEvents.Select(p => new PlayDto { Name = p.Play.Name }).ToList(),
                //Stationar = e.Event.Stationar != null ? new StationarDto { Hall = e.Event.Stationar.Hall } : null,

            }).ToList();

            return events;
        }

        [HttpGet("{employeeId}/events/preview")]
        public async Task<ActionResult<PagedResult<EventGetModel>>> GetEmployeeEventsPreview(
            int employeeId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 3)
        {
            var query = _context.EmployeeRoles
                .Where(er => er.EmployeeId == employeeId && er.EventId != null)
                .Select(er => er.Event)
                .Where(e => e != null)
                .OrderByDescending(e => e.LastEditTime);

            var totalCount = await query.CountAsync();

            var events = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(EventMappings.ToEventGetModelBasic!)
                .ToListAsync();

            return Ok(new PagedResult<EventGetModel>
            {
                Items = events,
                TotalCount = totalCount
            });
        }


        [HttpPost]
        public async Task<IActionResult> PostEmployee(EmployeeDto employeeDto)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var employee = new Employee
            {
                Name = employeeDto.Name,
                Surname = employeeDto.Surname,
                FatherName = employeeDto.FatherName,
                Post = employeeDto.Post,
                IsActive = true,
                User = currentUser,

                LastEditTime = DateTime.Now,
                DeletionTime = null
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> PutEmployee(EmployeeDto employeeDto)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var employee = await _context.Employees.Where(e => e.EmployeeId == employeeDto.EmployeeId)
                                             .FirstOrDefaultAsync();

            employee.Surname = employeeDto.Surname;
            employee.Name = employeeDto.Name;
            employee.FatherName = employeeDto.FatherName;
            employee.Post = employeeDto.Post;
            employee.IsActive = employeeDto.IsActive;
            employee.LastEditTime = DateTime.Now;

            employee.User = currentUser;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{employeeId}/soft-delete")]
        public async Task<IActionResult> SoftDeleteEmployee(int employeeId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }
            var employee = await _context.Employees.Where(e => e.EmployeeId == employeeId)
                                             .FirstOrDefaultAsync();

            employee.IsActive = false;
            employee.User = currentUser;
            employee.DeletionTime = DateTime.Now;


            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{employeeId}/restore")]
        public async Task<IActionResult> RestoreEmployee(int employeeId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }
            var employee = await _context.Employees.Where(e => e.EmployeeId == employeeId)
                                                   .IgnoreQueryFilters()
                                                   .FirstOrDefaultAsync();

            employee.IsActive = true;
            employee.User = currentUser;
            employee.LastEditTime = DateTime.Now;
            employee.DeletionTime = null;

            await _context.SaveChangesAsync();
            return Ok();
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }
    }
}
