using Mapster;
using ninx.Application.Interfaces.Services;
using ninx.Communication.Request;
using ninx.Communication.Response;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Application.Services
{
    public class VendaService : IVendaService
    {
        private readonly IVendaRepository _vendaRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IUnitOfWork _unitOfWork;
        public VendaService(
            IVendaRepository vendaRepository,
            IProdutoRepository produtoRepository,
            IUnitOfWork unitOfWork)
        {
            _vendaRepository = vendaRepository;
            _produtoRepository = produtoRepository;
            _unitOfWork = unitOfWork;
        }


        public async Task<IEnumerable<VendaResponse>> GetVendasFiltroAsync(
                DateTime? inicio = null,
                DateTime? fim = null,
                int? comercioID = null,
                int? usuarioID = null)
        {
            if (inicio is null && fim is null && comercioID is null && usuarioID is null)
            {
                throw new BadRequestException("Pelo menos um filtro deve ser fornecido.");
            }

            var vendas = await _vendaRepository.GetVendasFiltroAsync(inicio, fim, comercioID, usuarioID);

            if (vendas is null || !vendas.Any())
            {
                throw new NotFoundException("Nenhuma venda foi encontrada para os filtros.");
            }

            return vendas.Adapt<IEnumerable<VendaResponse>>();
        }

        public async Task<IEnumerable<VendaResponse>> GetByUsuarioIdAsync(int usuarioID)
        {
            var vendas = await _vendaRepository.GetVendasByUsuarioIdAsync(usuarioID);

            if (vendas is null || !vendas.Any())
            {
                throw new NotFoundException("Nenhuma venda encontrada para o usuário especificado.");
            }

            return vendas.Adapt<IEnumerable<VendaResponse>>();
        }

        public async Task<VendaResponse?> GetByVendaIdAsync(int id)
        {
            var venda = await _vendaRepository.GetByIdAsync(id);
            if (venda is null)
            {
                throw new NotFoundException("Venda não encontrada");
            }

            return venda.Adapt<VendaResponse>();
        }

        public async Task<VendaResponse> CriarAsync(CriarVendaRequest request)
        {
            if (request.ItensVenda == null || !request.ItensVenda.Any())
            {
                throw new BadRequestException("A venda deve conter pelo menos um item.");
            }

            var itensVendas = new List<ItemVenda>();
            var produtosAtt = new List<Produto>();
            decimal total = 0;

            foreach (var item in request.ItensVenda)
            {
                Produto? produto = await _produtoRepository.GetProdutoEstoqueByIdAsync(item.ProdutoID);
                ValidarVenda(produto, item);

                var precoUnitario = item.PrecoUnitario ?? produto.PrecoVenda;
                var subtotal = item.Quantidade * precoUnitario;
                total += subtotal;

                itensVendas.Add(new ItemVenda
                {
                    ProdutoID = produto.ProdutoID,
                    ProdutoNome = produto.Nome,
                    ProdutoCodigoBarras = produto.CodigoBarras,
                    UnidadeMedida = produto.UnidadeMedida,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = precoUnitario,
                    Subtotal = subtotal
                });

                produto.Estoque.Quantidade -= item.Quantidade;
                produtosAtt.Add(produto);
            }

            var venda = new Venda
            {
                ComercioID = request.ComercioID,
                UsuarioID = request.UsuarioID,
                Total = total,
                ItensVenda = itensVendas,
                CriadoEm = DateTime.UtcNow
            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var produto in produtosAtt)
                {
                    await _produtoRepository.UpdateAsync(produto);
                }
                await _vendaRepository.AddAsync(venda);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return venda.Adapt<VendaResponse>();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw new BadRequestException("Erro ao realizar venda.");
            }
        }

        private void ValidarVenda(Produto? produto, ItemVendaRequest item)
        {
            if (produto is null)
            {
                throw new NotFoundException($"Produto {item.ProdutoID} não encontrado.");
            }
            if (produto.Estoque == null)
            {
                throw new NotFoundException($"Estoque do produto {produto.Nome} não encontrado.");
            }
            if (produto.Estoque.Quantidade < item.Quantidade)
            {
                throw new BadRequestException($"Produto {produto.Nome} não tem estoque suficiente.");
            }

        }
    }
}
