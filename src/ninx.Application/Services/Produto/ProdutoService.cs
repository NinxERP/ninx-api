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

    public async Task<ProdutoResponse> CriarAsync(CriarProdutoRequest request, int comercioID)
    {
        
        if (request.ComercioID != comercioID)
        {
            throw new UnauthorizedAccessException("Usuário não tem permissão nesse comercio.");
        }

        var produto = request.Adapt<Produto>();
        produto.ComercioID = request.ComercioID; 
        produto.CriadoEm = DateTime.UtcNow;

        await _produtoRepository.AddAsync(produto);

        if (request.EstoqueInicial > 0)
        {
            var estoque = new Estoque
            {
                Produto = produto,
                Quantidade = request.EstoqueInicial,
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
            ?? throw new NotFoundException("Produto não encontrado ou acesso negado.");

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
            throw new NotFoundException("Produto não encontrado .");
        }

        produto.Ativo = false;
        produto.AtualizadoEm = DateTime.UtcNow;

        await _produtoRepository.UpdateAsync(produto);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<ProdutoResponse>> GetProdutosByComercioIdAsync(int comercioId)
    {
        var produtos = await _produtoRepository.GetProdutosByComercioIdAsync(comercioId);
        if (produtos == null)
        {
            throw new NotFoundException("Produtos não encontrados para o comercio informado");
        }
        return produtos.Adapt<IEnumerable<ProdutoResponse>>();
    }

    public async Task<ProdutoResponse?> GetByIdAsync(int id)
    {
        var produtos = await _produtoRepository.GetByIdAsync(id);
        if (produtos == null)
        {
            throw new NotFoundException("Produto não encontrado");
        }
        return produtos.Adapt<ProdutoResponse>();
    }

    public async Task<ProdutoResponse?> GetByCodigoBarrasAsync(int comercioId, string codigoBarras)
    {
        var produtos = await _produtoRepository.GetByCodigoBarrasAsync(comercioId, codigoBarras);
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