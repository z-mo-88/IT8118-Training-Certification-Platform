using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.API.DTOs;

namespace TrainingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "3")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    CPR = u.CPR,
                    RoleId = u.RoleId,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Where(u => u.UserId == id)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    CPR = u.CPR,
                    RoleId = u.RoleId,
                    IsActive = u.IsActive
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
                return BadRequest("Email already exists.");

            if (await _context.Users.AnyAsync(u => u.CPR == userDto.CPR))
                return BadRequest("CPR already exists.");
            var user = new User
            {
                Name = userDto.Name,
                Email = userDto.Email,
                PasswordHash = userDto.PasswordHash,
                PhoneNumber = userDto.PhoneNumber,
                CPR = userDto.CPR,
                RoleId = userDto.RoleId,
                IsActive = userDto.IsActive
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CPR = user.CPR,
                RoleId = user.RoleId,
                IsActive = user.IsActive
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            user.PhoneNumber = updatedUser.PhoneNumber;
            user.CPR = updatedUser.CPR;
            user.RoleId = updatedUser.RoleId;
            user.IsActive = updatedUser.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchUser(int id, PatchUserDto patchUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            if (patchUser.Name != null)
                user.Name = patchUser.Name;

            if (patchUser.Email != null)
                user.Email = patchUser.Email;

            if (patchUser.PhoneNumber != null)
                user.PhoneNumber = patchUser.PhoneNumber;

            if (patchUser.CPR != null)
                user.CPR = patchUser.CPR;

            if (patchUser.RoleId.HasValue)
                user.RoleId = patchUser.RoleId.Value;

            if (patchUser.IsActive.HasValue)
                user.IsActive = patchUser.IsActive.Value;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}