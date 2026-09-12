using Mapster;
using ninx.Communication;
using ninx.Domain.Constants;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class AssinaturaPlanoService : IAssinaturaPlanoService
    {
        private readonly IAssinaturaPlanoRepository _AssinaturaPlanoRepository;
        private readonly IAutorizacaoCargoService _autorizacaoCargoService;
        private readonly IUnitOfWork _unitOfWork;
        public AssinaturaPlanoService(IAssinaturaPlanoRepository AssinaturaPlanoRepository, IAutorizacaoCargoService autorizacaoCargoService, IUnitOfWork unitOfWork)
        {
            _AssinaturaPlanoRepository = AssinaturaPlanoRepository;
            _autorizacaoCargoService = autorizacaoCargoService;
            _unitOfWork = unitOfWork;
        }
    
        public async Task<PaginatedResponse<AssinaturaPlanoResponse>> GetAll(PaginationRequest request)
        {
            var (entidades, total) = await _AssinaturaPlanoRepository.GetPaginatedAsync(request.PageNumber, request.PageSize);
            var listaResponse = entidades.Adapt<List<AssinaturaPlanoResponse>>();

            return new PaginatedResponse<AssinaturaPlanoResponse>(
                listaResponse,
                request.PageNumber,
                request.PageSize,
                total
            );
        }
    
        public async Task<AssinaturaPlanoResponse> GetByIdAsync(int id)
        {
            var AssinaturaPlano = await _AssinaturaPlanoRepository.GetByIdAsync(id);
            if (AssinaturaPlano == null)
                throw new NotFoundException("AssinaturaPlano não encontrado.");

            return AssinaturaPlano.Adapt<AssinaturaPlanoResponse>();
        }

        public async Task<AssinaturaPlanoResponse> GetByComercioIdAsync(int comercioId, bool ehProprietarioLogado, IEnumerable<string> permissoesLogado)
        {
            _autorizacaoCargoService.GarantirPermissao(false, ehProprietarioLogado, permissoesLogado,
                PermissaoConstantes.GerenciarAssinatura, "Funcionários não podem consultar a assinatura do comércio.");

            var assinatura = await _AssinaturaPlanoRepository.GetByComercioIdAsync(comercioId);
            if (assinatura == null)
                throw new NotFoundException("Nenhuma assinatura encontrada para este comércio.");

            return assinatura.Adapt<AssinaturaPlanoResponse>();
        }

        public async Task CancelarAsync(int comercioId, bool ehProprietarioLogado, IEnumerable<string> permissoesLogado)
        {
            _autorizacaoCargoService.GarantirPermissao(false, ehProprietarioLogado, permissoesLogado,
                PermissaoConstantes.GerenciarAssinatura, "Funcionários não podem cancelar a assinatura do comércio.");

            var assinatura = await _AssinaturaPlanoRepository.GetByComercioIdAsync(comercioId);
            if (assinatura == null)
                throw new NotFoundException("Nenhuma assinatura encontrada para este comércio.");

            if (assinatura.Status != StatusAssinatura.Ativa)
                throw new BadRequestException("Assinatura não está ativa para ser cancelada.");

            if (assinatura.CancelamentoSolicitadoEm != null)
                throw new BadRequestException("Já existe um cancelamento agendado para esta assinatura.");

            assinatura.CancelamentoSolicitadoEm = DateTime.UtcNow;
            assinatura.AtualizadoEm = DateTime.UtcNow;
            await _AssinaturaPlanoRepository.UpdateAsync(assinatura);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
