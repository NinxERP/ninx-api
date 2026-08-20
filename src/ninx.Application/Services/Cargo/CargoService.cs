using Mapster;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class CargoService : ICargoService
    {
        private readonly ICargoRepository _cargoRepository;
        private readonly IUsuarioComercioRepository _usuarioComercioRepository;
        private readonly IAutorizacaoCargoService _autorizacaoCargoService;
        private readonly IAutorizacaoGlobalService _autorizacaoGlobalService;
        private readonly IUnitOfWork _unitOfWork;

        public CargoService(
            ICargoRepository cargoRepository,
            IUsuarioComercioRepository usuarioComercioRepository,
            IAutorizacaoCargoService autorizacaoCargoService,
            IAutorizacaoGlobalService autorizacaoGlobalService,
            IUnitOfWork unitOfWork)
        {
            _cargoRepository = cargoRepository;
            _usuarioComercioRepository = usuarioComercioRepository;
            _autorizacaoCargoService = autorizacaoCargoService;
            _autorizacaoGlobalService = autorizacaoGlobalService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CargoResponse>> GetAsync(int? comercioId)
        {
            var cargos = comercioId.HasValue
                ? await _cargoRepository.GetDisponiveisParaComercioAsync(comercioId.Value)
                : await _cargoRepository.GetCargosBaseAsync();

            return cargos.Adapt<IEnumerable<CargoResponse>>();
        }

        public async Task<CargoResponse> CriarAsync(CriarCargoRequest request, int usuarioLogadoId)
        {
            if (request.ComercioID is null)
            {
                await _autorizacaoGlobalService.GarantirAdministradorGlobalAsync(usuarioLogadoId);

                var nomeBaseEmUso = await _cargoRepository.ExisteNomeAsync(request.Nome, null);
                if (nomeBaseEmUso)
                    throw new BadRequestException("Já existe um cargo base com esse nome.");

                var cargoBase = new Cargo
                {
                    Nome = request.Nome,
                    Peso = request.Peso,
                    ComercioID = null,
                    Ativo = true
                };

                await _cargoRepository.AddAsync(cargoBase);
                await _unitOfWork.SaveChangesAsync();

                return cargoBase.Adapt<CargoResponse>();
            }

            var comercioId = request.ComercioID.Value;

            var vinculoChamador = await _usuarioComercioRepository.GetVinculoAsync(usuarioLogadoId, comercioId);
            var chamadorEhAdmin = await _autorizacaoCargoService.EhAdminGlobalAsync(usuarioLogadoId);

            if (!chamadorEhAdmin && (vinculoChamador == null || vinculoChamador.Cargo.Peso < Domain.Constants.CargoConstantes.PesoDono))
                throw new ForbiddenException("Você não tem permissão para criar cargos neste comércio.");

            if (!chamadorEhAdmin && request.Peso >= vinculoChamador!.Cargo.Peso)
                throw new ForbiddenException("Você só pode criar cargos com peso menor que o seu.");

            var nomeEmUso = await _cargoRepository.ExisteNomeAsync(request.Nome, comercioId);
            if (nomeEmUso)
                throw new BadRequestException("Já existe um cargo com esse nome neste comércio.");

            var cargo = new Cargo
            {
                Nome = request.Nome,
                Peso = request.Peso,
                ComercioID = comercioId,
                Ativo = true
            };

            await _cargoRepository.AddAsync(cargo);
            await _unitOfWork.SaveChangesAsync();

            return cargo.Adapt<CargoResponse>();
        }

        public async Task<CargoResponse> AtualizarAsync(int id, AtualizarCargoRequest request, int usuarioLogadoId)
        {
            var cargo = await _cargoRepository.GetByIdAsync(id);
            if (cargo == null)
                throw new NotFoundException("Cargo não encontrado.");

            if (cargo.ComercioID == null)
            {
                await _autorizacaoGlobalService.GarantirAdministradorGlobalAsync(usuarioLogadoId);

                if (cargo.Reservado)
                    throw new ForbiddenException("Este cargo é reservado pelo sistema e não pode ser alterado.");
            }
            else
            {
                var vinculoChamador = await _usuarioComercioRepository.GetVinculoAsync(usuarioLogadoId, cargo.ComercioID.Value);
                var chamadorEhAdmin = await _autorizacaoCargoService.EhAdminGlobalAsync(usuarioLogadoId);

                if (!chamadorEhAdmin && (vinculoChamador == null || vinculoChamador.Cargo.Peso < Domain.Constants.CargoConstantes.PesoDono))
                    throw new ForbiddenException("Você não tem permissão para editar cargos deste comércio.");

                if (!chamadorEhAdmin && request.Peso >= vinculoChamador!.Cargo.Peso)
                    throw new ForbiddenException("Você só pode atribuir peso menor que o seu ao cargo.");
            }

            if (!string.Equals(cargo.Nome, request.Nome, StringComparison.Ordinal))
            {
                var nomeEmUso = await _cargoRepository.ExisteNomeAsync(request.Nome, cargo.ComercioID);
                if (nomeEmUso)
                    throw new BadRequestException(cargo.ComercioID == null
                        ? "Já existe um cargo base com esse nome."
                        : "Já existe um cargo com esse nome neste comércio.");
            }

            cargo.Nome = request.Nome;
            cargo.Peso = request.Peso;
            cargo.AtualizadoEm = DateTime.UtcNow;

            await _cargoRepository.UpdateAsync(cargo);
            await _unitOfWork.SaveChangesAsync();

            return cargo.Adapt<CargoResponse>();
        }

        public async Task DesativarAsync(int id, int usuarioLogadoId)
        {
            var cargo = await _cargoRepository.GetByIdAsync(id);
            if (cargo == null)
                throw new NotFoundException("Cargo não encontrado.");

            if (cargo.ComercioID == null)
            {
                await _autorizacaoGlobalService.GarantirAdministradorGlobalAsync(usuarioLogadoId);

                if (cargo.Reservado)
                    throw new ForbiddenException("Este cargo é reservado pelo sistema e não pode ser desativado.");
            }
            else
            {
                var vinculoChamador = await _usuarioComercioRepository.GetVinculoAsync(usuarioLogadoId, cargo.ComercioID.Value);
                var chamadorEhAdmin = await _autorizacaoCargoService.EhAdminGlobalAsync(usuarioLogadoId);

                if (!chamadorEhAdmin && (vinculoChamador == null || vinculoChamador.Cargo.Peso < Domain.Constants.CargoConstantes.PesoDono))
                    throw new ForbiddenException("Você não tem permissão para desativar cargos deste comércio.");
            }

            cargo.Ativo = false;
            cargo.AtualizadoEm = DateTime.UtcNow;

            await _cargoRepository.UpdateAsync(cargo);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
