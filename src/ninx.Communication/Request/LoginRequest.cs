using System.ComponentModel.DataAnnotations;

namespace ninx.Communication.Request
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ser um endereço de email válido")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Senha é obrigatória")]
        public string Senha { get; set; }
        [Required(ErrorMessage = "ComércioID é obrigatório")]
        public int ComercioID { get; set; }
    }
}
