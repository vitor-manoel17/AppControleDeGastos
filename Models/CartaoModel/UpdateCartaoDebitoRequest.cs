using System;
using System.ComponentModel.DataAnnotations;

namespace AppControleDeGastos.Models.CartaoModel
{
    public class UpdateCartaoDebitoRequest
    {
      
        [StringLength(50, ErrorMessage = "O número do cartão não pode exceder 50 caracteres.")]
        public string Numero { get; set; }

        // Bandeira do cartão (por exemplo, "Visa", "MasterCard")
        [StringLength(50, ErrorMessage = "A bandeira do cartão não pode exceder 50 caracteres.")]
        public string Bandeira { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "O saldo deve ser um valor positivo.")]
        public decimal? Saldo { get; set; }  

        
        [DataType(DataType.Date)]
        public DateTime? DataValidade { get; set; }  
    }
}
