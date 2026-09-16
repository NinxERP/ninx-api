using System.Globalization;
using System.Runtime.CompilerServices;

namespace ninx.Tests
{
    internal static class CulturaTestes
    {
        /// <summary>
        /// Espelha a cultura fixada no Program.cs. O runner de teste nao passa por
        /// la, e no CI (Linux sem LANG) a cultura padrao e a invariante: as
        /// assercoes de "R$ 30,00" quebrariam so na esteira.
        /// </summary>
        [ModuleInitializer]
        internal static void Inicializar()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");
        }
    }
}
