using Mapster;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class ProdutoService : IProdutoService
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IEstoqueRepository _estoqueRepository; 
        private readonly IUnitOfWork _unitOfWork; 

        public ProdutoService(IProdutoRepository produtoRepository, IEstoqueRepository estoqueRepository, IUnitOfWork unitOfWork)
        {
            _produtoRepository = produtoRepository;
            _estoqueRepository = estoqueRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ProdutoResponse> CriarAsync(CriarProdutoRequest request, int comercioID)
        {
            if (request.ComercioID != comercioID)
            {
                throw new BadRequestException("Usuário não tem permissão nesse comércio.");
            }


            var produto = request.Adapt<Produto>();
            produto.ComercioID = request.ComercioID; 
            produto.CriadoEm = DateTime.UtcNow;

            await _produtoRepository.AddAsync(produto);

            if (request.EstoqueInicial > 0 || request.QuantidadeMinima > 0) 
            {
                var estoque = new Estoque
                {
                    Produto = produto,
                    Quantidade = request.EstoqueInicial,
                    QuantidadeMinima = request.QuantidadeMinima,
                    ComercioID = request.ComercioID
                };
                await _estoqueRepository.AddAsync(estoque);
            }

            await _unitOfWork.SaveChangesAsync();
            return produto.Adapt<ProdutoResponse>();
        }

        public async Task<ProdutoResponse> AtualizarAsync(int id, int comercioId, AtualizarProdutoRequest request)
        {
            var produto = await _produtoRepository.GetByIdAndComercioAsync(id, comercioId)
                ?? throw new NotFoundException("Produto não encontrado.");

            request.Adapt(produto);
            produto.AtualizadoEm = DateTime.UtcNow;

            await _produtoRepository.UpdateAsync(produto);
            await _unitOfWork.SaveChangesAsync();

            return produto.Adapt<ProdutoResponse>();
        }

        public async Task DesativarAsync(int id, int comercioId)
        {
            var produto = await _produtoRepository.GetByIdAndComercioAsync(id, comercioId);
            if (produto == null)
            {
                throw new NotFoundException("Produto não encontrado.");
            }

            produto.Ativo = false;
            produto.AtualizadoEm = DateTime.UtcNow;

            await _produtoRepository.UpdateAsync(produto);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PaginatedResponse<ProdutoResponse>> GetProdutosEstoqueByComercioIdAsync(
            int comercioId,
            PaginationRequest request)
        {
            var (data, totalFiltrado, metrics) = await _produtoRepository
                .GetProdutosEstoqueByComercioIdPaginatedAsync(comercioId, request);

            var produtosResponse = data.Adapt<List<ProdutoResponse>>();

            return new PaginatedResponse<ProdutoResponse>(
                produtosResponse,
                request.PageNumber,
                request.PageSize,
                totalFiltrado,
                metrics
            );
        }

        public async Task<ProdutoResponse> GetByIdAsync(int id, int comercioId)
        {
            var produtos = await _produtoRepository.GetByIdAndComercioAsync(id, comercioId);
            if (produtos == null)
            {
                throw new NotFoundException("Produto não encontrado");
            }
            return produtos.Adapt<ProdutoResponse>();
        }

        public async Task<ProdutoResponse> GetAtivosByCodigoBarrasAsync(int comercioId, string codigoBarras)
        {
            var produtos = await _produtoRepository.GetAtivosByCodigoBarrasAsync(comercioId, codigoBarras);
            if (produtos == null)
            {
                throw new NotFoundException("Produto não encontrado");
            }
            return produtos.Adapt<ProdutoResponse>();
        }

        public async Task<IEnumerable<ProdutoResponse>> GetByNomeAsync(int comercioId, string nome)
        {
            var produtos = await _produtoRepository.GetByNomeAsync(comercioId, nome);
            if (produtos == null)
            {
                throw new NotFoundException($"Produto {nome} não encontrado");
            }
            return produtos.Adapt<IEnumerable<ProdutoResponse>>();
        }
    }
}