using System.ComponentModel.DataAnnotations;

namespace ninx.Communication.Request
{
    public class ConfirmarAssinaturaEletronicaRequest
    {
        [Required]
        public string ImagemBase64 { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
