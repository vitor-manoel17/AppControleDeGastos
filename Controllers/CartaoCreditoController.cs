using AppControleDeGastos.Data;
using AppControleDeGastos.Models.CartaoCreditoModel;
using AppControleDeGastos.Models.CartaoModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppControleDeGastos.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartaoCreditoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartaoCreditoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Método para obter o ID do usuário da sessão
        private int? GetUsuarioId()
        {
            return HttpContext.Session.GetInt32("UsuarioId");
        }

        // Método para retornar resposta quando o usuário não está autenticado
        private IActionResult UsuarioNaoAutenticado()
        {
            return Unauthorized(new { Success = false, Message = "Usuário não está logado" });
        }

        // GET: /CartaoCredito
        // GET: /CartaoCredito
        [HttpGet]
        public async Task<IActionResult> GetCartoesCredito()
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

            // Obtém os cartões de crédito associados ao usuário autenticado
            var cartoesCredito = await _context.Cartoes
                .Where(c => c.UsuarioId == usuarioId.Value && c.Type == "Credit" && c.DataDelecao == null)
                .Select(c => new
                {
                    c.CartaoId,
                    c.Numero,
                    c.Bandeira,
                    c.Limite,
                    c.ValorAtual,
                    c.DataValidade
                })
                .ToListAsync();

            // Verifica se existem cartões de crédito para o usuário
            if (!cartoesCredito.Any())
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Nenhum cartão de crédito encontrado para o usuário."
                });
            }

            // Retorna a resposta com sucesso e dados dos cartões de crédito
            return Ok(new
            {
                Success = true,
                Message = "Cartões de crédito recuperados com sucesso.",
                CartoesCredito = cartoesCredito
            });
        }


        // GET: /CartaoCredito/{id}
        // GET: /CartaoCredito/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCartaoCreditoById(int id)
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

            // Obtém o cartão de crédito associado ao usuário autenticado
            var cartaoCredito = await _context.Cartoes
                .Where(c => c.CartaoId == id && c.UsuarioId == usuarioId.Value && c.Type == "Credit" && c.DataDelecao == null)
                .Select(c => new
                {
                    c.CartaoId,
                    c.Numero,
                    c.Bandeira,
                    c.Limite,
                    c.ValorAtual,
                    c.DataValidade
                })
                .FirstOrDefaultAsync();

            // Verifica se o cartão de crédito foi encontrado
            if (cartaoCredito == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Cartão de crédito não encontrado ou não pertence ao usuário."
                });
            }

            // Retorna a resposta com sucesso e dados do cartão de crédito
            return Ok(new
            {
                Success = true,
                Message = "Cartão de crédito recuperado com sucesso.",
                CartaoCredito = cartaoCredito
            });
        }


        // POST: /CartaoCredito
        [HttpPost]
        public async Task<IActionResult> CreateCartaoCredito([FromBody] CreateCartaoCreditoRequest request)
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
                return BadRequest(ModelState);
            }

            // Validação do número do cartão (apenas números e 16 dígitos)
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
                .Where(c => c.UsuarioId == usuarioId.Value
                            && c.Numero == request.Numero
                            && c.DataDelecao == null) 
                .FirstOrDefaultAsync();


            if (cartaoExistente != null)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "O número do cartão já está cadastrado para este usuário."
                });
            }

            // Validação da bandeira do cartão (pode ser personalizado para aceitar apenas bandeiras específicas)
            var bandeirasValidas = new[] { "Visa", "MasterCard", "Amex", "Discover", "Elo" };  // Exemplo de bandeiras aceitas
            if (!bandeirasValidas.Contains(request.Bandeira))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"Bandeira do cartão inválida. As bandeiras aceitas são: {string.Join(", ", bandeirasValidas)}."
                });
            }

            // Validação da data de validade - deve ser uma data futura
            if (request.DataValidade <= DateTime.Today)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "A data de validade deve ser uma data futura."
                });
            }

            // Validação do limite do cartão - deve ser maior que 0
            if (request.Limite <= 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "O limite do cartão deve ser um valor maior que zero."
                });
            }

            // Validação do dia de fechamento - deve ser entre 1 e 31
            if (request.DiaFechamento < 1 || request.DiaFechamento > 31)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "O dia de fechamento do cartão deve ser um valor entre 1 e 31."
                });
            }

            // Criação da instância de Cartão de Crédito
            var novoCartaoCredito = new Cartao
            {
                UsuarioId = usuarioId.Value,
                Numero = request.Numero,
                Bandeira = request.Bandeira,
                Type = "Credit",  
                Limite = request.Limite,
                ValorAtual = 0,  // Valor inicial do cartão de crédito
                DiaFechamento = request.DiaFechamento,
                DataValidade = request.DataValidade,
                DataCriacao = DateTime.Now
            };

            _context.Cartoes.Add(novoCartaoCredito);
            await _context.SaveChangesAsync();


            return Ok(new
            {
                Success = true,
                CartaoCreditoId = novoCartaoCredito.CartaoId,
                Message = "Cartão de crédito criado com sucesso."
            });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCartaoCredito(int id, [FromBody] UpdateCartaoCreditoRequest request)
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

            var cartaoCredito = await _context.Cartoes
                .FirstOrDefaultAsync(c => c.CartaoId == id && c.UsuarioId == usuarioId.Value && c.Type == "Credit" && c.DataDelecao == null);

            // Verifica se o cartão de crédito foi encontrado e pertence ao usuário
            if (cartaoCredito == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Cartão de crédito não encontrado ou não pertence ao usuário."
                });
            }

            // Verifica e atualiza os campos apenas se eles contêm valores válidos
            if (!string.IsNullOrWhiteSpace(request.Numero))
            {
                cartaoCredito.Numero = request.Numero;
            }

            if (!string.IsNullOrWhiteSpace(request.Bandeira))
            {
                cartaoCredito.Bandeira = request.Bandeira;
            }

            if (request.Limite.HasValue && request.Limite.Value > 0)
            {
                cartaoCredito.Limite = request.Limite.Value;
            }

            if (request.ValorAtual.HasValue && request.ValorAtual.Value >= 0)
            {
                cartaoCredito.ValorAtual = request.ValorAtual.Value;
            }

            if (request.DiaFechamento.HasValue && request.DiaFechamento.Value > 0 && request.DiaFechamento.Value <= 31)
            {
                cartaoCredito.DiaFechamento = request.DiaFechamento.Value;
            }

            cartaoCredito.DataAtualizacao = DateTime.Now;

            _context.Cartoes.Update(cartaoCredito);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Message = "Cartão de crédito atualizado com sucesso.",
                CartaoCredito = new
                {
                    cartaoCredito.CartaoId,
                    cartaoCredito.Numero,
                    cartaoCredito.Bandeira,
                    cartaoCredito.Limite,
                    cartaoCredito.ValorAtual,
                    cartaoCredito.DiaFechamento
                }
            });
        }


        // DELETE: /CartaoCredito/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCartaoCredito(int id)
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

            // Obtém o cartão de crédito que será deletado
            var cartaoCredito = await _context.Cartoes
                .FirstOrDefaultAsync(c => c.CartaoId == id && c.UsuarioId == usuarioId.Value && c.Type == "Credit" && c.DataDelecao == null);

            // Verifica se o cartão de crédito foi encontrado e pertence ao usuário
            if (cartaoCredito == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Cartão de crédito não encontrado ou não pertence ao usuário."
                });
            }

            cartaoCredito.DataDelecao = DateTime.Now;

            _context.Cartoes.Update(cartaoCredito);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Message = "Cartão de crédito deletado com sucesso.",
                CartaoCredito = new
                {
                    cartaoCredito.CartaoId,
                    cartaoCredito.Numero,
                    cartaoCredito.Bandeira,
                    cartaoCredito.DataDelecao
                }
            });
        }

    }
}
