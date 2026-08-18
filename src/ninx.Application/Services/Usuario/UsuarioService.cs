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
        private readonly IUsuarioComercioService _usuarioComercioService;
        private readonly IAutorizacaoGlobalService _autorizacaoGlobalService;
        private readonly ILogAuditoriaService _logAuditoriaService;
        private readonly IUnitOfWork _unitOfWork;
        public UsuarioService(IUsuarioRepository usuarioRepository, IUsuarioComercioRepository usuarioComercioRepository, IUsuarioComercioService usuarioComercioService, IAutorizacaoGlobalService autorizacaoGlobalService, ILogAuditoriaService logAuditoriaService, IUnitOfWork unitOfWork)
        {
            _usuarioRepository = usuarioRepository;
            _usuarioComercioRepository = usuarioComercioRepository;
            _usuarioComercioService = usuarioComercioService;
            _autorizacaoGlobalService = autorizacaoGlobalService;
            _logAuditoriaService = logAuditoriaService;
            _unitOfWork = unitOfWork;
        }

        public async Task<UsuarioResponse> GetById(int id, int usuarioIdLogado)
        {
            await _autorizacaoGlobalService.GarantirAdministradorGlobalAsync(usuarioIdLogado);

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario is null) throw new NotFoundException("Usuário não encontrado");

            return usuario.Adapt<UsuarioResponse>();
        }

        public async Task<PaginatedResponse<UsuarioResponse>> GetAll(int usuarioIdLogado, PaginationRequest request)
        {
            await _autorizacaoGlobalService.GarantirAdministradorGlobalAsync(usuarioIdLogado);

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
            var listaResponse = pagina.Select(u =>
            {
                var response = u.Adapt<UsuarioResponse>();
                response.Permissao = u.UsuarioComercios.First(uc => uc.ComercioID == comercioId).Permissao.ToString();
                return response;
            }).ToList();

            return new PaginatedResponse<UsuarioResponse>(
                listaResponse,
                request.PageNumber,
                request.PageSize,
                total
            );
        }

        public async Task<UsuarioResponse> BuscarPorEmailAsync(string email, Permissao permissaoLogado)
        {
            if (permissaoLogado == Permissao.Funcionario)
                throw new ForbiddenException("Funcionários não podem buscar usuários.");

            var usuario = await _usuarioRepository.GetUsuarioByEmail(email);
            if (usuario is null) throw new NotFoundException("Nenhum usuário encontrado com esse e-mail.");

            return usuario.Adapt<UsuarioResponse>();
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

            var response = usuario.Adapt<UsuarioResponse>();
            response.Permissao = permissaoNoComercio.ToString();
            return response;
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

        public async Task<UsuarioResponse> AtualizarAsync(int id, AtualizarUsuarioRequest request, int comercioId, int usuarioIdLogado, Permissao permissaoLogado)
        {
            if (permissaoLogado == Permissao.Funcionario)
                throw new ForbiddenException("Funcionários não podem atualizar usuários.");

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario is null) throw new NotFoundException("Usuario não encontrado");

            var vinculo = await _usuarioComercioRepository.GetVinculoAsync(id, comercioId);
            if (vinculo is null) throw new UnauthorizedException("Usuário não pertence ao seu comercio");

            if (permissaoLogado == Permissao.Dono && vinculo.Permissao != Permissao.Funcionario)
                throw new ForbiddenException("Você só pode atualizar usuários com permissão de funcionário.");

            await GarantirEmailDisponivelAsync(request.Email, id);

            request.Adapt(usuario);
            usuario.AtualizadoEm = DateTime.UtcNow;
            await _usuarioRepository.UpdateAsync(usuario);
            await _logAuditoriaService.RegistrarAsync(usuarioIdLogado, comercioId, "UsuarioIdentidadeAtualizada", "Usuario", id);
            await _unitOfWork.SaveChangesAsync();
            return usuario.Adapt<UsuarioResponse>();
        }

        public async Task DesativarAsync(int id, int comercioId, int usuarioIdLogado)
        {
            await _usuarioComercioService.DesativarAsync(id, comercioId, usuarioIdLogado);
        }

        public async Task DesativarGlobalAsync(int id, int usuarioIdLogado)
        {
            await _autorizacaoGlobalService.GarantirAdministradorGlobalAsync(usuarioIdLogado);

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario is null) throw new NotFoundException("Usuário não encontrado.");

            usuario.Ativo = false;
            usuario.AtualizadoEm = DateTime.UtcNow;
            await _usuarioRepository.UpdateAsync(usuario);
            await _logAuditoriaService.RegistrarAsync(usuarioIdLogado, null, "UsuarioDesativadoGlobal", "Usuario", id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ResetarSenhaAsync(int id, int usuarioIdLogado, ResetarSenhaRequest request)
        {
            await _autorizacaoGlobalService.GarantirAdministradorGlobalAsync(usuarioIdLogado);

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario is null) throw new NotFoundException("Usuário não encontrado.");

            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);
            usuario.AtualizadoEm = DateTime.UtcNow;
            await _usuarioRepository.UpdateAsync(usuario);
            await _logAuditoriaService.RegistrarAsync(usuarioIdLogado, null, "UsuarioSenhaResetada", "Usuario", id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<UsuarioResponse> AtualizarAdminAsync(int id, int usuarioIdLogado, AtualizarAdminRequest request)
        {
            await _autorizacaoGlobalService.GarantirAdministradorGlobalAsync(usuarioIdLogado);

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario is null) throw new NotFoundException("Usuário não encontrado.");

            if (usuario.Admin && !request.Admin)
            {
                var todosUsuarios = await _usuarioRepository.GetAllAsync();
                var restamOutrosAdmins = todosUsuarios.Any(u => u.Admin && u.UsuarioID != id);
                if (!restamOutrosAdmins)
                    throw new BadRequestException("Não é possível remover o último administrador de plataforma.");
            }

            usuario.Admin = request.Admin;
            usuario.AtualizadoEm = DateTime.UtcNow;
            await _usuarioRepository.UpdateAsync(usuario);
            await _logAuditoriaService.RegistrarAsync(usuarioIdLogado, null, request.Admin ? "UsuarioPromovidoAdmin" : "UsuarioRebaixadoAdmin", "Usuario", id);
            await _unitOfWork.SaveChangesAsync();
            return usuario.Adapt<UsuarioResponse>();
        }

        private async Task GarantirEmailDisponivelAsync(string email, int usuarioId)
        {
            var existente = await _usuarioRepository.GetUsuarioByEmail(email);
            if (existente != null && existente.UsuarioID != usuarioId)
                throw new BadRequestException("E-mail já cadastrado.");
        }
    }
}