using Mapster;
using ninx.Application.Interfaces.Services;
using ninx.Communication.Request;
using ninx.Communication.Response;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces.Repositories;

public class ComercioService : IComercioService
{
    private readonly IComercioRepository _comercioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioComercioRepository _usuarioComercioRepository; 

    public ComercioService(IComercioRepository comercioRepository, IUnitOfWork unitOfWork, IUsuarioComercioRepository usuarioComercio)
    {
        _comercioRepository = comercioRepository;
        _unitOfWork = unitOfWork;
        _usuarioComercioRepository = usuarioComercio;
    }

    public async Task<IEnumerable<ComercioResponse>> GetAll()
    {
        var comercios = await _comercioRepository.GetAllAsync();
        return comercios.Adapt<IEnumerable<ComercioResponse>>();
    }

    public async Task<IEnumerable<ComercioResponse>> GetByUsuarioId(int usuarioId)
    {
        var comercios = await _comercioRepository.GetByUsuarioId(usuarioId);
        return comercios.Adapt<IEnumerable<ComercioResponse>>();
    }

    public async Task<ComercioResponse> CriarAsync(ComercioRequest request)
    {
        var comercio = request.Adapt<Comercio>();

        await _comercioRepository.AddAsync(comercio);
        await _unitOfWork.SaveChangesAsync();

        return comercio.Adapt<ComercioResponse>();
    }

    public async Task<ComercioResponse> AtualizarAsync(int id, int usuarioLogadoId, ComercioRequest request)
    {
        var comercio = await _comercioRepository.GetByIdAsync(id);

        if (comercio == null)
            throw new NotFoundException("Comércio não encontrado.");

        var usuarioComercio = await _usuarioComercioRepository.GetByComercioIdAsync(comercio.ComercioID);
        var vinculoLogado = usuarioComercio.FirstOrDefault(x => x.UsuarioID == usuarioLogadoId);
        if (vinculoLogado == null || (vinculoLogado.Permissao != Permissao.Admin && vinculoLogado.Permissao != Permissao.Dono))
        {
            throw new UnauthorizedException("Acesso negado.");
        }

        request.Adapt(comercio);
        comercio.AtualizadoEm = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return comercio.Adapt<ComercioResponse>();
    }

    public async Task DesativarAsync(int id)
    {
        var comercio = await _comercioRepository.GetByIdAsync(id);
        if (comercio == null)
            throw new NotFoundException("Comércio não encontrado.");

        comercio.Ativo = false;
        comercio.AtualizadoEm = DateTime.UtcNow;

        await _comercioRepository.UpdateAsync(comercio);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ComercioResponse> GetByIdAsync(int id)
    {
        var comercio = await _comercioRepository.GetByIdAsync(id);
        if (comercio == null)
            throw new NotFoundException("Comércio não encontrado.");

        return comercio.Adapt<ComercioResponse>();
    }
}