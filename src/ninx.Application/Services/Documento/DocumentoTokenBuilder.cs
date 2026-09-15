using System.Net;
using System.Text;
using ninx.Communication.Helpers;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Regras;

namespace ninx.Application.Services
{
    public static class DocumentoTokenBuilder
    {
        private const string MarcadorInicio = "<!--BLOCO_ASSINATURA_INICIO-->";
        private const string MarcadorFim = "<!--BLOCO_ASSINATURA_FIM-->";

        public static Dictionary<string, string> BuildTermoCompromissoTokens(Venda venda, Cliente cliente, Comercio comercio)
        {
            var totalPago = SaldoDevedor.TotalPago(venda.PagamentosVenda);
            var saldoDevedor = SaldoDevedor.DaVenda(venda);

            var tokens = BuildTokensComuns(cliente, comercio, venda.CriadoEm);
            tokens["Html.TabelaItens"] = BuildTabelaItensHtml(venda);
            tokens["Venda.Total"] = $"R$ {venda.Total:N2}";
            tokens["Venda.ValorPago"] = $"R$ {totalPago:N2}";
            tokens["Venda.SaldoDevedor"] = $"R$ {saldoDevedor:N2}";
            tokens["Venda.DataVencimento"] = venda.DataVencimento?.ToString("dd/MM/yyyy") ?? "Não definida";
            return tokens;
        }

        public static Dictionary<string, string> BuildReciboPagamentoTokens(Venda venda, PagamentoVenda pagamento, Cliente cliente, Comercio comercio, decimal saldoDevedorAnterior)
        {
            var novoSaldoDevedor = saldoDevedorAnterior - pagamento.Valor;

            var tokens = BuildTokensComuns(cliente, comercio, pagamento.CriadoEm);
            tokens["Venda.VendaID"] = venda.VendaID.ToString();
            tokens["Pagamento.Valor"] = $"R$ {pagamento.Valor:N2}";
            tokens["Pagamento.FormaPagamento"] = pagamento.FormaPagamento.ToString();
            tokens["Venda.SaldoAnterior"] = $"R$ {saldoDevedorAnterior:N2}";
            tokens["Venda.SaldoNovo"] = $"R$ {novoSaldoDevedor:N2}";
            return tokens;
        }

        public static Dictionary<string, string> BuildReciboQuitacaoGlobalTokens(List<ItemAbatimentoGlobal> abatimentos, decimal valorTotal, FormaPagamento formaPagamento, Cliente cliente, Comercio comercio, DateTime dataOperacao)
        {
            var tokens = BuildTokensComuns(cliente, comercio, dataOperacao);
            tokens["Html.TabelaDistribuicao"] = BuildTabelaDistribuicaoHtml(abatimentos);
            tokens["ValorTotal"] = $"R$ {valorTotal:N2}";
            tokens["FormaPagamento"] = formaPagamento.ToString();
            return tokens;
        }

        private static Dictionary<string, string> BuildTokensComuns(Cliente cliente, Comercio comercio, DateTime data)
        {
            return new Dictionary<string, string>
            {
                ["Comercio.Nome"] = comercio.NomeComercio,
                ["Comercio.Cnpj"] = comercio.CNPJ ?? "Não informado",
                ["Comercio.Endereco"] = FormatarEnderecoComercio(comercio),
                ["Html.ComercioAssinatura"] = BuildComercioAssinaturaHtml(comercio),
                ["Cliente.Nome"] = cliente.Nome,
                ["Cliente.Cpf"] = FormatarCpf(cliente.Cpf),
                ["Cliente.Endereco"] = FormatarEnderecoCliente(cliente),
                ["Cliente.Telefone"] = cliente.Telefone ?? "Não informado",
                ["Data"] = data.ToString("dd/MM/yyyy HH:mm:ss") + " UTC",
                ["Html.BlocoAssinatura"] = BuildBlocoAssinaturaPendente()
            };
        }

        public static string FormatarCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf) || cpf.Length != 11)
                return "Não informado";

            return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
        }

        public static string FormatarEnderecoCliente(Cliente cliente)
        {
            var complemento = string.IsNullOrWhiteSpace(cliente.EnderecoComplemento) ? "" : $", {cliente.EnderecoComplemento}";
            return $"{cliente.EnderecoLogradouro}, {cliente.EnderecoNumero}{complemento} - {cliente.EnderecoBairro}, {cliente.EnderecoCidade}/{cliente.EnderecoUF} - CEP {cliente.EnderecoCEP}";
        }

        public static string FormatarEnderecoComercio(Comercio comercio)
        {
            if (string.IsNullOrWhiteSpace(comercio.EnderecoLogradouro))
                return "Não informado";

            // Partes vazias são omitidas: comércios que só tinham o endereço legado em texto
            // livre passam a tê-lo no logradouro, sem número, bairro ou CEP estruturados.
            var endereco = comercio.EnderecoLogradouro;
            if (!string.IsNullOrWhiteSpace(comercio.EnderecoNumero)) endereco += $", {comercio.EnderecoNumero}";
            if (!string.IsNullOrWhiteSpace(comercio.EnderecoComplemento)) endereco += $", {comercio.EnderecoComplemento}";
            if (!string.IsNullOrWhiteSpace(comercio.EnderecoBairro)) endereco += $" - {comercio.EnderecoBairro}";

            var cidadeUf = string.Join("/", new[] { comercio.EnderecoCidade, comercio.EnderecoUF }.Where(s => !string.IsNullOrWhiteSpace(s)));
            if (cidadeUf.Length > 0) endereco += $", {cidadeUf}";
            if (!string.IsNullOrWhiteSpace(comercio.EnderecoCEP)) endereco += $" - CEP {comercio.EnderecoCEP}";

            return endereco;
        }

        private static string BuildComercioAssinaturaHtml(Comercio comercio)
        {
            if (string.IsNullOrWhiteSpace(comercio.AssinaturaResponsavelBase64))
                return "";

            return $"<img src=\"data:image/png;base64,{comercio.AssinaturaResponsavelBase64}\" style=\"max-height:60px;\" />";
        }

        private static string BuildTabelaItensHtml(Venda venda)
        {
            var sb = new StringBuilder();
            sb.Append("<table style=\"width:100%;border-collapse:collapse;\">");
            sb.Append("<thead><tr>");
            sb.Append(CelulaCabecalhoItens("Descrição do Produto", "left"));
            sb.Append(CelulaCabecalhoItens("Qtd.", "center"));
            sb.Append(CelulaCabecalhoItens("VL. Unitário", "right"));
            sb.Append(CelulaCabecalhoItens("Subtotal", "right"));
            sb.Append("</tr></thead><tbody>");

            foreach (var item in venda.ItensVenda)
            {
                sb.Append("<tr>");
                sb.Append($"<td style=\"padding:10px;border-bottom:0.5px solid #E2E8F0;font-size:10pt;\">{WebUtility.HtmlEncode(item.ProdutoNome)}</td>");
                sb.Append($"<td style=\"padding:10px;border-bottom:0.5px solid #E2E8F0;font-size:10pt;text-align:center;\">{item.Quantidade:N2}</td>");
                sb.Append($"<td style=\"padding:10px;border-bottom:0.5px solid #E2E8F0;font-size:10pt;text-align:right;\">R$ {item.PrecoUnitario:N2}</td>");
                sb.Append($"<td style=\"padding:10px;border-bottom:0.5px solid #E2E8F0;font-size:10pt;text-align:right;\">R$ {item.Subtotal:N2}</td>");
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");
            return sb.ToString();
        }

        private static string CelulaCabecalhoItens(string texto, string alinhamento)
        {
            return $"<th style=\"background:#F8FAFC;border-bottom:1.5px solid #64748B;padding:8px;font-size:9.5pt;color:#64748B;text-align:{alinhamento};\">{WebUtility.HtmlEncode(texto)}</th>";
        }

        private static string BuildTabelaDistribuicaoHtml(List<ItemAbatimentoGlobal> abatimentos)
        {
            var sb = new StringBuilder();
            sb.Append("<table style=\"width:100%;border-collapse:collapse;\">");
            sb.Append("<thead><tr>");
            foreach (var h in new[] { "Cód. Venda", "Data Venda", "Saldo Anterior", "Valor Abatido", "Saldo Restante" })
                sb.Append($"<th style=\"background:#0D1B2A;color:#FFFFFF;padding:6px;font-size:9pt;text-align:center;\">{WebUtility.HtmlEncode(h)}</th>");
            sb.Append("</tr></thead><tbody>");

            foreach (var item in abatimentos)
            {
                sb.Append("<tr>");
                sb.Append($"<td style=\"padding:5px;font-size:9pt;text-align:center;\">#{item.VendaId}</td>");
                sb.Append($"<td style=\"padding:5px;font-size:9pt;text-align:center;\">{item.DataVenda:dd/MM/yyyy}</td>");
                sb.Append($"<td style=\"padding:5px;font-size:9pt;text-align:right;\">R$ {item.SaldoAnterior:N2}</td>");
                sb.Append($"<td style=\"padding:5px;font-size:9pt;text-align:right;color:#0EA5E9;\">R$ {item.ValorAbatido:N2}</td>");
                sb.Append($"<td style=\"padding:5px;font-size:9pt;text-align:right;\">R$ {item.SaldoRestante:N2}</td>");
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");
            return sb.ToString();
        }

        public static string BuildBlocoAssinaturaPendente()
        {
            return $"{MarcadorInicio}<div class=\"bloco-assinatura\"><p style=\"font-size:9pt;color:#64748B;\">Assinatura eletrônica pendente.</p></div>{MarcadorFim}";
        }

        /// <summary>
        /// Página de evidências anexada ao final do documento assinado. As páginas anteriores são o
        /// PDF exatamente como o signatário o enviou; os resumos permitem verificar isso.
        /// </summary>
        public static string BuildCertificadoAssinaturaHtml(AssinaturaEletronica assinatura)
        {
            static string Linha(string rotulo, string? valor) =>
                $"<tr><td style=\"padding:6px;font-size:9pt;color:#64748B;width:32%;vertical-align:top;\">{rotulo}</td>" +
                $"<td style=\"padding:6px;font-size:9pt;word-break:break-all;\">{WebUtility.HtmlEncode(valor ?? "Não registrado")}</td></tr>";

            var sb = new StringBuilder();
            sb.Append("<html><head><meta charset=\"utf-8\" /></head><body style=\"font-family:Helvetica,Arial,sans-serif;color:#1A1A18;\">");
            sb.Append("<h2 style=\"font-size:14pt;\">Certificado de assinatura eletrônica</h2>");
            sb.Append("<table style=\"width:100%;border-collapse:collapse;\">");
            sb.Append(Linha("Documento", assinatura.TipoDocumento?.ToString()));
            sb.Append(Linha("Identificador", assinatura.DocumentoGuid.ToString()));
            sb.Append(Linha("Emitido em", assinatura.CriadoEm.ToString("dd/MM/yyyy HH:mm:ss") + " UTC"));
            sb.Append(Linha("Assinado em", assinatura.DataAssinatura?.ToString("dd/MM/yyyy HH:mm:ss") + " UTC (relógio do servidor)"));
            sb.Append(Linha("IP de origem", assinatura.IpAssinante));
            sb.Append(Linha("Dispositivo", assinatura.DispositivoInfo));
            sb.Append(Linha("SHA-256 do documento apresentado", assinatura.HashDocumentoOriginal));
            sb.Append(Linha("SHA-256 do documento assinado", assinatura.HashDocumentoAssinado));
            sb.Append("</table>");
            sb.Append("<p style=\"font-size:8pt;color:#64748B;margin-top:16px;\">As páginas anteriores reproduzem o documento assinado tal como recebido pelo servidor. " +
                      "O resumo SHA-256 do documento assinado refere-se a esse arquivo antes da inclusão desta página, e o arquivo original permanece armazenado para verificação.</p>");
            sb.Append("</body></html>");
            return sb.ToString();
        }
    }
}
