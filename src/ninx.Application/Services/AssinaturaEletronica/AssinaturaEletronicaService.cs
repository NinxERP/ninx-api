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
        public AssinaturaEletronicaService
            (IAssinaturaEletronicaRepository assinaturaEletronicaRepository,
            IUnitOfWork unitOfWork,
            IVendaRepository vendaRepository,
            IDocumentoRendererService documentoRendererService)
        {
            _assinaturaEletronicaRepository = assinaturaEletronicaRepository;
            _unitOfWork = unitOfWork;
            _vendaRepository = vendaRepository;
            _documentoRendererService = documentoRendererService;
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
            if (primeira.Assinado) throw new BadRequestException("Este documento já foi assinado.");
            if (primeira.Status != Domain.Enums.StatusAssinatura.Ativa) throw new BadRequestException("Este documento não está mais disponível para assinatura.");

            var dataAssinatura = DateTime.UtcNow;

            // Um mesmo DocumentoGuid pode estar vinculado a mais de uma venda (ex: quitação global de fiado
            // abate várias vendas de uma vez) — todos os registros precisam ser assinados juntos, não só o primeiro.
            foreach (var assinatura in assinaturas)
            {
                var venda = await _vendaRepository.GetByIdAsync(assinatura.VendaID);
                if (venda.Status == Domain.Enums.StatusVenda.Cancelada || venda.Status == Domain.Enums.StatusVenda.Estornada)
                    throw new BadRequestException("Não é possível assinar o documento de uma venda cancelada ou estornada.");

                venda.AtualizadoEm = dataAssinatura;
                venda.Status = Domain.Enums.StatusVenda.Aberta;

                assinatura.ImagemAssinatura = imagemBase64;
                assinatura.IpAssinante = ip;
                assinatura.DispositivoInfo = dispositivo;
                assinatura.DataAssinatura = dataAssinatura;
                assinatura.Assinado = true;

                if (assinatura.TipoDocumento.HasValue && !string.IsNullOrEmpty(assinatura.DocumentoHtmlMesclado))
                {
                    var blocoAssinado = DocumentoTokenBuilder.BuildBlocoAssinaturaConfirmada(imagemBase64, dataAssinatura, ip, dispositivo);
                    var htmlAssinado = DocumentoTokenBuilder.SubstituirBlocoAssinatura(assinatura.DocumentoHtmlMesclado, blocoAssinado);
                    assinatura.DocumentoAssinadoBase64 = await _documentoRendererService.ConverterParaPdfBase64Async(htmlAssinado);
                }

                await _vendaRepository.UpdateAsync(venda);
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
