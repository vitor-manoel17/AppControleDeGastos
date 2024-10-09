using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Linq;
using AppControleDeGastos.Data;
using AppControleDeGastos.Models.ReceitaModel; // Certifique-se de que você tem um model Receita e suas request/response models

namespace AppControleDeGastos.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReceitaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReceitaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Receita
        [HttpGet]
        public async Task<IActionResult> GetReceitas()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado" });
            }

            // Busca todas as receitas do usuário logado e mapeia para ReceitaResponse
            var receitas = await _context.Receitas
                .Where(r => r.UsuarioId == usuarioId)
                .Select(r => new ReceitaResponse
                {
                    ReceitaId = r.ReceitaId,
                    Valor = r.Valor,
                    Data = r.Data,
                    Descricao = r.Descricao
                })
                .ToListAsync();

            return Ok(new
            {
                Success = true,
                Receitas = receitas
            });
        }


        // GET: /Receita/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReceitaById(int id)
        {
            // Verifica se o usuário está logado
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado" });
            }

            // Busca a receita pelo ID e verifica se pertence ao usuário logado
            var receita = await _context.Receitas
                .Where(r => r.ReceitaId == id && r.UsuarioId == usuarioId && r.DataDelecao == null)
                .Select(r => new ReceitaResponse
                {
                    ReceitaId = r.ReceitaId,
                    Valor = r.Valor,
                    Data = r.Data,
                    Descricao = r.Descricao
                })
                .FirstOrDefaultAsync();

            if (receita == null)
            {
                return NotFound(new { Success = false, Message = "Receita não encontrada ou não pertence ao usuário" });
            }

            return Ok(new
            {
                Success = true,
                Receita = receita
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateReceita([FromBody] CreateReceitaRequest request)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var receita = new Receita
            {
                UsuarioId = usuarioId.Value,
                Valor = request.Valor,
                Data = request.Data,
                Descricao = request.Descricao,
                DataAtualizacao = DateTime.Now 
            };

            try
            {
                _context.Receitas.Add(receita);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
            {
                if (sqlEx.Number == 242) // Código de erro para problemas de conversão de tipo datetime
                {
                    return BadRequest(new { Success = false, Message = "Erro de conversão de data/hora. Verifique os dados enviados." });
                }
                throw; // Relança a exceção se não for relacionada à conversão de data/hora
            }

            return Ok(new
            {
                Success = true,
                ReceitaId = receita.ReceitaId,
                Message = "Receita criada com sucesso"
            });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReceita(int id, [FromBody] UpdateReceitaRequest request)
        {
           
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado" });
            }

            // Procura a receita pelo ID e verifica se pertence ao usuário logado
            var receita = await _context.Receitas
                .FirstOrDefaultAsync(r => r.ReceitaId == id && r.UsuarioId == usuarioId);

            if (receita == null)
            {
                return NotFound(new { Success = false, Message = "Receita não encontrada ou não pertence ao usuário" });
            }

            if (request.Valor.HasValue)
            {
                if (request.Valor.Value <= 0)
                {
                    return BadRequest(new { Success = false, Message = "O valor da receita deve ser positivo." });
                }
                receita.Valor = request.Valor.Value;
            }

            if (!string.IsNullOrWhiteSpace(request.Descricao))
            {
                if (request.Descricao.Length > 255)
                {
                    return BadRequest(new { Success = false, Message = "A descrição não pode ter mais de 255 caracteres." });
                }
                receita.Descricao = request.Descricao;
            }

            receita.DataAtualizacao = DateTime.Now;

            _context.Receitas.Update(receita);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReceitaExists(id))
                {
                    return NotFound(new { Success = false, Message = "Erro de concorrência: Receita não existe mais." });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new
            {
                Success = true,
                Message = "Receita atualizada com sucesso",
                Receita = new
                {
                    receita.ReceitaId,
                    receita.Valor,
                    receita.Data,
                    receita.Descricao,
                    receita.DataAtualizacao
                }
            });
        }

        private bool ReceitaExists(int id)
        {
            return _context.Receitas.Any(e => e.ReceitaId == id);
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReceita(int id)
        {
           
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado" });
            }

            var receita = await _context.Receitas
                .FirstOrDefaultAsync(r => r.ReceitaId == id && r.UsuarioId == usuarioId && r.DataDelecao == null);

            if (receita == null)
            {
                return NotFound(new { Success = false, Message = "Receita não encontrada ou não pertence ao usuário" });
            }

            receita.DataDelecao = DateTime.UtcNow;

            _context.Receitas.Update(receita);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Message = "Receita deletada com sucesso"
            });
        }



    }
}
