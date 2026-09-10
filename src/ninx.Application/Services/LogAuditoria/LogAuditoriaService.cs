using Mapster;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Application.Services
{
    public class LogAuditoriaService : ILogAuditoriaService
    {
        private readonly ILogAuditoriaRepository _logAuditoriaRepository;
        private readonly IAutorizacaoGlobalService _autorizacaoGlobalService;

        public LogAuditoriaService(ILogAuditoriaRepository logAuditoriaRepository, IAutorizacaoGlobalService autorizacaoGlobalService)
        {
            _logAuditoriaRepository = logAuditoriaRepository;
            _autorizacaoGlobalService = autorizacaoGlobalService;
        }

        public async Task RegistrarAsync(int usuarioId, int? comercioId, string acao, string entidade, int entidadeId, string? detalhes = null)
        {
            var log = new LogAuditoria
            {
                UsuarioID = usuarioId,
                ComercioID = comercioId,
                Acao = acao,
                Entidade = entidade,
                EntidadeID = entidadeId,
                Detalhes = detalhes,
                CriadoEm = DateTime.UtcNow
            };

            await _logAuditoriaRepository.AddAsync(log);
        }

        public async Task<PaginatedResponse<LogAuditoriaResponse>> GetAllAsync(int usuarioIdLogado, int? comercioId, int? usuarioId, PaginationRequest request)
        {
            await _autorizacaoGlobalService.GarantirAdministradorGlobalAsync(usuarioIdLogado);

            var (entidades, total) = await _logAuditoriaRepository.GetPaginatedFiltradoAsync(comercioId, usuarioId, request.PageNumber, request.PageSize);
            var listaResponse = entidades.Adapt<List<LogAuditoriaResponse>>();

            return new PaginatedResponse<LogAuditoriaResponse>(
                listaResponse,
                request.PageNumber,
                request.PageSize,
                total
            );
        }
    }
}
