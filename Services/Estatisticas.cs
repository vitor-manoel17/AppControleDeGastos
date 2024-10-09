using System;
using System.Collections.Generic;
using System.Linq;
using AppControleDeGastos.Models.DespesaModel;
using MathNet.Numerics.Distributions;

namespace AppControleDeGastos.Services
{
    public static class Estatisticas
    {
        // Cálculos Estatísticos

        public static decimal Media(IEnumerable<decimal> valores)
        {
            if (valores == null || !valores.Any())
                throw new ArgumentException("A lista de valores não pode estar vazia.");

            return valores.Average();
        }

        public static decimal Mediana(IEnumerable<decimal> valores)
        {
            if (valores == null || !valores.Any())
                throw new ArgumentException("A lista de valores não pode estar vazia.");

            var sorted = valores.OrderBy(v => v).ToList();
            int count = sorted.Count;
            if (count % 2 == 0)
            {
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;
            }
            else
            {
                return sorted[count / 2];
            }
        }

        public static decimal Variancia(IEnumerable<decimal> valores)
        {
            if (valores == null || !valores.Any())
                throw new ArgumentException("A lista de valores não pode estar vazia.");

            decimal media = Media(valores);
            return valores.Average(v => (v - media) * (v - media));
        }

        public static decimal DesvioPadrao(IEnumerable<decimal> valores)
        {
            if (valores == null || !valores.Any())
                throw new ArgumentException("A lista de valores não pode estar vazia.");

            return (decimal)Math.Sqrt((double)Variancia(valores));
        }

        public static (decimal Inferior, decimal Superior) IntervaloConfiança(decimal media, decimal desvioPadrao, int tamanho, double nivelConfiança = 0.95)
        {
            if (tamanho <= 0)
                throw new ArgumentException("O tamanho da amostra deve ser maior que zero.");

            // Para um nível de confiança de 95%, o valor de Z é aproximadamente 1.96
            double z = 1.96;

            // Converta o desvio padrão para double para a operação de divisão
            double erroPadrao = (double)desvioPadrao / Math.Sqrt(tamanho);

            // Calcule o intervalo de confiança
            decimal intervaloInferior = media - (decimal)(erroPadrao * z);
            decimal intervaloSuperior = media + (decimal)(erroPadrao * z);

            return (intervaloInferior, intervaloSuperior);
        }

        // Proporções e Distribuições

        public static decimal Proporcao(IEnumerable<Despesa> despesas, string formaPagamento)
        {
            if (despesas == null || !despesas.Any())
                throw new ArgumentException("A lista de despesas não pode estar vazia.");

            int total = despesas.Count();
            int count = despesas.Count(d => d.FormaPagamento.Equals(formaPagamento, StringComparison.OrdinalIgnoreCase));
            return total == 0 ? 0 : (decimal)count / total;
        }

        public static decimal ProporcaoCategoria(IEnumerable<Despesa> despesas, int categoriaId)
        {
            if (despesas == null || !despesas.Any())
                throw new ArgumentException("A lista de despesas não pode estar vazia.");

            int total = despesas.Count();
            int count = despesas.Count(d => d.CategoriaId == categoriaId);
            return total == 0 ? 0 : (decimal)count / total;
        }

        // Distribuição Normal

        public static double DistribuicaoNormal(double valor, double media, double desvioPadrao)
        {
            var normal = new Normal(media, desvioPadrao);
            return normal.Density(valor);
        }
    }
}
