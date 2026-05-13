using Mapster;
using ninx.Communication;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

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

        public async Task<UsuarioResponse> GetById(int id, int usuarioIdLogado)
        {
            var usuarioLogado = await _usuarioRepository.GetByIdAsync(usuarioIdLogado);
            if (usuarioLogado.Permissao != Permissao.Administrador) throw new UnauthorizedException("Você não possui permissão para utilizar esse endpoint");

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario is null) throw new NotFoundException("Usuário não encontrado");

            return usuario.Adapt<UsuarioResponse>();
        }

        public async Task<PaginatedResponse<UsuarioResponse>> GetAll(int usuarioIdLogado, PaginationRequest request)
        {
            var usuarioLogado = await _usuarioRepository.GetByIdAsync(usuarioIdLogado);
            if (usuarioLogado.Permissao != Permissao.Administrador) throw new UnauthorizedException("Você não possui permissão para utilizar esse endpoint");

            var usuarios = await _usuarioRepository.GetAllAsync();
            if (usuarios is null || !usuarios.Any()) throw new NotFoundException("Nenhum usuário foi encontrado");

            var (entidades, total) = await _usuarioRepository.GetPaginatedAsync(request.PageNumber, request.PageSize);
            var listaResponse = entidades.Adapt<List<UsuarioResponse>>();

            return new PaginatedResponse<UsuarioResponse>(
                listaResponse,
                request.PageNumber,
                request.PageSize,
                total
            );
        }

        public async Task<PaginatedResponse<UsuarioResponse>> GetAllByComercioId(int comercioId, PaginationRequest request)
        {
            var usuarios = await _usuarioRepository.GetAllByComercioIdAsync(comercioId);
            if (usuarios is null || !usuarios.Any()) 
                throw new NotFoundException("Nenhum usuário foi encontrado");

            var (entidades, total) = await _usuarioRepository.GetPaginatedAsync(request.PageNumber, request.PageSize);
            var listaResponse = entidades.Adapt<List<UsuarioResponse>>();

            return new PaginatedResponse<UsuarioResponse>(
                listaResponse,
                request.PageNumber,
                request.PageSize,
                total
            );
        }

        public async Task<UsuarioResponse> GetByIdAndComercioIdAsync(int id, int comercioid)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario is null) throw new NotFoundException("Usuário não encontrado");

            return usuario.Adapt<UsuarioResponse>();
        }

        public async Task<UsuarioResponse> CriarAsync(
            CriarUsuarioRequest request,
            int executorId,
            Permissao permissao,
            int? comercioIdLogado)
        {

            if (request.ComercioId == comercioIdLogado) throw new BadRequestException("Contexto de comércio inválido.");
            if (permissao == Permissao.Funcionario) throw new ForbiddenException("Funcionários não podem cadastrar novos usuários.");
            if (permissao == Permissao.Dono) request.Permissao = (int)Permissao.Funcionario;

            var existente = await _usuarioRepository.GetUsuarioByEmail(request.Email);
            if (existente != null) throw new BadRequestException("E-mail já cadastrado.");
                
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

        public async Task<UsuarioResponse> AtualizarAsync(int id, AtualizarUsuarioRequest request, int comercioId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario is null) throw new NotFoundException("Usuario não encontrado");
            var usuarioComercio = await _usuarioComercioRepository.GetByUsuarioIdAsync(id);
            if (usuarioComercio.Any(x => x.ComercioID == comercioId)) throw new UnauthorizedException("Usuário não pertence ao seu comercio");

            request.Adapt(usuario);
            await _usuarioRepository.UpdateAsync(usuario);
            await _unitOfWork.SaveChangesAsync();
            return usuario.Adapt<UsuarioResponse>();
        }

        public async Task DesativarAsync(int id, int comercioId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null) throw new NotFoundException("Usuário não encontrado.");
            var usuarioComercio = await _usuarioComercioRepository.GetByUsuarioIdAsync(id);
            if (usuarioComercio.Any(x => x.ComercioID == comercioId)) throw new UnauthorizedException("Usuário não pertence ao seu comercio");

            
            usuario.Ativo = false;
            usuario.AtualizadoEm = DateTime.UtcNow;
            await _usuarioRepository.UpdateAsync(usuario);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}