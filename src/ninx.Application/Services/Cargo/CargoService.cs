using Mapster;
using ninx.Communication;
using ninx.Domain.Constants;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class CargoService : ICargoService
    {
        private readonly ICargoRepository _cargoRepository;
        private readonly IPermissaoRepository _permissaoRepository;
        private readonly IUsuarioComercioRepository _usuarioComercioRepository;
        private readonly IAutorizacaoCargoService _autorizacaoCargoService;
        private readonly IAutorizacaoGlobalService _autorizacaoGlobalService;
        private readonly IUnitOfWork _unitOfWork;

        public CargoService(
            ICargoRepository cargoRepository,
            IPermissaoRepository permissaoRepository,
            IUsuarioComercioRepository usuarioComercioRepository,
            IAutorizacaoCargoService autorizacaoCargoService,
            IAutorizacaoGlobalService autorizacaoGlobalService,
            IUnitOfWork unitOfWork)
        {
            _cargoRepository = cargoRepository;
            _permissaoRepository = permissaoRepository;
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

        public async Task<IEnumerable<PermissaoResponse>> GetPermissoesDisponiveisAsync()
        {
            var permissoes = await _permissaoRepository.GetTodasAsync();
            return permissoes.Adapt<IEnumerable<PermissaoResponse>>();
        }

        public async Task<CargoResponse> CriarAsync(CriarCargoRequest request, int usuarioLogadoId)
        {
            if (request.ComercioID is null)
            {
                await _autorizacaoGlobalService.GarantirAdministradorGlobalAsync(usuarioLogadoId);

                var nomeBaseEmUso = await _cargoRepository.ExisteNomeAsync(request.Nome, null);
                if (nomeBaseEmUso)
                    throw new BadRequestException("Já existe um cargo base com esse nome.");

                var permissoesBase = await _permissaoRepository.GetByIdsAsync(request.PermissaoIds);
                var cargoBase = new Cargo
                {
                    Nome = request.Nome,
                    ComercioID = null,
                    Ativo = true,
                    CargoPermissoes = permissoesBase.Select(p => new CargoPermissao { PermissaoID = p.PermissaoID }).ToList()
                };

                await _cargoRepository.AddAsync(cargoBase);
                await _unitOfWork.SaveChangesAsync();

                return cargoBase.Adapt<CargoResponse>();
            }

            var comercioId = request.ComercioID.Value;

            var vinculoChamador = await _usuarioComercioRepository.GetVinculoAsync(usuarioLogadoId, comercioId);
            var chamadorEhAdmin = await _autorizacaoCargoService.EhAdminGlobalAsync(usuarioLogadoId);
            var chamadorEhProprietario = vinculoChamador?.Cargo.EhProprietario ?? false;
            var permissoesChamador = vinculoChamador?.Cargo.CargoPermissoes.Select(cp => cp.Permissao.Chave) ?? [];

            if (!chamadorEhAdmin && vinculoChamador == null)
                throw new ForbiddenException("Você não tem permissão para criar cargos neste comércio.");

            _autorizacaoCargoService.GarantirPermissao(chamadorEhAdmin, chamadorEhProprietario, permissoesChamador,
                PermissaoConstantes.GerenciarCargos, "Você não tem permissão para criar cargos neste comércio.");

            var nomeEmUso = await _cargoRepository.ExisteNomeAsync(request.Nome, comercioId);
            if (nomeEmUso)
                throw new BadRequestException("Já existe um cargo com esse nome neste comércio.");

            var permissoes = await _permissaoRepository.GetByIdsAsync(request.PermissaoIds);
            var chavesRequisitadas = permissoes.Select(p => p.Chave);

            _autorizacaoCargoService.GarantirSemEscalonamento(chamadorEhAdmin, chamadorEhProprietario, permissoesChamador, chavesRequisitadas,
                "Você só pode conceder permissões que você mesmo possui.");

            var cargo = new Cargo
            {
                Nome = request.Nome,
                ComercioID = comercioId,
                Ativo = true,
                CargoPermissoes = permissoes.Select(p => new CargoPermissao { PermissaoID = p.PermissaoID }).ToList()
            };

            await _cargoRepository.AddAsync(cargo);
            await _unitOfWork.SaveChangesAsync();

            return cargo.Adapt<CargoResponse>();
        }

        public async Task<CargoResponse> AtualizarAsync(int id, AtualizarCargoRequest request, int usuarioLogadoId)
        {
            var cargo = await _cargoRepository.GetComPermissoesAsync(id);
            if (cargo == null)
                throw new NotFoundException("Cargo não encontrado.");

            _autorizacaoCargoService.GarantirNaoProprietario(false, cargo.EhProprietario, "O cargo de proprietário não pode ser alterado.");

            IEnumerable<string> permissoesChamador = [];
            bool chamadorEhAdmin;
            bool chamadorEhProprietario = false;

            if (cargo.ComercioID == null)
            {
                await _autorizacaoGlobalService.GarantirAdministradorGlobalAsync(usuarioLogadoId);
                chamadorEhAdmin = true;

                if (cargo.Reservado)
                    throw new ForbiddenException("Este cargo é reservado pelo sistema e não pode ser alterado.");
            }
            else
            {
                var vinculoChamador = await _usuarioComercioRepository.GetVinculoAsync(usuarioLogadoId, cargo.ComercioID.Value);
                chamadorEhAdmin = await _autorizacaoCargoService.EhAdminGlobalAsync(usuarioLogadoId);
                chamadorEhProprietario = vinculoChamador?.Cargo.EhProprietario ?? false;
                permissoesChamador = vinculoChamador?.Cargo.CargoPermissoes.Select(cp => cp.Permissao.Chave) ?? [];

                if (!chamadorEhAdmin && vinculoChamador == null)
                    throw new ForbiddenException("Você não tem permissão para editar cargos deste comércio.");

                _autorizacaoCargoService.GarantirPermissao(chamadorEhAdmin, chamadorEhProprietario, permissoesChamador,
                    PermissaoConstantes.GerenciarCargos, "Você não tem permissão para editar cargos deste comércio.");
            }

            if (!string.Equals(cargo.Nome, request.Nome, StringComparison.Ordinal))
            {
                var nomeEmUso = await _cargoRepository.ExisteNomeAsync(request.Nome, cargo.ComercioID);
                if (nomeEmUso)
                    throw new BadRequestException(cargo.ComercioID == null
                        ? "Já existe um cargo base com esse nome."
                        : "Já existe um cargo com esse nome neste comércio.");
            }

            var permissoes = await _permissaoRepository.GetByIdsAsync(request.PermissaoIds);
            var chavesRequisitadas = permissoes.Select(p => p.Chave).ToList();

            _autorizacaoCargoService.GarantirSemEscalonamento(chamadorEhAdmin, chamadorEhProprietario, permissoesChamador, chavesRequisitadas,
                "Você só pode conceder permissões que você mesmo possui.");

            cargo.Nome = request.Nome;
            cargo.CargoPermissoes = permissoes.Select(p => new CargoPermissao { CargoID = cargo.CargoID, PermissaoID = p.PermissaoID }).ToList();
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

            _autorizacaoCargoService.GarantirNaoProprietario(false, cargo.EhProprietario, "O cargo de proprietário não pode ser desativado.");

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
                var chamadorEhProprietario = vinculoChamador?.Cargo.EhProprietario ?? false;
                var permissoesChamador = vinculoChamador?.Cargo.CargoPermissoes.Select(cp => cp.Permissao.Chave) ?? [];

                if (!chamadorEhAdmin && vinculoChamador == null)
                    throw new ForbiddenException("Você não tem permissão para desativar cargos deste comércio.");

                _autorizacaoCargoService.GarantirPermissao(chamadorEhAdmin, chamadorEhProprietario, permissoesChamador,
                    PermissaoConstantes.GerenciarCargos, "Você não tem permissão para desativar cargos deste comércio.");
            }

            cargo.Ativo = false;
            cargo.AtualizadoEm = DateTime.UtcNow;

            await _cargoRepository.UpdateAsync(cargo);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
