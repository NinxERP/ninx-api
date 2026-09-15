using System.Security.Cryptography;

namespace ninx.Application.Services
{
    /// <summary>
    /// Resumo criptográfico dos documentos do fluxo de assinatura.
    /// </summary>
    /// <remarks>
    /// O resumo é calculado sobre os bytes do PDF, não sobre o texto em base64, para que
    /// qualquer pessoa possa conferi-lo com uma ferramenta comum a partir do arquivo baixado
    /// (por exemplo, <c>certutil -hashfile documento.pdf SHA256</c> no Windows).
    /// </remarks>
    public static class HashDocumento
    {
        /// <returns>SHA-256 em hexadecimal minúsculo, ou <c>null</c> se não houver documento.</returns>
        /// <exception cref="FormatException">Se o conteúdo não for base64 válido.</exception>
        public static string? Sha256Hex(string? documentoBase64)
        {
            if (string.IsNullOrEmpty(documentoBase64))
                return null;

            var bytes = Convert.FromBase64String(documentoBase64);
            return Convert.ToHexStringLower(SHA256.HashData(bytes));
        }
    }
}
