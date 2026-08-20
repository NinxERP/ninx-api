using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Application.Services
{
    public class LoginService : ILoginService
    {
        private readonly ITokenProvider _tokenProvider;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IUsuarioComercioRepository _usuarioComercioRepository;
        private readonly IAssinaturaPlanoRepository _assinaturaPlanoRepository;
        private readonly ICargoEfetivoService _cargoEfetivoService;
        private readonly IUnitOfWork _unitOfWork;
        public LoginService(ITokenProvider tokenProvider,
            IUsuarioRepository usuarioRepository,
            IUsuarioComercioRepository usuarioComercioRepository,
            IAssinaturaPlanoRepository assinaturaPlanoRepository,
            ICargoEfetivoService cargoEfetivoService,
            IUnitOfWork unitOfWork)
        {
            _tokenProvider = tokenProvider;
            _usuarioRepository = usuarioRepository;
            _usuarioComercioRepository = usuarioComercioRepository;
            _assinaturaPlanoRepository = assinaturaPlanoRepository;
            _cargoEfetivoService = cargoEfetivoService;
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var usuario = await _usuarioRepository.GetUsuarioByEmail(request.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            {
                throw new BadRequestException("E-mail ou senha incorretos");
            }

            if (!usuario.Ativo)
            {
                throw new ForbiddenException("Este usuário está desativado.");
            }

            var todosVinculos = await _usuarioComercioRepository.GetByUsuarioIdAsync(usuario.UsuarioID);
            var usuarioComercios = (todosVinculos ?? Enumerable.Empty<UsuarioComercio>())
                .Where(x => x.Ativo)
                .ToList();

            if (!usuarioComercios.Any())
            {
                throw new ForbiddenException("Este usuário não possui nenhum comércio vinculado.");
            }

            if (request.ComercioID.HasValue && request.ComercioID > 0)
            {
                var usuarioC = usuarioComercios.FirstOrDefault(x => x.ComercioID == request.ComercioID);
                if (usuarioC == null)
                    throw new UnauthorizedException("Acesso negado ao comércio selecionado.");

                await ValidarPlanoAsync(usuarioC.ComercioID);

                var cargoEfetivoC = await _cargoEfetivoService.ResolverCargoEfetivoAsync(usuario, usuarioC.Cargo);
                return new LoginResponse { Token = _tokenProvider.GerarToken(usuario, usuarioC.ComercioID, cargoEfetivoC, usuarioC.Comercio.NomeComercio) };
            }

            if (usuarioComercios.Count == 1)
            {
                var unico = usuarioComercios.First();
                await ValidarPlanoAsync(unico.ComercioID);
                var cargoEfetivoUnico = await _cargoEfetivoService.ResolverCargoEfetivoAsync(usuario, unico.Cargo);
                return new LoginResponse { Token = _tokenProvider.GerarToken(usuario, unico.ComercioID, cargoEfetivoUnico, unico.Comercio.NomeComercio) };
            }
            else
            {
                return new LoginResponse
                {
                    Comercios = usuarioComercios.Select(x => new ComercioSimplificado
                    {
                        ComercioID = x.ComercioID,
                        Nome = x.Comercio.NomeComercio
                    }).ToList()
                };
            }
        }

        private async Task ValidarPlanoAsync(int comercioId)
        {
            var plano = await _assinaturaPlanoRepository.GetByComercioIdAsync(comercioId);
            if (plano == null) throw new NotFoundException("Comércio sem plano vinculado.");
            if (plano.Status == StatusAssinatura.Cancelada || plano.Status == StatusAssinatura.Vencida) throw new ForbiddenException("Assinatura vencida ou cancelada.");
            if (plano.DataFim < DateTime.UtcNow)
            {
                plano.Status = StatusAssinatura.Vencida;
                await _assinaturaPlanoRepository.UpdateAsync(plano);
                await _unitOfWork.SaveChangesAsync();
                throw new ForbiddenException("Sua assinatura está vencida.");
            }
        }
    }
}
    