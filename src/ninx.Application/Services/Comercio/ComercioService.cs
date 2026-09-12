using Mapster;
using ninx.Communication;
using ninx.Domain.Constants;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Application.Services
{
    public class ComercioService : IComercioService
    {
        private readonly IComercioRepository _comercioRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUsuarioComercioRepository _usuarioComercioRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAssinaturaPlanoRepository _assinaturaPlanoRepository;
        private readonly IPagamentoHistoricoAssinaturaPlanoRepository _pagamentoHistoricoAssinaturaPlanoRepository;
        private readonly IAutorizacaoCargoService _autorizacaoCargoService;
        private readonly IAutorizacaoGlobalService _autorizacaoGlobalService;
        private readonly ILogAuditoriaService _logAuditoriaService;

        public ComercioService(IComercioRepository comercioRepository, IUnitOfWork unitOfWork, IUsuarioComercioRepository usuarioComercioRepository, IUsuarioRepository usuarioRepository, IAssinaturaPlanoRepository assinaturaPlanoRepository, IPagamentoHistoricoAssinaturaPlanoRepository PagamentoHistoricoAssinaturaPlanoRepository, IAutorizacaoCargoService autorizacaoCargoService, IAutorizacaoGlobalService autorizacaoGlobalService, ILogAuditoriaService logAuditoriaService)
        {
            _comercioRepository = comercioRepository;
            _unitOfWork = unitOfWork;
            _usuarioComercioRepository = usuarioComercioRepository;
            _usuarioRepository = usuarioRepository;
            _assinaturaPlanoRepository = assinaturaPlanoRepository;
            _pagamentoHistoricoAssinaturaPlanoRepository = PagamentoHistoricoAssinaturaPlanoRepository;
            _autorizacaoCargoService = autorizacaoCargoService;
            _autorizacaoGlobalService = autorizacaoGlobalService;
            _logAuditoriaService = logAuditoriaService;
        }


        public async Task<PaginatedResponse<ComercioResponse>> GetAll(PaginationRequest request, int usuarioIdLogado)
        {
            await _autorizacaoGlobalService.GarantirAdministradorGlobalAsync(usuarioIdLogado);

            var (entidades, total) = await _comercioRepository.GetPaginatedAsync(request.PageNumber, request.PageSize, request.TermoBusca);
            var listaResponse = entidades.Adapt<List<ComercioResponse>>();

            return new PaginatedResponse<ComercioResponse>(
                listaResponse,
                request.PageNumber,
                request.PageSize,
                total
            );
        }
        public async Task<ComercioResponse> GetByIdAsync(int id, int usuarioIdLogado)
        {
            var comercio = await _comercioRepository.GetByIdAsync(id);
            if (comercio == null)
                throw new NotFoundException("Comércio não encontrado.");

            var chamadorEhAdmin = await _autorizacaoCargoService.EhAdminGlobalAsync(usuarioIdLogado);
            if (!chamadorEhAdmin)
            {
                var vinculo = await _usuarioComercioRepository.GetVinculoAsync(usuarioIdLogado, id);
                if (vinculo == null)
                    throw new NotFoundException("Comércio não encontrado.");
            }

            return comercio.Adapt<ComercioResponse>();
        }
        public async Task<IEnumerable<ComercioResponse>> GetByUsuarioId(int usuarioId, int usuarioIdLogado)
        {
            if (usuarioId != usuarioIdLogado)
                throw new UnauthorizedException("Acesso negado.");

            var comercios = await _comercioRepository.GetByUsuarioId(usuarioId);
            return comercios.Adapt<IEnumerable<ComercioResponse>>();
        }

        public async Task<ComercioResponse> CriarAsync(ComercioRequest request, int usuarioIdLogado)
        {
            await _autorizacaoGlobalService.GarantirAdministradorGlobalAsync(usuarioIdLogado);

            var comercio = request.Adapt<Comercio>();
            var assinatura = new AssinaturaPlano
            {
                Comercio = comercio,
                Plano = PlanoAssinatura.Mensal,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddMonths(1),
            };
           var primeiroPagamento = new PagamentoHistoricoAssinaturaPlano
            {
                Assinatura = assinatura,
                Valor = 0,
                DataPagamento = DateTime.UtcNow,
            };
            await _pagamentoHistoricoAssinaturaPlanoRepository.AddAsync(primeiroPagamento);
            await _assinaturaPlanoRepository.AddAsync(assinatura);
            await _comercioRepository.AddAsync(comercio);
            await _unitOfWork.SaveChangesAsync();

            await _logAuditoriaService.RegistrarAsync(usuarioIdLogado, comercio.ComercioID, "ComercioCriado", "Comercio", comercio.ComercioID);
            await _unitOfWork.SaveChangesAsync();

            return comercio.Adapt<ComercioResponse>();
        }

        public async Task<ComercioResponse> AtualizarAsync(int id, int usuarioLogadoId, ComercioRequest request)
        {
            var comercio = await _comercioRepository.GetByIdAsync(id);
            if (comercio == null) throw new NotFoundException("Comércio não encontrado.");

            var vinculoLogado = await _usuarioComercioRepository.GetVinculoAsync(usuarioLogadoId, comercio.ComercioID);
            var chamadorEhAdmin = await _autorizacaoCargoService.EhAdminGlobalAsync(usuarioLogadoId);
            var chamadorEhProprietario = vinculoLogado?.Cargo.EhProprietario ?? false;
            var permissoesChamador = vinculoLogado?.Cargo.CargoPermissoes.Select(cp => cp.Permissao.Chave) ?? [];
            _autorizacaoCargoService.GarantirPermissao(chamadorEhAdmin, chamadorEhProprietario, permissoesChamador,
                PermissaoConstantes.GerenciarComercio, "Acesso negado.");

            request.Adapt(comercio);
            comercio.AtualizadoEm = DateTime.UtcNow;

            await _comercioRepository.UpdateAsync(comercio);
            await _unitOfWork.SaveChangesAsync();

            return comercio.Adapt<ComercioResponse>();
        }

        public async Task DesativarAsync(int id, int usuarioIdLogado)
        {
            var comercio = await _comercioRepository.GetByIdAsync(id);
            if (comercio == null) throw new NotFoundException("Comércio não encontrado.");

            var vinculoLogado = await _usuarioComercioRepository.GetVinculoAsync(usuarioIdLogado, comercio.ComercioID);
            var chamadorEhAdmin = await _autorizacaoCargoService.EhAdminGlobalAsync(usuarioIdLogado);
            var chamadorEhProprietario = vinculoLogado?.Cargo.EhProprietario ?? false;
            _autorizacaoCargoService.GarantirProprietario(chamadorEhAdmin, chamadorEhProprietario, "Você não tem permissão pra excluir comercios");

            comercio.Ativo = false;
            comercio.AtualizadoEm = DateTime.UtcNow;

            await _comercioRepository.UpdateAsync(comercio);
            await _logAuditoriaService.RegistrarAsync(usuarioIdLogado, comercio.ComercioID, "ComercioDesativado", "Comercio", comercio.ComercioID);
            await _unitOfWork.SaveChangesAsync();
        }

    }
}