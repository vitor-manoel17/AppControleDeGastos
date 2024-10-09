using System;
using System.ComponentModel.DataAnnotations;

namespace AppControleDeGastos.Models.DespesaModel
{
    public class RequestDespesa
    {
        [Required(ErrorMessage = "O valor da despesa é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor da despesa deve ser positivo.")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "A data da despesa é obrigatória.")]
        [DataType(DataType.Date, ErrorMessage = "Data inválida.")]
        public DateTime Data { get; set; }

        [StringLength(255, ErrorMessage = "A descrição não pode exceder 255 caracteres.")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "A forma de pagamento é obrigatória.")]
        [StringLength(50, ErrorMessage = "A forma de pagamento não pode exceder 50 caracteres.")]
        public string FormaPagamento { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O número de parcelas deve ser maior que zero.")]
        public int? NumeroParcelas { get; set; }  // Opcional, usado apenas para cartão de crédito

        [Required(ErrorMessage = "O status de pagamento é obrigatório.")]
        public bool Pago { get; set; }

        // IDs opcionais para cartão de crédito ou débito
        public int? CartaoCreditoId { get; set; }
        public int? CartaoDebitoId { get; set; }

        [Required(ErrorMessage = "A categoria da despesa é obrigatória.")]
        public int CategoriaId { get; set; }
    }
}
