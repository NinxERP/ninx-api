using ninx.Communication;
using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IProdutoService
    {
        Task<IEnumerable<ProdutoResponse>> GetProdutosByComercioIdAsync(int comercioId);
        Task<ProdutoResponse> GetByIdAsync(int id, int comercioId);
        Task<ProdutoResponse> GetByCodigoBarrasAsync(int comercioId, string codigoBarras);
        Task<IEnumerable<ProdutoResponse>> GetByNomeAsync(int comercioId, string nome);
        Task<ProdutoResponse> CriarAsync(CriarProdutoRequest request, int comercioID);
        Task<ProdutoResponse> AtualizarAsync(int id, int comercioId, AtualizarProdutoRequest request);
        Task DesativarAsync(int id, int comercioId);
    }
}