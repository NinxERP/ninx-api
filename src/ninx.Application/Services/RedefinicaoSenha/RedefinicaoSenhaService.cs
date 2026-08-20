using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Application.Services
{
    public class RedefinicaoSenhaService : IRedefinicaoSenhaService
    {
        private const int CooldownSegundos = 60;
        private const int ExpiracaoMinutos = 15;
        private const int MaxTentativas = 5;
        private const string MensagemCodigoInvalido = "Código inválido ou expirado.";

        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IRedefinicaoSenhaRepository _redefinicaoSenhaRepository;
        private readonly IEmailService _emailService;
        private readonly ILogAuditoriaService _logAuditoriaService;
        private readonly IUnitOfWork _unitOfWork;

        public RedefinicaoSenhaService(
            IUsuarioRepository usuarioRepository,
            IRedefinicaoSenhaRepository redefinicaoSenhaRepository,
            IEmailService emailService,
            ILogAuditoriaService logAuditoriaService,
            IUnitOfWork unitOfWork)
        {
            _usuarioRepository = usuarioRepository;
            _redefinicaoSenhaRepository = redefinicaoSenhaRepository;
            _emailService = emailService;
            _logAuditoriaService = logAuditoriaService;
            _unitOfWork = unitOfWork;
        }

        public async Task SolicitarAsync(SolicitarRedefinicaoSenhaRequest request)
        {
            var usuario = await _usuarioRepository.GetUsuarioByEmail(request.Email);
            if (usuario is null || !usuario.Ativo)
                return;

            var ultimoAtivo = await _redefinicaoSenhaRepository.GetUltimoAtivoAsync(usuario.UsuarioID);
            if (ultimoAtivo is not null && DateTime.UtcNow - ultimoAtivo.CriadoEm < TimeSpan.FromSeconds(CooldownSegundos))
                return;

            await _redefinicaoSenhaRepository.InvalidarAtivosAsync(usuario.UsuarioID);

            var codigo = Random.Shared.Next(0, 1_000_000).ToString("D6");

            var redefinicao = new RedefinicaoSenha
            {
                UsuarioID = usuario.UsuarioID,
                CodigoHash = BCrypt.Net.BCrypt.HashPassword(codigo),
                CriadoEm = DateTime.UtcNow,
                ExpiraEm = DateTime.UtcNow.AddMinutes(ExpiracaoMinutos)
            };
            await _redefinicaoSenhaRepository.AddAsync(redefinicao);

            await _logAuditoriaService.RegistrarAsync(usuario.UsuarioID, null, "UsuarioSolicitouRedefinicaoSenha", "Usuario", usuario.UsuarioID);

            await _unitOfWork.SaveChangesAsync();

            try
            {
                var htmlContent = $"<p>Olá {usuario.Nome},</p>" +
                    $"<p>Seu código de redefinição de senha é: <strong>{codigo}</strong></p>" +
                    $"<p>Este código expira em {ExpiracaoMinutos} minutos. Se você não solicitou esta redefinição, ignore este e-mail.</p>";

                await _emailService.EnviarAsync(usuario.Email, usuario.Nome, "Código de redefinição de senha - Ninx", htmlContent);
            }
            catch (Exception)
            {
                // Falha no envio não deve vazar informação diferente da resposta genérica para o chamador anônimo.
            }
        }

        public async Task ConfirmarAsync(ConfirmarRedefinicaoSenhaRequest request)
        {
            var usuario = await _usuarioRepository.GetUsuarioByEmail(request.Email);
            if (usuario is null)
                throw new BadRequestException(MensagemCodigoInvalido);

            var ativo = await _redefinicaoSenhaRepository.GetUltimoAtivoAsync(usuario.UsuarioID);
            if (ativo is null)
                throw new BadRequestException(MensagemCodigoInvalido);

            if (ativo.Tentativas >= MaxTentativas)
            {
                ativo.Utilizado = true;
                await _redefinicaoSenhaRepository.UpdateAsync(ativo);
                await _unitOfWork.SaveChangesAsync();
                throw new BadRequestException(MensagemCodigoInvalido);
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Codigo, ativo.CodigoHash))
            {
                ativo.Tentativas++;
                await _redefinicaoSenhaRepository.UpdateAsync(ativo);
                await _unitOfWork.SaveChangesAsync();
                throw new BadRequestException(MensagemCodigoInvalido);
            }

            ativo.Utilizado = true;
            await _redefinicaoSenhaRepository.UpdateAsync(ativo);

            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);
            usuario.AtualizadoEm = DateTime.UtcNow;
            await _usuarioRepository.UpdateAsync(usuario);

            await _logAuditoriaService.RegistrarAsync(usuario.UsuarioID, null, "UsuarioRedefiniuSenha", "Usuario", usuario.UsuarioID);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
