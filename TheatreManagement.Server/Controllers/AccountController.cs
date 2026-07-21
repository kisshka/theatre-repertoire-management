using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheatreManagement.Domain.Data;
using TheatreManagement.Domain.Entities;
using TheatreManagement.Shared.DTOs.Users;
using TheatreManagement.Shared.Helpers;

namespace TheatreManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly DataContext _context;
        private readonly IEmailSender<User> _emailSender;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<User> userManager, DataContext context, IEmailSender<User> emailSender, IConfiguration configuration)
        {
            _userManager = userManager;
            _context = context;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        [HttpPost("register")]
        [Authorize]
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
        [Authorize]
        public async Task<IActionResult> PutUser(UserDto userDto)
        {
            var user = await _userManager.FindByIdAsync(userDto.Id);
            if (user == null)
                return Unauthorized();

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
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(currentUser);

            return new UserDto
            {
                Id = currentUser.Id,
                Email = currentUser.Email,
                Surname = currentUser.Surname,
                Name = currentUser.Name,
                FatherName = currentUser.FatherName,
                Role = roles[0]
            };
        }

        [HttpGet("{userId}")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetUser(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

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
        [Authorize]
        public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
        [FromQuery] string searchText = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool isArchive = false)
        {

            var query = _context.Users.AsQueryable();

            if (isArchive == true)
            {
                query = query.Where(p => p.DeletionTime != null);
            }
            else
            {
                query = query.Where(u => u.DeletionTime == null);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var normalizeSearchText = searchText.Trim().ToLower();

                query = query.Where(u =>
                    DataContext.CustomLike(u.Surname, normalizeSearchText));
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
                    DeletionTime = x.User.DeletionTime
                })
                .ToListAsync();

            return Ok(new PagedResult<UserDto>
            {
                Items = items,
                TotalCount = totalCount
            });
        }

        [HttpPut("{userId}/soft-delete")]
        [Authorize]
        public async Task<IActionResult> SoftDeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();

            user.LockoutEnd = DateTime.UtcNow.AddYears(100);
            user.LockoutEnabled = true;


            user.DeletionTime = DateTime.UtcNow;
            _context.SaveChanges();
            return Ok();
        }

        [HttpPut("{userId}/restore")]
        [Authorize]
        public async Task<IActionResult> RestoreUser(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            var user = await _context.Users.Where(u => u.Id == userId)
                                           .IgnoreQueryFilters()
                                           .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            user.DeletionTime = null;
            user.LockoutEnd = null;
            user.LockoutEnabled = false;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("forgot")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Ok(new { message = "Если пользователь существует, ссылка отправлена" });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = System.Web.HttpUtility.UrlEncode(token);
            var resetLink = $"{_configuration["FrontendUrl"]}/reset-password?email={request.Email}&token={encodedToken}";

            await _emailSender.SendPasswordResetLinkAsync(user, request.Email, resetLink);

            return Ok(new { message = "Ссылка для сброса пароля отправлена на email" });
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) ||
                string.IsNullOrEmpty(request.ResetCode) ||
                string.IsNullOrEmpty(request.NewPassword))
                return BadRequest("Все поля обязательны");

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return BadRequest("Не удалось сбросить пароль");

            var result = await _userManager.ResetPasswordAsync(user, request.ResetCode, request.NewPassword);

            if (result.Succeeded)
                return Ok(new { message = "Пароль успешно изменен" });

            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }
    }
}
