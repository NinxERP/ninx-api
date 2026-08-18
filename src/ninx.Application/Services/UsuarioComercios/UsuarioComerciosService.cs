using Mapster;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class UsuarioComercioService : IUsuarioComercioService
    {
        private readonly IUsuarioComercioRepository _usuarioComercioRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ILogAuditoriaService _logAuditoriaService;
        private readonly IUnitOfWork _unitOfWork;

        public UsuarioComercioService(
            IUsuarioComercioRepository usuarioComercioRepository,
            IUsuarioRepository usuarioRepository, ILogAuditoriaService logAuditoriaService, IUnitOfWork unitOfWork)
        {
            _usuarioComercioRepository = usuarioComercioRepository;
            _usuarioRepository = usuarioRepository;
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
            if (vinculoChamador == null || (vinculoChamador.Permissao != Permissao.Administrador && vinculoChamador.Permissao != Permissao.Dono))
                throw new ForbiddenException("Você não tem permissão para vincular usuários a este comércio.");

            if (!Enum.IsDefined(typeof(Permissao), request.Permissao))
                throw new BadRequestException("A permissão informada é inválida.");

            var permissaoConvite = (Permissao)request.Permissao;
            if (vinculoChamador.Permissao == Permissao.Dono)
                permissaoConvite = Permissao.Funcionario;

            var existe = await _usuarioComercioRepository.ExisteVinculoAsync(request.UsuarioID, request.ComercioID);
            if (existe)
            {
                throw new BadRequestException("Usuário já vinculado a esse comércio.");
            }
            var usuario = await _usuarioRepository.GetByIdAsync(request.UsuarioID);
            if(usuario == null)
            {
                throw new NotFoundException("Usuário não encontrado.");
            }
            var usuarioComercio = new UsuarioComercio
            {
                UsuarioID = request.UsuarioID,
                ComercioID = request.ComercioID,
                Permissao = permissaoConvite,
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
            if (vinculoChamador == null || vinculoChamador.Permissao == Permissao.Funcionario)
                throw new ForbiddenException("Funcionários não possuem permissão para atualizar vínculos.");

            var usuarioComercio = await _usuarioComercioRepository.GetVinculoAsync(request.UsuarioID, request.ComercioID);
            if (usuarioComercio == null) throw new NotFoundException("Vínculo entre usuário e comércio não encontrado.");

            if (request.Permissao != 0 && usuarioComercio.Permissao != (Permissao)request.Permissao)
            {
                if (!Enum.IsDefined(typeof(Permissao), request.Permissao)) throw new BadRequestException($"A permissão não foi informada ou está invalida");
                if (vinculoChamador.Permissao != Permissao.Administrador) throw new ForbiddenException("Apenas administradores podem alterar o nível de permissão.");
                usuarioComercio.Permissao = (Permissao)request.Permissao;
                await _logAuditoriaService.RegistrarAsync(usuarioLogadoId, request.ComercioID, "UsuarioComercioPermissaoAlterada", "UsuarioComercio", request.UsuarioID, $"NovaPermissao={usuarioComercio.Permissao}");
            }

            if (request.Ativo.HasValue)
            {
                GarantirDonoSoGerenciaFuncionario(vinculoChamador.Permissao, usuarioComercio.Permissao);
                usuarioComercio.Ativo = request.Ativo.Value;
            }

            await _usuarioComercioRepository.UpdateAsync(usuarioComercio);
            await _unitOfWork.SaveChangesAsync();

            return usuarioComercio.Adapt<UsuarioComercioResponse>();
        }

        public async Task DesativarAsync(int usuarioId, int comercioId, int usuarioLogadoId)
        {
            var vinculoChamador = await _usuarioComercioRepository.GetVinculoAsync(usuarioLogadoId, comercioId);
            if (vinculoChamador == null || (vinculoChamador.Permissao != Permissao.Administrador && vinculoChamador.Permissao != Permissao.Dono))
                throw new ForbiddenException("Você não tem permissão para desativar vínculos deste comércio.");

            var usuarioComercioFiltrado = await _usuarioComercioRepository.GetVinculoAsync(usuarioId, comercioId);
            if (usuarioComercioFiltrado == null)
            {
                throw new NotFoundException("Usuário não possui vínculo com o comércio.");
            }

            GarantirDonoSoGerenciaFuncionario(vinculoChamador.Permissao, usuarioComercioFiltrado.Permissao);

            usuarioComercioFiltrado.Ativo = false;
            await _usuarioComercioRepository.UpdateAsync(usuarioComercioFiltrado);
            await DesativarUsuarioSeSemVinculoAtivoAsync(usuarioId);
            await _logAuditoriaService.RegistrarAsync(usuarioLogadoId, comercioId, "UsuarioComercioDesativado", "UsuarioComercio", usuarioId);
            await _unitOfWork.SaveChangesAsync();
        }

        private static void GarantirDonoSoGerenciaFuncionario(Permissao permissaoChamador, Permissao permissaoAlvo)
        {
            if (permissaoChamador == Permissao.Dono && permissaoAlvo != Permissao.Funcionario)
                throw new ForbiddenException("Donos só podem gerenciar vínculos de funcionários.");
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
