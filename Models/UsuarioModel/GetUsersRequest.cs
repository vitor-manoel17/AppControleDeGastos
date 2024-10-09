using System.ComponentModel.DataAnnotations;

namespace AppControleDeGastos.Models.UsuarioModel
{
    public class GetUsersRequest
    {
        [Required]
        public string Method { get; set; }

        [Required]
        public int UsuarioId { get; set; }
    }
}
