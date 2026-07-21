
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
    public class SceneTypesController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly DataContext _context;

        public SceneTypesController(UserManager<User> userManager, DataContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<List<Guide>>> GetSceneTypes()
        {
            var sceneTypes = await _context.SceneTypes
                .Include(st => st.Plays)
                .OrderBy(st => st.Name)
                .Select(st => new Guide
                {
                    Id = st.SceneTypeId,
                    Name = st.Name,
                    IsUsed = st.Plays.Count() > 0
                    
                }).ToListAsync();

            return Ok(sceneTypes);
        }

        [HttpPost]
        public async Task<ActionResult> AddSceneType([FromBody] Guide request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Название не может быть пустым");

            var exists = await _context.SceneTypes.AnyAsync(st => st.Name == request.Name);
            if (exists)
                return Conflict("Тип сцены с таким названием уже существует");

            var sceneType = new SceneType { Name = request.Name };
            _context.SceneTypes.Add(sceneType);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class AddSceneTypeRequest
        {
            public string Name { get; set; }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateSceneType(int id, [FromBody] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Название не может быть пустым");

            var sceneType = await _context.SceneTypes.FindAsync(id);
            if (sceneType == null)
                return NotFound("Тип сцены не найден");

            var exists = await _context.SceneTypes.AnyAsync(st => st.Name == name && st.SceneTypeId != id);
            if (exists)
                return Conflict("Тип сцены с таким названием уже существует");

            sceneType.Name = name;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSceneType(int id)
        {
            var sceneType = await _context.SceneTypes.FindAsync(id);
            if (sceneType == null)
                return NotFound("Тип сцены не найден");

            // Проверка на использование
            var isUsed = await _context.Plays.AnyAsync(e => e.SceneTypeId == id);
            if (isUsed)
                return BadRequest("Нельзя удалить тип сцены, который используется в одном или более спектаклях");

            _context.SceneTypes.Remove(sceneType);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
