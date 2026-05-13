using ninx.Communication;
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
        private readonly IPagamentoHistoricoAssinaturaPlanoRepository _pagamentoHistoricoAssinaturaPlanoRepository;
        private readonly IUnitOfWork _unitOfWork;
        public LoginService(ITokenProvider tokenProvider,
            IUsuarioRepository usuarioRepository, 
            IUsuarioComercioRepository usuarioComercioRepository, 
            IAssinaturaPlanoRepository assinaturaPlanoRepository, 
            IPagamentoHistoricoAssinaturaPlanoRepository PagamentoHistoricoAssinaturaPlanoRepository,
            IUnitOfWork unitOfWork)
        {
            _tokenProvider = tokenProvider;
            _usuarioRepository = usuarioRepository;
            _usuarioComercioRepository = usuarioComercioRepository;
            _assinaturaPlanoRepository = assinaturaPlanoRepository;
            _pagamentoHistoricoAssinaturaPlanoRepository = PagamentoHistoricoAssinaturaPlanoRepository;
            _unitOfWork = unitOfWork;
        }   

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var usuario = await _usuarioRepository.GetUsuarioByEmail(request.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            {
                throw new BadRequestException("E-mail ou senha incorretos");
            }

            var usuarioComercios = await _usuarioComercioRepository.GetByUsuarioIdAsync(usuario.UsuarioID);
            if (usuarioComercios == null || !usuarioComercios.Any())
            {
                throw new ForbiddenException("Este usuário não possui nenhum comércio vinculado.");
            }

            var plano = await _assinaturaPlanoRepository.GetByComercioIdAsync(usuarioComercios.FirstOrDefault().ComercioID);
            if (plano == null) throw new Exception("Comércio sem plano vinculado.");
            if (plano.Status == StatusAssinatura.Cancelada || plano.Status == StatusAssinatura.Vencida) throw new ForbiddenException("Assinatura vencida ou cancelada.");

            var ultimoPagamento = await _pagamentoHistoricoAssinaturaPlanoRepository.GetUltimoPagamentoByAssinaturaPlanoIdAsync(plano.AssinaturaID);
            if (ultimoPagamento == null) throw new Exception("Comércio sem pagamentos registrados.");
            if (ultimoPagamento.DataVencimento < DateTime.UtcNow)
            {
                plano.Status = StatusAssinatura.Vencida;
                await _assinaturaPlanoRepository.UpdateAsync(plano);
                await _unitOfWork.CommitAsync();
                throw new ForbiddenException("Sua assinatura está vencida.");
            }
                
            if (request.ComercioID.HasValue && request.ComercioID > 0)
            {
                var usuarioC = usuarioComercios.FirstOrDefault(x => x.ComercioID == request.ComercioID);
                if (usuarioC == null)
                    throw new UnauthorizedException("Acesso negado ao comércio selecionado.");

                return new LoginResponse { Token = _tokenProvider.GerarToken(usuario, usuarioC.ComercioID, usuarioC.Permissao) };
            }

            if (usuarioComercios.Count() == 1)
            {
                var unico = usuarioComercios.First();
                return new LoginResponse { Token = _tokenProvider.GerarToken(usuario, unico.ComercioID, unico.Permissao) };
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
    }
}
    