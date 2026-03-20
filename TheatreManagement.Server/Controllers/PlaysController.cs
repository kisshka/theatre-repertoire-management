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

        //// GET: api/Plays
        //[HttpGet]
        //public async Task<ActionResult<List<PlayDTO>>> GetPlays()
        //{
        //    var plays = await _context.Plays.ToListAsync();
        //    List<PlayDTO> playDtos = [];

        //    foreach (var play in plays)
        //    {

        //        playDtos.Add(new PlayDTO
        //        {
        //            PlayId = play.PlayId,
        //            Name = play.Name,
        //            Duration = play.Duration,
        //            AgeCategory = play.AgeCategory,

        //            IsActive = play.IsActive,
        //            LastEditTime = play.LastEditTime,
        //            DeletionTime = play.DeletionTime
        //        });
        //    }

        //    return playDtos;
        //}

        [HttpGet]
        public async Task<ActionResult<PagedResult<PlayDTO>>> GetPlays(
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
                .Select(p => new PlayDTO
                {
                    PlayId = p.PlayId,
                    Name = p.Name,
                    Duration = p.Duration,
                    AgeCategory = p.AgeCategory,
                    IsActive = p.IsActive,
                    LastEditTime = p.LastEditTime
                })
                .ToListAsync();

            return Ok(new PagedResult<PlayDTO>
            {
                Items = items,
                TotalCount = totalCount
            });
        }


        // GET: api/Plays/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Play>> GetPlay(int id)
        {
            var play = await _context.Plays.FindAsync(id);

            if (play == null)
            {
                return NotFound();
            }

            return play;
        }

        // PUT: api/Plays/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlay(int id, Play play)
        {
            if (id != play.PlayId)
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
                if (!PlayExists(id))
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
        public async Task<IActionResult> PostPlay(PlayDTO playDto)
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
