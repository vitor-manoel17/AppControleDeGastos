using System;
using System.ComponentModel.DataAnnotations;

namespace AppControleDeGastos.Models.ReceitaModel
{
    public class ReceitaResponse
    {
        [Required]
        public int ReceitaId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }

        [Required]
        public DateTime Data { get; set; }

        [MaxLength(255)]
        public string Descricao { get; set; }
    }
}
