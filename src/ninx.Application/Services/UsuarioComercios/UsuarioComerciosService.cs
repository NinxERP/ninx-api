using Mapster;
using ninx.Communication;
using ninx.Domain.Constants;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class UsuarioComercioService : IUsuarioComercioService
    {
        private readonly IUsuarioComercioRepository _usuarioComercioRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ICargoRepository _cargoRepository;
        private readonly IAutorizacaoCargoService _autorizacaoCargoService;
        private readonly ILogAuditoriaService _logAuditoriaService;
        private readonly IUnitOfWork _unitOfWork;

        public UsuarioComercioService(
            IUsuarioComercioRepository usuarioComercioRepository,
            IUsuarioRepository usuarioRepository,
            ICargoRepository cargoRepository,
            IAutorizacaoCargoService autorizacaoCargoService,
            ILogAuditoriaService logAuditoriaService,
            IUnitOfWork unitOfWork)
        {
            _usuarioComercioRepository = usuarioComercioRepository;
            _usuarioRepository = usuarioRepository;
            _cargoRepository = cargoRepository;
            _autorizacaoCargoService = autorizacaoCargoService;
            _logAuditoriaService = logAuditoriaService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UsuarioComercioResponse>> GetByUsuarioIdAsync(int usuarioId)
        {
            var result = await _usuarioComercioRepository.GetByUsuarioIdAsync(usuarioId);
            return result.Adapt<IEnumerable<UsuarioComercioResponse>>();
        }

        public async Task<IEnumerable<UsuarioComercioResponse>> GetByComercioIdAsync(int comercioId)
        {
            var result = await _usuarioComercioRepository.GetByComercioIdAsync(comercioId);
            return result.Adapt<IEnumerable<UsuarioComercioResponse>>();
        }

        public async Task<UsuarioComercioResponse> CriarAsync(CriarUsuarioComercioRequest request, int usuarioLogadoId)
        {
            var vinculoChamador = await _usuarioComercioRepository.GetVinculoAsync(usuarioLogadoId, request.ComercioID);
            var chamadorEhAdmin = await _autorizacaoCargoService.EhAdminGlobalAsync(usuarioLogadoId);

            if (!chamadorEhAdmin && (vinculoChamador == null || vinculoChamador.Cargo.Peso < CargoConstantes.PesoDono))
                throw new ForbiddenException("Você não tem permissão para vincular usuários a este comércio.");

            var cargo = await _cargoRepository.GetByIdAsync(request.CargoID);
            if (cargo == null || !cargo.Ativo || (cargo.ComercioID != null && cargo.ComercioID != request.ComercioID))
                throw new BadRequestException("O cargo informado é inválido para este comércio.");

            if (!chamadorEhAdmin && cargo.Peso >= vinculoChamador!.Cargo.Peso)
                throw new ForbiddenException("Você só pode atribuir cargos com peso menor que o seu.");

            var existe = await _usuarioComercioRepository.ExisteVinculoAsync(request.UsuarioID, request.ComercioID);
            if (existe)
            {
                throw new BadRequestException("Usuário já vinculado a esse comércio.");
            }
            var usuario = await _usuarioRepository.GetByIdAsync(request.UsuarioID);
            if (usuario == null)
            {
                throw new NotFoundException("Usuário não encontrado.");
            }
            var usuarioComercio = new UsuarioComercio
            {
                UsuarioID = request.UsuarioID,
                ComercioID = request.ComercioID,
                CargoID = cargo.CargoID,
                Cargo = cargo,
                Ativo = true
            };
            await _usuarioComercioRepository.AddAsync(usuarioComercio);
            await _logAuditoriaService.RegistrarAsync(usuarioLogadoId, request.ComercioID, "UsuarioVinculado", "UsuarioComercio", request.UsuarioID);
            await _unitOfWork.SaveChangesAsync();
            return usuarioComercio.Adapt<UsuarioComercioResponse>();
        }

        public async Task<UsuarioComercioResponse> AtualizarAsync(AtualizarUsuarioComercioRequest request, int usuarioLogadoId)
        {
            var vinculoChamador = await _usuarioComercioRepository.GetVinculoAsync(usuarioLogadoId, request.ComercioID);
            var chamadorEhAdmin = await _autorizacaoCargoService.EhAdminGlobalAsync(usuarioLogadoId);

            if (!chamadorEhAdmin && (vinculoChamador == null || vinculoChamador.Cargo.Peso < CargoConstantes.PesoDono))
                throw new ForbiddenException("Você não tem permissão para atualizar vínculos deste comércio.");

            var usuarioComercio = await _usuarioComercioRepository.GetVinculoAsync(request.UsuarioID, request.ComercioID);
            if (usuarioComercio == null) throw new NotFoundException("Vínculo entre usuário e comércio não encontrado.");

            if (request.CargoID != 0 && usuarioComercio.CargoID != request.CargoID)
            {
                if (!chamadorEhAdmin)
                    throw new ForbiddenException("Apenas administradores da plataforma podem alterar o cargo de um vínculo.");

                var novoCargo = await _cargoRepository.GetByIdAsync(request.CargoID);
                if (novoCargo == null || !novoCargo.Ativo || (novoCargo.ComercioID != null && novoCargo.ComercioID != request.ComercioID))
                    throw new BadRequestException("O cargo informado é inválido para este comércio.");

                usuarioComercio.CargoID = novoCargo.CargoID;
                usuarioComercio.Cargo = novoCargo;
                await _logAuditoriaService.RegistrarAsync(usuarioLogadoId, request.ComercioID, "UsuarioComercioCargoAlterado", "UsuarioComercio", request.UsuarioID, $"NovoCargo={novoCargo.Nome}");
            }

            if (request.Ativo.HasValue)
            {
                _autorizacaoCargoService.GarantirGerencia(chamadorEhAdmin, vinculoChamador?.Cargo.Peso ?? 0, usuarioComercio.Cargo.Peso);
                usuarioComercio.Ativo = request.Ativo.Value;
            }

            await _usuarioComercioRepository.UpdateAsync(usuarioComercio);
            await _unitOfWork.SaveChangesAsync();

            return usuarioComercio.Adapt<UsuarioComercioResponse>();
        }

        public async Task DesativarAsync(int usuarioId, int comercioId, int usuarioLogadoId)
        {
            var vinculoChamador = await _usuarioComercioRepository.GetVinculoAsync(usuarioLogadoId, comercioId);
            var chamadorEhAdmin = await _autorizacaoCargoService.EhAdminGlobalAsync(usuarioLogadoId);

            if (!chamadorEhAdmin && (vinculoChamador == null || vinculoChamador.Cargo.Peso < CargoConstantes.PesoDono))
                throw new ForbiddenException("Você não tem permissão para desativar vínculos deste comércio.");

            var usuarioComercioFiltrado = await _usuarioComercioRepository.GetVinculoAsync(usuarioId, comercioId);
            if (usuarioComercioFiltrado == null)
            {
                throw new NotFoundException("Usuário não possui vínculo com o comércio.");
            }

            _autorizacaoCargoService.GarantirGerencia(chamadorEhAdmin, vinculoChamador?.Cargo.Peso ?? 0, usuarioComercioFiltrado.Cargo.Peso);

            usuarioComercioFiltrado.Ativo = false;
            await _usuarioComercioRepository.UpdateAsync(usuarioComercioFiltrado);
            await DesativarUsuarioSeSemVinculoAtivoAsync(usuarioId);
            await _logAuditoriaService.RegistrarAsync(usuarioLogadoId, comercioId, "UsuarioComercioDesativado", "UsuarioComercio", usuarioId);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task DesativarUsuarioSeSemVinculoAtivoAsync(int usuarioId)
        {
            var vinculos = await _usuarioComercioRepository.GetByUsuarioIdAsync(usuarioId);
            if (vinculos.Any(v => v.Ativo)) return;

            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario is null || !usuario.Ativo) return;

            usuario.Ativo = false;
            usuario.AtualizadoEm = DateTime.UtcNow;
            await _usuarioRepository.UpdateAsync(usuario);
        }
    }
}
