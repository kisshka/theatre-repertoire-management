using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheatreManagement.Domain.Data;
using TheatreManagement.Domain.Entities;
using TheatreManagement.Shared.Helpers;

namespace TheatreManagement.Server.Controllers.TypesControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InstitutionTypesController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly DataContext _context;

        public InstitutionTypesController(UserManager<User> userManager, DataContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Guide>>> GetInstitutionTypes()
        {
            var institutionTypes = await _context.InstitutionTypes
                .Include(it => it.Institutions)
                .OrderBy(it => it.Name)
                .Select(it => new Guide
                {
                    Id = it.InstitutionTypeId,
                    Name = it.Name,
                    IsUsed = it.Institutions.Count() > 0
                }).ToListAsync();

            return Ok(institutionTypes);
        }

        [HttpPost]
        public async Task<ActionResult> AddInstitutionType([FromBody] Guide request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Название не может быть пустым");

            var exists = await _context.InstitutionTypes.AnyAsync(it => it.Name == request.Name);
            if (exists)
                return Conflict("Тип учреждения с таким названием уже существует");

            var institutionType = new InstitutionType { Name = request.Name };
            _context.InstitutionTypes.Add(institutionType);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateInstitutionType(int id, [FromBody] Guide request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Название не может быть пустым");

            var institutionType = await _context.InstitutionTypes.FindAsync(id);
            if (institutionType == null)
                return NotFound("Тип учреждения не найден");

            var exists = await _context.InstitutionTypes.AnyAsync(it => it.Name == request.Name && it.InstitutionTypeId != id);
            if (exists)
                return Conflict("Тип учреждения с таким названием уже существует");

            institutionType.Name = request.Name;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteInstitutionType(int id)
        {
            var institutionType = await _context.InstitutionTypes.FindAsync(id);
            if (institutionType == null)
                return NotFound("Тип учреждения не найден");

            var isUsed = await _context.Institutions.AnyAsync(i => i.InstitutionTypeId == id);
            if (isUsed)
                return BadRequest("Нельзя удалить тип учреждения, который используется в одном или более учреждениях");

            _context.InstitutionTypes.Remove(institutionType);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}