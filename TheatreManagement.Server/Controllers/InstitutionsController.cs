using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheatreManagement.Domain.Data;
using TheatreManagement.Domain.Entities;
using TheatreManagement.Shared.DTOs;
using TheatreManagement.Shared;
using TheatreManagement.Shared.DTOs.Events;

namespace TheatreManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstitutionsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly DataContext _context;


        public InstitutionsController(UserManager<User> userManager, DataContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<InstitutionDto>>> GetInstitutions(
         [FromQuery] string searchText = null,
         [FromQuery] int page = 1,
         [FromQuery] int pageSize = 10)
        {
            var query = _context.Institutions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var normalizeSearchText = searchText.Trim().ToLower();

                    query = _context.Institutions.Where(p =>
                    DataContext.CustomLike(p.Name, normalizeSearchText));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new InstitutionDto
                {
                    InstitutionId = p.InstitutionId,
                    Name = p.Name,
                    House = p.House,
                    Town = p.Town,
                    Street = p.Street,
                })
                .ToListAsync();

            return Ok(new PagedResult<InstitutionDto>
            {
                Items = items,
                TotalCount = totalCount
            });
        }


        [HttpGet("{institutionId}")]
        public async Task<ActionResult<InstitutionDto>> GetInstitution(int institutionId)
        {

            var institution = await _context.Institutions.FirstOrDefaultAsync(p => p.InstitutionId == institutionId);

            if (institution == null)
            {
                return NotFound();
            }

            var institutionDto = new InstitutionDto
            {
                InstitutionId = institution.InstitutionId,
                Name = institution.Name,
                Town = institution.Town,
                Street = institution.Street,
                House = institution.House,
            };

            return institutionDto;
        }

        [HttpPost]
        public async Task<IActionResult> PostInstitution(InstitutionDto institutionDto)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var institution = new Institution
            {
                Name = institutionDto.Name,
                Town = institutionDto.Town,
                Street = institutionDto.Street,
                House = institutionDto.House,
            };

            _context.Institutions.Add(institution);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> PutEmployee(InstitutionDto UnstitutionDto)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var institution = await _context.Institutions.Where(e => e.InstitutionId == UnstitutionDto.InstitutionId)
                                             .FirstOrDefaultAsync();

            institution.Name = UnstitutionDto.Name;
            institution.Street = UnstitutionDto.Street;
            institution.House = UnstitutionDto.House;
            institution.Town = UnstitutionDto.Town;

            await _context.SaveChangesAsync();

            return Ok();
        }

        //[HttpPut("{employeeId}/soft-delete")]
        //public async Task<IActionResult> SoftDeleteEmployee(int employeeId)
        //{
        //    var currentUser = await _userManager.GetUserAsync(User);

        //    if (currentUser == null)
        //    {
        //        return Unauthorized("Пользователь не авторизован");
        //    }
        //    var employee = await _context.Employees.Where(e => e.EmployeeId == employeeId)
        //                                     .FirstOrDefaultAsync();

        //    employee.IsActive = false;
        //    employee.User = currentUser;
        //    employee.DeletionTime = DateTime.Now;


        //    await _context.SaveChangesAsync();

        //    return Ok();
        //}
    }
}
