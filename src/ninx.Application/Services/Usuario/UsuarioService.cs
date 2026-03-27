using Mapster;
using ninx.Communication.Request;
using ninx.Communication.Response;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces.Repositories;
using ninx.Domain.Interfaces.Services;

namespace ninx.Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IUsuarioComercioRepository _usuarioComercioRepository;
        private readonly IUnitOfWork _unitOfWork;
        public UsuarioService(IUsuarioRepository usuarioRepository, IUsuarioComercioRepository usuarioComercioRepository, IUnitOfWork unitOfWork)
        {
            _usuarioRepository = usuarioRepository;
            _usuarioComercioRepository = usuarioComercioRepository;
            _unitOfWork = unitOfWork;
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

        public async Task<UsuarioResponse> CriarComVinculoAsync(
            CriarUsuarioRequest request,
            int executorId,
            string executorRole,
            int? executorComercioId)
        {
            if (executorRole == "Funcionario")
            {
                throw new ForbiddenException("Funcionários não podem cadastrar novos usuários.");
            }

            if (executorRole == "Dono")
            {
                request.ComercioId = executorComercioId ?? throw new BadRequestException("Contexto de comércio inválido.");

                request.Permissao = 3;
            }

            var existente = await _usuarioRepository.GetUsuarioByEmail(request.Email);
            if (existente != null)
            {
                throw new BadRequestException("E-mail já cadastrado.");
            }
                
            var novoUsuario = request.Adapt<Usuario>();
            novoUsuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

            await _usuarioRepository.AddAsync(novoUsuario);

            var vinculo = new UsuarioComercio
            {
                Usuario = novoUsuario,
                ComercioID = request.ComercioId,
                Permissao = (Permissao)request.Permissao
            };

            await _usuarioComercioRepository.AddAsync(vinculo);

            await _unitOfWork.SaveChangesAsync();

            return novoUsuario.Adapt<UsuarioResponse>();
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