using Mapster;
using ninx.Communication;
using ninx.Communication;
using ninx.Communication.Response;
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
            var (entidades, total) = await _estoqueRepository.GetPaginatedAsync(request.PageNumber, request.PageSize);
            entidades = entidades.Where(e => e.ComercioID == comercioId).ToList();
            var listaResponse = entidades.Adapt<List<EstoqueResponse>>();

            return new PaginatedResponse<EstoqueResponse>(
                listaResponse,
                request.PageNumber,
                request.PageSize,
                total
            );
        }

        public async Task<EstoqueResponse> GetByIdAsync(int estoqueId)
        {
            var estoque = await _estoqueRepository.GetByIdAsync(estoqueId);
            if (estoque == null)
                throw new NotFoundException($"Estoque com ID {estoqueId} não encontrado.");

            return estoque.Adapt<EstoqueResponse>();
        }

        public async Task<EstoqueResponse> CreateAsync(EstoqueRequest request, int comercioId)
        {
            var comercio = await _comercioRepository.GetByIdAsync(comercioId);
            if (comercio == null)
                throw new NotFoundException($"Comércio com ID {comercioId} não encontrado.");

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
                throw new NotFoundException($"Estoque com ID {estoqueId} não encontrado.");

            if (estoque.ComercioID != comercioId)
                throw new ForbiddenException("Você não tem permissão para atualizar este estoque.");

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
                throw new NotFoundException($"Estoque com ID {estoqueId} não encontrado.");

            if (estoque.ComercioID != comercioId)
                throw new ForbiddenException("Você não tem permissão para deletar este estoque.");

            _estoqueRepository.Delete(estoque);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
