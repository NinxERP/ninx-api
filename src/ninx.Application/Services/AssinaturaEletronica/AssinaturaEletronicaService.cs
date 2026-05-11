using Mapster;
using ninx.Communication.Response;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class AssinaturaEletronicaService : IAssinaturaEletronicaService
    {
        private readonly IAssinaturaEletronicaRepository _assinaturaEletronicaRepository;
        private readonly IUnitOfWork _unitOfWork;
        public AssinaturaEletronicaService
            (IAssinaturaEletronicaRepository assinaturaEletronicaRepository, 
            IUnitOfWork unitOfWork)
        {
            _assinaturaEletronicaRepository = assinaturaEletronicaRepository;
            _unitOfWork = unitOfWork;
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

            assinatura.ImagemAssinatura = imagemBase64;
            assinatura.IpAssinante = ip;
            assinatura.DispositivoInfo = dispositivo;
            assinatura.DataAssinatura = DateTime.UtcNow;
            assinatura.Assinado = true;

            await _assinaturaEletronicaRepository.UpdateAsync(assinatura);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<AssinaturaEletronicaResponse> ObterDadosParaAssinaturaAsync(Guid guid)
        {
            var assinatura = await _assinaturaEletronicaRepository.GetClienteLojaAssinaturaByGuidAsync(guid);
            if (assinatura == null) throw new NotFoundException("Documento não encontrado.");
            return assinatura.Adapt<AssinaturaEletronicaResponse>();
        }
    }
}
