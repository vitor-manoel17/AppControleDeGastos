using AppControleDeGastos.Data;
using AppControleDeGastos.Models.CartaoModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppControleDeGastos.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartaoDebitoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartaoDebitoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Método para obter o ID do usuário logado da sessão
        private int? GetUsuarioId()
        {
            return HttpContext.Session.GetInt32("UsuarioId");
        }

        // Método para retornar resposta de usuário não autenticado
        private IActionResult UsuarioNaoAutenticado()
        {
            return Unauthorized(new { Success = false, Message = "Usuário não está logado" });
        }

        // GET: /CartaoDebito
        [HttpGet]
        public async Task<IActionResult> GetCartoesDebito()
        {
            // Verifica se o usuário está autenticado
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Usuário não autenticado."
                });
            }

            // Busca cartões de débito do usuário autenticado
            var cartoesDebito = await _context.Cartoes
                .Where(c => c.UsuarioId == usuarioId.Value && c.Type == "Debit" && c.DataDelecao == null)
                .Select(c => new
                {
                    c.CartaoId,           // ID do cartão
                    c.Bandeira,           // Bandeira do cartão (Visa, MasterCard, etc.)
                    c.DataValidade,       // Data de validade
                    c.Saldo               // Saldo disponível no cartão
                })
                .ToListAsync();

            // Se o usuário não possui nenhum cartão de débito, retornar mensagem apropriada
            if (cartoesDebito == null || !cartoesDebito.Any())
            {
                return Ok(new
                {
                    Success = true,
                    Message = "Nenhum cartão de débito encontrado para o usuário."
                });
            }

            // Retornar os cartões de débito com as informações básicas
            return Ok(new
            {
                Success = true,
                Message = "Cartões de débito recuperados com sucesso.",
                CartoesDebito = cartoesDebito
            });
        }


        // GET: /CartaoDebito/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCartaoDebitoById(int id)
        {
            // Verifica se o usuário está autenticado
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Usuário não autenticado."
                });
            }

            // Busca o cartão de débito com o ID fornecido, que pertença ao usuário autenticado
            var cartaoDebito = await _context.Cartoes
                .Where(c => c.CartaoId == id && c.UsuarioId == usuarioId.Value && c.Type == "Debit" && c.DataDelecao == null)
                .Select(c => new
                {
                    c.CartaoId,         // ID do cartão
                    c.Bandeira,         // Bandeira do cartão
                    c.DataValidade,     // Data de validade
                    c.Saldo             // Saldo disponível
                })
                .FirstOrDefaultAsync();

            // Verifica se o cartão foi encontrado
            if (cartaoDebito == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Cartão de débito não encontrado ou não pertence ao usuário."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Cartão de débito encontrado com sucesso.",
                CartaoDebito = cartaoDebito
            });
        }


        // POST: /CartaoDebito
        [HttpPost]
        public async Task<IActionResult> CreateCartaoDebito([FromBody] CreateCartaoDebitoRequest request)
        {
           
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Usuário não autenticado."
                });
            }

            // Validação do modelo da requisição
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Modelo inválido.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            // Validação do número do cartão (deve conter exatamente 16 dígitos numéricos)
            if (!System.Text.RegularExpressions.Regex.IsMatch(request.Numero, @"^\d{16}$"))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "O número do cartão deve conter exatamente 16 dígitos numéricos."
                });
            }

            // Validação de duplicidade de número de cartão para o mesmo usuário
            var cartaoExistente = await _context.Cartoes
                .FirstOrDefaultAsync(c => c.Numero == request.Numero && c.UsuarioId == usuarioId.Value && c.DataDelecao == null);

            if (cartaoExistente != null)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Já existe um cartão de débito com esse número para o usuário."
                });
            }

            // Validação da bandeira do cartão (somente bandeiras permitidas)
            var bandeirasPermitidas = new[] { "Visa", "MasterCard", "Elo", "Amex" };  // Exemplo de bandeiras aceitas
            if (!bandeirasPermitidas.Contains(request.Bandeira))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"Bandeira inválida. As bandeiras permitidas são: {string.Join(", ", bandeirasPermitidas)}."
                });
            }

            // Validação da data de validade (deve ser uma data futura)
            if (request.DataValidade <= DateTime.Today)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "A data de validade deve ser uma data futura."
                });
            }

            // Validação do saldo (deve ser um valor positivo)
            if (request.Saldo < 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "O saldo inicial deve ser um valor positivo."
                });
            }

            // Criar o novo Cartão de Débito com as propriedades informadas
            var novoCartaoDebito = new Cartao
            {
                UsuarioId = usuarioId.Value,
                Numero = request.Numero,
                Bandeira = request.Bandeira,
                Type = "Debit",  // Definir o tipo como "Débito" automaticamente
                Saldo = request.Saldo,
                DataValidade = request.DataValidade,
                DataCriacao = DateTime.Now
            };

            // Adicionar o cartão ao banco de dados
            _context.Cartoes.Add(novoCartaoDebito);
            await _context.SaveChangesAsync();

            // Retornar sucesso na criação
            return Ok(new
            {
                Success = true,
                CartaoDebitoId = novoCartaoDebito.CartaoId,
                Message = "Cartão de débito criado com sucesso."
            });
        }



        // PUT: /CartaoDebito/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCartaoDebito(int id, [FromBody] UpdateCartaoDebitoRequest request)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Usuário não autenticado."
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Modelo inválido.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            // Verifica se o cartão de débito existe e pertence ao usuário autenticado
            var cartaoDebito = await _context.Cartoes
                .FirstOrDefaultAsync(c => c.CartaoId == id && c.UsuarioId == usuarioId.Value && c.Type == "Debit" && c.DataDelecao == null);

            if (cartaoDebito == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Cartão de débito não encontrado ou não pertence ao usuário."
                });
            }

            if (request.Saldo.HasValue && request.Saldo < 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "O saldo não pode ser negativo."
                });
            }

            if (request.DataValidade.HasValue && request.DataValidade.Value <= DateTime.Now)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "A data de validade deve ser uma data futura."
                });
            }

            if (!string.IsNullOrEmpty(request.Numero))
            {
                cartaoDebito.Numero = request.Numero;
            }
            if (!string.IsNullOrEmpty(request.Bandeira))
            {
                cartaoDebito.Bandeira = request.Bandeira;
            }
            if (request.Saldo.HasValue)
            {
                cartaoDebito.Saldo = request.Saldo.Value;
            }
            if (request.DataValidade.HasValue)
            {
                cartaoDebito.DataValidade = request.DataValidade.Value;
            }
            cartaoDebito.DataAtualizacao = DateTime.Now;  

            _context.Cartoes.Update(cartaoDebito);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Message = "Cartão de débito atualizado com sucesso.",
                CartaoDebito = new
                {
                    cartaoDebito.CartaoId,
                    cartaoDebito.Bandeira,
                    cartaoDebito.Saldo,
                    cartaoDebito.DataValidade
                }
            });
        }

        // DELETE: /CartaoDebito/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCartaoDebito(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Usuário não autenticado."
                });
            }

            var cartaoDebito = await _context.Cartoes
                .FirstOrDefaultAsync(c => c.CartaoId == id && c.UsuarioId == usuarioId.Value && c.Type == "Debit" && c.DataDelecao == null);

            if (cartaoDebito == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Cartão de débito não encontrado ou não pertence ao usuário."
                });
            }

            if (cartaoDebito.Saldo > 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "O cartão de débito não pode ser excluído enquanto houver saldo."
                });
            }

            cartaoDebito.DataDelecao = DateTime.UtcNow;

            _context.Cartoes.Update(cartaoDebito);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Message = "Cartão de débito excluído com sucesso."
            });
        }

    }
}
