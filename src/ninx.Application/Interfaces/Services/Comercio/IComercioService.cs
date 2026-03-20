namespace ninx.Application.Interfaces.Services.Comercio
{
    public interface IComercioService
    {
        public Task<IEnumerable<Domain.Entities.Comercio>> GetAll();
        public Task<IEnumerable<Domain.Entities.Comercio>> GetByUsuarioId(int usuarioId);

    }
}
