using Mapster;
using ninx.Application.Interfaces.Services;
using ninx.Communication.Request;
using ninx.Communication.Request.Alt;
using ninx.Communication.Response;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Application.Services
{
    public class VendaService : IVendaService
    {
        private readonly IVendaRepository _vendaRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEstoqueRepository _estoqueRepository;
        private readonly IMovimentacaoEstoqueRepository _movimentacaoEstoqueRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IComercioRepository _comercioRepository;
        private readonly IUsuarioComercioRepository _usuarioComercioRepository;
        public VendaService(
            IVendaRepository vendaRepository,
            IProdutoRepository produtoRepository,
            IUnitOfWork unitOfWork,
            IEstoqueRepository estoqueRepository,
            IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository,
            IUsuarioRepository usuarioRepository,
            IComercioRepository comercioRepository,
            IUsuarioComercioRepository usuarioComercioRepository)
        {
            _vendaRepository = vendaRepository;
            _produtoRepository = produtoRepository;
            _unitOfWork = unitOfWork;
            _estoqueRepository = estoqueRepository;
            _movimentacaoEstoqueRepository = movimentacaoEstoqueRepository;
            _usuarioRepository = usuarioRepository;
            _comercioRepository = comercioRepository;
            _usuarioComercioRepository = usuarioComercioRepository;
        }


        public async Task<IEnumerable<VendaResponse>> GetVendasFiltroAsync(FiltroRequest request)
        {
            if (request is null)
            {
                throw new BadRequestException("Pelo menos um filtro deve ser fornecido.");
            }

            var vendas = await _vendaRepository.GetVendasFiltroAsync(request.inicio, request.fim, request.comercioID, request.usuarioID);

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
            var usuario = await _usuarioRepository.GetByIdAsync(request.UsuarioID);
            if (usuario == null || !usuario.Ativo)
                throw new BadRequestException("Usuário inválido ou inativo.");
            var comercio = await _comercioRepository.GetByIdAsync(request.ComercioID);
            if (comercio == null)
                throw new BadRequestException("Comércio inválido ou não encontrado.");
            var usuarioComercio = await _usuarioComercioRepository.GetByUsuarioIdAsync(request.UsuarioID);
            if (!usuarioComercio.Any(x => x.ComercioID == request.ComercioID))
                throw new BadRequestException("Este usuário não tem permissão para vender neste comércio.");

            int tentativas = 0;
            while (tentativas < 10)
            {
                try
                {
                    await _unitOfWork.BeginTransactionAsync();

                    var produtoIds = request.ItensVenda.Select(i => i.ProdutoID).ToList();
                    var produtosDb = await _produtoRepository.GetProdutosById(produtoIds);
                    var estoquesDb = await _estoqueRepository.GetByProdutosIdsAsync(produtoIds, request.ComercioID);

                    decimal totalVenda = 0;
                    var itensParaInserir = new List<ItemVenda>();
                    var movimentacoesEstoque = new List<MovimentacaoEstoque>();

                    foreach (var itemReq in request.ItensVenda)
                    {
                        var produto = produtosDb.FirstOrDefault(p => p.ProdutoID == itemReq.ProdutoID);
                        var estoque = estoquesDb.FirstOrDefault(e => e.ProdutoID == itemReq.ProdutoID);

                        ValidarVenda(produto, estoque, itemReq);

                        var subtotal = itemReq.Quantidade * produto!.PrecoVenda;
                        totalVenda += subtotal;

                        itensParaInserir.Add(new ItemVenda
                        {
                            ProdutoID = produto.ProdutoID,
                            ProdutoNome = produto.Nome,
                            ProdutoCodigoBarras = produto.CodigoBarras,
                            UnidadeMedida = produto.UnidadeMedida,
                            Quantidade = itemReq.Quantidade,
                            PrecoUnitario = produto.PrecoVenda,
                            Subtotal = subtotal
                        });

                        estoque!.Quantidade -= itemReq.Quantidade;
                        estoque.AtualizadoEm = DateTime.UtcNow;
                        await _estoqueRepository.UpdateAsync(estoque);

                        movimentacoesEstoque.Add(new MovimentacaoEstoque
                        {
                            ComercioID = request.ComercioID,
                            ProdutoID = produto.ProdutoID,
                            UsuarioID = request.UsuarioID,
                            Tipo = "VENDA",
                            Quantidade = itemReq.Quantidade,
                            DataHora = DateTime.UtcNow
                        });
                    }

                    decimal totalPago = request.Pagamentos?.Sum(p => p.Valor) ?? 0;
                    bool ehFiado = request.TipoVenda == 2;
                    if (ehFiado)
                    {
                        if (!request.ClienteID.HasValue)
                            throw new BadRequestException("Para vendas fiado, é obrigatório selecionar um cliente.");
                        if (totalPago >= totalVenda)
                            throw new BadRequestException("Uma venda fiado não pode estar totalmente paga no ato da criação.");

                        // preciso adicionar o limite de crédido aq...
                    }

                    var novaVenda = new Venda
                    {
                        ComercioID = request.ComercioID,
                        UsuarioID = request.UsuarioID,
                        Total = totalVenda,
                        TipoVenda = ehFiado ? TipoVenda.Fiado : TipoVenda.Normal,
                        Status = StatusVenda.Finalizada,
                        ItensVenda = itensParaInserir, 
                        CriadoEm = DateTime.UtcNow,
                        PagamentosVenda = request.Pagamentos?.Select(p => new PagamentoVenda
                        {
                            FormaPagamento = (FormaPagamento)p.FormaPagamento,
                            Valor = p.Valor,
                            DataHora = DateTime.UtcNow
                        }).ToList() 
                        ?? new List<PagamentoVenda>()
                    };

                    if (ehFiado)
                    {
                        novaVenda.VendaFiado = new VendaFiado
                        {
                            ClienteID = request.ClienteID!.Value,
                            Status = StatusFiado.Pendente,
                            CriadoEm = DateTime.UtcNow
                        };
                    }

                    await _vendaRepository.AddAsync(novaVenda);
                    await _unitOfWork.SaveChangesAsync();

                    foreach (var mov in movimentacoesEstoque)
                    {
                        mov.VendaID = novaVenda.VendaID;
                        await _movimentacaoEstoqueRepository.AddAsync(mov);
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();

                    return novaVenda.Adapt<VendaResponse>();
                }
                catch (ConcurrencyException)
                {
                    await _unitOfWork.RollbackAsync();
                    tentativas++;
                    if (tentativas >= 10) throw;
                    await Task.Delay(50);
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }
            throw new BadRequestException("Não foi possível processar a venda devido a múltiplas tentativas de estoque.");
        }

        public async Task EstornarAsync(int vendaId, int usuarioId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var venda = await _vendaRepository.GetByIdAsync(vendaId);
                if (venda == null) throw new NotFoundException("Venda não encontrada.");

                if (venda.Status == StatusVenda.Cancelada)
                    throw new BadRequestException("Esta venda já foi estornada.");

                var usuarioC = await _usuarioComercioRepository.GetByUsuarioIdAsync(usuarioId);
                if (!usuarioC.Any(x => x.ComercioID == venda.ComercioID))
                    throw new BadRequestException("O usuário não possui permissão nesse comercio.");

                foreach (var item in venda.ItensVenda)
                {
                    var estoque = await _estoqueRepository.GetByProdutoIdAsync(item.ProdutoID, venda.ComercioID);

                    if (estoque != null)
                    {
                        estoque.Quantidade += item.Quantidade;
                        estoque.AtualizadoEm = DateTime.UtcNow;
                        await _estoqueRepository.UpdateAsync(estoque);

                        await _movimentacaoEstoqueRepository.AddAsync(new MovimentacaoEstoque
                        {
                            ComercioID = venda.ComercioID,
                            ProdutoID = item.ProdutoID,
                            UsuarioID = usuarioId, 
                            VendaID = venda.VendaID,
                            Tipo = "ENTRADA_ESTORNO",
                            Quantidade = item.Quantidade,
                            DataHora = DateTime.UtcNow,
                            Observacao = $"Estorno da Venda #{venda.VendaID}"
                        });
                    }
                }

                if (venda.TipoVenda == TipoVenda.Fiado && venda.VendaFiado != null)
                {
                    venda.VendaFiado.Status = StatusFiado.Cancelado;
                }

                venda.Status = StatusVenda.Cancelada;
                venda.AtualizadoEm = DateTime.UtcNow;

                await _vendaRepository.UpdateAsync(venda);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        private void ValidarVenda(Produto? produto, Estoque? estoque, ItemVendaRequest item)
        {
            if (produto is null)
            {
                throw new NotFoundException($"Produto ID {item.ProdutoID} não encontrado no catálogo.");
            }

            if (estoque is null)
            {
                throw new NotFoundException($"Registro de estoque não encontrado para o produto {produto.Nome}.");
            }

            if (item.Quantidade <= 0)
            {
                throw new BadRequestException($"A quantidade para o produto {produto.Nome} deve ser maior que zero.");
            }

            if (estoque.Quantidade < item.Quantidade)
            {
                throw new BadRequestException(
                    $"Estoque insuficiente para {produto.Nome}. " +
                    $"Solicitado: {item.Quantidade:N3}, Disponível: {estoque.Quantidade:N3}.");
            }
        }
    }
}
