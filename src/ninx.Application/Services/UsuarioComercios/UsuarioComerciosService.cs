using Mapster;
using ninx.Application.Interfaces.Services;
using ninx.Communication.Request;
using ninx.Communication.Response;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces.Repositories;

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