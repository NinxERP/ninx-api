using Mapster;
using ninx.Application.Interfaces.Services;
using ninx.Communication.Request;
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
        private readonly IClienteRepository _clienteRepository;
        private readonly IPagamentoVendaRepository _pagamentoVendaRepository;
        private readonly IAssinaturaEletronicaRepository _assinaturaEletronicaRepository; 
        public VendaService(
            IVendaRepository vendaRepository,
            IProdutoRepository produtoRepository,
            IUnitOfWork unitOfWork,
            IEstoqueRepository estoqueRepository,
            IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository,
            IUsuarioRepository usuarioRepository,
            IComercioRepository comercioRepository,
            IUsuarioComercioRepository usuarioComercioRepository,
            IClienteRepository clienteRepository,
            IPagamentoVendaRepository pagamentoVendaRepository,
            IAssinaturaEletronicaRepository assinaturaEletronicaRepository)
        {
            _vendaRepository = vendaRepository;
            _produtoRepository = produtoRepository;
            _unitOfWork = unitOfWork;
            _estoqueRepository = estoqueRepository;
            _movimentacaoEstoqueRepository = movimentacaoEstoqueRepository;
            _usuarioRepository = usuarioRepository;
            _comercioRepository = comercioRepository;
            _usuarioComercioRepository = usuarioComercioRepository;
            _clienteRepository = clienteRepository;
            _pagamentoVendaRepository = pagamentoVendaRepository;
            _assinaturaEletronicaRepository = assinaturaEletronicaRepository;
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

        public async Task<VendaResponse> GetByVendaIdAsync(int id)
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
            
            await ValidarPermissaoUsuarioComercioAsync(request.UsuarioID, request.ComercioID);

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
                            Tipo = TipoMovimentacao.Venda,
                            Quantidade = itemReq.Quantidade,
                            DataHora = DateTime.UtcNow
                        });
                    }

                    decimal totalPago = request.Pagamentos?.Sum(p => p.Valor) ?? 0;
                    bool ehFiado = request.TipoVenda == 2;
                    Guid? identificadorAssinatura = null;
                    if (ehFiado)
                    {
                        if (!request.ClienteID.HasValue)
                            throw new BadRequestException("Para vendas fiado, é obrigatório selecionar um cliente.");
                        if (totalPago >= totalVenda)
                            throw new BadRequestException("Uma venda fiado não pode estar totalmente paga no ato da criação.");
                        var cliente = await _clienteRepository.GetByIdAsync(request.ClienteID);
                        if (cliente == null) 
                            throw new NotFoundException("Cliente não encontrado.");

                        var saldoDevedorAtual = await CalcularSaldoDevedor(request.ClienteID.Value);
                        decimal valorFiadoDestaVenda = totalVenda - totalPago;

                        if ((saldoDevedorAtual + valorFiadoDestaVenda) > cliente.LimiteCredito)
                        {
                            var limiteDisponivel = cliente.LimiteCredito - saldoDevedorAtual;
                            throw new BadRequestException($"Limite excedido! O cliente já deve R$ {saldoDevedorAtual:N2}. " + $"Disponível para esta compra: R$ {limiteDisponivel:N2}");
                        }
                        identificadorAssinatura = Guid.NewGuid();
                    }

                    var novaVenda = new Venda
                    {
                        ComercioID = request.ComercioID,
                        UsuarioID = request.UsuarioID,
                        ClienteID = request.ClienteID,
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
                        novaVenda.TipoVenda = TipoVenda.Fiado;

                        var novaAssinaturaEletronica = new AssinaturaEletronica
                        {
                            Venda = novaVenda,
                            DocumentoGuid = identificadorAssinatura.Value,
                            Assinado = false,
                            CriadoEm = DateTime.UtcNow
                        };

                        await _assinaturaEletronicaRepository.AddAsync(novaAssinaturaEletronica);
                    }

                    await _vendaRepository.AddAsync(novaVenda);

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
                catch (BadRequestException)
                {
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
                catch (NotFoundException)
                {
                    await _unitOfWork.RollbackAsync();
                    throw;
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
                if (venda == null) 
                    throw new NotFoundException("Venda não encontrada.");

                if (venda.Status == StatusVenda.Cancelada)
                    throw new BadRequestException("Esta venda já foi estornada.");

                await ValidarPermissaoUsuarioComercioAsync(usuarioId, venda.ComercioID);

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
                            Tipo = TipoMovimentacao.Estorno,
                            Quantidade = item.Quantidade,
                            DataHora = DateTime.UtcNow,
                            Observacao = $"Estorno da Venda #{venda.VendaID}"
                        });
                    }
                }

                venda.Status = StatusVenda.Cancelada;
                venda.AtualizadoEm = DateTime.UtcNow;

                await _vendaRepository.UpdateAsync(venda);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (NotFoundException)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            catch (BadRequestException)
            {
                await _unitOfWork.RollbackAsync();
                throw;
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

        private async Task ValidarPermissaoUsuarioComercioAsync(int usuarioId, int comercioId)
        {
            var usuarioComercio = await _usuarioComercioRepository.GetByUsuarioIdAsync(usuarioId);
            if (!usuarioComercio.Any(x => x.ComercioID == comercioId))
                throw new BadRequestException("Este usuário não tem permissão para acessar este comércio.");
        }

        private async Task<decimal> CalcularSaldoDevedor(int? clienteId)
        {
            var vendasFiadoCliente = await _vendaRepository.GetVendasFiadoByClienteIDAsync(clienteId);
            var valorTotalPago = await _pagamentoVendaRepository.GetPagamentosFiadosByClienteIdAsync(clienteId);

            decimal saldoDevedorTotal = 0;
            foreach (var venda in vendasFiadoCliente)
            {
                    var totalJaPagoDestaVenda = valorTotalPago
                        .Where(p => p.VendaID == venda.VendaID)
                        .Sum(p => p.Valor);

                    var saldoDestaVenda = venda.Total - totalJaPagoDestaVenda;
                    saldoDevedorTotal += saldoDestaVenda;
            }
            return saldoDevedorTotal;
        }
    }
}
