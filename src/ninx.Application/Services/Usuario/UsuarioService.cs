using Mapster;
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

        public async Task<PaginatedResponse<UsuarioResponse>> GetAllByComercioId(int comercioId, Permissao permissaoLogado, PaginationRequest request)
        {
            if (permissaoLogado == Permissao.Funcionario)
                throw new ForbiddenException("Funcionários não podem consultar usuários.");

            var usuarios = (await _usuarioRepository.GetAllByComercioIdAsync(comercioId)).ToList();

            if (permissaoLogado == Permissao.Dono)
                usuarios = usuarios
                    .Where(u => u.UsuarioComercios.Any(uc => uc.ComercioID == comercioId && uc.Permissao == Permissao.Funcionario))
                    .ToList();

            if (usuarios is null || !usuarios.Any())
                throw new NotFoundException("Nenhum usuário foi encontrado");

            var total = usuarios.Count;
            var pagina = usuarios
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            var listaResponse = pagina.Adapt<List<UsuarioResponse>>();

            return new PaginatedResponse<UsuarioResponse>(
                listaResponse,
                request.PageNumber,
                request.PageSize,
                total
            );
        }

        public async Task<UsuarioResponse> GetByIdAndComercioIdAsync(int id, int comercioid, Permissao permissaoLogado)
        {
            if (permissaoLogado == Permissao.Funcionario)
                throw new ForbiddenException("Funcionários não podem consultar usuários.");

            var usuario = await _usuarioRepository.GetByIdAndComercioIdAsync(id, comercioid);
            if (usuario is null) throw new NotFoundException("Usuário não encontrado");

            var permissaoNoComercio = usuario.UsuarioComercios.First(uc => uc.ComercioID == comercioid).Permissao;
            if (permissaoLogado == Permissao.Dono && permissaoNoComercio != Permissao.Funcionario)
                throw new ForbiddenException("Você só pode consultar usuários com permissão de funcionário.");

            return usuario.Adapt<UsuarioResponse>();
        }

        public async Task<UsuarioResponse> CriarAsync(
            CriarUsuarioRequest request,
            int executorId,
            Permissao permissao,
            int? comercioIdLogado)
        {

            if (request.ComercioId != comercioIdLogado) throw new BadRequestException("Contexto de comércio inválido.");
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

        public async Task<UsuarioResponse> AtualizarAsync(int id, AtualizarUsuarioRequest request, int comercioId, Permissao permissaoLogado)
        {
            if (permissaoLogado == Permissao.Funcionario)
                throw new ForbiddenException("Funcionários não podem atualizar usuários.");

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario is null) throw new NotFoundException("Usuario não encontrado");

            var vinculo = await _usuarioComercioRepository.GetVinculoAsync(id, comercioId);
            if (vinculo is null) throw new UnauthorizedException("Usuário não pertence ao seu comercio");

            if (permissaoLogado == Permissao.Dono && vinculo.Permissao != Permissao.Funcionario)
                throw new ForbiddenException("Você só pode atualizar usuários com permissão de funcionário.");

            request.Adapt(usuario);
            await _usuarioRepository.UpdateAsync(usuario);
            await _unitOfWork.SaveChangesAsync();
            return usuario.Adapt<UsuarioResponse>();
        }

        public async Task DesativarAsync(int id, int comercioId, Permissao permissaoLogado)
        {
            if (permissaoLogado == Permissao.Funcionario)
                throw new ForbiddenException("Funcionários não podem desativar usuários.");

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null) throw new NotFoundException("Usuário não encontrado.");

            var vinculo = await _usuarioComercioRepository.GetVinculoAsync(id, comercioId);
            if (vinculo is null) throw new UnauthorizedException("Usuário não pertence ao seu comercio");

            if (permissaoLogado == Permissao.Dono && vinculo.Permissao != Permissao.Funcionario)
                throw new ForbiddenException("Você só pode desativar usuários com permissão de funcionário.");

            usuario.Ativo = false;
            usuario.AtualizadoEm = DateTime.UtcNow;
            await _usuarioRepository.UpdateAsync(usuario);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}