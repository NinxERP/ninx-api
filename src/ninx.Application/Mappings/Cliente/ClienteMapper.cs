using Mapster;
using ninx.Communication;
using ninx.Domain.Entities;

namespace ninx.Application.Mappings
{
    public class ClienteMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Cliente, ClienteResponse>()
                .Map(dest => dest.Cpf, src => MascararCpf(src.Cpf));
        }

        private static string MascararCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf) || cpf.Length != 11)
                return cpf;

            return $"***.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-**";
        }
    }
}
