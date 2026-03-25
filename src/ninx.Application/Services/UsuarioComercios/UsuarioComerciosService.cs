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

        public UsuarioComercioService(
            IUsuarioComercioRepository usuarioComercioRepository,
            IUsuarioRepository usuarioRepository)
        {
            _usuarioComercioRepository = usuarioComercioRepository;
            _usuarioRepository = usuarioRepository;
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
            var existe = await _usuarioComercioRepository.GetByUsuarioIdAndComercioIdAsync(request.UsuarioID, request.ComercioID);
            if (existe != null)
                throw new BadRequestException("Usuário já vinculado a esse comércio.");

            var usuario = await _usuarioRepository.GetByIdAsync(request.UsuarioID)
                ?? throw new NotFoundException("Usuário não encontrado.");

            var usuarioComercio = new UsuarioComercio
            {
                UsuarioID = request.UsuarioID,
                ComercioID = request.ComercioID,
                Permissao = usuario.Permissao,
                Ativo = true
            };

            await _usuarioComercioRepository.AddAsync(usuarioComercio);
            return usuarioComercio.Adapt<UsuarioComercioResponse>();
        }

        public async Task DesativarAsync(int usuarioId, int comercioId)
        {
            var usuarioComercio = await _usuarioComercioRepository.GetByUsuarioIdAndComercioIdAsync(usuarioId, comercioId)
                ?? throw new NotFoundException("Vínculo não encontrado.");

            usuarioComercio.Ativo = false;
            await _usuarioComercioRepository.UpdateAsync(usuarioComercio);
        }
    }
}