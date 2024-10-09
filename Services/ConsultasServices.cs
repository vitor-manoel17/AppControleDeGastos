using AppControleDeGastos.Data;
using AppControleDeGastos.Models;
using AppControleDeGastos.Models.CartaoModel;
using AppControleDeGastos.Models.DespesaModel;
using AppControleDeGastos.Models.ReceitaModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppControleDeGastos.Services
{
    public class ConsultasServices
    {
        private readonly ApplicationDbContext _context;

        // Construtor para injetar o contexto do banco de dados
        public ConsultasServices(ApplicationDbContext context)
        {
            _context = context;
        }

        // Consulta para obter todas as despesas que não foram deletadas
        public async Task<List<Despesa>> GetDespesasAsync()
        {
            return await _context.Despesa.Where(d => d.DataDelecao == null).ToListAsync();
        }

        // Consulta para obter todas as receitas que não foram deletadas
        public async Task<List<Receita>> GetReceitasAsync()
        {
            return await _context.Receitas.Where(r => r.DataDelecao == null).ToListAsync();
        }

        // Consulta para obter todos os cartões de crédito que não foram deletados
        public async Task<List<Cartao>> GetCartoesCreditoAsync()
        {
            return await _context.Cartoes.Where(c => c.DataDelecao == null && c.Type == "Credit").ToListAsync();
        }

        // Consulta para obter todos os cartões de débito que não foram deletados
        public async Task<List<Cartao>> GetCartoesDebitoAsync()
        {
            return await _context.Cartoes.Where(c => c.DataDelecao == null && c.Type == "Debit").ToListAsync();
        }

        // Consulta para obter todas as despesas de um usuário específico
        public async Task<List<Despesa>> GetDespesasPorUsuarioAsync(int usuarioId)
        {
            return await _context.Despesa.Where(d => d.UsuarioId == usuarioId && d.DataDelecao == null).ToListAsync();
        }

        // Consulta para obter todas as receitas de um usuário específico
        public async Task<List<Receita>> GetReceitasPorUsuarioAsync(int usuarioId)
        {
            return await _context.Receitas.Where(r => r.UsuarioId == usuarioId && r.DataDelecao == null).ToListAsync();
        }

        // Consulta para obter o saldo total de um cartão de crédito específico
        public async Task<decimal> GetSaldoCartaoCreditoAsync(int cartaoId)
        {
            var cartaoCredito = await _context.Cartoes.FirstOrDefaultAsync(c => c.CartaoId == cartaoId && c.DataDelecao == null && c.Type == "Credit");
            return cartaoCredito?.ValorAtual ?? 0;
        }

        // Consulta para obter o saldo total de um cartão de débito específico
        public async Task<decimal> GetSaldoCartaoDebitoAsync(int cartaoId)
        {
            var cartaoDebito = await _context.Cartoes.FirstOrDefaultAsync(c => c.CartaoId == cartaoId && c.DataDelecao == null && c.Type == "Debit");
            return cartaoDebito?.Saldo ?? 0;
        }
    }
}
