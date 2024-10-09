using System.ComponentModel.DataAnnotations;

namespace AppControleDeGastos.Models.UsuarioModel
{
    public class GetUserByIdRequest
    {
        [Required]
        public string Method { get; set; }

        [Required]
        public int UsuarioId { get; set; }
    }
}
