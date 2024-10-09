using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppControleDeGastos.Models.CartaoModel
{
    public class CreateCartaoDebitoRequest
    {
        [Required]
        [StringLength(16, ErrorMessage = "O número do cartão não pode ter mais que 16 caracteres.")]
        [MinLength(16, ErrorMessage = "O número do cartão deve ter 16 caracteres.")]
        public string Numero { get; set; }  // Número do cartão de débito

        [Required]
        [StringLength(50, ErrorMessage = "A bandeira do cartão não pode ter mais que 50 caracteres.")]
        public string Bandeira { get; set; }  // Bandeira do cartão (ex: Visa, MasterCard)

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 9999999999999999.99, ErrorMessage = "O saldo deve ser um valor positivo.")]
        public decimal Saldo { get; set; }  // Saldo disponível no cartão de débito

        [Required]
        [DataType(DataType.Date)]
        public DateTime DataValidade { get; set; }  // Data de validade do cartão

        [Required]
        [StringLength(10, ErrorMessage = "O tipo do cartão deve ser 'Debit'.")]
        [RegularExpression("Debit", ErrorMessage = "O tipo do cartão deve ser 'Debit'.")]
        public string Type { get; set; } = "Debit";  // Tipo do cartão, que será sempre 'Debit' neste caso
    }
}
