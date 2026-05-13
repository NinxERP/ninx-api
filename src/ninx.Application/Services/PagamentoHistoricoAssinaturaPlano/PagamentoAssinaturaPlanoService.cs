using Mapster;
using Microsoft.Extensions.Logging;
using ninx.Application.Services.PagamentoHistoricoAssinaturaPlano;
using ninx.Communication;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PagamentoHistoricoAssinaturaPlanoService> _logger;

        public PagamentoHistoricoAssinaturaPlanoService(
            IPagamentoHistoricoAssinaturaPlanoRepository PagamentoHistoricoAssinaturaPlanoRepository, 
            IAssinaturaPlanoRepository assinaturaPlanoRepository, 
            IUnitOfWork unitOfWork,
            ILogger<PagamentoHistoricoAssinaturaPlanoService> logger)
        {
            _pagamentoHistoricoAssinaturaPlanoRepository = PagamentoHistoricoAssinaturaPlanoRepository;
            _assinaturaPlanoRepository = assinaturaPlanoRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task RegistrarPagamentos(PagamentoHistoricoAssinaturaPlanoRequest request, Permissao permissao)
        {
            if (request == null) throw new BadRequestException("Solicitação de pagamento não pode ser nula.");
            if (permissao != Permissao.Administrador) throw new ForbiddenException("Permissão insuficiente para registrar pagamento.");
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
            await _unitOfWork.CommitAsync();

            _logger.LogInformation($"Pagamento registrado com sucesso. AssinaturaID: {assinatura.AssinaturaID} ComercioID: {request.ComercioId} DataVencimento: {novoVencimento}" 
           );
        }
    }
}
