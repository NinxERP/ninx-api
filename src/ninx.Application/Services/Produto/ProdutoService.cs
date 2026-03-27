using Mapster;
using ninx.Communication.Request;
using ninx.Communication.Response;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces.Repositories;
using ninx.Domain.Interfaces.Services;

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

    public async Task<ProdutoResponse> CriarAsync(CriarProdutoRequest request, int comercioId)
    {
        var produto = request.Adapt<Produto>();
        produto.ComercioID = comercioId; 
        produto.CriadoEm = DateTime.UtcNow;

        await _produtoRepository.AddAsync(produto);

        if (request.EstoqueInicial > 0)
        {
            var estoque = new Estoque
            {
                Produto = produto,
                Quantidade = request.EstoqueInicial,
                ComercioID = comercioId
            };
            await _estoqueRepository.AddAsync(estoque);
        }

        await _unitOfWork.SaveChangesAsync();
        return produto.Adapt<ProdutoResponse>();
    }

    public async Task<ProdutoResponse> AtualizarAsync(int id, int comercioId, AtualizarProdutoRequest request)
    {
        var produto = await _produtoRepository.GetByIdAndComercioAsync(id, comercioId)
            ?? throw new NotFoundException("Produto não encontrado ou acesso negado.");

        request.Adapt(produto);
        produto.AtualizadoEm = DateTime.UtcNow;

        await _produtoRepository.UpdateAsync(produto);
        await _unitOfWork.SaveChangesAsync();

        return produto.Adapt<ProdutoResponse>();
    }

    public async Task DesativarAsync(int id, int comercioId)
    {
        var produto = await _produtoRepository.GetByIdAndComercioAsync(id, comercioId)
            ?? throw new NotFoundException("Produto não encontrado.");

        produto.Ativo = false;
        produto.AtualizadoEm = DateTime.UtcNow;

        await _produtoRepository.UpdateAsync(produto);
        await _unitOfWork.SaveChangesAsync();
    }
}