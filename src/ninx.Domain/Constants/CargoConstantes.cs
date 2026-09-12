namespace ninx.Domain.Constants
{
    public static class CargoConstantes
    {
        public const string NomeAdmin = "Admin";
    }

    public static class PermissaoConstantes
    {
        public const string GerenciarUsuarios = "GerenciarUsuarios";
        public const string GerenciarCargos = "GerenciarCargos";
        public const string GerenciarComercio = "GerenciarComercio";
        public const string GerenciarAssinatura = "GerenciarAssinatura";
        public const string GerenciarPagamentos = "GerenciarPagamentos";
        public const string VisualizarRelatorios = "VisualizarRelatorios";

        public static readonly string[] Todas =
        [
            GerenciarUsuarios,
            GerenciarCargos,
            GerenciarComercio,
            GerenciarAssinatura,
            GerenciarPagamentos,
            VisualizarRelatorios
        ];
    }
}
