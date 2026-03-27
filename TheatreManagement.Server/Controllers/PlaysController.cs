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
                Name = r.Name,
                LastEditTime = r.LastEditTime,
            }
            ).ToList();

            playDto.RoleDtos = roles;

            return playDto;
        }

        [HttpPut("{playId}")]
        public async Task<IActionResult> PutPlay(int playId, Play play)
        {
            if (playId != play.PlayId)
            {
                return BadRequest();
            }

            _context.Entry(play).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayExists(playId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Plays
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
                    LastEditTime = DateTime.UtcNow,
                    Play = play
                });
            }

            _context.Plays.Add(play);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool PlayExists(int id)
        {
            return _context.Plays.Any(e => e.PlayId == id);
        }
    }
}
