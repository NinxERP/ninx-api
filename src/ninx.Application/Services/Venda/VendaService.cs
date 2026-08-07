using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Layout;
using Mapster;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;
using ninx.Communication.Helpers;

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

        public async Task<IEnumerable<VendaResponse>> GetByClienteIdAsync(int clienteId)
        {
            var vendas = await _vendaRepository.GetVendasByClienteIdAsync(clienteId);

            if (vendas is null || !vendas.Any())
            {
                throw new NotFoundException("Nenhuma venda encontrada para o usuário especificado.");
            }

            var vendaResponse = PopulaSaldoTotal(vendas);
            return vendaResponse;
        }
        public async Task<VendaResponse> GetByVendaIdAsync(int id)
        {
            var venda = await _vendaRepository.GetByIdComItensAsync(id);
            if (venda is null)
            {
                throw new NotFoundException("Venda não encontrada");
            }

            var response = venda.Adapt<VendaResponse>();

            if (venda.AssinaturasEletronicas.Any(x => x.DocumentoGuid != Guid.Empty))
            {
                response.DocumentoGuid = venda.AssinaturasEletronicas.Select(x => x.DocumentoGuid).ToList();
            }

            return response;
        }
        public async Task EstornarAsync(int vendaId, int usuarioId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var venda = await _vendaRepository.GetByIdComItensAsync(vendaId);
                if (venda == null)
                    throw new NotFoundException("Venda não encontrada.");

                if (venda.Status == StatusVenda.Cancelada || venda.Status == StatusVenda.Estornada)
                    throw new BadRequestException("Esta venda já foi estornada.");

                await ValidarPermissaoUsuarioComercioAsync(usuarioId, venda.ComercioID);

                await ProcessarEstornoEstoqueAsync(venda, usuarioId);

                venda.Status = StatusVenda.Estornada;
                venda.AtualizadoEm = DateTime.UtcNow;

                await _vendaRepository.UpdateAsync(venda);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await RollbackTransacaoAsync();
                throw;
            }
        }
        public async Task<VendaResponse> CriarAsync(CriarVendaRequest request)
        {
            ValidarRequestVenda(request);
            var (usuario, comercio) = await ValidarDadosVendaAsync(request);

            const int maxTentativas = 5;
            for (int tentativa = 0; tentativa < maxTentativas; tentativa++)
            {
                try
                {
                    await _unitOfWork.BeginTransactionAsync();

                    var (venda, documentoGuid) = await ProcessarCriacaoVendaAsync(request);

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();

                    var response = venda.Adapt<VendaResponse>();
                    if (documentoGuid.HasValue)
                    {
                        response.DocumentoGuid.Add(documentoGuid.Value);
                    }
                    return response;
                }
                catch (ConcurrencyException) when (tentativa < maxTentativas - 1)
                {
                    await RollbackTransacaoAsync();
                    await Task.Delay(50);
                }
                catch
                {
                    await RollbackTransacaoAsync();
                    throw;
                }
            }
            throw new BadRequestException("Não foi possível processar a venda após múltiplas tentativas.");
        }

        public async Task<Guid> ReceberPagamentoFiadoAsync(int vendaId, int usuarioId, decimal valorPago, int formaPagamento)
        {
            if (valorPago <= 0)
                throw new BadRequestException("O valor do pagamento deve ser maior que zero.");
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var venda = await _vendaRepository.GetByIdComItensAsync(vendaId);
                if (venda == null)
                    throw new NotFoundException("Venda não encontrada.");

                if (venda.TipoVenda != TipoVenda.Fiado)
                    throw new BadRequestException("Esta venda não é do tipo fiado.");

                if (venda.Status == StatusVenda.Finalizada)
                    throw new BadRequestException("Não é possível receber pagamentos para uma venda que está finalizada.");

                if (venda.Status == StatusVenda.Aguardando)
                    throw new BadRequestException("Não é possível receber pagamentos para uma venda que não foi aberta.");

                if (venda.AssinaturasEletronicas.Any(x => x.Assinado == false))
                    throw new BadRequestException("Não é possível receber pagamentos para essa venda, pois ela tem assinaturas pendentes.");

                await ValidarPermissaoUsuarioComercioAsync(usuarioId, venda.ComercioID);

                var pagamentosAnteriores = venda.PagamentosVenda
                    .Where(p => p.Status == StatusPagamento.Pago)
                    .Sum(p => p.Valor);

                var saldoDevedorVenda = venda.Total - pagamentosAnteriores;

                if (valorPago > saldoDevedorVenda)
                    throw new BadRequestException($"Valor informado (R$ {valorPago:N2}) é maior que o saldo devedor da venda (R$ {saldoDevedorVenda:N2}).");

                var dataOperacao = DateTime.UtcNow;

                var novoPagamento = new PagamentoVenda
                {
                    VendaID = venda.VendaID,
                    FormaPagamento = (FormaPagamento)formaPagamento,
                    Valor = valorPago,
                    Status = StatusPagamento.Pago,
                    CriadoEm = dataOperacao,
                    UsuarioID = usuarioId,
                };

                await _pagamentoVendaRepository.AddAsync(novoPagamento);

                var identificadorAssinatura = Guid.NewGuid();
                var cliente = await _clienteRepository.GetByIdAsync(venda.ClienteID!.Value);
                var comercio = await _comercioRepository.GetByIdAsync(venda.ComercioID);

                var assinatura = new AssinaturaEletronica
                {
                    Venda = venda,
                    DocumentoGuid = identificadorAssinatura,
                    Assinado = false,
                    CriadoEm = dataOperacao,
                    ImagemAssinatura = await CriarDocReciboPagamento(venda, novoPagamento, cliente!, comercio!, saldoDevedorVenda)
                };

                venda.AtualizadoEm = DateTime.UtcNow;
                await _assinaturaEletronicaRepository.AddAsync(assinatura);

                await _vendaRepository.UpdateAsync(venda);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return identificadorAssinatura; 
            }
            catch
            {
                await RollbackTransacaoAsync();
                throw;
            }
        }

        public async Task<Guid> ReceberPagamentoGeralFiadoAsync(int clienteId, int usuarioId, decimal valorTotalPago, int formaPagamento)
        {
            if (valorTotalPago <= 0)
                throw new BadRequestException("O valor do pagamento global deve ser maior que zero.");

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var vendasDoCliente = await _vendaRepository.GetVendasFiadoAtivasPorClienteAsync(clienteId);
                
                if (vendasDoCliente == null || !vendasDoCliente.Any())
                    throw new NotFoundException("Nenhuma venda fiada em aberto foi encontrada para este cliente.");

                if (vendasDoCliente.Any(x => x.Status == StatusVenda.Finalizada))
                    throw new BadRequestException("Não é possível receber pagamentos para uma venda que está finalizada.");

                if (vendasDoCliente.Any(x => x.Status == StatusVenda.Aguardando))
                    throw new BadRequestException("Não é possível receber pagamentos para uma venda que não foi aberta.");

                var primeiraVenda = vendasDoCliente.First();
                await ValidarPermissaoUsuarioComercioAsync(usuarioId, primeiraVenda.ComercioID);

                var dataOperacao = DateTime.UtcNow;
                var detalheAbatimentos = new List<ItemAbatimentoGlobal>();
                decimal valorRestanteParaDistribuir = valorTotalPago;

                foreach (var venda in vendasDoCliente.OrderBy(v => v.CriadoEm))
                {
                    if (valorRestanteParaDistribuir <= 0)
                        break;

                    var pagamentosAnteriores = venda.PagamentosVenda
                        .Where(p => p.Status == StatusPagamento.Pago)
                        .Sum(p => p.Valor);

                    var saldoDevedorVenda = venda.Total - pagamentosAnteriores;

                    if (saldoDevedorVenda <= 0)
                        continue;

                    decimal valorAbatidoNestaVenda = Math.Min(valorRestanteParaDistribuir, saldoDevedorVenda);

                    var novoPagamento = new PagamentoVenda
                    {
                        VendaID = venda.VendaID,
                        FormaPagamento = (FormaPagamento)formaPagamento,
                        Valor = valorAbatidoNestaVenda,
                        Status = StatusPagamento.Pago,
                        CriadoEm = dataOperacao,
                        UsuarioID = usuarioId,
                    };

                    await _pagamentoVendaRepository.AddAsync(novoPagamento);

                    detalheAbatimentos.Add(new ItemAbatimentoGlobal
                    {
                        VendaId = venda.VendaID,
                        DataVenda = venda.CriadoEm,
                        SaldoAnterior = saldoDevedorVenda,
                        ValorAbatido = valorAbatidoNestaVenda,
                        SaldoRestante = saldoDevedorVenda - valorAbatidoNestaVenda
                    });

                    venda.AtualizadoEm = DateTime.UtcNow;

                    await _vendaRepository.UpdateAsync(venda);
                    valorRestanteParaDistribuir -= valorAbatidoNestaVenda;


                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                }

                if (valorRestanteParaDistribuir > 0)
                    throw new BadRequestException($"O valor informado é maior do que o total da dívida acumulada do cliente. Sobra: R$ {valorRestanteParaDistribuir:N2}");

                var identificadorAssinatura = Guid.NewGuid();
                var cliente = await _clienteRepository.GetByIdAsync(clienteId);
                var comercio = await _comercioRepository.GetByIdAsync(primeiraVenda.ComercioID);

                var pdfBase64 = await CriarDocReciboGlobalPagamento(detalheAbatimentos, valorTotalPago, (FormaPagamento)formaPagamento, cliente!, comercio!, dataOperacao);

                foreach (var abatimento in detalheAbatimentos)
                {
                    var assinaturaVinculada = new AssinaturaEletronica
                    {
                        VendaID = abatimento.VendaId,           
                        DocumentoGuid = identificadorAssinatura, 
                        Assinado = false,
                        CriadoEm = dataOperacao,
                        ImagemAssinatura = pdfBase64             
                    };

                    await _assinaturaEletronicaRepository.AddAsync(assinaturaVinculada);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return identificadorAssinatura;
            }
            catch
            {
                await RollbackTransacaoAsync();
                throw;
            }
        }

        public IEnumerable<VendaResponse> PopulaSaldoTotal(IEnumerable<Venda> vendas)
        {
            var responses = vendas.Adapt<List<VendaResponse>>();

            var lookup = responses.ToDictionary(x => x.VendaID);

            foreach (var venda in vendas)
            {
                var response = lookup[venda.VendaID];

                var totalPago = venda.PagamentosVenda
                    .Where(p => p.Status == StatusPagamento.Pago)
                    .Sum(p => p.Valor);

                response.ValorPago = totalPago;
                response.SaldoDevedor = venda.Total - totalPago;
            }

            return responses;
        }

        private void ValidarRequestVenda(CriarVendaRequest request)
        {
            if (request is null)
                throw new BadRequestException("Dados da venda são obrigatórios.");

            if (request.ItensVenda is null || !request.ItensVenda.Any())
                throw new BadRequestException("A venda deve conter pelo menos um item.");

            if (request.TipoVenda != (int)TipoVenda.Normal && request.TipoVenda != (int)TipoVenda.Fiado)
                throw new BadRequestException("Tipo de venda inválido.");
        }
        private async Task<(Usuario, Comercio)> ValidarDadosVendaAsync(CriarVendaRequest request)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(request.UsuarioID);
            if (usuario == null || !usuario.Ativo)
                throw new BadRequestException("Usuário inválido ou inativo.");

            var comercio = await _comercioRepository.GetByIdAsync(request.ComercioID);
            if (comercio == null)
                throw new BadRequestException("Comércio inválido ou não encontrado.");

            await ValidarPermissaoUsuarioComercioAsync(request.UsuarioID, request.ComercioID);
            
            return (usuario, comercio);
        }
        private async Task<(Venda, Guid?)> ProcessarCriacaoVendaAsync(CriarVendaRequest request)
        {
            var dataOperacao = DateTime.UtcNow;

            var produtoIds = request.ItensVenda.Select(i => i.ProdutoID).ToList();
            var produtosDb = await _produtoRepository.GetProdutosById(produtoIds);
            var estoquesDb = await _estoqueRepository.GetByProdutosIdsAsync(produtoIds, request.ComercioID);

            var (itensVenda, movimentacoes, totalVenda) = await PrepararItensVendaAsync(
                request, produtosDb, estoquesDb, dataOperacao);

            var pagamentos = PrepararPagamentosVenda(request, dataOperacao);
            decimal totalPago = pagamentos.Sum(p => p.Valor);

            var ehFiado = request.TipoVenda == (int)TipoVenda.Fiado;
            Guid? identificadorAssinatura = null;

            if (ehFiado)
            {
                identificadorAssinatura = await ValidarEPreparVendaFiadoAsync(
                    request, totalVenda, totalPago, dataOperacao);
            }

            var venda = new Venda
            {
                ComercioID = request.ComercioID,
                UsuarioID = request.UsuarioID,
                ClienteID = request.ClienteID == 0 ? null : request.ClienteID,
                Total = totalVenda,
                TipoVenda = ehFiado ? TipoVenda.Fiado : TipoVenda.Normal,
                Status = ehFiado ? StatusVenda.Aguardando : StatusVenda.Finalizada,
                ItensVenda = itensVenda,
                CriadoEm = dataOperacao,
                PagamentosVenda = pagamentos
            };

            await _vendaRepository.AddAsync(venda);

            if (ehFiado && identificadorAssinatura.HasValue)
            {
                var cliente = await _clienteRepository.GetByIdAsync(request.ClienteID!.Value);
                var comercio = await _comercioRepository.GetByIdAsync(request.ComercioID);

                var assinatura = new AssinaturaEletronica
                {
                    Venda = venda,
                    DocumentoGuid = identificadorAssinatura.Value,
                    Assinado = false,
                    CriadoEm = dataOperacao,
                    ImagemAssinatura = await CriarDocAssinatura(venda, cliente!, comercio!)
                };
                await _assinaturaEletronicaRepository.AddAsync(assinatura);
            }

            foreach (var mov in movimentacoes)
            {
                mov.Venda = venda;
            }

            await _movimentacaoEstoqueRepository.AddBatchAsync(movimentacoes);

            return (venda, identificadorAssinatura);
        }
        private async Task<(List<ItemVenda>, List<MovimentacaoEstoque>, decimal)> PrepararItensVendaAsync(
            CriarVendaRequest request,
            IEnumerable<Produto> produtosDb,
            IEnumerable<Estoque> estoquesDb,
            DateTime dataOperacao)
        {
            decimal totalVenda = 0;
            var itensVenda = new List<ItemVenda>();
            var movimentacoes = new List<MovimentacaoEstoque>();
            var estoquesParaAtualizar = new List<Estoque>();

            foreach (var itemReq in request.ItensVenda)
            {
                var produto = produtosDb.FirstOrDefault(p => p.ProdutoID == itemReq.ProdutoID);
                var estoque = estoquesDb.FirstOrDefault(e => e.ProdutoID == itemReq.ProdutoID);

                ValidarVenda(produto, estoque, itemReq);

                var subtotal = itemReq.Quantidade * produto!.PrecoVenda;
                totalVenda += subtotal;

                itensVenda.Add(new ItemVenda
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
                estoque.AtualizadoEm = dataOperacao;
                estoquesParaAtualizar.Add(estoque);

                movimentacoes.Add(new MovimentacaoEstoque
                {
                    ComercioID = request.ComercioID,
                    ProdutoID = produto.ProdutoID,
                    UsuarioID = request.UsuarioID,
                    Tipo = TipoMovimentacao.Venda,
                    Quantidade = itemReq.Quantidade,
                    DataHora = dataOperacao
                });
            }

            await _estoqueRepository.UpdateBatchAsync(estoquesParaAtualizar);

            return (itensVenda, movimentacoes, totalVenda);
        }
        private List<PagamentoVenda> PrepararPagamentosVenda(CriarVendaRequest request, DateTime dataOperacao)
        {
            return request.Pagamentos?.Select(p => new PagamentoVenda
            {
                FormaPagamento = (FormaPagamento)p.FormaPagamento,
                Valor = p.Valor,
                CriadoEm = dataOperacao,
                UsuarioID = request.UsuarioID
            }).ToList() ?? new List<PagamentoVenda>();
        }
        private async Task<Guid> ValidarEPreparVendaFiadoAsync(
            CriarVendaRequest request,
            decimal totalVenda,
            decimal totalPago,
            DateTime dataOperacao)
        {
            if (!request.ClienteID.HasValue)
                throw new BadRequestException("Para vendas fiado, é obrigatório selecionar um cliente.");

            if (totalPago >= totalVenda)
                throw new BadRequestException("Uma venda fiado não pode estar totalmente paga no ato da criação.");

            var cliente = await _clienteRepository.GetByIdAsync(request.ClienteID);
            if (cliente == null)
                throw new NotFoundException("Cliente não encontrado.");

            var saldoDevedorAtual = await CalcularSaldoDevedorAsync(request.ClienteID.Value);
            decimal valorFiadoDestaVenda = totalVenda - totalPago;

            if ((saldoDevedorAtual + valorFiadoDestaVenda) > cliente.LimiteCredito)
            {
                var limiteDisponivel = cliente.LimiteCredito - saldoDevedorAtual;
                throw new BadRequestException(
                    $"Limite excedido! O cliente já deve R$ {saldoDevedorAtual:N2}. " +
                    $"Disponível para esta compra: R$ {limiteDisponivel:N2}");
            }

            return Guid.NewGuid();
        }
        private async Task RollbackTransacaoAsync()
        {
            try
            {
                await _unitOfWork.RollbackAsync();
            }
            catch
            {
                // Log ou ignorar erro de rollback
            }
        }

        private async Task ProcessarEstornoEstoqueAsync(Venda venda, int usuarioId)
        {
            var dataOperacao = DateTime.UtcNow;

            var produtoIds = venda.ItensVenda.Select(i => i.ProdutoID).ToList();
            var estoquesDb = await _estoqueRepository.GetByProdutosIdsAsync(produtoIds, venda.ComercioID);

            var estoquesParaAtualizar = new List<Estoque>();
            var movimentacoesParaInserir = new List<MovimentacaoEstoque>();
            var assinaturasParaAtualizar = new List<AssinaturaEletronica>();

            foreach (var item in venda.ItensVenda)
            {
                var estoque = estoquesDb.FirstOrDefault(e => e.ProdutoID == item.ProdutoID);

                if (estoque == null)
                    throw new NotFoundException($"Estoque não encontrado para o produto {item.ProdutoNome} no estorno.");

                estoque.Quantidade += item.Quantidade;
                estoque.AtualizadoEm = dataOperacao;
                estoquesParaAtualizar.Add(estoque);

                movimentacoesParaInserir.Add(new MovimentacaoEstoque
                {
                    ComercioID = venda.ComercioID,
                    ProdutoID = item.ProdutoID,
                    UsuarioID = usuarioId,
                    VendaID = venda.VendaID,
                    Tipo = TipoMovimentacao.Estorno,
                    Quantidade = item.Quantidade,
                    DataHora = dataOperacao,
                    Observacao = $"Estorno da Venda #{venda.VendaID}"
                });
            }

            var estornosParaInserir = new List<PagamentoVenda>();
            var pagamentosParaAtualizar = new List<PagamentoVenda>();

            foreach (var pagamento in venda.PagamentosVenda.Where(p => p.Status == StatusPagamento.Pago))
            {
                estornosParaInserir.Add(new PagamentoVenda
                {
                    VendaID = venda.VendaID,
                    FormaPagamento = pagamento.FormaPagamento,
                    Valor = -pagamento.Valor,
                    Status = StatusPagamento.Estornado,
                    CriadoEm = dataOperacao,
                    PagamentoVinculoID = pagamento.PagamentoID,
                    UsuarioID = usuarioId
                });

                pagamento.Status = StatusPagamento.Estornado;
                pagamento.AtualizadoEm = dataOperacao;
                pagamentosParaAtualizar.Add(pagamento);
            }

            if (venda.AssinaturasEletronicas != null && venda.AssinaturasEletronicas.Any())
            {
                foreach (var assinatura in venda.AssinaturasEletronicas.Where(a => a.Status != StatusAssinatura.Cancelada))
                {
                    assinatura.Status = StatusAssinatura.Cancelada;
                    assinatura.AtualizadoEm = dataOperacao;
                    assinaturasParaAtualizar.Add(assinatura);
                }
            }

            await _estoqueRepository.UpdateBatchAsync(estoquesParaAtualizar);
            await _movimentacaoEstoqueRepository.AddBatchAsync(movimentacoesParaInserir);

            if (estornosParaInserir.Any())
            {
                await _pagamentoVendaRepository.AddBatchAsync(estornosParaInserir);
            }

            if (pagamentosParaAtualizar.Any())
            {
                await _pagamentoVendaRepository.UpdateBatchAsync(pagamentosParaAtualizar);
            }

            if (assinaturasParaAtualizar.Any())
            {
                await _assinaturaEletronicaRepository.UpdateBatchAsync(assinaturasParaAtualizar);
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
        private async Task<decimal> CalcularSaldoDevedorAsync(int clienteId)
        {
            var vendasFiadoCliente = await _vendaRepository.GetVendasFiadoByClienteIDAsync(clienteId);
            var vendasAtivas = vendasFiadoCliente.Where(v => v.Status == StatusVenda.Finalizada).ToList();

            if (!vendasAtivas.Any())
                return 0m;

            var pagamentosValidos = await _pagamentoVendaRepository.GetByClienteId(clienteId);

            var pagamentosPorVenda = pagamentosValidos
                .Where(p => p.Status == StatusPagamento.Pago)
                .GroupBy(p => p.VendaID)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Valor));

            return vendasAtivas.Sum(venda =>
            {
                pagamentosPorVenda.TryGetValue(venda.VendaID, out var totalPago);
                return venda.Total - totalPago;
            });
        }

        private async Task<string> CriarDocAssinatura(Venda venda, Cliente cliente, Comercio comercio)
        {
            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdfDocument = new PdfDocument(writer);
                var document = new Document(pdfDocument, PageSize.A4);

                // Define margens limpas
                document.SetMargins(35, 45, 35, 45);

                // 🎨 Definição da Paleta de Cores Ninx (Adaptada para papel)
                Color azulNinxEscuro = new DeviceRgb(13, 27, 42);   // #0D1B2A (Cor do App)
                Color azulNinxDestaque = new DeviceRgb(14, 165, 233); // #0EA5E9 (Ciano do App)
                Color cinzaCardFundo = new DeviceRgb(248, 250, 252); // #F8FAFC
                Color cinzaLinhaSutil = new DeviceRgb(226, 232, 240); // #E2E8F0
                Color cinzaTextoMuted = new DeviceRgb(100, 116, 139); // #64748B
                Color pretoSuveTexto = new DeviceRgb(30, 41, 59);    // #1E293B

                // Fontes padrão
                var fonteNormal = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                var fonteNegrito = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);

                document.SetFont(fonteNormal);
                document.SetFontColor(pretoSuveTexto);

                // ── 1. CABEÇALHO INSTITUCIONAL ──
                var titulo = new Paragraph("TERMO DE COMPROMISSO")
                    .SetFontSize(20)
                    .SetFont(fonteNegrito)
                    .SetFontColor(azulNinxEscuro)
                    .SetMarginBottom(0);
                document.Add(titulo);

                var textoSubtitulo = new Text("PAGAMENTO E VENDA A PRAZO")
                    .SetFont(fonteNegrito)
                    .SetFontColor(azulNinxDestaque)
                    .SetHorizontalScaling(1.1f); // Expandido sutilmente em 10% para criar o efeito "spacing"

                var subtitulo = new Paragraph()
                    .Add(textoSubtitulo)
                    .SetFontSize(10)
                    .SetMarginBottom(25);

                document.Add(subtitulo);

                // ── 2. CARDS DO CREDOR E DEVEDOR (Lado a lado usando tabela invisível) ──
                var tableEnvolvidos = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 })).UseAllAvailableWidth();

                // Card Credor
                var cellCredor = new Cell()
                    .SetBackgroundColor(cinzaCardFundo)
                    .SetBorder(new SolidBorder(cinzaLinhaSutil, 1))
                    .SetPadding(12)
                    .SetBorderRadius(new BorderRadius(6));

                cellCredor.Add(new Paragraph("CREDOR (EMPRESA)").SetFont(fonteNegrito).SetFontSize(9.5f).SetFontColor(cinzaTextoMuted).SetMarginBottom(6));
                cellCredor.Add(new Paragraph($"Razão Social: {comercio.NomeComercio}").SetFontSize(10));
                cellCredor.Add(new Paragraph($"CNPJ: {comercio.CNPJ ?? "Não informado"}").SetFontSize(10));
                cellCredor.Add(new Paragraph($"Endereço: {comercio.Endereco ?? "Não informado"}").SetFontSize(10));

                // Card Devedor
                var cellDevedor = new Cell()
                    .SetBackgroundColor(cinzaCardFundo)
                    .SetBorder(new SolidBorder(cinzaLinhaSutil, 1))
                    .SetPadding(12)
                    .SetBorderRadius(new BorderRadius(6));

                cellDevedor.Add(new Paragraph("DEVEDOR (CLIENTE)").SetFont(fonteNegrito).SetFontSize(9.5f).SetFontColor(cinzaTextoMuted).SetMarginBottom(6));
                cellDevedor.Add(new Paragraph($"Nome: {cliente.Nome}").SetFontSize(10));
                cellDevedor.Add(new Paragraph($"Telefone: {cliente.Telefone ?? "Não informado"}").SetFontSize(10));

                // Adiciona à tabela estrutural com margem de separação
                tableEnvolvidos.AddCell(cellCredor.SetMarginRight(6));
                tableEnvolvidos.AddCell(cellDevedor.SetMarginLeft(6));
                document.Add(tableEnvolvidos);


                // ── 3. TABELA DE ITENS (Sem bordas pesadas) ──
                document.Add(new Paragraph("ITENS DA VENDA")
                    .SetFont(fonteNegrito).SetFontSize(11).SetFontColor(azulNinxEscuro).SetMarginTop(25).SetMarginBottom(8));

                var tableItens = new Table(UnitValue.CreatePercentArray(new float[] { 50, 15, 15, 20 })).UseAllAvailableWidth();

                string[] cabecalhos = { "Descrição do Produto", "Qtd.", "VL. Unitário", "Subtotal" };
                foreach (var text in cabecalhos)
                {
                    var textAlignment = text == "Descrição do Produto" ? TextAlignment.LEFT : (text == "Qtd." ? TextAlignment.CENTER : TextAlignment.RIGHT);

                    tableItens.AddHeaderCell(new Cell()
                        .SetBackgroundColor(cinzaCardFundo)
                        .SetBorder(Border.NO_BORDER)
                        .SetBorderBottom(new SolidBorder(cinzaTextoMuted, 1.5f))
                        .SetPadding(8)
                        .Add(new Paragraph(text).SetFontSize(9.5f).SetFont(fonteNegrito).SetFontColor(cinzaTextoMuted).SetTextAlignment(textAlignment)));
                }

                foreach (var item in venda.ItensVenda)
                {
                    tableItens.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetBorderBottom(new SolidBorder(cinzaLinhaSutil, 0.5f)).SetPadding(10).Add(new Paragraph(item.ProdutoNome).SetFontSize(10)));
                    tableItens.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetBorderBottom(new SolidBorder(cinzaLinhaSutil, 0.5f)).SetPadding(10).Add(new Paragraph(item.Quantidade.ToString("N2")).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER)));
                    tableItens.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetBorderBottom(new SolidBorder(cinzaLinhaSutil, 0.5f)).SetPadding(10).Add(new Paragraph($"R$ {item.PrecoUnitario:N2}").SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT)));
                    tableItens.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetBorderBottom(new SolidBorder(cinzaLinhaSutil, 0.5f)).SetPadding(10).Add(new Paragraph($"R$ {item.Subtotal:N2}").SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT)));
                }
                document.Add(tableItens);


                // ── 4. RESUMO FINANCEIRO (Formato de Card Focado) ──
                document.Add(new Paragraph("RESUMO FINANCEIRO E CONDIÇÕES")
                    .SetFont(fonteNegrito).SetFontSize(11).SetFontColor(azulNinxEscuro).SetMarginTop(20).SetMarginBottom(8));

                var pagamentosValidos = venda.PagamentosVenda
                    .Where(p => p.Status == StatusPagamento.Pago)
                    .ToList();

                decimal totalPago = pagamentosValidos.Sum(p => p.Valor);
                decimal saldoDevedor = venda.Total - totalPago;

                var tableResumo = new Table(UnitValue.CreatePercentArray(new float[] { 75, 25 })).UseAllAvailableWidth();
                var cardResumo = new Cell(1, 2)
                    .SetBackgroundColor(cinzaCardFundo)
                    .SetBorder(new SolidBorder(cinzaLinhaSutil, 1))
                    .SetBorderRadius(new BorderRadius(6))
                    .SetPadding(14);

                var innerTable = new Table(UnitValue.CreatePercentArray(new float[] { 75, 25 })).UseAllAvailableWidth();
                innerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph("Valor Total da Venda").SetFontSize(10).SetFontColor(cinzaTextoMuted)));
                innerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph($"R$ {venda.Total:N2}").SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT)));

                innerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph("Valor Pago de Entrada").SetFontSize(10).SetFontColor(cinzaTextoMuted)));
                innerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph($"R$ {totalPago:N2}").SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT)));

                // Linha divisória interna pontilhada para o totalizador importante
                innerTable.AddCell(new Cell(1, 2).SetBorder(Border.NO_BORDER).SetBorderTop(new DashedBorder(cinzaLinhaSutil, 1)).SetMarginTop(6));

                innerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetPaddingTop(6).Add(new Paragraph("Saldo Devedor Remanescente").SetFont(fonteNegrito).SetFontSize(11.5f)));
                innerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetPaddingTop(6).Add(new Paragraph($"R$ {saldoDevedor:N2}").SetFont(fonteNegrito).SetFontSize(11.5f).SetFontColor(azulNinxDestaque).SetTextAlignment(TextAlignment.RIGHT)));

                cardResumo.Add(innerTable);
                tableResumo.AddCell(cardResumo);
                document.Add(tableResumo);


                // ── 5. SEÇÃO DE ASSINATURAS MINIMALISTA ──
                var tableAssinaturas = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 })).UseAllAvailableWidth().SetMarginTop(50);

                tableAssinaturas.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetPaddingRight(20)
                    .Add(new Paragraph()
                        .SetHeight(45) // Espaço para a caneta/vetor assinar
                        .SetBorderTop(new SolidBorder(cinzaTextoMuted, 0.75f))
                        .Add(new Text("ASSINATURA DO DEVEDOR\n").SetFont(fonteNegrito).SetFontSize(8.5f).SetFontColor(cinzaTextoMuted))
                        .Add(new Text(cliente.Nome).SetFontSize(9.5f))
                        .SetTextAlignment(TextAlignment.CENTER).SetMarginTop(10)));

                tableAssinaturas.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetPaddingLeft(20)
                    .Add(new Paragraph()
                        .SetHeight(45)
                        .SetBorderTop(new SolidBorder(cinzaTextoMuted, 0.75f))
                        .Add(new Text("ASSINATURA DO CREDOR\n").SetFont(fonteNegrito).SetFontSize(8.5f).SetFontColor(cinzaTextoMuted))
                        .Add(new Text(comercio.NomeComercio).SetFontSize(9.5f))
                        .SetTextAlignment(TextAlignment.CENTER).SetMarginTop(10)));

                document.Add(tableAssinaturas);

                // ── 6. RODAPÉ DE EMISSÃO ──
                document.Add(new Paragraph($"Data de Emissão: {venda.CriadoEm:dd/MM/yyyy}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontColor(cinzaTextoMuted)
                    .SetFontSize(8.5f)
                    .SetMarginTop(40));

                document.Close();

                var pdfBytes = memoryStream.ToArray();
                return Convert.ToBase64String(pdfBytes);
            }
        }

        private async Task<string> CriarDocReciboPagamento(Venda venda, PagamentoVenda pagamento, Cliente cliente, Comercio comercio, decimal saldoDevedorAnterior)
        {
            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdfDocument = new PdfDocument(writer);
                var document = new Document(pdfDocument, PageSize.A4);

                document.SetMargins(35, 45, 35, 45);

                // 🎨 Paleta Ninx
                Color azulNinxEscuro = new DeviceRgb(13, 27, 42);
                Color azulNinxDestaque = new DeviceRgb(14, 165, 233);
                Color cinzaCardFundo = new DeviceRgb(248, 250, 252);
                Color cinzaLinhaSutil = new DeviceRgb(226, 232, 240);
                Color cinzaTextoMuted = new DeviceRgb(100, 116, 139);
                Color pretoSuveTexto = new DeviceRgb(30, 41, 59);

                var fonteNormal = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                var fonteNegrito = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);

                document.SetFont(fonteNormal);
                document.SetFontColor(pretoSuveTexto);

                // ── 1. CABEÇALHO ──
                var titulo = new Paragraph("RECIBO DE PAGAMENTO PARCIAL")
                    .SetFontSize(20)
                    .SetFont(fonteNegrito)
                    .SetFontColor(azulNinxEscuro)
                    .SetMarginBottom(0);
                document.Add(titulo);

                var subtitulo = new Paragraph()
                    .Add(new Text($"REFERENTE À VENDA #{venda.VendaID}").SetFont(fonteNegrito).SetFontColor(azulNinxDestaque).SetHorizontalScaling(1.1f))
                    .SetFontSize(10)
                    .SetMarginBottom(25);
                document.Add(subtitulo);

                // ── 2. ENVOLVIDOS ──
                var tableEnvolvidos = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 })).UseAllAvailableWidth();

                var cellCredor = new Cell()
                    .SetBackgroundColor(cinzaCardFundo)
                    .SetBorder(new SolidBorder(cinzaLinhaSutil, 1))
                    .SetPadding(12)
                    .SetBorderRadius(new BorderRadius(6));
                cellCredor.Add(new Paragraph("CREDOR").SetFont(fonteNegrito).SetFontSize(9.5f).SetFontColor(cinzaTextoMuted).SetMarginBottom(6));
                cellCredor.Add(new Paragraph($"{comercio.NomeComercio}").SetFontSize(10));
                cellCredor.Add(new Paragraph($"CNPJ: {comercio.CNPJ ?? "Não informado"}").SetFontSize(10));

                var cellDevedor = new Cell()
                    .SetBackgroundColor(cinzaCardFundo)
                    .SetBorder(new SolidBorder(cinzaLinhaSutil, 1))
                    .SetPadding(12)
                    .SetBorderRadius(new BorderRadius(6));
                cellDevedor.Add(new Paragraph("DEVEDOR").SetFont(fonteNegrito).SetFontSize(9.5f).SetFontColor(cinzaTextoMuted).SetMarginBottom(6));
                cellDevedor.Add(new Paragraph($"{cliente.Nome}").SetFontSize(10));
                cellDevedor.Add(new Paragraph($"Telefone: {cliente.Telefone ?? "Não informado"}").SetFontSize(10));

                tableEnvolvidos.AddCell(cellCredor.SetMarginRight(6));
                tableEnvolvidos.AddCell(cellDevedor.SetMarginLeft(6));
                document.Add(tableEnvolvidos);

                var declaracao = new Paragraph()
                    .SetMarginTop(30)
                    .SetMarginBottom(30)
                    .SetFontSize(11)
                    .SetMultipliedLeading(1.3f) 
                    .Add(new Text("Declaramos para os devidos fins que o devedor acima identificado realizou o pagamento manual da quantia de "))
                    .Add(new Text($"R$ {pagamento.Valor:N2}").SetFont(fonteNegrito).SetFontColor(azulNinxEscuro))
                    .Add(new Text($" através da forma de pagamento "))
                    .Add(new Text($"{pagamento.FormaPagamento}").SetFont(fonteNegrito))
                    .Add(new Text(", abatendo do saldo devedor remanescente desta transação comercial."));
                document.Add(declaracao);

                // ── 4. RESUMO FINANCEIRO ATUALIZADO ──
                document.Add(new Paragraph("DEMONSTRATIVO DO SALDO")
                    .SetFont(fonteNegrito).SetFontSize(11).SetFontColor(azulNinxEscuro).SetMarginBottom(8));

                decimal novoSaldoDevedor = saldoDevedorAnterior - pagamento.Valor;

                var tableResumo = new Table(UnitValue.CreatePercentArray(new float[] { 100 })).UseAllAvailableWidth();
                var cardResumo = new Cell()
                    .SetBackgroundColor(cinzaCardFundo)
                    .SetBorder(new SolidBorder(cinzaLinhaSutil, 1))
                    .SetBorderRadius(new BorderRadius(6))
                    .SetPadding(14);

                var innerTable = new Table(UnitValue.CreatePercentArray(new float[] { 75, 25 })).UseAllAvailableWidth();
                innerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph("Saldo Devedor Antes deste Pagamento").SetFontSize(10).SetFontColor(cinzaTextoMuted)));
                innerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph($"R$ {saldoDevedorAnterior:N2}").SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT)));

                innerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph("Valor Pago Neste Ato (-)").SetFontSize(10).SetFontColor(azulNinxDestaque).SetFont(fonteNegrito)));
                innerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(new Paragraph($"R$ {pagamento.Valor:N2}").SetFontSize(10).SetFontColor(azulNinxDestaque).SetFont(fonteNegrito).SetTextAlignment(TextAlignment.RIGHT)));

                innerTable.AddCell(new Cell(1, 2).SetBorder(Border.NO_BORDER).SetBorderTop(new DashedBorder(cinzaLinhaSutil, 1)).SetMarginTop(6));

                innerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetPaddingTop(6).Add(new Paragraph("Saldo Devedor Atual Restante").SetFont(fonteNegrito).SetFontSize(11.5f)));
                innerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetPaddingTop(6).Add(new Paragraph($"R$ {novoSaldoDevedor:N2}").SetFont(fonteNegrito).SetFontSize(11.5f).SetFontColor(azulNinxEscuro).SetTextAlignment(TextAlignment.RIGHT)));

                cardResumo.Add(innerTable);
                tableResumo.AddCell(cardResumo);
                document.Add(tableResumo);

                // ── 5. SEÇÃO DE ASSINATURAS ──
                var tableAssinaturas = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 })).UseAllAvailableWidth().SetMarginTop(60);

                tableAssinaturas.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetPaddingRight(20)
                    .Add(new Paragraph()
                        .SetHeight(45)
                        .SetBorderTop(new SolidBorder(cinzaTextoMuted, 0.75f))
                        .Add(new Text("ASSINATURA DO CLIENTE\n").SetFont(fonteNegrito).SetFontSize(8.5f).SetFontColor(cinzaTextoMuted))
                        .Add(new Text(cliente.Nome).SetFontSize(9.5f))
                        .SetTextAlignment(TextAlignment.CENTER).SetMarginTop(10)));

                tableAssinaturas.AddCell(new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetPaddingLeft(20)
                    .Add(new Paragraph()
                        .SetHeight(45)
                        .SetBorderTop(new SolidBorder(cinzaTextoMuted, 0.75f))
                        .Add(new Text("RESPONSÁVEL RECEBIMENTO\n").SetFont(fonteNegrito).SetFontSize(8.5f).SetFontColor(cinzaTextoMuted))
                        .Add(new Text(comercio.NomeComercio).SetFontSize(9.5f))
                        .SetTextAlignment(TextAlignment.CENTER).SetMarginTop(10)));

                document.Add(tableAssinaturas);

                // ── 6. RODAPÉ ──
                document.Add(new Paragraph($"Recibo emitido em: {pagamento.CriadoEm:dd/MM/yyyy HH:mm:ss} UTC")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontColor(cinzaTextoMuted)
                    .SetFontSize(8.5f)
                    .SetMarginTop(40));

                document.Close();

                var pdfBytes = memoryStream.ToArray();
                return Convert.ToBase64String(pdfBytes);
            }
        }

        private async Task<string> CriarDocReciboGlobalPagamento(List<ItemAbatimentoGlobal> abatimentos, decimal valorTotal, FormaPagamento formaPagamento, Cliente cliente, Comercio comercio, DateTime dataOperacao)
        {
            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdfDocument = new PdfDocument(writer);
                var document = new Document(pdfDocument, PageSize.A4);

                document.SetMargins(35, 45, 35, 45);

                Color azulNinxEscuro = new DeviceRgb(13, 27, 42);
                Color azulNinxDestaque = new DeviceRgb(14, 165, 233);
                Color cinzaCardFundo = new DeviceRgb(248, 250, 252);
                Color cinzaLinhaSutil = new DeviceRgb(226, 232, 240);
                Color cinzaTextoMuted = new DeviceRgb(100, 116, 139);
                Color pretoSuveTexto = new DeviceRgb(30, 41, 59);

                var fonteNormal = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                var fonteNegrito = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);

                document.SetFont(fonteNormal).SetFontColor(pretoSuveTexto);

                // ── CABEÇALHO ──
                document.Add(new Paragraph("RECIBO DE QUITAÇÃO GLOBAL / ABATIMENTO")
                    .SetFontSize(18).SetFont(fonteNegrito).SetFontColor(azulNinxEscuro).SetMarginBottom(0));

                document.Add(new Paragraph("CONTA FIADO - MULTIPLAS TRANSAÇÕES")
                    .SetFontSize(9).SetFont(fonteNegrito).SetFontColor(azulNinxDestaque).SetMarginBottom(20));

                // ── ENVOLVIDOS ──
                var tableEnvolvidos = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 })).UseAllAvailableWidth();

                var cellCredor = new Cell().SetBackgroundColor(cinzaCardFundo).SetBorder(new SolidBorder(cinzaLinhaSutil, 1)).SetPadding(10).SetBorderRadius(new BorderRadius(4));
                cellCredor.Add(new Paragraph("CREDOR").SetFont(fonteNegrito).SetFontSize(8.5f).SetFontColor(cinzaTextoMuted));
                cellCredor.Add(new Paragraph(comercio.NomeComercio).SetFontSize(9.5f));

                var cellDevedor = new Cell().SetBackgroundColor(cinzaCardFundo).SetBorder(new SolidBorder(cinzaLinhaSutil, 1)).SetPadding(10).SetBorderRadius(new BorderRadius(4));
                cellDevedor.Add(new Paragraph("DEVEDOR").SetFont(fonteNegrito).SetFontSize(8.5f).SetFontColor(cinzaTextoMuted));
                cellDevedor.Add(new Paragraph(cliente.Nome).SetFontSize(9.5f));

                tableEnvolvidos.AddCell(cellCredor.SetMarginRight(4));
                tableEnvolvidos.AddCell(cellDevedor.SetMarginLeft(4));
                document.Add(tableEnvolvidos);

                // ── DECLARAÇÃO ──
                document.Add(new Paragraph()
                    .SetMarginTop(20).SetMarginBottom(20).SetFontSize(10.5f).SetMultipliedLeading(1.3f)
                    .Add(new Text("Confirmamos o recebimento do valor total de "))
                    .Add(new Text($"R$ {valorTotal:N2}").SetFont(fonteNegrito).SetFontColor(azulNinxEscuro))
                    .Add(new Text($" via de pagamento "))
                    .Add(new Text($"{formaPagamento}").SetFont(fonteNegrito))
                    .Add(new Text(", utilizado para abater e/ou quitar o saldo devedor das vendas descritas no demonstrativo abaixo:")));

                // ── TABELA DE DISTRIBUIÇÃO DO VALOR ──
                var tableDistribuição = new Table(UnitValue.CreatePercentArray(new float[] { 15, 20, 22, 21, 22 })).UseAllAvailableWidth();

                // Headers
                string[] headers = { "Cód. Venda", "Data Venda", "Saldo Anterior", "Valor Abatido", "Saldo Restante" };
                foreach (var h in headers)
                {
                    tableDistribuição.AddHeaderCell(new Cell().SetBackgroundColor(azulNinxEscuro).SetPadding(6)
                        .Add(new Paragraph(h).SetFont(fonteNegrito).SetFontSize(9).SetFontColor(ColorConstants.WHITE).SetTextAlignment(TextAlignment.CENTER)));
                }

                // Linhas das Vendas
                foreach (var item in abatimentos)
                {
                    tableDistribuição.AddCell(new Cell().SetPadding(5).SetTextAlignment(TextAlignment.CENTER).Add(new Paragraph($"#{item.VendaId}").SetFontSize(9)));
                    tableDistribuição.AddCell(new Cell().SetPadding(5).SetTextAlignment(TextAlignment.CENTER).Add(new Paragraph($"{item.DataVenda:dd/MM/yyyy}").SetFontSize(9)));
                    tableDistribuição.AddCell(new Cell().SetPadding(5).SetTextAlignment(TextAlignment.RIGHT).Add(new Paragraph($"R$ {item.SaldoAnterior:N2}").SetFontSize(9)));
                    tableDistribuição.AddCell(new Cell().SetPadding(5).SetTextAlignment(TextAlignment.RIGHT).Add(new Paragraph($"R$ {item.ValorAbatido:N2}").SetFont(fonteNegrito).SetFontColor(azulNinxDestaque).SetFontSize(9)));
                    tableDistribuição.AddCell(new Cell().SetPadding(5).SetTextAlignment(TextAlignment.RIGHT).Add(new Paragraph($"R$ {item.SaldoRestante:N2}").SetFontSize(9)));
                }
                document.Add(tableDistribuição);

                // ── ASSINATURAS ──
                var tableAssinaturas = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 })).UseAllAvailableWidth().SetMarginTop(50);
                tableAssinaturas.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetPaddingRight(20)
                    .Add(new Paragraph().SetHeight(40).SetBorderTop(new SolidBorder(cinzaTextoMuted, 0.75f))
                        .Add(new Text("ASSINATURA DO CLIENTE\n").SetFont(fonteNegrito).SetFontSize(8f).SetFontColor(cinzaTextoMuted))
                        .Add(new Text(cliente.Nome).SetFontSize(9f)).SetTextAlignment(TextAlignment.CENTER).SetMarginTop(5)));

                tableAssinaturas.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetPaddingLeft(20)
                    .Add(new Paragraph().SetHeight(40).SetBorderTop(new SolidBorder(cinzaTextoMuted, 0.75f))
                        .Add(new Text("RESPONSÁVEL RECEBIMENTO\n").SetFont(fonteNegrito).SetFontSize(8f).SetFontColor(cinzaTextoMuted))
                        .Add(new Text(comercio.NomeComercio).SetFontSize(9f)).SetTextAlignment(TextAlignment.CENTER).SetMarginTop(5)));
                document.Add(tableAssinaturas);

                // ── RODAPÉ ──
                document.Add(new Paragraph($"Documento Global emitido em: {dataOperacao:dd/MM/yyyy HH:mm:ss} UTC")
                    .SetTextAlignment(TextAlignment.CENTER).SetFontColor(cinzaTextoMuted).SetFontSize(8f).SetMarginTop(35));

                document.Close();
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
    }
}
