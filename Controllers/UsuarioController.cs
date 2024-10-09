using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; 
using System.Linq;
using System.Threading.Tasks;
using AppControleDeGastos.Data;
using AppControleDeGastos.Services;
using AppControleDeGastos.Models.UsuarioModel;
using Microsoft.AspNetCore.Authorization;

namespace AppControleDeGastos.Controllers
{
   // [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher; 

        public UsuarioController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Usuario>(); 
        }

        [HttpPost("get-user")]
        public async Task<IActionResult> GetUserById([FromBody] GetUserByIdRequest request)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado" });
            }

            if (request.Method != "GetUserById")
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Método inválido, revise sua nomenclatura!"
                });
            }

            var usuario = await _context.Usuarios.FindAsync(request.UsuarioId);
            if (usuario == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Usuário não encontrado pelo Id fornecido!"
                });
            }

            var usuarioResponse = new UsuarioResponse
            {
                UsuarioId = usuario.UsuarioId,
                NomeCompleto = usuario.NomeCompleto,
                Email = usuario.Email,
                DataCriacao = usuario.DataCriacao
            };

            return Ok(new
            {
                Success = true,
                Usuario = usuarioResponse
            });
        }

        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsers([FromBody] GetUsersRequest request)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado" });
            }

            if (request.Method != "GetUsers")
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Método inválido, revise sua nomenclatura!"
                });
            }

            var usuarios = await _context.Usuarios
                                         .Where(u => u.DataDelecao == null)
                                         .ToListAsync();

            if (!usuarios.Any())
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Nenhum usuário encontrado!"
                });
            }

            var usuariosResponse = usuarios.Select(usuario => new UsuarioResponse
            {
                UsuarioId = usuario.UsuarioId,
                NomeCompleto = usuario.NomeCompleto,
                Email = usuario.Email,
                DataCriacao = usuario.DataCriacao
            }).ToList();

            return Ok(new
            {
                Success = true,
                Usuarios = usuariosResponse
            });
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Usuarios.AnyAsync(u => u.Email == request.Email))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Email já está em uso"
                    });
                }

                var salt = PasswordService.GenerateSalt();
                var hashedPassword = PasswordService.HashPassword(request.Senha, salt);

                var newUser = new Usuario
                {
                    NomeCompleto = request.NomeCompleto,
                    Email = request.Email,
                    Avatar = request.Avatar,
                    PasswordSalt = salt,
                    PasswordHash = hashedPassword,
                    DataCriacao = DateTime.UtcNow
                };

                _context.Usuarios.Add(newUser);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    newUser.UsuarioId,
                    Message = "Usuário registrado com sucesso!"
                });
            }
            return BadRequest(ModelState);
        }

        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUserById([FromBody] UpdateUserByIdRequest request)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado" });
            }

            var usuario = await _context.Usuarios.FindAsync(request.UsuarioId);
            if (usuario == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Usuário não encontrado pelo Id informado!"
                });
            }

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(request.NomeCompleto))
                {
                    usuario.NomeCompleto = request.NomeCompleto;
                }
                if (!string.IsNullOrEmpty(request.Email))
                {
                    usuario.Email = request.Email;
                }
                if (!string.IsNullOrEmpty(request.Avatar))
                {
                    usuario.Avatar = request.Avatar;
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Usuarios.Any(e => e.UsuarioId == request.UsuarioId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Ok(new
                {
                    Success = true,
                    UsuarioId = usuario.UsuarioId,
                    Message = "Usuário atualizado com sucesso!"
                });
            }
            return BadRequest(ModelState);
        }


        [HttpDelete("delete-user")]
        public async Task<IActionResult> DeleteUserById([FromBody] DeleteUserByIdRequest request)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado" });
            }

            if (request.Method != "DeleteUserById")
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Método inválido, revise sua nomenclatura!"
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = await _context.Usuarios.FindAsync(request.UsuarioId);
            if (usuario == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Usuário não encontrado pelo Id informado!"
                });
            }

            if (usuario.DataDelecao != null)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Usuário já foi deletado!"
                });
            }

            usuario.DataDelecao = DateTime.UtcNow;
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                UsuarioId = usuario.UsuarioId,
                Message = "Usuário marcado como deletado com sucesso!"
            });
        }


    }
}
