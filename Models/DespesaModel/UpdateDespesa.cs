using System;
using System.ComponentModel.DataAnnotations;

namespace AppControleDeGastos.Models.DespesaModel
{
    public class UpdateDespesa
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor da despesa deve ser positivo.")]
        public decimal? Valor { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Data { get; set; }

        [StringLength(255, ErrorMessage = "A descrição não pode exceder 255 caracteres.")]
        public string Descricao { get; set; }

        [StringLength(50, ErrorMessage = "A forma de pagamento não pode exceder 50 caracteres.")]
        public string FormaPagamento { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O número de parcelas deve ser maior que zero.")]
        public int? NumeroParcelas { get; set; }

        public bool? Pago { get; set; }

        // Adicionar a propriedade CategoriaId
        public int? CategoriaId { get; set; }

        // Cartão de crédito ou débito (caso forma de pagamento seja com cartão)
        public int? CartaoCreditoId { get; set; }
        public int? CartaoDebitoId { get; set; }
    }
}
