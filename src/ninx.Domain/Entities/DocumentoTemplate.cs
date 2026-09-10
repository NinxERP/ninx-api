using ninx.Domain.Enums;

namespace ninx.Domain.Entities
{
    public class DocumentoTemplate
    {
        public int TemplateID { get; set; }
        public TipoDocumento TipoDocumento { get; set; }
        public string ConteudoHtml { get; set; } = null!;
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AtualizadoEm { get; set; }
    }
}
