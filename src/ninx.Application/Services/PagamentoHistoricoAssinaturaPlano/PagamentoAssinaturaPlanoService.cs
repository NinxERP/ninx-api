using Mapster;
using Microsoft.Extensions.Logging;
using ninx.Communication;
using ninx.Domain.Constants;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Application.Services
{
    public class PagamentoHistoricoAssinaturaPlanoService : IPagamentoHistoricoAssinaturaPlanoService
    {
        private readonly IPagamentoHistoricoAssinaturaPlanoRepository _pagamentoHistoricoAssinaturaPlanoRepository;
        private readonly IAssinaturaPlanoRepository _assinaturaPlanoRepository;
        private readonly IAutorizacaoGlobalService _autorizacaoGlobalService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PagamentoHistoricoAssinaturaPlanoService> _logger;

        public PagamentoHistoricoAssinaturaPlanoService(
            IPagamentoHistoricoAssinaturaPlanoRepository PagamentoHistoricoAssinaturaPlanoRepository,
            IAssinaturaPlanoRepository assinaturaPlanoRepository,
            IAutorizacaoGlobalService autorizacaoGlobalService,
            IUnitOfWork unitOfWork,
            ILogger<PagamentoHistoricoAssinaturaPlanoService> logger)
        {
            _pagamentoHistoricoAssinaturaPlanoRepository = PagamentoHistoricoAssinaturaPlanoRepository;
            _assinaturaPlanoRepository = assinaturaPlanoRepository;
            _autorizacaoGlobalService = autorizacaoGlobalService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task RegistrarPagamentos(PagamentoHistoricoAssinaturaPlanoRequest request, int usuarioLogadoId)
        {
            if (request == null) throw new BadRequestException("Solicitação de pagamento não pode ser nula.");
            await _autorizacaoGlobalService.GarantirAdministradorGlobalAsync(usuarioLogadoId);
            if (request.ComercioId <= 0) throw new BadRequestException("ComercioId inválido.");


            var assinatura = await _assinaturaPlanoRepository.GetByComercioIdAsync(request.ComercioId);
            if (assinatura == null) throw new NotFoundException("Nenhuma assinatura encontrada para este comércio.");

            var hoje = DateTime.UtcNow;
            DateTime dataBase = (assinatura.DataFim > hoje) ? assinatura.DataFim : hoje;
            int mesesParaAdicionar = (int)assinatura.Plano;
            DateTime novoVencimento = dataBase.AddMonths(mesesParaAdicionar);

            assinatura.Status = StatusAssinatura.Ativa;
            assinatura.DataFim = novoVencimento;
            assinatura.AtualizadoEm = hoje;
            await _assinaturaPlanoRepository.UpdateAsync(assinatura);

            var newPagamento = request.Adapt<PagamentoHistoricoAssinaturaPlano>();

            newPagamento.AssinaturaID = assinatura.AssinaturaID;
            newPagamento.DataPagamento = hoje;
            newPagamento.DataVencimento = novoVencimento;
            newPagamento.AtualizadoEm = hoje;

            await _pagamentoHistoricoAssinaturaPlanoRepository.AddAsync(newPagamento);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Pagamento registrado com sucesso. AssinaturaID: {assinatura.AssinaturaID} ComercioID: {request.ComercioId} DataVencimento: {novoVencimento}"
           );
        }

        public async Task<PaginatedResponse<PagamentoHistoricoAssinaturaPlanoResponse>> GetHistoricoByComercioIdAsync(int comercioId, int pesoLogado, PaginationRequest request)
        {
            if (pesoLogado < CargoConstantes.PesoDono)
                throw new ForbiddenException("Funcionários não podem consultar o histórico de pagamentos da assinatura.");

            var (entidades, total) = await _pagamentoHistoricoAssinaturaPlanoRepository.GetPaginadoByComercioIdAsync(comercioId, request.PageNumber, request.PageSize);
            var listaResponse = entidades.Adapt<List<PagamentoHistoricoAssinaturaPlanoResponse>>();

            return new PaginatedResponse<PagamentoHistoricoAssinaturaPlanoResponse>(
                listaResponse,
                request.PageNumber,
                request.PageSize,
                total
            );
        }
    }
}
