using Mapster;
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
            var venda = await _vendaRepository.GetByIdComItensAsync(id);
            if (venda is null)
            {
                throw new NotFoundException("Venda não encontrada");
            }

            var response = venda.Adapt<VendaResponse>();
            
            if (venda.AssinaturasEletronicas?.Any() == true)
            {
                var assinatura = venda.AssinaturasEletronicas.First();
                response.DocumentoGuid = assinatura.DocumentoGuid;
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
                        response.DocumentoGuid = documentoGuid.Value;
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
        public async Task ReceberPagamentoFiadoAsync(int vendaId, int usuarioId, decimal valorPago, int formaPagamento)
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

                if (venda.Status != StatusVenda.Finalizada)
                    throw new BadRequestException("Não é possível receber pagamentos para uma venda que não está finalizada.");

                await ValidarPermissaoUsuarioComercioAsync(usuarioId, venda.ComercioID);

                var pagamentosAnteriores = venda.PagamentosVenda
                    .Where(p => p.Status == StatusPagamento.Pago)
                    .Sum(p => p.Valor);

                var saldoDevedorVenda = venda.Total - pagamentosAnteriores;

                if (valorPago > saldoDevedorVenda)
                    throw new BadRequestException($"Valor informado (R$ {valorPago:N2}) é maior que o saldo devedor da venda (R$ {saldoDevedorVenda:N2}).");

                var novoPagamento = new PagamentoVenda
                {
                    VendaID = venda.VendaID,
                    FormaPagamento = (FormaPagamento)formaPagamento,
                    Valor = valorPago,
                    Status = StatusPagamento.Pago,
                    CriadoEm = DateTime.UtcNow,
                    UsuarioID = usuarioId,
                };

                await _pagamentoVendaRepository.AddAsync(novoPagamento);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await RollbackTransacaoAsync();
                throw;
            }
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
                ClienteID = request.ClienteID,
                Total = totalVenda,
                TipoVenda = ehFiado ? TipoVenda.Fiado : TipoVenda.Normal,
                Status = StatusVenda.Finalizada,
                ItensVenda = itensVenda,
                CriadoEm = dataOperacao,
                PagamentosVenda = pagamentos
            };

            await _vendaRepository.AddAsync(venda);

            if (ehFiado && identificadorAssinatura.HasValue)
            {
                var assinatura = new AssinaturaEletronica
                {
                    Venda = venda,
                    DocumentoGuid = identificadorAssinatura.Value,
                    Assinado = false,
                    CriadoEm = dataOperacao
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
                CriadoEm = dataOperacao
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
            }

            if (venda.AssinaturasEletronicas?.Any() == true)
            {
                foreach (var assinatura in venda.AssinaturasEletronicas)
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

            if (venda.PagamentosVenda.Any())
            {
                await _pagamentoVendaRepository.UpdateBatchAsync(venda.PagamentosVenda.ToList());
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
    }
}
