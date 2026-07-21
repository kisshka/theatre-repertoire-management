using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheatreManagement.Domain.Data;
using TheatreManagement.Domain.Entities;
using TheatreManagement.Server.Mappings;
using TheatreManagement.Shared.DTOs;
using TheatreManagement.Shared.Helpers;

namespace TheatreManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlaysController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly DataContext _context;

        public PlaysController(UserManager<User> userManager, DataContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        [HttpGet("all")]
        public async Task<ActionResult<List<PlayDto>>> GetPlays()
        {
            var plays = await _context.Plays
                .Where(p => p.IsActive && p.DeletionTime == null)
                .Include(p => p.RoleInPlays)
                .Include(p => p.SceneType)
                .Select(PlayMappings.ToPlayDto)
                .ToListAsync();

            return plays;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<PlayDto>>> GetPlays(
            [FromQuery] string searchText = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool isArchive = false)
        {
            var query = _context.Plays
                .Include(p => p.User)
                .Include(p => p.SceneType)
                .AsQueryable();

            if (isArchive == true)
            {
                query = query.IgnoreQueryFilters().Where(p => p.DeletionTime != null);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var normalizeSearchText = searchText.Trim().ToLower();

                query = query.Where(p =>
                    DataContext.CustomLike(p.Name, normalizeSearchText));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.PlayId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(PlayMappings.ToPlayDto)
                .ToListAsync();

            return Ok(new PagedResult<PlayDto>
            {
                Items = items,
                TotalCount = totalCount
            });
        }


        [HttpGet("{playId}")]
        public async Task<ActionResult<PlayDto>> GetPlay(int playId)
        {
            var playDto = await _context.Plays
                .Where(p => p.PlayId == playId)
                .Include(p => p.SceneType)
                .Select(PlayMappings.ToPlayDto)
                .FirstOrDefaultAsync();

            if (playDto == null)
            {
                return NotFound();
            }

            var roles = await _context.RoleInPlays
                .Where(r => r.PlayId == playId)
                .Select(r => new RoleDto
                {
                    RoleInPlayId = r.RoleInPlayId,
                    RoleType = r.Type,
                    IsUsed = _context.EmployeeRoles.Any(er => er.RoleInPlayId == r.RoleInPlayId),
                    Name = r.Name
                })
                .ToListAsync();

            playDto.RoleDtos = roles;
            return Ok(playDto);
        }

        [HttpPost]
        public async Task<IActionResult> PostPlay(PlayDto playDto)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }


                SceneType sceneType = await _context.SceneTypes
                    .FirstOrDefaultAsync(st => st.SceneTypeId == playDto.SceneTypeId);

                if (sceneType == null)
                {
                    return BadRequest($"Тип сцены с ID {playDto.SceneTypeId} не найден");
                }


            var play = new Play
            {
                Name = playDto.Name,
                Duration = playDto.Duration,
                IsActive = playDto.IsActive,
                AgeCategory = playDto.AgeCategory,
                LastEditTime = DateTime.Now,
                DeletionTime = null,
                User = currentUser,
                SceneType = sceneType,
                SceneTypeId = playDto.SceneTypeId
            };

            foreach (var role in playDto.RoleDtos)
            {
                _context.RoleInPlays.Add(new RoleInPlay
                {
                    Name = role.Name,
                    Type = role.RoleType,
                    Play = play
                });
            }

            _context.Plays.Add(play);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> PutPlay(PlayDto playDto)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var play = await _context.Plays
                .Where(p => p.PlayId == playDto.PlayId)
                .Include(p => p.RoleInPlays)
                .Include(p => p.SceneType)
                .FirstOrDefaultAsync();

            if (play == null)
            {
                return NotFound("Спектакль не найден");
            }

            // Обновляем тип сцены
            if (playDto.SceneTypeId != play.SceneTypeId)
            {
                var sceneType = await _context.SceneTypes
                    .FirstOrDefaultAsync(st => st.SceneTypeId == playDto.SceneTypeId);

                if (sceneType == null && playDto.SceneTypeId > 0)
                {
                    return BadRequest($"Тип сцены с ID {playDto.SceneTypeId} не найден");
                }

                play.SceneType = sceneType;
                play.SceneTypeId = playDto.SceneTypeId;
            }

            play.Name = playDto.Name;
            play.Duration = playDto.Duration;
            play.IsActive = playDto.IsActive;
            play.AgeCategory = playDto.AgeCategory;
            play.LastEditTime = DateTime.Now;
            play.User = currentUser;

            var updatedRoleIds = playDto.RoleDtos
                .Where(r => r.RoleInPlayId > 0)
                .Select(r => r.RoleInPlayId)
                .ToHashSet();

            var existingRoleIds = play.RoleInPlays
                .Select(r => r.RoleInPlayId)
                .ToHashSet();

            foreach (var role in playDto.RoleDtos.Where(r => r.RoleInPlayId > 0))
            {
                var existingRole = play.RoleInPlays
                     .FirstOrDefault(r => r.RoleInPlayId == role.RoleInPlayId);

                if (existingRole != null)
                {
                    existingRole.Name = role.Name;
                    existingRole.Type = role.RoleType;
                }
            }

            foreach (var roleDto in playDto.RoleDtos.Where(r => r.RoleInPlayId == 0))
            {
                var newRole = new RoleInPlay
                {
                    Name = roleDto.Name,
                    Type = roleDto.RoleType,
                    Play = play
                };
                _context.RoleInPlays.Add(newRole);
            }

            var rolesToDelete = existingRoleIds.Except(updatedRoleIds).ToList();

            foreach (var roleId in rolesToDelete)
            {
                var role = play.RoleInPlays.First(r => r.RoleInPlayId == roleId);

                // Используется ли роль в ассоциативной таблице
                var isUsed = await _context.EmployeeRoles
                    .AnyAsync(er => er.RoleInPlayId == roleId);

                if (isUsed)
                {
                    return BadRequest(new
                    {
                        Message = $"{role.Name} используется в составах или мероприятиях"
                    });
                }
                _context.RoleInPlays.Remove(role);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{playId}/soft-delete")]
        public async Task<IActionResult> SoftDeletePlay(int playId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var play = await _context.Plays
                .Where(p => p.PlayId == playId)
                .FirstOrDefaultAsync();

            if (play == null)
            {
                return NotFound("Спектакль не найден");
            }

            play.DeletionTime = DateTime.Now;
            play.IsActive = false;
            play.User = currentUser;
            play.LastEditTime = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{playId}/restore")]
        public async Task<IActionResult> RestorePlay(int playId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var play = await _context.Plays
                .Where(p => p.PlayId == playId)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync();

            if (play == null)
            {
                return NotFound("Спектакль не найден");
            }

            play.DeletionTime = null;
            play.IsActive = true;
            play.LastEditTime = DateTime.Now;
            play.User = currentUser;

            await _context.SaveChangesAsync();
            return Ok();
        }

        // Типы сцен

        [HttpGet("scene-types")]
        public async Task<ActionResult<List<Guide>>> GetSceneTypes()
        {
            var sceneTypes = await _context.SceneTypes
                .OrderBy(st => st.Name)
                .Select(st => new Guide
                {
                    Id = st.SceneTypeId,
                    Name = st.Name
                })
                .ToListAsync();

            return Ok(sceneTypes);
        }

    }
}