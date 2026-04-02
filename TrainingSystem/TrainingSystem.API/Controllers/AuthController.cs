using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TrainingSystem.API.Data;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;

    public AuthController(IConfiguration config, AppDbContext context)
    {
        _config = config;
        _context = context;
    }

    [HttpPost("login")]
    public IActionResult Login(string email, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == password);

        if (user == null)
            return Unauthorized();

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.RoleId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }
}