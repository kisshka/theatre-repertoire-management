using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheatreManagement.Domain.Data;
using TheatreManagement.Domain.Entities;
using TheatreManagement.Server.Mappings;
using TheatreManagement.Shared.DTOs;
using TheatreManagement.Shared.DTOs.Events;
using TheatreManagement.Shared.Helpers;

namespace TheatreManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
            var query = _context.Institutions
                .Include(i => i.Type)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var normalizeSearchText = searchText.Trim().ToLower();
                query = query.Where(p => DataContext.CustomLike(p.Name, normalizeSearchText));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(InstitutionMappings.ToInstitutionDto)
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
            var institution = await _context.Institutions
                .Include(i => i.Type)
                .Where(i => i.InstitutionId == institutionId)
                .Select(InstitutionMappings.ToInstitutionDto)
                .FirstOrDefaultAsync();

            if (institution == null)
            {
                return NotFound();
            }
            return Ok(institution);
        }

        [HttpPost]
        public async Task<IActionResult> PostInstitution(InstitutionDto institutionDto)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            // Находим тип учреждения
            InstitutionType institutionType = null;
            if (institutionDto.InstitutionTypeId > 0)
            {
                institutionType = await _context.InstitutionTypes
                    .FirstOrDefaultAsync(it => it.InstitutionTypeId == institutionDto.InstitutionTypeId);

                if (institutionType == null)
                {
                    return BadRequest($"Тип учреждения с ID {institutionDto.InstitutionTypeId} не найден");
                }
            }

            var institution = new Institution
            {
                Name = institutionDto.Name,
                Town = institutionDto.Town,
                Street = institutionDto.Street,
                House = institutionDto.House,
                PhoneNumber = institutionDto.PhoneNumber,
                Comment = institutionDto.Comment,
                Type = institutionType,
                InstitutionTypeId = institutionDto.InstitutionTypeId
            };

            _context.Institutions.Add(institution);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> PutInstitution(InstitutionDto institutionDto)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var institution = await _context.Institutions
                .Include(i => i.Type)
                .FirstOrDefaultAsync(i => i.InstitutionId == institutionDto.InstitutionId);

            if (institution == null)
            {
                return NotFound("Учреждение не найдено");
            }

            // Обновляем тип учреждения, если изменился
            if (institutionDto.InstitutionTypeId != institution.InstitutionTypeId)
            {
                var institutionType = await _context.InstitutionTypes
                    .FirstOrDefaultAsync(it => it.InstitutionTypeId == institutionDto.InstitutionTypeId);

                if (institutionType == null && institutionDto.InstitutionTypeId > 0)
                {
                    return BadRequest($"Тип учреждения с ID {institutionDto.InstitutionTypeId} не найден");
                }

                institution.Type = institutionType;
                institution.InstitutionTypeId = institutionDto.InstitutionTypeId;
            }

            institution.Name = institutionDto.Name;
            institution.Town = institutionDto.Town;
            institution.Street = institutionDto.Street;
            institution.House = institutionDto.House;
            institution.PhoneNumber = institutionDto.PhoneNumber;
            institution.Comment = institutionDto.Comment;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<InstitutionDto>>> SearchInstitutions([FromQuery] string searchText = null)
        {
            var query = _context.Institutions
                .Include(i => i.Type)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText) && searchText != string.Empty)
            {
                query = query.Where(i => DataContext.CustomLike(i.Name, searchText));
            }

            var institutions = await query
                .Select(InstitutionMappings.ToInstitutionDto)
                .ToListAsync();

            return Ok(institutions);
        }

        [HttpGet("by-name")]
        public async Task<ActionResult<InstitutionDto>> GetInstitutionByName([FromQuery] string name)
        {
            var institution = await _context.Institutions
                .Include(i => i.Type)
                .Where(i => i.Name == name)
                .Select(InstitutionMappings.ToInstitutionDto)
                .FirstOrDefaultAsync();

            return Ok(institution);
        }

        // Новый метод для получения всех типов учреждений
        [HttpGet("institution-types")]
        public async Task<ActionResult<List<Guide>>> GetInstitutionTypes()
        {
            var institutionTypes = await _context.InstitutionTypes
                .OrderBy(it => it.Name)
                .Select(it => new Guide
                {
                    Id = it.InstitutionTypeId,
                    Name = it.Name
                })
                .ToListAsync();

            return Ok(institutionTypes);
        }
    }
}