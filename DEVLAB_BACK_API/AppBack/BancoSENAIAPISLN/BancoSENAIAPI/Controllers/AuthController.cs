using BancoSENAIAPI.Data;
using BancoSENAIAPI.Dtos;
using BancoSENAIAPI.Models;
using BancoSENAIAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;


namespace BancoSENAIAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly TokenService _TokenService;

        public AuthController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _TokenService = tokenService;
        }

        [HttpPost("registrar")]
        public async Task<ActionResult> Registrar([FromBody] RegisterRequest dto)
        {
            if (await _context.Usuario.AnyAsync(u => u.NomeUsuario == dto.NomeUsuario))
            {
                return BadRequest(new { mensage = "Este nome de usuário já está em uso SEU BURRO" });
            }
            var usuario = new Usuario
            {
                NomeUsuario = dto.NomeUsuario,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha)
            };
            _context.Usuario.Add(usuario);
            await _context.SaveChangesAsync();

            return Created("", new { usuario.Id, usuario.NomeUsuario });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {

            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.NomeUsuario == dto.NomeUsuario);
            {
                return Unauthorized(new { menssage = "SEU BURRO, TÁ ERRADO ALGUMA COISA AI, OU O USUÁRIO OU A SENHA" });
            }
        }
    }
}
