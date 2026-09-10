using Mapster;
using ninx.Communication;
using ninx.Communication.Response;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class CategoriaProdutoService : ICategoriaProdutoService
    {
        private readonly ICategoriaProdutoRepository _categoriaProdutoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CategoriaProdutoService(
            ICategoriaProdutoRepository categoriaProdutoRepository,
            IUnitOfWork unitOfWork)
        {
            _categoriaProdutoRepository = categoriaProdutoRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedResponse<CategoriaProdutoResponse>> GetAllAsync(int comercioId, PaginationRequest request)
        {
            var (entidades, total) = await _categoriaProdutoRepository.GetByComercioIdPaginatedAsync(comercioId, request.PageNumber, request.PageSize);
            var listaResponse = entidades.Adapt<List<CategoriaProdutoResponse>>();

            return new PaginatedResponse<CategoriaProdutoResponse>(
                listaResponse,
                request.PageNumber,
                request.PageSize,
                total
            );
        }

        public async Task<CategoriaProdutoResponse> GetByIdAsync(int categoriaId, int comercioId)
        {
            var categoria = await _categoriaProdutoRepository.GetByIdAndComercioIdAsync(categoriaId, comercioId);
            if (categoria == null)
                throw new NotFoundException($"Categoria com ID {categoriaId} n�o encontrada.");

            return categoria.Adapt<CategoriaProdutoResponse>();
        }

        public async Task<CategoriaProdutoResponse> CreateAsync(CategoriaProdutoRequest request, int comercioId)
        {
            var categoria = new CategoriaProduto
            {
                Nome = request.Nome,
                ComercioID = comercioId
            };

            var novaCategoria = await _categoriaProdutoRepository.AddAsync(categoria);
            await _unitOfWork.SaveChangesAsync();

            return novaCategoria.Adapt<CategoriaProdutoResponse>();
        }

        public async Task<CategoriaProdutoResponse> UpdateAsync(int categoriaId, CategoriaProdutoRequest request, int comercioId)
        {
            var categoria = await _categoriaProdutoRepository.GetByIdAndComercioIdAsync(categoriaId, comercioId);
            if (categoria == null)
                throw new NotFoundException($"Categoria com ID {categoriaId} n�o encontrada.");

            categoria.Nome = request.Nome;

            var categoriaAtualizada = await _categoriaProdutoRepository.UpdateAsync(categoria);
            await _unitOfWork.SaveChangesAsync();

            return categoriaAtualizada.Adapt<CategoriaProdutoResponse>();
        }

        public async Task DeleteAsync(int categoriaId, int comercioId)
        {
            var categoria = await _categoriaProdutoRepository.GetByIdAndComercioIdAsync(categoriaId, comercioId);
            if (categoria == null)
                throw new NotFoundException($"Categoria com ID {categoriaId} n�o encontrada.");

            if (await _categoriaProdutoRepository.ExisteProdutoVinculadoAsync(categoriaId))
                throw new BadRequestException("N�o � poss�vel excluir uma categoria que possui produtos vinculados.");

            await _categoriaProdutoRepository.DeleteAsync(categoria);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
