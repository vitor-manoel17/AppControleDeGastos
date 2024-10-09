using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AppControleDeGastos.Data;
using AppControleDeGastos.Models.LoginModel;
using AppControleDeGastos.Services;
using System;

namespace ControleFinanceiroAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var usuario = await _context.Usuarios
                                        .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (usuario == null)
            {
                return Unauthorized(new { Success = false, Message = "Email ou senha incorretos" });
            }

            var hashedPassword = PasswordService.HashPassword(request.Password, usuario.PasswordSalt);
            if (hashedPassword != usuario.PasswordHash)
            {
                return Unauthorized(new { Success = false, Message = "Email ou senha incorretos" });
            }

            HttpContext.Session.SetInt32("UsuarioId", usuario.UsuarioId);

            return Ok(new { Success = true, Message = "Login bem-sucedido" });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UsuarioId");

            return Ok(new
            {
                Success = true,
                Message = "Logout realizado com sucesso"
            });
        }

        [HttpGet("check-session")]
        public IActionResult CheckSession()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado" });
            }

            return Ok(new { Success = true, UsuarioId = usuarioId });
        }

    }
}
