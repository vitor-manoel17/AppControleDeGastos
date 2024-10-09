using Microsoft.AspNetCore.Mvc;
using AppControleDeGastos.Data;
using AppControleDeGastos.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AppControleDeGastos.Models.DespesaModel;
using Microsoft.AspNetCore.Authorization;
using AppControleDeGastos.Services;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

namespace AppControleDeGastos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DespesaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DespesaController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: api/Despesa
        [HttpGet]
        public async Task<ActionResult> GetDespesas()
        {
            // Verificar se o usuário está logado e obter o ID do usuário
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado." });
            }

            // Recuperar a lista de despesas que pertencem ao usuário e não foram deletadas (DataDelecao == null)
            var despesas = await _context.Despesa
                .Where(d => d.UsuarioId == usuarioId && d.DataDelecao == null)
                .ToListAsync();

            // Verificar se há despesas retornadas
            if (despesas == null || !despesas.Any())
            {
                return NotFound(new { Success = false, Message = "Nenhuma despesa encontrada." });
            }

            // Retornar a lista de despesas com uma mensagem de sucesso
            return Ok(new
            {
                Success = true,
                Message = "Despesas recuperadas com sucesso.",
                Data = despesas.Select(d => new
                {
                    d.DespesaId,
                    d.Valor,
                    d.Data,
                    d.Descricao,
                    d.CategoriaId,
                    d.FormaPagamento,
                    d.NumeroParcelas,
                    d.Pago
                }).ToList()
            });
        }


        // GET: api/Despesa/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetDespesa(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado." });
            }

            var despesa = await _context.Despesa
                .FirstOrDefaultAsync(d => d.DespesaId == id && d.UsuarioId == usuarioId && d.DataDelecao == null);

            if (despesa == null)
            {
                return NotFound(new { Success = false, Message = "Despesa não encontrada ou foi deletada." });
            }

            return Ok(new
            {
                Success = true,
                Message = "Despesa recuperada com sucesso.",
                Data = new
                {
                    despesa.DespesaId,
                    despesa.Valor,
                    despesa.Data,
                    despesa.Descricao,
                    despesa.CategoriaId,
                    despesa.FormaPagamento,
                    despesa.NumeroParcelas,
                    despesa.Pago
                }
            });
        }


        // POST: api/Despesa
        [HttpPost]
        public async Task<ActionResult<Despesa>> AddDespesa([FromBody] RequestDespesa requestDespesa)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado." });
            }

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário inválido." });
            }

            // Verificar a receita disponível no mês atual
            var receitaDoMes = await _context.Receitas
                .Where(r => r.UsuarioId == usuarioId && r.Data.Month == DateTime.Now.Month && r.Data.Year == DateTime.Now.Year && r.DataDelecao == null)
                .SumAsync(r => r.Valor);

            if (receitaDoMes == 0)
            {
                return BadRequest(new { Success = false, Message = "Não há receita cadastrada no mês para efetivar a despesa." });
            }

            // Calcular o total das despesas do mês
            var totalDespesasDoMes = await _context.Despesa
                .Where(d => d.UsuarioId == usuarioId && d.Data.Month == DateTime.Now.Month && d.Data.Year == DateTime.Now.Year && d.DataDelecao == null)
                .SumAsync(d => d.Valor);

            // Calcular a receita disponível
            var receitaDisponivel = receitaDoMes - totalDespesasDoMes;

            // Verificar se há receita suficiente para a nova despesa
            if (requestDespesa.Valor > receitaDisponivel)
            {
                return BadRequest(new { Success = false, Message = "Não há receita suficiente no mês para esta despesa." });
            }

            // Verificação e processamento para cartão de débito
            if (requestDespesa.FormaPagamento.Equals("cartao de debito", StringComparison.OrdinalIgnoreCase))
            {
                var cartaoDebito = await _context.Cartoes
                    .FirstOrDefaultAsync(c => c.CartaoId == requestDespesa.CartaoDebitoId && c.UsuarioId == usuarioId && c.DataDelecao == null);

                if (cartaoDebito == null)
                {
                    return BadRequest(new { Success = false, Message = "Cartão de débito inválido ou deletado." });
                }

                if (cartaoDebito.Saldo < requestDespesa.Valor)
                {
                    return BadRequest(new { Success = false, Message = "Saldo insuficiente no cartão de débito." });
                }

                // Deduzir o valor da despesa do saldo do cartão de débito
                cartaoDebito.Saldo -= requestDespesa.Valor;

                // Ajustar a despesa para ser marcada como paga e definir numeroParcelas como 0 ou null
                requestDespesa.Pago = true;
                requestDespesa.NumeroParcelas = null;
            }

            // Verificação e processamento para cartão de crédito
            if (requestDespesa.FormaPagamento.Equals("cartao de credito", StringComparison.OrdinalIgnoreCase))
            {
                var cartaoCredito = await _context.Cartoes
                    .FirstOrDefaultAsync(c => c.CartaoId == requestDespesa.CartaoCreditoId && c.UsuarioId == usuarioId && c.DataDelecao == null);

                if (cartaoCredito == null)
                {
                    return BadRequest(new { Success = false, Message = "Cartão de crédito inválido ou deletado." });
                }

                // Verificar se o limite será excedido
                if (cartaoCredito.ValorAtual + requestDespesa.Valor > cartaoCredito.Limite)
                {
                    return BadRequest(new { Success = false, Message = "Limite de crédito excedido." });
                }

                // Verificar se a despesa ultrapassa o dia de fechamento da fatura
                if (requestDespesa.Data.Day > cartaoCredito.DiaFechamento)
                {
                    // Despesa será adicionada à próxima fatura
                    requestDespesa.Data = requestDespesa.Data.AddMonths(1);  // Adicionar um mês para a próxima fatura
                }

                // Verificação para despesas parceladas
                if (requestDespesa.NumeroParcelas.HasValue && requestDespesa.NumeroParcelas.Value > 1)
                {
                    decimal valorParcela = requestDespesa.Valor / requestDespesa.NumeroParcelas.Value;

                    for (int i = 0; i < requestDespesa.NumeroParcelas.Value; i++)
                    {
                        // Adiciona parcelas nas faturas futuras
                        var despesaParcela = new Despesa
                        {
                            UsuarioId = usuario.UsuarioId,
                            CartaoId = requestDespesa.CartaoCreditoId,
                            Valor = valorParcela,
                            Data = requestDespesa.Data.AddMonths(i),  // Incrementar o mês da parcela
                            Descricao = $"{requestDespesa.Descricao} - Parcela {i + 1}/{requestDespesa.NumeroParcelas}",
                            FormaPagamento = requestDespesa.FormaPagamento,
                            NumeroParcelas = 1,  // Cada parcela é uma "despesa única"
                            Pago = false,
                            DataCriacao = DateTime.Now,
                            DataAtualizacao = DateTime.Now
                        };

                        _context.Despesa.Add(despesaParcela);
                    }
                }
                else
                {
                    cartaoCredito.ValorAtual += requestDespesa.Valor;
                }
            }

            var despesa = new Despesa
            {
                UsuarioId = usuario.UsuarioId,
                Valor = requestDespesa.Valor,
                Data = requestDespesa.Data,
                Descricao = requestDespesa.Descricao,
                FormaPagamento = requestDespesa.FormaPagamento,
                NumeroParcelas = requestDespesa.NumeroParcelas ?? 0,  // Usar 0 se não especificado
                Pago = requestDespesa.Pago,
                CategoriaId = requestDespesa.CategoriaId,
                CartaoId = requestDespesa.CartaoCreditoId ?? requestDespesa.CartaoDebitoId,  // Usar o ID do cartão adequado
                DataCriacao = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            _context.Despesa.Add(despesa);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Message = "Despesa adicionada com sucesso.",
                Data = new
                {
                    despesa.DespesaId,
                    despesa.Valor,
                    despesa.FormaPagamento,
                    despesa.Data,
                    despesa.NumeroParcelas
                }
            });
        }



        // PUT: api/Despesa/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDespesa(int id, [FromBody] UpdateDespesa updateDespesa)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado." });
            }

            // Verificar se a despesa fornecida corresponde ao ID da URL
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID inválido fornecido." });
            }

            // Recuperar a despesa existente que pertence ao usuário e não foi deletada
            var despesaExistente = await _context.Despesa
                .FirstOrDefaultAsync(d => d.DespesaId == id && d.UsuarioId == usuarioId && d.DataDelecao == null);

            if (despesaExistente == null)
            {
                return NotFound(new { Success = false, Message = "Despesa não encontrada ou já deletada." });
            }

            // Calcular o valor disponível da receita no mês
            var receitaDoMes = await _context.Receitas
                .Where(r => r.UsuarioId == usuarioId && r.Data.Month == DateTime.Now.Month && r.Data.Year == DateTime.Now.Year && r.DataDelecao == null)
                .SumAsync(r => r.Valor);

            var totalDespesasDoMes = await _context.Despesa
                .Where(d => d.UsuarioId == usuarioId && d.Data.Month == DateTime.Now.Month && d.Data.Year == DateTime.Now.Year && d.DataDelecao == null)
                .SumAsync(d => d.Valor);

            var receitaDisponivel = receitaDoMes - totalDespesasDoMes;

            // Atualizar os campos fornecidos (campos opcionais)
            if (updateDespesa.Valor.HasValue)
            {
                var valorNovo = updateDespesa.Valor.Value;
                var valorAntigo = despesaExistente.Valor;

                // Verificar se o novo valor não excede o valor disponível
                if (valorNovo > receitaDisponivel + valorAntigo)
                {
                    return BadRequest(new { Success = false, Message = "O valor atualizado da despesa excede a receita disponível no mês." });
                }

                despesaExistente.Valor = valorNovo;
            }

            if (updateDespesa.Data.HasValue) despesaExistente.Data = updateDespesa.Data.Value;
            if (!string.IsNullOrWhiteSpace(updateDespesa.Descricao)) despesaExistente.Descricao = updateDespesa.Descricao;
            if (updateDespesa.CategoriaId.HasValue) despesaExistente.CategoriaId = updateDespesa.CategoriaId.Value;
            if (!string.IsNullOrWhiteSpace(updateDespesa.FormaPagamento)) despesaExistente.FormaPagamento = updateDespesa.FormaPagamento;
            if (updateDespesa.NumeroParcelas.HasValue) despesaExistente.NumeroParcelas = updateDespesa.NumeroParcelas.Value;
            if (updateDespesa.Pago.HasValue) despesaExistente.Pago = updateDespesa.Pago.Value;

            // Atualizar o campo de DataAtualizacao
            despesaExistente.DataAtualizacao = DateTime.Now;

            // Atualizar a entrada no banco de dados
            _context.Entry(despesaExistente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DespesaExists(id))
                {
                    return NotFound(new { Success = false, Message = "Erro de concorrência: Despesa não existe mais." });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new
            {
                Success = true,
                Message = "Despesa atualizada com sucesso.",
                Data = new
                {
                    despesaExistente.DespesaId,
                    despesaExistente.Valor,
                    despesaExistente.Data,
                    despesaExistente.Descricao,
                    despesaExistente.CategoriaId,
                    despesaExistente.FormaPagamento,
                    despesaExistente.NumeroParcelas,
                    despesaExistente.Pago
                }
            });
        }

        // DELETE: api/Despesa/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDespesa(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado." });
            }

            // Recuperar a despesa existente que pertence ao usuário e não foi deletada
            var despesa = await _context.Despesa
                .FirstOrDefaultAsync(d => d.DespesaId == id && d.UsuarioId == usuarioId && d.DataDelecao == null);

            if (despesa == null)
            {
                return NotFound(new { Success = false, Message = "Despesa não encontrada ou já foi deletada." });
            }

            despesa.DataDelecao = DateTime.Now;
            despesa.DataAtualizacao = DateTime.Now;

            _context.Entry(despesa).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Message = "Despesa deletada com sucesso.",
                Data = new
                {
                    despesa.DespesaId,
                    despesa.Descricao,
                    despesa.DataDelecao
                }
            });
        }

        [HttpGet("estatisticas")]
        public async Task<IActionResult> GetEstatisticas()
        {
            // Verificar se o usuário está autenticado
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Unauthorized(new { Success = false, Message = "Usuário não está logado." });
            }

            if (usuarioId.HasValue && !IsAdmin(usuarioId.Value))
            {
                return Unauthorized(new { Success = false, Message = "Usuário não tem permissão para acessar este método." });
            }

            try
            {
                // Obter dados do usuário autenticado
                var despesas = await _context.Despesa
                    .Where(d => d.DataDelecao == null) 
                    .ToListAsync();

                var receitas = await _context.Receitas
                    .Where(r => r.DataDelecao == null) 
                    .ToListAsync();

                var cartoes = await _context.Cartoes
                    .Where(c => c.DataDelecao == null) 
                    .ToListAsync();

                if (despesas == null || receitas == null || cartoes == null)
                {
                    return NotFound(new { Success = false, Message = "Dados não encontrados para o usuário." });
                }

                if (!despesas.Any())
                {
                    return Ok(new { Success = true, Message = "Nenhuma despesa encontrada para o usuário." });
                }

                if (!receitas.Any())
                {
                    return Ok(new { Success = true, Message = "Nenhuma receita encontrada para o usuário." });
                }

                if (!cartoes.Any())
                {
                    return Ok(new { Success = true, Message = "Nenhum cartão encontrado para o usuário." });
                }

                // Receita do Mês
                var receitaDoMes = receitas.Sum(r => r.Valor);

                // Limite do Cartão de Crédito por Cartão
                var limiteCartoes = cartoes.ToDictionary(c => c.CartaoId, c => c.Limite);

                // Estatísticas das Despesas
                var valoresDespesas = despesas.Select(d => d.Valor).ToList();
                var mediaDespesa = Estatisticas.Media(valoresDespesas);
                var medianaDespesa = Estatisticas.Mediana(valoresDespesas);
                var varianciaDespesa = Estatisticas.Variancia(valoresDespesas);
                var desvioPadraoDespesa = Estatisticas.DesvioPadrao(valoresDespesas);
                var intervaloConfiança = Estatisticas.IntervaloConfiança(mediaDespesa, desvioPadraoDespesa, valoresDespesas.Count);

                // Proporções
                var proporcaoFormaPagamento = Estatisticas.Proporcao(despesas, "cartão de crédito");
                var proporcaoCategoria = Estatisticas.ProporcaoCategoria(despesas, /* CategoriaId */ 1); // Ajuste conforme necessário

                // Distribuições
                var distribuicaoNormal = Estatisticas.DistribuicaoNormal(
                    (double)mediaDespesa,
                    (double)mediaDespesa,
                    (double)desvioPadraoDespesa
                );

                // Retornar as estatísticas calculadas com sucesso
                return Ok(new
                {
                    Success = true,
                    Message = "Estatísticas obtidas com sucesso.",
                    Data = new
                    {
                        ReceitaDoMes = receitaDoMes,
                        LimiteCartoes = limiteCartoes,
                        MediaDespesa = mediaDespesa,
                        MedianaDespesa = medianaDespesa,
                        VarianciaDespesa = varianciaDespesa,
                        DesvioPadraoDespesa = desvioPadraoDespesa,
                        IntervaloConfiançaInferior = intervaloConfiança.Inferior,
                        IntervaloConfiançaSuperior = intervaloConfiança.Superior,
                        ProporcaoFormaPagamento = proporcaoFormaPagamento,
                        ProporcaoCategoria = proporcaoCategoria,
                        DistribuicaoBinomial = 0.0, 
                        DistribuicaoNormal = distribuicaoNormal
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Erro ao obter estatísticas.", Error = ex.Message });
            }
        }



        private bool DespesaExists(int id)
        {
            return _context.Despesa.Any(e => e.DespesaId == id && e.DataDelecao == null);
        }

        private bool IsAdmin(int id)
        {
            return _context.Usuarios
                .Any(u => u.UsuarioId == id && u.Email == "user.admin@gmail.com" && u.DataDelecao == null);
        }

    }
}
