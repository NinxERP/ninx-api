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
        private readonly IContaFiadoService _contaFiadoService;
        public AssinaturaEletronicaService
            (IAssinaturaEletronicaRepository assinaturaEletronicaRepository,
            IUnitOfWork unitOfWork,
            IVendaRepository vendaRepository,
            IDocumentoRendererService documentoRendererService,
            IVendaService vendaService,
            IContaFiadoService contaFiadoService)
        {
            _assinaturaEletronicaRepository = assinaturaEletronicaRepository;
            _unitOfWork = unitOfWork;
            _vendaRepository = vendaRepository;
            _documentoRendererService = documentoRendererService;
            _vendaService = vendaService;
            _contaFiadoService = contaFiadoService;
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
        public async Task ConfirmarAssinaturaAsync(Guid guid, IReadOnlyCollection<TracoAssinaturaRequest> tracos, string ip, string dispositivo)
        {
            var assinaturas = await _assinaturaEletronicaRepository.GetAllByGuidAsync(guid);
            if (assinaturas.Count == 0) throw new NotFoundException("Documento não encontrado.");

            var primeira = assinaturas[0];
            var dataAssinatura = DateTime.UtcNow;
            if (primeira.Assinado) throw new BadRequestException("Este documento já foi assinado.");
            if (primeira.Status != Domain.Enums.StatusAssinatura.Ativa) throw new BadRequestException("Este documento não está mais disponível para assinatura.");

            // O cliente só envia os traços: o servidor os desenha sobre o PDF que ele mesmo gerou e
            // exibiu, de modo que o conteúdo assinado não pode ser trocado pelo navegador.
            var documentoAssinado = _documentoRendererService.AplicarTracosAssinatura(primeira.DocumentoOriginalBase64, tracos);
            var hashDocumento = HashDocumento.Sha256Hex(documentoAssinado)!;

            // O PDF servido ao comércio é o documento assinado seguido de uma página de certificado
            // com as evidências. Como todas as vendas do GUID compartilham o mesmo documento, o
            // certificado é montado uma vez.
            string? documentoAssinadoComCertificado = null;
            if (primeira.TipoDocumento.HasValue)
            {
                primeira.HashDocumentoAssinado = hashDocumento;
                primeira.IpAssinante = ip;
                primeira.DispositivoInfo = dispositivo;
                primeira.DataAssinatura = dataAssinatura;

                var certificadoPdf = await _documentoRendererService.ConverterParaPdfBase64Async(
                    DocumentoTokenBuilder.BuildCertificadoAssinaturaHtml(primeira));
                documentoAssinadoComCertificado = _documentoRendererService.AnexarPdfBase64(documentoAssinado, certificadoPdf);
            }

            // Um mesmo DocumentoGuid pode estar vinculado a mais de uma venda (ex: quitação global de fiado
            // abate várias vendas de uma vez) — todos os registros precisam ser assinados juntos, não só o primeiro.
            foreach (var assinatura in assinaturas)
            {
                // Só aqui a venda fiada baixa estoque e o pagamento passa a contar no saldo;
                // no termo de abertura, é aqui que a conta passa a aceitar fiado.
                if (assinatura.TermoAberturaID.HasValue)
                    await _contaFiadoService.EfetivarTermoAssinadoAsync(assinatura, dataAssinatura);
                else
                    await _vendaService.EfetivarDocumentoAssinadoAsync(assinatura, dataAssinatura);

                // ImagemAssinatura guarda o PDF assinado, sem o certificado: é sobre ele que o hash foi calculado.
                assinatura.ImagemAssinatura = documentoAssinado;
                assinatura.HashDocumentoAssinado = hashDocumento;
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
            var comercioDoDocumento = assinatura?.Venda?.ComercioID ?? assinatura?.TermoAbertura?.Cliente.ComercioID;
            if (assinatura == null || comercioDoDocumento != comercioId)
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
