using Mapster;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class EstoqueService : IEstoqueService
    {
        private readonly IEstoqueRepository _estoqueRepository;
        private readonly IComercioRepository _comercioRepository;
        private readonly IUnitOfWork _unitOfWork;

        public EstoqueService(
            IEstoqueRepository estoqueRepository,
            IComercioRepository comercioRepository,
            IUnitOfWork unitOfWork)
        {
            _estoqueRepository = estoqueRepository;
            _comercioRepository = comercioRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedResponse<EstoqueResponse>> GetAllByComercioIdAsync(int comercioId, PaginationRequest request)
        {
            var (entidades, total) = await _estoqueRepository.GetByComercioIdPaginatedAsync(comercioId, request.PageNumber, request.PageSize);
            var listaResponse = entidades.Adapt<List<EstoqueResponse>>();

            return new PaginatedResponse<EstoqueResponse>(
                listaResponse,
                request.PageNumber,
                request.PageSize,
                total
            );
        }

        public async Task<EstoqueResponse> GetByIdAsync(int estoqueId, int comercioId)
        {
            var estoque = await _estoqueRepository.GetByIdAsync(estoqueId);
            if (estoque == null)
                throw new NotFoundException($"Estoque com ID {estoqueId} n�o encontrado.");

            if (estoque.ComercioID != comercioId)
                throw new NotFoundException($"Estoque com ID {estoqueId} n�o encontrado.");

            return estoque.Adapt<EstoqueResponse>();
        }

        public async Task<EstoqueResponse> CreateAsync(EstoqueRequest request, int comercioId)
        {
            var comercio = await _comercioRepository.GetByIdAsync(comercioId);
            if (comercio == null)
                throw new NotFoundException($"Com�rcio com ID {comercioId} n�o encontrado.");

            var estoque = new Estoque
            {
                ComercioID = comercioId,
                ProdutoID = request.ProdutoID,
                Quantidade = request.Quantidade,
                QuantidadeMinima = request.QuantidadeMinima,
                UltimaAtualizacao = DateTime.UtcNow
            };

            var novoEstoque = await _estoqueRepository.AddAsync(estoque);
            await _unitOfWork.SaveChangesAsync();

            return novoEstoque.Adapt<EstoqueResponse>();
        }

        public async Task<EstoqueResponse> UpdateAsync(int estoqueId, EstoqueRequest request, int comercioId)
        {
            var estoque = await _estoqueRepository.GetByIdAsync(estoqueId);
            if (estoque == null)
                throw new NotFoundException($"Estoque com ID {estoqueId} n�o encontrado.");

            if (estoque.ComercioID != comercioId)
                throw new ForbiddenException("Voc� n�o tem permiss�o para atualizar este estoque.");

            if (request.Quantidade < 0)
                throw new BadRequestException("A quantidade em estoque n�o pode ser negativa.");

            estoque.Quantidade = request.Quantidade;
            estoque.QuantidadeMinima = request.QuantidadeMinima;
            estoque.UltimaAtualizacao = DateTime.UtcNow;

            var estoqueAtualizado = await _estoqueRepository.UpdateAsync(estoque);
            await _unitOfWork.SaveChangesAsync();

            return estoqueAtualizado.Adapt<EstoqueResponse>();
        }

        public async Task DeleteAsync(int estoqueId, int comercioId)
        {
            var estoque = await _estoqueRepository.GetByIdAsync(estoqueId);
            if (estoque == null)
                throw new NotFoundException($"Estoque com ID {estoqueId} n�o encontrado.");

            if (estoque.ComercioID != comercioId)
                throw new ForbiddenException("Voc� n�o tem permiss�o para deletar este estoque.");

            await _estoqueRepository.DeleteAsync(estoque);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
