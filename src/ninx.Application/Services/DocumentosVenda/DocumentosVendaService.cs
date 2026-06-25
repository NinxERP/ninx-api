using Mapster;
using ninx.Communication;
using ninx.Domain.Enums;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class DocumentosVendaService : IDocumentosVendaService
    {
        private readonly IDocumentosVendaRepository _documentosVidaRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVendaService _vendaService;
        public DocumentosVendaService
            (IDocumentosVendaRepository documentosVidasRepository, 
            IUnitOfWork unitOfWork, 
            IVendaService vendaService)
        {
            _documentosVidaRepository = documentosVidasRepository;
            _unitOfWork = unitOfWork;
            _vendaService = vendaService;
        }
 
        public async Task<IEnumerable<DocumentosVendaResponse>> GetAll()
        {
            var assinaturas = await _documentosVidaRepository.GetAllAsync();
            return assinaturas.Adapt<IEnumerable<DocumentosVendaResponse>>();
        }

        public async Task<IEnumerable<DocumentosVendaResponse>> GetByAssinaturaEletronicaId(int AssinaturaEletronicaId)
        {
            var assinaturas = await _documentosVidaRepository.GetByIdAsync(AssinaturaEletronicaId);
            if (assinaturas == null)
                throw new NotFoundException("AssinaturaEletronica não encontrado.");

            return assinaturas.Adapt<IEnumerable<DocumentosVendaResponse>>();
        }

        public async Task<DocumentosVendaResponse> GetByIdAsync(int id)
        {
            var AssinaturaEletronica = await _documentosVidaRepository.GetByIdAsync(id);
            if (AssinaturaEletronica == null)
                throw new NotFoundException("AssinaturaEletronica não encontrado.");

            return AssinaturaEletronica.Adapt<DocumentosVendaResponse>();
        }
        public async Task ConfirmarAssinaturaAsync(Guid guid, string imagemBase64, string ip, string dispositivo)
        {
            var assinatura = await _documentosVidaRepository.GetClienteLojaAssinaturaByGuidAsync(guid);

            if (assinatura == null) throw new NotFoundException("Documento não encontrado.");
            if (assinatura.Assinado) throw new BadRequestException("Este documento já foi assinado.");

            assinatura.ImagemAssinatura = imagemBase64;
            assinatura.IpAssinante = ip;
            assinatura.DispositivoInfo = dispositivo;
            assinatura.DataAssinatura = DateTime.UtcNow;
            assinatura.Assinado = true;
            assinatura.Status = StatusAssinaturaEletronica.Assinada;

            assinatura.Venda.Status = StatusVenda.Finalizada;   

            await _documentosVidaRepository.UpdateAsync(assinatura);
            await _unitOfWork.CommitAsync();
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<DocumentosVendaResponse> ObterDadosParaAssinaturaAsync(Guid guid)
        {
            var assinatura = await _documentosVidaRepository.GetByGuidAsync(guid);
            if (assinatura == null) throw new NotFoundException("Documento não encontrado.");
            return assinatura.Adapt<DocumentosVendaResponse>();
        }

        public async Task<bool> ValidaAssinado(Guid guid)
        {
            var assinatura = await _documentosVidaRepository.GetByGuidAsync(guid);
            if (assinatura == null) throw new NotFoundException("Documento não encontrado.");
            return assinatura.Assinado;
        }

    }
}
