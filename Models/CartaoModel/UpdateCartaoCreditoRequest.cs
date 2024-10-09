using System;
using System.ComponentModel.DataAnnotations;

namespace AppControleDeGastos.Models.CartaoCreditoModel
{
    public class UpdateCartaoCreditoRequest
    {
        // Número do cartão de crédito
        [StringLength(50, ErrorMessage = "O número do cartão não pode exceder 50 caracteres.")]
        public string Numero { get; set; }

        // Bandeira do cartão (por exemplo, "Visa", "MasterCard")
        [StringLength(50, ErrorMessage = "A bandeira do cartão não pode exceder 50 caracteres.")]
        public string Bandeira { get; set; }

        // Limite de crédito do cartão
        [Range(0, double.MaxValue, ErrorMessage = "O limite do cartão deve ser um valor positivo.")]
        public decimal? Limite { get; set; }  // Tornar o limite opcional

        // Valor atual utilizado do cartão de crédito
        [Range(0, double.MaxValue, ErrorMessage = "O valor atual do cartão deve ser um valor positivo.")]
        public decimal? ValorAtual { get; set; }  // Tornar o valor atual opcional

        // Dia de fechamento da fatura do cartão (de 1 a 31)
        [Range(1, 31, ErrorMessage = "O dia de fechamento deve estar entre 1 e 31.")]
        public int? DiaFechamento { get; set; }  // Tornar o dia de fechamento opcional
    }
}
