using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IProdutoService
    {
        Task<PaginatedResponse<ProdutoResponse>> GetProdutosEstoqueByComercioIdAsync(int comercioId, PaginationRequest request);
        Task<ProdutoResponse> GetByIdAsync(int id, int comercioId);
        Task<ProdutoResponse> GetAtivosByCodigoBarrasAsync(int comercioId, string codigoBarras);
        Task<IEnumerable<ProdutoResponse>> GetByNomeAsync(int comercioId, string nome);
        Task<ProdutoResponse> CriarAsync(CriarProdutoRequest request, int comercioID);
        Task<ProdutoResponse> AtualizarAsync(int id, int comercioId, AtualizarProdutoRequest request);
        Task DesativarAsync(int id, int comercioId);
    }
}