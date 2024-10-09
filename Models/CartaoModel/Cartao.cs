using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppControleDeGastos.Models.UsuarioModel;

namespace AppControleDeGastos.Models.CartaoModel
{
    [Table("Cartoes")]  
    public class Cartao
    {
        [Key]
        public int CartaoId { get; set; }  // Identificador do cartão

        [Required]
        public int UsuarioId { get; set; }  // ID do usuário associado ao cartão

        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }  // Relacionamento com o usuário

        [Required]
        [StringLength(16, ErrorMessage = "O número do cartão não pode ter mais que 16 caracteres.")]
        [MinLength(16, ErrorMessage = "O número do cartão deve ter 16 caracteres.")]
        public string Numero { get; set; }  // Número do cartão (débito ou crédito)

        [Required]
        [StringLength(50, ErrorMessage = "A bandeira do cartão não pode ter mais que 50 caracteres.")]
        public string Bandeira { get; set; }  // Bandeira do cartão (ex: Visa, MasterCard)

        [Required]
        [StringLength(10, ErrorMessage = "O tipo do cartão deve ser 'Debit' ou 'Credit'.")]
        [RegularExpression("Debit|Credit", ErrorMessage = "O tipo do cartão deve ser 'Debit' ou 'Credit'.")]
        public string Type { get; set; }  // Tipo do cartão (Débito ou Crédito)

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 9999999999999999.99, ErrorMessage = "O limite de crédito deve ser um valor positivo.")]
        public decimal? Limite { get; set; }  // Limite de crédito (somente aplicável para cartões de crédito)

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 9999999999999999.99, ErrorMessage = "O saldo deve ser um valor positivo.")]
        public decimal? Saldo { get; set; }  // Saldo disponível (somente aplicável para cartões de débito)

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 9999999999999999.99, ErrorMessage = "O valor utilizado deve ser um valor positivo.")]
        public decimal? ValorAtual { get; set; }  // Valor utilizado (aplicável para cartões de crédito)

        [Range(1, 31, ErrorMessage = "O dia de fechamento deve ser entre 1 e 31.")]
        public int? DiaFechamento { get; set; }  // Dia de fechamento da fatura do cartão (somente aplicável para crédito)

        [Required]
        [DataType(DataType.Date)]
        public DateTime DataValidade { get; set; }  

        public DateTime? DataDelecao { get; set; }  

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DataCriacao { get; set; }  

        [DataType(DataType.DateTime)]
        public DateTime? DataAtualizacao { get; set; }  
    }
}
