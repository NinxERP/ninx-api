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

        public async Task<PaginatedResponse<CategoriaProdutoResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var categorias = await _categoriaProdutoRepository.GetAllAsync();
            var totalRecords = categorias.Count();

            var paginatedData = categorias
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var data = paginatedData.Adapt<List<CategoriaProdutoResponse>>();
            return new PaginatedResponse<CategoriaProdutoResponse>(data, pageNumber, pageSize, totalRecords);
        }

        public async Task<CategoriaProdutoResponse> GetByIdAsync(int categoriaId)
        {
            var categoria = await _categoriaProdutoRepository.GetByIdAsync(categoriaId);
            if (categoria == null)
                throw new NotFoundException($"Categoria com ID {categoriaId} não encontrada.");

            return categoria.Adapt<CategoriaProdutoResponse>();
        }

        public async Task<CategoriaProdutoResponse> CreateAsync(CategoriaProdutoRequest request)
        {
            var categoria = new CategoriaProduto
            {
                Nome = request.Nome
            };

            var novaCategoria = await _categoriaProdutoRepository.AddAsync(categoria);
            await _unitOfWork.SaveChangesAsync();

            return novaCategoria.Adapt<CategoriaProdutoResponse>();
        }

        public async Task<CategoriaProdutoResponse> UpdateAsync(int categoriaId, CategoriaProdutoRequest request)
        {
            var categoria = await _categoriaProdutoRepository.GetByIdAsync(categoriaId);
            if (categoria == null)
                throw new NotFoundException($"Categoria com ID {categoriaId} não encontrada.");

            categoria.Nome = request.Nome;

            var categoriaAtualizada = await _categoriaProdutoRepository.UpdateAsync(categoria);
            await _unitOfWork.SaveChangesAsync();

            return categoriaAtualizada.Adapt<CategoriaProdutoResponse>();
        }

        public async Task DeleteAsync(int categoriaId)
        {
            var categoria = await _categoriaProdutoRepository.GetByIdAsync(categoriaId);
            if (categoria == null)
                throw new NotFoundException($"Categoria com ID {categoriaId} não encontrada.");

            _categoriaProdutoRepository.Delete(categoria);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
