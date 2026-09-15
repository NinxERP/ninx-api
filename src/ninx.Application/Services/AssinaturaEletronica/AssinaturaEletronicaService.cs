using Mapster;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class AssinaturaEletronicaService : IAssinaturaEletronicaService
    {
        private readonly IAssinaturaEletronicaRepository _assinaturaEletronicaRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVendaRepository _vendaRepository;
        private readonly IDocumentoRendererService _documentoRendererService;
        private readonly IVendaService _vendaService;
        public AssinaturaEletronicaService
            (IAssinaturaEletronicaRepository assinaturaEletronicaRepository,
            IUnitOfWork unitOfWork,
            IVendaRepository vendaRepository,
            IDocumentoRendererService documentoRendererService,
            IVendaService vendaService)
        {
            _assinaturaEletronicaRepository = assinaturaEletronicaRepository;
            _unitOfWork = unitOfWork;
            _vendaRepository = vendaRepository;
            _documentoRendererService = documentoRendererService;
            _vendaService = vendaService;
        }
 
        public async Task<IEnumerable<AssinaturaEletronicaResponse>> GetAll()
        {
            var assinaturas = await _assinaturaEletronicaRepository.GetAllAsync();
            return assinaturas.Adapt<IEnumerable<AssinaturaEletronicaResponse>>();
        }

        public async Task<IEnumerable<AssinaturaEletronicaResponse>> GetByAssinaturaEletronicaId(int AssinaturaEletronicaId)
        {
            var assinaturas = await _assinaturaEletronicaRepository.GetByIdAsync(AssinaturaEletronicaId);
            if (assinaturas == null)
                throw new NotFoundException("AssinaturaEletronica não encontrado.");

            return assinaturas.Adapt<IEnumerable<AssinaturaEletronicaResponse>>();
        }

        public async Task<AssinaturaEletronicaResponse> GetByIdAsync(int id)
        {
            var AssinaturaEletronica = await _assinaturaEletronicaRepository.GetByIdAsync(id);
            if (AssinaturaEletronica == null)
                throw new NotFoundException("AssinaturaEletronica não encontrado.");

            return AssinaturaEletronica.Adapt<AssinaturaEletronicaResponse>();
        }
        public async Task ConfirmarAssinaturaAsync(Guid guid, string imagemBase64, string ip, string dispositivo)
        {
            var assinaturas = await _assinaturaEletronicaRepository.GetAllByGuidAsync(guid);
            if (assinaturas.Count == 0) throw new NotFoundException("Documento não encontrado.");

            var primeira = assinaturas[0];
            var dataAssinatura = DateTime.UtcNow;
            if (primeira.Assinado) throw new BadRequestException("Este documento já foi assinado.");
            if (primeira.Status != Domain.Enums.StatusAssinatura.Ativa) throw new BadRequestException("Este documento não está mais disponível para assinatura.");

            string hashRecebido;
            try
            {
                hashRecebido = HashDocumento.Sha256Hex(imagemBase64)
                    ?? throw new BadRequestException("O documento assinado não foi enviado.");
            }
            catch (FormatException)
            {
                throw new BadRequestException("O documento assinado enviado é inválido.");
            }

            // O PDF servido ao comércio é o que o cliente assinou, sem regeneração, seguido de uma
            // página de certificado com as evidências. Como todas as vendas do GUID compartilham o
            // mesmo documento, o certificado é montado uma vez.
            string? documentoAssinadoComCertificado = null;
            if (primeira.TipoDocumento.HasValue)
            {
                primeira.HashDocumentoAssinado = hashRecebido;
                primeira.IpAssinante = ip;
                primeira.DispositivoInfo = dispositivo;
                primeira.DataAssinatura = dataAssinatura;

                var certificadoPdf = await _documentoRendererService.ConverterParaPdfBase64Async(
                    DocumentoTokenBuilder.BuildCertificadoAssinaturaHtml(primeira));
                try
                {
                    documentoAssinadoComCertificado = _documentoRendererService.AnexarPdfBase64(imagemBase64, certificadoPdf);
                }
                catch (Exception ex) when (ex is not BadRequestException)
                {
                    // O PDF vem de fora: se não abrir, é erro de entrada, não do servidor.
                    throw new BadRequestException("O documento assinado enviado não é um PDF válido.");
                }
            }

            // Um mesmo DocumentoGuid pode estar vinculado a mais de uma venda (ex: quitação global de fiado
            // abate várias vendas de uma vez) — todos os registros precisam ser assinados juntos, não só o primeiro.
            foreach (var assinatura in assinaturas)
            {
                // Só aqui a venda fiada baixa estoque e o pagamento passa a contar no saldo.
                await _vendaService.EfetivarDocumentoAssinadoAsync(assinatura, dataAssinatura);

                // ImagemAssinatura guarda o PDF exatamente como recebido: é sobre ele que o hash foi calculado.
                assinatura.ImagemAssinatura = imagemBase64;
                assinatura.HashDocumentoAssinado = hashRecebido;
                assinatura.IpAssinante = ip;
                assinatura.DispositivoInfo = dispositivo;
                assinatura.DataAssinatura = dataAssinatura;
                assinatura.Assinado = true;
                assinatura.DocumentoAssinadoBase64 = documentoAssinadoComCertificado;

                await _assinaturaEletronicaRepository.UpdateAsync(assinatura);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<AssinaturaEletronicaResponse> ObterDadosParaAssinaturaAsync(Guid guid)
        {
            var assinatura = await _assinaturaEletronicaRepository.GetByGuidParaAssinarAsync(guid);
            if (assinatura == null) throw new NotFoundException("Documento não encontrado.");
            return assinatura.Adapt<AssinaturaEletronicaResponse>();
        }

        public async Task<AssinaturaEletronicaResponse> ObterDocumentoAssinadoAsync(Guid guid, int comercioId)
        {
            var assinatura = await _assinaturaEletronicaRepository.GetClienteLojaAssinaturaByGuidAsync(guid);
            if (assinatura == null || assinatura.Venda.ComercioID != comercioId)
                throw new NotFoundException("Documento não encontrado.");

            return assinatura.Adapt<AssinaturaEletronicaResponse>();
        }

        public async Task<bool> ValidaAssinado(Guid guid)
        {
            var assinatura = await _assinaturaEletronicaRepository.GetByGuidAsync(guid);
            if (assinatura == null) throw new NotFoundException("Documento não encontrado.");
            return assinatura.Assinado;
        }

    }
}
