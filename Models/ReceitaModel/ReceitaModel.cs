using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppControleDeGastos.Models.UsuarioModel; // Adicione este namespace

namespace AppControleDeGastos.Models.ReceitaModel
{
    [Table("Receitas")]
    public class Receita
    {
        [Key]
        public int ReceitaId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }

        [Required]
        public DateTime Data { get; set; }

        [MaxLength(255)]
        public string Descricao { get; set; }

        // Relacionamento com o Usuário
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }

        // Campos adicionais
        public DateTime? DataDelecao { get; set; } // Campo para deletar de forma lógica

        [Required]
        public DateTime DataAtualizacao { get; set; } // Campo obrigatório para rastrear atualizações
    }
}
