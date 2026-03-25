using Mapster;
using ninx.Communication.Request;
using ninx.Communication.Response;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces.Repositories;
using ninx.Domain.Interfaces.Services;

namespace ninx.Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<UsuarioResponse?> GetByIdAsync(int id)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario is null)
            {
                throw new NotFoundException("Usuario não encontrado");
            }
            return usuario.Adapt<UsuarioResponse>();
        }

        public async Task<UsuarioResponse> CriarAsync(CriarUsuarioRequest request)
        {
            var usuario = request.Adapt<Usuario>();
            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

            await _usuarioRepository.AddAsync(usuario);
            return usuario.Adapt<UsuarioResponse>();
        }

        public async Task<UsuarioResponse> AtualizarAsync(int id, AtualizarUsuarioRequest request)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario is null)
            {
                throw new NotFoundException("Usuario não encontrado");
            }

            request.Adapt(usuario);

            await _usuarioRepository.UpdateAsync(usuario);
            return usuario.Adapt<UsuarioResponse>();
        }

        public async Task DesativarAsync(int id)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Usuário não encontrado.");

            usuario.Ativo = false;
            usuario.AtualizadoEm = DateTime.Now;
            await _usuarioRepository.UpdateAsync(usuario);
        }
    }
}