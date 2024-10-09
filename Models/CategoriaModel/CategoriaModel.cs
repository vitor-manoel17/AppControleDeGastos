using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppControleDeGastos.Models.CategoriaModel
{
    [Table("Categorias")]
    public class Categoria
    {
        [Key]
        public int CategoriaId { get; set; }  // Chave primária

        [Required]
        [StringLength(100, ErrorMessage = "O nome da categoria não pode ter mais que 100 caracteres.")]
        public string Nome { get; set; }  // Nome da categoria

        [Required]
        public DateTime DataCriacao { get; set; } = DateTime.Now;  // Data de criação, com valor padrão para a data atual

        [Required]
        public DateTime DataAtualizacao { get; set; } = DateTime.Now;  // Data de última atualização, com valor padrão para a data atual

        public DateTime? DataDelecao { get; set; }  // Exclusão lógica
    }
}
