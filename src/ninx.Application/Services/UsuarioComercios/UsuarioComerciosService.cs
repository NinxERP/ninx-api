using Mapster;
using ninx.Communication.Request;
using ninx.Communication.Response;
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
        private readonly IUnitOfWork _unitOfWork;

        public UsuarioComercioService(
            IUsuarioComercioRepository usuarioComercioRepository,
            IUsuarioRepository usuarioRepository, IUnitOfWork unitOfWork)
        {
            _usuarioComercioRepository = usuarioComercioRepository;
            _usuarioRepository = usuarioRepository;
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

        public async Task<UsuarioComercioResponse> CriarAsync(CriarUsuarioComercioRequest request)
        {
            var existe = await _usuarioComercioRepository.GetByUsuarioIdAsync(request.UsuarioID);
            var existeFiltrado = existe.FirstOrDefault(uc => uc.ComercioID == request.ComercioID);
            if (existeFiltrado != null)
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
                Permissao = usuario.Permissao,
                Ativo = true
            };
            await _usuarioComercioRepository.AddAsync(usuarioComercio);
            await _unitOfWork.SaveChangesAsync();
            return usuarioComercio.Adapt<UsuarioComercioResponse>();
        }

        public async Task<UsuarioComercioResponse> AtualizarAsync(AtualizarUsuarioComercioRequest request, Permissao usuarioLogadoPermissao)
        {
            if (usuarioLogadoPermissao == Permissao.Funcionario) throw new ForbiddenException("Funcionários não possuem permissão para atualizar vínculos.");

            var comerciosPorUsuario = await _usuarioComercioRepository.GetByUsuarioIdAsync(request.UsuarioID);
            var usuarioComercio = comerciosPorUsuario.FirstOrDefault(x => x.ComercioID == request.ComercioID);
            if (usuarioComercio == null) throw new NotFoundException("Vínculo entre usuário e comércio não encontrado.");

            if (request.Permissao != 0 && usuarioComercio.Permissao != (Permissao)request.Permissao)
            {
                if (!Enum.IsDefined(typeof(Permissao), request.Permissao)) throw new BadRequestException($"A permissão não foi informada ou está invalida");
                if (usuarioLogadoPermissao != Permissao.Administrador) throw new ForbiddenException("Apenas administradores podem alterar o nível de permissão.");
                usuarioComercio.Permissao = (Permissao)request.Permissao;
            }

            if (request.Ativo.HasValue) usuarioComercio.Ativo = request.Ativo.Value;

            await _usuarioComercioRepository.UpdateAsync(usuarioComercio);
            await _unitOfWork.SaveChangesAsync();

            return usuarioComercio.Adapt<UsuarioComercioResponse>();
        }

        public async Task DesativarAsync(int usuarioId, int comercioId)
        {
            var usuarioComercio = await _usuarioComercioRepository.GetByUsuarioIdAsync(usuarioId);
            var usuarioComercioFiltrado = usuarioComercio.FirstOrDefault(uc => uc.ComercioID == comercioId);
            if (usuarioComercioFiltrado == null)
            {
                throw new NotFoundException("Usuário não possui vínculo com o comércio.");
            }

            usuarioComercioFiltrado.Ativo = false;
            await _usuarioComercioRepository.UpdateAsync(usuarioComercioFiltrado);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
