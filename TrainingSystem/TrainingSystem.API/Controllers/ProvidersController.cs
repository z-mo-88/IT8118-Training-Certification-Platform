using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.DTOs;
using TrainingSystem.API.Models;

namespace TrainingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "3")]
    public class ProvidersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProvidersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProviderDto>>> GetProviders()
        {
            var providers = await _context.Providers
                .Select(p => new ProviderDto
                {
                    ProviderId = p.ProviderId,
                    ProviderName = p.ProviderName,
                    Email = p.Email,
                    PhoneNumber = p.PhoneNumber,
                    Address = p.Address,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return Ok(providers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProviderDto>> GetProvider(int id)
        {
            var provider = await _context.Providers.FindAsync(id);

            if (provider == null)
                return NotFound();

            return Ok(new ProviderDto
            {
                ProviderId = provider.ProviderId,
                ProviderName = provider.ProviderName,
                Email = provider.Email,
                PhoneNumber = provider.PhoneNumber,
                Address = provider.Address,
                IsActive = provider.IsActive
            });
        }

        [HttpPost]
        public async Task<ActionResult<ProviderDto>> CreateProvider(CreateProviderDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ProviderName))
                return BadRequest("Provider name is required.");

            var provider = new Provider
            {
                ProviderName = dto.ProviderName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                IsActive = dto.IsActive
            };

            _context.Providers.Add(provider);
            await _context.SaveChangesAsync();

            var result = new ProviderDto
            {
                ProviderId = provider.ProviderId,
                ProviderName = provider.ProviderName,
                Email = provider.Email,
                PhoneNumber = provider.PhoneNumber,
                Address = provider.Address,
                IsActive = provider.IsActive
            };

            return CreatedAtAction(nameof(GetProvider), new { id = provider.ProviderId }, result);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateProvider(int id, UpdateProviderDto dto)
        {
            var provider = await _context.Providers.FindAsync(id);

            if (provider == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(dto.ProviderName))
                return BadRequest("Provider name is required.");

            provider.ProviderName = dto.ProviderName;
            provider.Email = dto.Email;
            provider.PhoneNumber = dto.PhoneNumber;
            provider.Address = dto.Address;
            provider.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProvider(int id)
        {
            var provider = await _context.Providers.FindAsync(id);

            if (provider == null)
                return NotFound();

            _context.Providers.Remove(provider);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}