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
    public class HallTypesController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly DataContext _context;

        public HallTypesController(UserManager<User> userManager, DataContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Guide>>> GetHallTypes()
        {
            var hallTypes = await _context.HallTypes
                .Include(ht => ht.Stationars)
                .OrderBy(ht => ht.Name)
                .Select(ht => new Guide
                {
                    Id = ht.HallTypeId,
                    Name = ht.Name,
                    IsUsed = ht.Stationars.Count() > 0
                }).ToListAsync();

            return Ok(hallTypes);
        }

        [HttpPost]
        public async Task<ActionResult> AddHallType([FromBody] Guide request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Название не может быть пустым");

            var exists = await _context.HallTypes.AnyAsync(ht => ht.Name == request.Name);
            if (exists)
                return Conflict("Тип зала с таким названием уже существует");

            var hallType = new HallType { Name = request.Name };
            _context.HallTypes.Add(hallType);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateHallType(int id, [FromBody] Guide request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Название не может быть пустым");

            var hallType = await _context.HallTypes.FindAsync(id);
            if (hallType == null)
                return NotFound("Тип зала не найден");

            var exists = await _context.HallTypes.AnyAsync(ht => ht.Name == request.Name && ht.HallTypeId != id);
            if (exists)
                return Conflict("Тип зала с таким названием уже существует");

            hallType.Name = request.Name;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteHallType(int id)
        {
            var hallType = await _context.HallTypes.FindAsync(id);
            if (hallType == null)
                return NotFound("Тип зала не найден");

            var isUsed = await _context.Stationars.AnyAsync(s => s.HallTypeId == id);
            if (isUsed)
                return BadRequest("Нельзя удалить тип зала, который используется в одном или более стационарах");

            _context.HallTypes.Remove(hallType);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}