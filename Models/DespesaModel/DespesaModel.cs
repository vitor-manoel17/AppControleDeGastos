using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppControleDeGastos.Models.CategoriaModel;
using AppControleDeGastos.Models.UsuarioModel;
using AppControleDeGastos.Models.CartaoModel;

namespace AppControleDeGastos.Models.DespesaModel
{
    [Table("Despesa")]
    public class Despesa
    {
        [Key]
        public int DespesaId { get; set; }  // Chave primária

        [Required]
        public int UsuarioId { get; set; }  // Chave estrangeira para o usuário
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }  // Relacionamento com a tabela de usuário

        [Required]
        public int CategoriaId { get; set; }  // Chave estrangeira para a categoria
        [ForeignKey("CategoriaId")]
        public Categoria Categoria { get; set; }  // Relacionamento com a tabela de categoria

        public int? CartaoId { get; set; }  // Chave estrangeira para cartão de crédito, caso seja usado
        [ForeignKey("CartaoId")]
        public Cartao Cartao { get; set; }  // Relacionamento com a tabela de cartão de crédito

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "O valor da despesa deve ser positivo.")]
        public decimal Valor { get; set; }  // Valor da despesa

        [Required]
        public DateTime Data { get; set; }  // Data da despesa

        [StringLength(255)]
        public string Descricao { get; set; }  // Descrição opcional da despesa

        [Required]
        [StringLength(50)]
        public string FormaPagamento { get; set; }  // Forma de pagamento (cartão de crédito, débito, dinheiro, etc.)

        public int? NumeroParcelas { get; set; }  // Número de parcelas (somente para cartão de crédito)

        [Required]
        public bool Pago { get; set; }  // Indica se a despesa foi paga ou não

        [Required]
        public DateTime DataCriacao { get; set; } = DateTime.Now;  // Data de criação da despesa

        [Required]
        public DateTime DataAtualizacao { get; set; } = DateTime.Now;  // Data de última atualização da despesa

        public DateTime? DataDelecao { get; set; }  // Exclusão lógica
    }
}
