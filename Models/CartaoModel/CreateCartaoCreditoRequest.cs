using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppControleDeGastos.Models.CartaoCreditoModel
{
    public class CreateCartaoCreditoRequest
    {
        [Required]
        [StringLength(16, ErrorMessage = "O número do cartão não pode ter mais que 16 caracteres.")]
        public string Numero { get; set; }  // Número do cartão de crédito

        [Required]
        [StringLength(50, ErrorMessage = "A bandeira do cartão não pode ter mais que 50 caracteres.")]
        public string Bandeira { get; set; }  // Bandeira do cartão (por exemplo, "Visa", "MasterCard")

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 9999999999999999.99, ErrorMessage = "O limite de crédito deve ser um valor positivo.")]
        public decimal Limite { get; set; }  // Limite de crédito do cartão

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 9999999999999999.99, ErrorMessage = "O valor atual deve ser um valor positivo.")]
        public decimal ValorAtual { get; set; }  // Valor utilizado no cartão

        [Required]
        [Range(1, 31, ErrorMessage = "O dia de fechamento deve ser entre 1 e 31.")]
        public int DiaFechamento { get; set; }  // Dia de fechamento da fatura do cartão

        [Required]
        [DataType(DataType.Date)]
        public DateTime DataValidade { get; set; }  // Data de validade do cartão
    }
}
