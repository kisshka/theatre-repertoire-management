using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheatreManagement.Domain.Data;
using TheatreManagement.Domain.Entities;
using TheatreManagement.Shared.DTOs;
using TheatreManagement.Shared;

namespace TheatreManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly DataContext _context;

        public EmployeesController(DataContext context)
        {
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
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Employees
                .Where(p => p.IsActive && p.DeletionTime == null);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var normalizeSearchText = searchText.Trim().ToLower();

                query = query.Where(p =>
                    DataContext.CustomLike(p.Name, normalizeSearchText));

            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.EmployeeId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new EmployeeDto
                {
                    EmployeeId = p.EmployeeId,
                    Name = p.Name,
                    Surname = p.Surname,
                    FatherName = p.FatherName,
                    Post = p.Post,
                    IsActive = p.IsActive,
                    LastEditTime = p.LastEditTime
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
            var employee = await _context.Employees.FirstOrDefaultAsync(p => p.EmployeeId == employeeId);

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
                LastEditTime = employee.LastEditTime
            };

            return employeeDto;
        }

        [HttpPost]
        public async Task<IActionResult> PostEmployee(EmployeeDto employeeDto)
        {

            var employee = new Employee
            {
                Name = employeeDto.Name,
                Surname = employeeDto.Surname,
                FatherName = employeeDto.FatherName,
                Post = employeeDto.Post,
                IsActive = true,

                LastEditTime = DateTime.UtcNow,
                DeletionTime = null
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> PutEmployee(EmployeeDto employeeDto)
        {

            var employee = await _context.Employees.Where(e => e.EmployeeId == employeeDto.EmployeeId)
                                             .FirstOrDefaultAsync();

            employee.Surname = employeeDto.Surname;
            employee.Name = employeeDto.Name;
            employee.FatherName = employeeDto.FatherName;
            employee.Post = employeeDto.Post;
            employee.IsActive = employeeDto.IsActive;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{employeeId}/soft-delete")]
        public async Task<IActionResult> SoftDeleteEmployee(int employeeId)
        {
            var employee = await _context.Employees.Where(e => e.EmployeeId == employeeId)
                                             .FirstOrDefaultAsync();

            employee.IsActive = false;
            employee.DeletionTime = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }
    }
}
