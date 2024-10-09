namespace AppControleDeGastos.Models.CartaoCreditoModel
{
    /// <summary>
    /// Representa a resposta contendo os dados de um cartão de crédito.
    /// </summary>
    public class CartaoCreditoResponse
    {
        /// <summary>
        /// Identificador único do cartão de crédito.
        /// </summary>
        public int CartaoCreditoId { get; set; }

        /// <summary>
        /// Número do cartão de crédito.
        /// </summary>
        public string Numero { get; set; }

        /// <summary>
        /// Tipo do cartão de crédito (por exemplo, "Pessoal", "Corporativo").
        /// </summary>
        public string Tipo { get; set; }

        /// <summary>
        /// Bandeira do cartão de crédito (por exemplo, "Visa", "MasterCard").
        /// </summary>
        public string Bandeira { get; set; }

        /// <summary>
        /// Limite de crédito disponível no cartão.
        /// </summary>
        public decimal Limite { get; set; }

        /// <summary>
        /// Valor atual utilizado no cartão de crédito.
        /// </summary>
        public decimal ValorAtual { get; set; }

        /// <summary>
        /// Dia do mês em que a fatura do cartão é fechada.
        /// </summary>
        public int DiaFechamento { get; set; }
    }
}
