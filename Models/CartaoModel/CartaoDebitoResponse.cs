using System;

namespace AppControleDeGastos.Models.CartaoModel
{
    public class CartaoDebitoResponse
    {
        public int CartaoDebitoId { get; set; }
        public string Numero { get; set; }
        public string Bandeira { get; set; }
        public decimal Saldo { get; set; }
        public DateTime DataValidade { get; set; }
    }
}
