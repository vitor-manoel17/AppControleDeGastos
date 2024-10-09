using System;
using System.ComponentModel.DataAnnotations;

namespace AppControleDeGastos.Models.ReceitaModel
{
    public class CreateReceitaRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor da receita deve ser positivo.")]
        public decimal Valor { get; set; }

        [Required]
        public DateTime Data { get; set; }

        [MaxLength(255)]
        public string Descricao { get; set; }
    }
}
