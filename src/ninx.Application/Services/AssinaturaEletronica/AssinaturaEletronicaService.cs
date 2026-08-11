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
        public AssinaturaEletronicaService
            (IAssinaturaEletronicaRepository assinaturaEletronicaRepository, 
            IUnitOfWork unitOfWork,
            IVendaRepository vendaRepository)
        {
            _assinaturaEletronicaRepository = assinaturaEletronicaRepository;
            _unitOfWork = unitOfWork;
            _vendaRepository = vendaRepository;
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
            var assinatura = await _assinaturaEletronicaRepository.GetByGuidAsync(guid);
            if (assinatura == null) throw new NotFoundException("Documento não encontrado.");
            if (assinatura.Assinado) throw new BadRequestException("Este documento já foi assinado.");
            if (assinatura.Status != Domain.Enums.StatusAssinatura.Ativa) throw new BadRequestException("Este documento não está mais disponível para assinatura.");

            var venda = await _vendaRepository.GetByIdAsync(assinatura.VendaID);
            if (venda.Status == Domain.Enums.StatusVenda.Cancelada || venda.Status == Domain.Enums.StatusVenda.Estornada)
                throw new BadRequestException("Não é possível assinar o documento de uma venda cancelada ou estornada.");

            venda.AtualizadoEm = DateTime.UtcNow;
            venda.Status = Domain.Enums.StatusVenda.Aberta;

            assinatura.ImagemAssinatura = imagemBase64;
            assinatura.IpAssinante = ip;
            assinatura.DispositivoInfo = dispositivo;
            assinatura.DataAssinatura = DateTime.UtcNow;
            assinatura.Assinado = true;

            await _vendaRepository.UpdateAsync(venda);
            await _assinaturaEletronicaRepository.UpdateAsync(assinatura);
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
