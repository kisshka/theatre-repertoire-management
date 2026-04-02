using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheatreManagement.Domain.Data;
using TheatreManagement.Shared;
using TheatreManagement.Shared.DTOs;

namespace TheatreManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly DataContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<User> userManager, DataContext context, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Surname = model.Surname,
                Name = model.Name,
                FatherName = model.FatherName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                return Ok(new { Succeeded = true });
            }

            return BadRequest(new
            {
                Succeeded = false,
                Errors = result.Errors.Select(e => e.Description).ToArray()
            });
        }

        [HttpPut]
        public async Task<IActionResult> PutUser(UserDto userDto)
        {
            var user = await _userManager.FindByIdAsync(userDto.Id);
            if (user == null)
                return NotFound();

            user.Surname = userDto.Surname;
            user.Name = userDto.Name;
            user.FatherName = userDto.FatherName;
            user.Email = userDto.Email;

            var roles = await _userManager.GetRolesAsync(user);
            if (roles[0] != userDto.Role)
            {
                await _userManager.RemoveFromRoleAsync(user, roles[0]);
                await _userManager.AddToRoleAsync(user, userDto.Role);
            }

            _context.SaveChanges();
            return Ok();
        }

        [HttpGet("current")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Surname = user.Surname,
                Name = user.Name,
                FatherName = user.FatherName,
                Role = roles[0]
            };
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<UserDto>> GetUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Surname = user.Surname,
                Name = user.Name,
                FatherName = user.FatherName,
                Role = roles[0]
            };
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
        [FromQuery] string searchText = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            var query = _context.Users
                .Where(p => p.DeletionTime == null);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var normalizeSearchText = searchText.Trim().ToLower();

                query = query.Where(p =>
                    DataContext.CustomLike(p.Surname, normalizeSearchText));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(u => new
                {
                    User = u,
                    Role = _context.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                        .FirstOrDefault()
                })
                .OrderBy(x => x.User.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new UserDto
                {
                    Id = x.User.Id,
                    Name = x.User.Name,
                    Surname = x.User.Surname,
                    FatherName = x.User.FatherName,
                    Email = x.User.Email,
                    Role = x.Role,
                })
                .ToListAsync();

            return Ok(new PagedResult<UserDto>
            {
                Items = items,
                TotalCount = totalCount
            });
        }

        [HttpPut("{userId}/soft-delete")]
        public async Task<IActionResult> SoftDeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            user.DeletionTime = DateTime.UtcNow;

            return Ok();
        }
    }
}
