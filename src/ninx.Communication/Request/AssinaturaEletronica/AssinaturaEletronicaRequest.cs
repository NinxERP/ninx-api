using System.ComponentModel.DataAnnotations;

namespace ninx.Communication
{
    /// <summary>
    /// O portal envia só os traços; o servidor desenha a assinatura sobre o PDF original que ele mesmo gerou.
    /// </summary>
    public class ConfirmarAssinaturaEletronicaRequest
    {
        [Required]
        public List<TracoAssinaturaRequest> Tracos { get; set; } = new();
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class TracoAssinaturaRequest
    {
        /// <summary>Página do PDF, começando em 1.</summary>
        public int Pagina { get; set; }

        /// <summary>Espessura da linha em pontos do PDF.</summary>
        public double Espessura { get; set; }

        /// <summary>Pontos normalizados de 0 a 1, com origem no canto superior esquerdo da página.</summary>
        public List<PontoAssinaturaRequest> Pontos { get; set; } = new();
    }

    public class PontoAssinaturaRequest
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
