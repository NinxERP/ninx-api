using ninx.Domain.Enums;

namespace ninx.Domain.Entities
{
    /// <summary>
    /// Documento que o titular assina uma vez para abrir a conta de fiado: reconhece como suas as
    /// compras registradas na conta e lista as pessoas autorizadas a comprar em seu nome.
    /// </summary>
    public class TermoAberturaConta
    {
        public int TermoAberturaID { get; set; }
        public int ClienteID { get; set; }

        /// <summary>Sequencial por cliente. Nenhuma versão é apagada: as anteriores ficam como histórico.</summary>
        public int Versao { get; set; }
        public StatusTermoAbertura Status { get; set; } = StatusTermoAbertura.Aguardando;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AssinadoEm { get; set; }
        public Cliente Cliente { get; set; } = null!;
    }
}
