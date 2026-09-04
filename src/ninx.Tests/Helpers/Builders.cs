using ninx.Domain.Entities;
using ninx.Domain.Enums;

namespace ninx.Tests.Helpers
{
    /// <summary>
    /// Fábricas simples para montar entidades de domínio válidas nos testes,
    /// evitando repetir a criação de objetos "boilerplate" em cada teste.
    /// </summary>
    public static class Builders
    {
        public static Usuario NovoUsuario(int id = 1, bool ativo = true, bool admin = false) => new()
        {
            UsuarioID = id,
            Nome = "Usuário Teste",
            Email = $"usuario{id}@teste.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("senha123"),
            Admin = admin,
            Ativo = ativo
        };

        public static Cargo NovoCargo(int id = 1, int peso = 50, int? comercioId = 1, bool ativo = true) => new()
        {
            CargoID = id,
            Nome = "Gerente",
            Peso = peso,
            ComercioID = comercioId,
            Ativo = ativo
        };

        public static Comercio NovoComercio(int id = 1, string nome = "Comércio Teste") => new()
        {
            ComercioID = id,
            NomeComercio = nome,
            Ativo = true
        };

        public static UsuarioComercio NovoVinculo(int usuarioId = 1, int comercioId = 1, Cargo? cargo = null) => new()
        {
            UsuarioComercioID = 1,
            UsuarioID = usuarioId,
            ComercioID = comercioId,
            Cargo = cargo ?? NovoCargo(comercioId: comercioId),
            CargoID = (cargo ?? NovoCargo(comercioId: comercioId)).CargoID,
            Comercio = NovoComercio(comercioId),
            Usuario = NovoUsuario(usuarioId),
            Ativo = true
        };

        public static Cliente NovoCliente(int id = 1, int comercioId = 1, decimal? limiteCredito = 500m) => new()
        {
            ClienteID = id,
            ComercioID = comercioId,
            Nome = "Cliente Teste",
            Cpf = "12345678909",
            EnderecoLogradouro = "Rua Teste",
            EnderecoNumero = "100",
            EnderecoBairro = "Centro",
            EnderecoCidade = "Cidade Teste",
            EnderecoUF = "SP",
            EnderecoCEP = "01000000",
            LimiteCredito = limiteCredito,
            Ativo = true
        };

        public static Produto NovoProduto(int id = 1, int comercioId = 1, decimal precoVenda = 10m) => new()
        {
            ProdutoID = id,
            ComercioID = comercioId,
            Nome = "Produto Teste",
            PrecoVenda = precoVenda,
            UnidadeMedida = UnidadeMedida.UN,
            Ativo = true
        };

        public static Estoque NovoEstoque(int produtoId = 1, int comercioId = 1, decimal quantidade = 100m) => new()
        {
            EstoqueID = 1,
            ProdutoID = produtoId,
            ComercioID = comercioId,
            Quantidade = quantidade,
            QuantidadeMinima = 1,
            RowVersion = new byte[] { 1 }
        };

        public static Venda NovaVenda(
            int id = 1,
            int comercioId = 1,
            int usuarioId = 1,
            int? clienteId = null,
            decimal total = 100m,
            StatusVenda status = StatusVenda.Finalizada,
            TipoVenda tipoVenda = TipoVenda.Normal) => new()
        {
            VendaID = id,
            ComercioID = comercioId,
            UsuarioID = usuarioId,
            ClienteID = clienteId,
            Total = total,
            Status = status,
            TipoVenda = tipoVenda,
            ItensVenda = new List<ItemVenda>(),
            PagamentosVenda = new List<PagamentoVenda>()
        };

        public static AssinaturaEletronica NovaAssinatura(
            int vendaId = 1,
            Guid? guid = null,
            bool assinado = false,
            StatusAssinatura status = StatusAssinatura.Ativa,
            TipoDocumento tipoDocumento = TipoDocumento.TermoCompromisso) => new()
        {
            AssinaturaID = 1,
            VendaID = vendaId,
            DocumentoGuid = guid ?? Guid.NewGuid(),
            Assinado = assinado,
            Status = status,
            TipoDocumento = tipoDocumento,
            DocumentoHtmlMesclado = "<html></html>",
            DocumentoOriginalBase64 = "base64"
        };

        public static AssinaturaPlano NovaAssinaturaPlano(
            int comercioId = 1,
            StatusAssinatura status = StatusAssinatura.Ativa,
            DateTime? dataFim = null) => new()
        {
            AssinaturaID = 1,
            ComercioID = comercioId,
            Plano = PlanoAssinatura.Mensal,
            DataInicio = DateTime.UtcNow.AddMonths(-1),
            DataFim = dataFim ?? DateTime.UtcNow.AddMonths(1),
            Status = status
        };
    }
}
