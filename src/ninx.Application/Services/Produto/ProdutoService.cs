using Mapster;
using ninx.Communication.Request.Produto;
using ninx.Communication.Response.Produto;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Interfaces.Repositories;
using ninx.Domain.Interfaces.Services;

namespace ninx.Application.Services
{
    public class ProdutoService : IProdutoService
    {
        private readonly IProdutoRepository _produtoRepository;

        public ProdutoService(IProdutoRepository produtoRepository)
        {
            _produtoRepository = produtoRepository;
        }

        public async Task<IEnumerable<ProdutoResponse>> GetAllByComercioAsync(int comercioId)
        {
            var produtos = await _produtoRepository.GetAllByComercioAsync(comercioId);
            return produtos.Adapt<IEnumerable<ProdutoResponse>>();
        }

        public async Task<ProdutoResponse?> GetByCodigoBarrasAsync(int comercioId, string codigoBarras)
        {
            var produto = await _produtoRepository.GetByCodigoBarrasAsync(comercioId, codigoBarras);
            return produto.Adapt<ProdutoResponse>();
        }

        public async Task<IEnumerable<ProdutoResponse>> GetByNomeAsync(int comercioId, string nome)
        {
            var produtos = await _produtoRepository.GetByNomeAsync(comercioId, nome);
            return produtos.Adapt<IEnumerable<ProdutoResponse>>();
        }
        public async Task<ProdutoResponse?> GetByIdAsync(int id)
        {
            var produto = await _produtoRepository.GetByIdAsync(id);
            if (produto is null) return null;
            return produto.Adapt<ProdutoResponse>();
        }

        public async Task<ProdutoResponse> CriarAsync(CriarProdutoRequest request)
        {
            var produto = request.Adapt<Produto>();
  
            await _produtoRepository.AddAsync(produto);
            return produto.Adapt<ProdutoResponse>();
        }

        public async Task<ProdutoResponse> AtualizarAsync(int id, AtualizarProdutoRequest request)
        {
            var produto = await _produtoRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Produto não encontrado.");

            request.Adapt(produto);
            produto.AtualizadoEm = DateTime.Now;

            await _produtoRepository.UpdateAsync(produto);
            return produto.Adapt<ProdutoResponse>();
        }

        public async Task DesativarAsync(int id)
        {
            var produto = await _produtoRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Produto não encontrado.");

            produto.Ativo = false;
            produto.AtualizadoEm = DateTime.Now;
            await _produtoRepository.UpdateAsync(produto);
        }
    }
}