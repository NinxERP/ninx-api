using Mapster;
using ninx.Communication;
using ninx.Domain.Constants;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IUsuarioComercioRepository _usuarioComercioRepository;
        private readonly IUsuarioComercioService _usuarioComercioService;
        private readonly ICargoRepository _cargoRepository;
        private readonly IAutorizacaoCargoService _autorizacaoCargoService;
        private readonly IAutorizacaoGlobalService _autorizacaoGlobalService;
        private readonly ILogAuditoriaService _logAuditoriaService;
        private readonly IUnitOfWork _unitOfWork;
        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            IUsuarioComercioRepository usuarioComercioRepository,
            IUsuarioComercioService usuarioComercioService,
            ICargoRepository cargoRepository,
            IAutorizacaoCargoService autorizacaoCargoService,
            IAutorizacaoGlobalService autorizacaoGlobalService,
            ILogAuditoriaService logAuditoriaService,
            IUnitOfWork unitOfWork)
        {
            _usuarioRepository = usuarioRepository;
            _usuarioComercioRepository = usuarioComercioRepository;
            _usuarioComercioService = usuarioComercioService;
            _cargoRepository = cargoRepository;
            _autorizacaoCargoService = autorizacaoCargoService;
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

        public async Task<PaginatedResponse<UsuarioListaResponse>> GetAll(int usuarioIdLogado, PaginationRequest request)
        {
            await _autorizacaoGlobalService.GarantirAdministradorGlobalAsync(usuarioIdLogado);

            var usuarios = await _usuarioRepository.GetAllComVinculosAsync();

            var vinculos = usuarios
                .SelectMany(u => u.UsuarioComercios.Select(uc => (Usuario: u, Vinculo: uc)))
                .OrderBy(x => x.Usuario.UsuarioID)
                .ThenBy(x => x.Vinculo.ComercioID)
                .ToList();

            if (vinculos.Count == 0) throw new NotFoundException("Nenhum usuário foi encontrado");

            var total = vinculos.Count;
            var pagina = vinculos
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            var listaResponse = pagina.Select(MapParaListaResponse).ToList();

            return new PaginatedResponse<UsuarioListaResponse>(
                listaResponse,
                request.PageNumber,
                request.PageSize,
                total
            );
        }

        public async Task<PaginatedResponse<UsuarioListaResponse>> GetAllByComercioId(int comercioId, int usuarioIdLogado, bool ehProprietarioLogado, IEnumerable<string> permissoesLogado, PaginationRequest request)
        {
            var chamadorEhAdmin = await _autorizacaoCargoService.EhAdminGlobalAsync(usuarioIdLogado);
            _autorizacaoCargoService.GarantirPermissao(chamadorEhAdmin, ehProprietarioLogado, permissoesLogado,
                PermissaoConstantes.GerenciarUsuarios, "Você não pode consultar usuários deste comércio.");

            var usuarios = (await _usuarioRepository.GetAllByComercioIdAsync(comercioId)).ToList();

            if (!chamadorEhAdmin)
                usuarios = usuarios
                    .Where(u => u.UsuarioComercios.Any(uc => uc.ComercioID == comercioId && !uc.Cargo.EhProprietario))
                    .ToList();

            var vinculos = usuarios
                .Select(u => (Usuario: u, Vinculo: u.UsuarioComercios.First(uc => uc.ComercioID == comercioId)))
                .OrderByDescending(x => x.Usuario.UsuarioID == usuarioIdLogado)
                .ThenByDescending(x => x.Vinculo.Cargo.EhProprietario)
                .ToList();

            if (vinculos.Count == 0)
                throw new NotFoundException("Nenhum usuário foi encontrado");

            var total = vinculos.Count;
            var pagina = vinculos
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            var listaResponse = pagina.Select(MapParaListaResponse).ToList();

            return new PaginatedResponse<UsuarioListaResponse>(
                listaResponse,
                request.PageNumber,
                request.PageSize,
                total
            );
        }

        private static UsuarioListaResponse MapParaListaResponse((Usuario Usuario, UsuarioComercio Vinculo) item)
        {
            var response = item.Usuario.Adapt<UsuarioListaResponse>();
            response.ComercioID = item.Vinculo.ComercioID;
            response.ComercioNome = item.Vinculo.Comercio.NomeComercio;
            response.CargoNome = item.Vinculo.Cargo.Nome;
            return response;
        }

        public async Task<UsuarioResponse> BuscarPorEmailAsync(string email, int usuarioIdLogado, bool ehProprietarioLogado, IEnumerable<string> permissoesLogado)
        {
            var chamadorEhAdmin = await _autorizacaoCargoService.EhAdminGlobalAsync(usuarioIdLogado);
            _autorizacaoCargoService.GarantirPermissao(chamadorEhAdmin, ehProprietarioLogado, permissoesLogado,
                PermissaoConstantes.GerenciarUsuarios, "Você não pode buscar usuários.");

            var usuario = await _usuarioRepository.GetUsuarioByEmail(email);
            if (usuario is null) throw new NotFoundException("Nenhum usuário encontrado com esse e-mail.");

            return usuario.Adapt<UsuarioResponse>();
        }

        public async Task<UsuarioResponse> GetByIdAndComercioIdAsync(int id, int comercioId, int usuarioIdLogado, bool ehProprietarioLogado, IEnumerable<string> permissoesLogado)
        {
            var chamadorEhAdmin = await _autorizacaoCargoService.EhAdminGlobalAsync(usuarioIdLogado);
            _autorizacaoCargoService.GarantirPermissao(chamadorEhAdmin, ehProprietarioLogado, permissoesLogado,
                PermissaoConstantes.GerenciarUsuarios, "Você não pode consultar usuários deste comércio.");

            var usuario = await _usuarioRepository.GetByIdAndComercioIdAsync(id, comercioId);
            if (usuario is null) throw new NotFoundException("Usuário não encontrado");

            var vinculoAlvo = usuario.UsuarioComercios.First(uc => uc.ComercioID == comercioId);
            _autorizacaoCargoService.GarantirNaoProprietario(chamadorEhAdmin, vinculoAlvo.Cargo.EhProprietario, "Você não tem permissão para gerenciar um vínculo com esse cargo.");

            var response = usuario.Adapt<UsuarioResponse>();
            response.CargoNome = vinculoAlvo.Cargo.Nome;
            return response;
        }

        public async Task<UsuarioResponse> CriarAsync(
            CriarUsuarioRequest request,
            int executorId,
            bool ehProprietarioLogado,
            IEnumerable<string> permissoesLogado,
            int? comercioIdLogado)
        {
            if (request.ComercioId != comercioIdLogado) throw new BadRequestException("Contexto de comércio inválido.");

            var chamadorEhAdmin = await _autorizacaoCargoService.EhAdminGlobalAsync(executorId);
            _autorizacaoCargoService.GarantirPermissao(chamadorEhAdmin, ehProprietarioLogado, permissoesLogado,
                PermissaoConstantes.GerenciarUsuarios, "Você não pode cadastrar novos usuários.");

            var cargo = await _cargoRepository.GetByIdAsync(request.CargoID);
            if (cargo == null || !cargo.Ativo || (cargo.ComercioID != null && cargo.ComercioID != request.ComercioId))
                throw new BadRequestException("O cargo informado é inválido para este comércio.");

            _autorizacaoCargoService.GarantirNaoProprietario(chamadorEhAdmin, cargo.EhProprietario, "Você não pode cadastrar usuários com o cargo de proprietário.");

            var existente = await _usuarioRepository.GetUsuarioByEmail(request.Email);
            if (existente != null) throw new BadRequestException("E-mail já cadastrado.");

            var novoUsuario = request.Adapt<Usuario>();
            novoUsuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

            await _usuarioRepository.AddAsync(novoUsuario);

            var vinculo = new UsuarioComercio
            {
                Usuario = novoUsuario,
                ComercioID = request.ComercioId,
                CargoID = cargo.CargoID,
                Cargo = cargo
            };

            await _usuarioComercioRepository.AddAsync(vinculo);
            await _unitOfWork.SaveChangesAsync();

            return novoUsuario.Adapt<UsuarioResponse>();
        }

        public async Task<UsuarioResponse> AtualizarAsync(int id, AtualizarUsuarioRequest request, int comercioId, int usuarioIdLogado, bool ehProprietarioLogado, IEnumerable<string> permissoesLogado)
        {
            var chamadorEhAdmin = await _autorizacaoCargoService.EhAdminGlobalAsync(usuarioIdLogado);
            _autorizacaoCargoService.GarantirPermissao(chamadorEhAdmin, ehProprietarioLogado, permissoesLogado,
                PermissaoConstantes.GerenciarUsuarios, "Você não pode atualizar usuários deste comércio.");

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario is null) throw new NotFoundException("Usuario não encontrado");

            var vinculo = await _usuarioComercioRepository.GetVinculoAsync(id, comercioId);
            if (vinculo is null) throw new UnauthorizedException("Usuário não pertence ao seu comercio");

            _autorizacaoCargoService.GarantirNaoProprietario(chamadorEhAdmin, vinculo.Cargo.EhProprietario, "Você não tem permissão para gerenciar um vínculo com esse cargo.");

            await GarantirEmailDisponivelAsync(request.Email, id);

            if (request.CargoID.HasValue && request.CargoID.Value != vinculo.CargoID)
            {
                var novoCargo = await _cargoRepository.GetByIdAsync(request.CargoID.Value);
                if (novoCargo == null || !novoCargo.Ativo || (novoCargo.ComercioID != null && novoCargo.ComercioID != comercioId))
                    throw new BadRequestException("O cargo informado é inválido para este comércio.");

                _autorizacaoCargoService.GarantirNaoProprietario(chamadorEhAdmin, novoCargo.EhProprietario, "Você não pode atribuir o cargo de proprietário.");

                vinculo.CargoID = novoCargo.CargoID;
                vinculo.Cargo = novoCargo;
                await _usuarioComercioRepository.UpdateAsync(vinculo);
                await _logAuditoriaService.RegistrarAsync(usuarioIdLogado, comercioId, "UsuarioComercioCargoAlterado", "UsuarioComercio", id, $"NovoCargo={novoCargo.Nome}");
            }

            request.Adapt(usuario);
            usuario.AtualizadoEm = DateTime.UtcNow;
            await _usuarioRepository.UpdateAsync(usuario);
            await _logAuditoriaService.RegistrarAsync(usuarioIdLogado, comercioId, "UsuarioIdentidadeAtualizada", "Usuario", id);
            await _unitOfWork.SaveChangesAsync();

            var response = usuario.Adapt<UsuarioResponse>();
            response.CargoNome = vinculo.Cargo.Nome;
            return response;
        }

        public async Task<UsuarioResponse> AtualizarGlobalAsync(int id, AtualizarUsuarioRequest request, int usuarioIdLogado)
        {
            await _autorizacaoGlobalService.GarantirAdministradorGlobalAsync(usuarioIdLogado);

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario is null) throw new NotFoundException("Usuário não encontrado.");

            await GarantirEmailDisponivelAsync(request.Email, id);

            request.Adapt(usuario);
            usuario.AtualizadoEm = DateTime.UtcNow;
            await _usuarioRepository.UpdateAsync(usuario);
            await _logAuditoriaService.RegistrarAsync(usuarioIdLogado, null, "UsuarioIdentidadeAtualizadaGlobal", "Usuario", id);
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

        private async Task GarantirEmailDisponivelAsync(string email, int usuarioId)
        {
            var existente = await _usuarioRepository.GetUsuarioByEmail(email);
            if (existente != null && existente.UsuarioID != usuarioId)
                throw new BadRequestException("E-mail já cadastrado.");
        }
    }
}
