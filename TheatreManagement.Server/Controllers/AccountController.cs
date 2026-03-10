using Domain.Entities;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheatreManagement.Domain.Data;
using TheatreManagement.Shared.DTOs;

namespace TheatreManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly DataContext _context;

        public AccountController(UserManager<User> userManager, DataContext context)
        {
            _userManager = userManager;
            _context = context;
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
                return Ok(new { Succeeded = true });
            }

            return BadRequest(new
            {
                Succeeded = false,
                Errors = result.Errors.Select(e => e.Description).ToArray()
            });
        }

        [HttpGet("current")]
        public async Task<ActionResult<UserViewModel>> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            return new UserViewModel
            {
                Email = user.Email,
                Surname = user.Surname,
                Name = user.Name,
                FatherName = user.FatherName
            };

        }

        [HttpGet("all")]
        public async Task<ActionResult<List<UserViewModel>>> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            List<UserViewModel> userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                userViewModels.Add( new UserViewModel
                {
                    Email = user.Email,
                    Surname = user.Surname,
                    Name = user.Name,
                    FatherName = user.FatherName
                } );
            }
            return userViewModels;
        }

    }
}
