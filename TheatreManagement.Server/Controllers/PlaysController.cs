using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheatreManagement.Domain.Data;
using TheatreManagement.Shared.DTOs;
using TheatreManagement.Domain.Entities;
using TheatreManagement.Shared;

namespace TheatreManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaysController : ControllerBase
    {
        private readonly DataContext _context;

        public PlaysController(DataContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<PagedResult<PlayDto>>> GetPlays(
            [FromQuery] string searchText = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Plays
                .Where(p => p.IsActive && p.DeletionTime == null);

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
                .Select(p => new PlayDto
                {
                    PlayId = p.PlayId,
                    Name = p.Name,
                    Duration = p.Duration,
                    AgeCategory = p.AgeCategory,
                    IsActive = p.IsActive,
                    LastEditTime = p.LastEditTime
                })
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
            var play = await _context.Plays.Include(p => p.RoleInPlays)
                                           .FirstOrDefaultAsync(p => p.PlayId == playId);

            if (play == null)
            {
                return NotFound();
            }

            var playDto = new PlayDto
            {
                PlayId = play.PlayId,
                Name = play.Name,
                Duration = play.Duration,
                AgeCategory = play.AgeCategory,
                IsActive = play.IsActive,
                LastEditTime = play.LastEditTime,
            };

            var roles = play.RoleInPlays.Select(r =>
            new RoleDto
            {
                RoleInPlayId = r.RoleInPlayId,
                RoleType = r.Type,
                IsUsed = _context.EmployeeRoles.Any(er => er.RoleInPlayId == r.RoleInPlayId),
                Name = r.Name
            }
            ).ToList();

            playDto.RoleDtos = roles;

            return playDto;
        }

        [HttpPost]
        public async Task<IActionResult> PostPlay(PlayDto playDto)
        {

            var play = new Play
            {
                Name = playDto.Name,
                Duration = playDto.Duration,
                IsActive = true,
                AgeCategory = playDto.AgeCategory,
                LastEditTime = DateTime.UtcNow,
                DeletionTime = null
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
            var play = await _context.Plays.Where(p => p.PlayId == playDto.PlayId)
                                     .Include(p => p.RoleInPlays)
                                     .FirstOrDefaultAsync();

            var errors = new List<string>();

            play.Name = playDto.Name;
            play.Duration = playDto.Duration;
            play.IsActive = true;
            play.AgeCategory = playDto.AgeCategory;
            play.LastEditTime = DateTime.Now;


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
                        Message = $"{role.Name} Используется в составах или мероприятиях",
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
            var play = await _context.Plays.Where(p => p.PlayId == playId)
                                           .FirstOrDefaultAsync();

            var errors = new List<string>();

            play.DeletionTime = DateTime.Now;
            play.IsActive = false;


            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
