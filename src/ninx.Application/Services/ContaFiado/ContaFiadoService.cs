using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public interface IContaFiadoService
    {
        Task<ContaFiadoResponse> ObterAsync(int clienteId, int comercioId);
        Task<PessoaAutorizadaResponse> AdicionarAutorizadoAsync(int clienteId, PessoaAutorizadaRequest request, int comercioId);
        Task RevogarAutorizadoAsync(int clienteId, int pessoaAutorizadaId, int comercioId);
        Task<TermoAberturaGeradoResponse> GerarTermoAberturaAsync(int clienteId, int comercioId);

        /// <summary>
        /// Gera a próxima versão do termo sem abrir transação, para quem já está dentro de uma
        /// (cadastro e edição do cliente). <paramref name="novoLimite"/> é o limite que a versão
        /// concede; sem ele, vale o de uma versão ainda pendente ou, na falta dela, o vigente.
        /// </summary>
        Task<Guid> GerarTermoAberturaNaTransacaoAsync(Cliente cliente, Comercio comercio, decimal? novoLimite = null);

        /// <summary>
        /// Efeitos da assinatura do termo de abertura. Não salva: a confirmação da assinatura
        /// grava tudo na mesma operação.
        /// </summary>
        Task EfetivarTermoAssinadoAsync(AssinaturaEletronica assinatura, DateTime dataAssinatura);
    }

    public class ContaFiadoService : IContaFiadoService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IComercioRepository _comercioRepository;
        private readonly IPessoaAutorizadaRepository _pessoaAutorizadaRepository;
        private readonly ITermoAberturaContaRepository _termoRepository;
        private readonly IAssinaturaEletronicaRepository _assinaturaRepository;
        private readonly IVendaRepository _vendaRepository;
        private readonly IDocumentoRendererService _documentoRendererService;
        private readonly IUnitOfWork _unitOfWork;

        public ContaFiadoService(
            IClienteRepository clienteRepository,
            IComercioRepository comercioRepository,
            IPessoaAutorizadaRepository pessoaAutorizadaRepository,
            ITermoAberturaContaRepository termoRepository,
            IAssinaturaEletronicaRepository assinaturaRepository,
            IVendaRepository vendaRepository,
            IDocumentoRendererService documentoRendererService,
            IUnitOfWork unitOfWork)
        {
            _vendaRepository = vendaRepository;
            _clienteRepository = clienteRepository;
            _comercioRepository = comercioRepository;
            _pessoaAutorizadaRepository = pessoaAutorizadaRepository;
            _termoRepository = termoRepository;
            _assinaturaRepository = assinaturaRepository;
            _documentoRendererService = documentoRendererService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ContaFiadoResponse> ObterAsync(int clienteId, int comercioId)
        {
            var cliente = await GetClienteDoComercioAsync(clienteId, comercioId);

            var termos = await _termoRepository.GetPorClienteAsync(clienteId);
            var guids = await _assinaturaRepository.GetGuidsPorTermosAberturaAsync(termos.Select(t => t.TermoAberturaID).ToList());
            var ativo = termos.FirstOrDefault(t => t.Status == StatusTermoAbertura.Ativo);
            var pendente = termos.FirstOrDefault(t => t.Status == StatusTermoAbertura.Aguardando);
            var autorizados = await _pessoaAutorizadaRepository.GetPorClienteAsync(clienteId);
            var saldos = await _vendaRepository.GetSaldoDevedorPorAutorizadoAsync(clienteId);

            return new ContaFiadoResponse
            {
                ClienteID = clienteId,
                TermoAtivo = ativo != null,
                LimiteCredito = cliente.LimiteCredito,
                LimitePendente = pendente != null && pendente.LimiteCredito != cliente.LimiteCredito ? pendente.LimiteCredito : null,
                TermoAssinadoEm = ativo?.AssinadoEm,
                DocumentoGuidTermoAtivo = ativo != null && guids.TryGetValue(ativo.TermoAberturaID, out var guidAtivo) ? guidAtivo : null,
                DocumentoGuidTermoPendente = pendente != null && guids.TryGetValue(pendente.TermoAberturaID, out var guidPendente) ? guidPendente : null,
                PrecisaNovoTermo = ativo == null || autorizados.Any(p => !p.RevogadaEm.HasValue && (!p.AutorizadaEm.HasValue || p.RevogacaoSolicitadaEm.HasValue)),
                Autorizados = autorizados.Select(p => ParaResponse(p, saldos.GetValueOrDefault(p.PessoaAutorizadaID))).ToList(),
                Termos = termos.Select(t => new TermoAberturaResumoResponse
                {
                    Versao = t.Versao,
                    LimiteCredito = t.LimiteCredito,
                    Status = t.Status.ToString(),
                    CriadoEm = t.CriadoEm,
                    AssinadoEm = t.AssinadoEm,
                    DocumentoGuid = guids.TryGetValue(t.TermoAberturaID, out var guid) ? guid : null
                }).ToList()
            };
        }

        public async Task<PessoaAutorizadaResponse> AdicionarAutorizadoAsync(int clienteId, PessoaAutorizadaRequest request, int comercioId)
        {
            await GetClienteDoComercioAsync(clienteId, comercioId);

            var pessoa = new PessoaAutorizada
            {
                ClienteID = clienteId,
                Nome = request.Nome.Trim(),
                Cpf = string.IsNullOrWhiteSpace(request.Cpf) ? null : new string(request.Cpf.Where(char.IsDigit).ToArray()),
                Parentesco = (ParentescoAutorizado)request.Parentesco,
                MenorDeIdade = request.MenorDeIdade,
                LimiteCredito = request.LimiteCredito,
                CriadoEm = DateTime.UtcNow
            };

            await _pessoaAutorizadaRepository.AddAsync(pessoa);
            await _unitOfWork.SaveChangesAsync();
            return ParaResponse(pessoa, 0);
        }

        public async Task RevogarAutorizadoAsync(int clienteId, int pessoaAutorizadaId, int comercioId)
        {
            await GetClienteDoComercioAsync(clienteId, comercioId);

            var pessoa = await _pessoaAutorizadaRepository.GetDoClienteAsync(pessoaAutorizadaId, clienteId)
                ?? throw new NotFoundException("Pessoa autorizada não encontrada.");
            if (pessoa.RevogadaEm.HasValue || pessoa.RevogacaoSolicitadaEm.HasValue)
                throw new BadRequestException("A revogação desta autorização já foi solicitada.");

            // Como a inclusão, a revogação só vale depois que o titular assina a próxima versão do
            // termo, que deixa de listar a pessoa. Compras anteriores continuam válidas.
            pessoa.RevogacaoSolicitadaEm = DateTime.UtcNow;
            await _pessoaAutorizadaRepository.UpdateAsync(pessoa);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<TermoAberturaGeradoResponse> GerarTermoAberturaAsync(int clienteId, int comercioId)
        {
            var cliente = await GetClienteDoComercioAsync(clienteId, comercioId);
            var comercio = await _comercioRepository.GetByIdAsync(comercioId)
                ?? throw new NotFoundException("Comércio não encontrado.");

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var guid = await GerarTermoAberturaNaTransacaoAsync(cliente, comercio);
                await _unitOfWork.CommitAsync();
                return new TermoAberturaGeradoResponse { DocumentoGuid = guid };
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<Guid> GerarTermoAberturaNaTransacaoAsync(Cliente cliente, Comercio comercio, decimal? novoLimite = null)
        {
            var agora = DateTime.UtcNow;
            var termos = await _termoRepository.GetPorClienteAsync(cliente.ClienteID);

            // Uma alteração de limite ainda não assinada não se perde quando outra versão é gerada.
            var limite = novoLimite
                ?? termos.FirstOrDefault(t => t.Status == StatusTermoAbertura.Aguardando)?.LimiteCredito
                ?? cliente.LimiteCredito;

            // Só uma versão pode esperar assinatura: gerar outra cancela a anterior, que continua
            // guardada no histórico com seu documento.
            foreach (var antigo in termos.Where(t => t.Status == StatusTermoAbertura.Aguardando))
            {
                antigo.Status = StatusTermoAbertura.Cancelado;
                await _termoRepository.UpdateAsync(antigo);
                await _assinaturaRepository.CancelarPorTermoAberturaIdAsync(antigo.TermoAberturaID, agora);
            }

            var termo = new TermoAberturaConta
            {
                ClienteID = cliente.ClienteID,
                Versao = termos.Count == 0 ? 1 : termos.Max(t => t.Versao) + 1,
                LimiteCredito = limite,
                Status = StatusTermoAbertura.Aguardando,
                CriadoEm = agora
            };
            await _termoRepository.AddAsync(termo);

            var pessoas = await _pessoaAutorizadaRepository.GetPorClienteAsync(cliente.ClienteID);
            var autorizados = pessoas.Where(p => !p.RevogadaEm.HasValue && !p.RevogacaoSolicitadaEm.HasValue);
            var revogados = pessoas.Where(p => !p.RevogadaEm.HasValue && p.RevogacaoSolicitadaEm.HasValue && p.AutorizadaEm.HasValue);
            var tokens = DocumentoTokenBuilder.BuildTermoAberturaTokens(cliente, comercio, termo.Versao, limite, autorizados, revogados, agora);
            var html = await _documentoRendererService.RenderizarHtmlAsync(TipoDocumento.TermoAberturaConta, tokens);
            var pdf = await _documentoRendererService.ConverterParaPdfBase64Async(html);

            var guid = Guid.NewGuid();
            await _assinaturaRepository.AddAsync(new AssinaturaEletronica
            {
                TermoAbertura = termo,
                DocumentoGuid = guid,
                TipoDocumento = TipoDocumento.TermoAberturaConta,
                DocumentoHtmlMesclado = html,
                DocumentoOriginalBase64 = pdf,
                HashDocumentoOriginal = HashDocumento.Sha256Hex(pdf),
                CriadoEm = agora
            });

            await _unitOfWork.SaveChangesAsync();
            return guid;
        }

        public async Task EfetivarTermoAssinadoAsync(AssinaturaEletronica assinatura, DateTime dataAssinatura)
        {
            var termo = await _termoRepository.GetByIdAsync(assinatura.TermoAberturaID!.Value)
                ?? throw new NotFoundException("Termo de abertura não encontrado.");
            if (termo.Status != StatusTermoAbertura.Aguardando)
                throw new BadRequestException("Este termo foi substituído por um mais novo e não pode mais ser assinado.");

            var anterior = await _termoRepository.GetAtivoAsync(termo.ClienteID);
            if (anterior != null)
            {
                anterior.Status = StatusTermoAbertura.Substituido;
                await _termoRepository.UpdateAsync(anterior);
            }

            termo.Status = StatusTermoAbertura.Ativo;
            termo.AssinadoEm = dataAssinatura;
            await _termoRepository.UpdateAsync(termo);

            // O limite do cliente é o da versão assinada: alterá-lo também passa pela assinatura.
            var cliente = await _clienteRepository.GetByIdAsync(termo.ClienteID)
                ?? throw new NotFoundException("Cliente não encontrado.");
            if (cliente.LimiteCredito != termo.LimiteCredito)
            {
                cliente.LimiteCredito = termo.LimiteCredito;
                cliente.AtualizadoEm = dataAssinatura;
                await _clienteRepository.UpdateAsync(cliente);
            }

            // Aplica exatamente o que o documento assinado mostra: inclusões e revogações pedidas
            // depois de gerada esta versão não estão nela e esperam a próxima.
            foreach (var pessoa in await _pessoaAutorizadaRepository.GetPorClienteAsync(termo.ClienteID))
            {
                if (pessoa.RevogadaEm.HasValue || pessoa.CriadoEm > termo.CriadoEm)
                    continue;

                if (pessoa.RevogacaoSolicitadaEm <= termo.CriadoEm)
                    pessoa.RevogadaEm = dataAssinatura;
                else if (!pessoa.AutorizadaEm.HasValue)
                    pessoa.AutorizadaEm = dataAssinatura;
                else
                    continue;

                await _pessoaAutorizadaRepository.UpdateAsync(pessoa);
            }
        }

        private async Task<Cliente> GetClienteDoComercioAsync(int clienteId, int comercioId)
        {
            return await _clienteRepository.GetByIdAndComercioIdAsync(clienteId, comercioId)
                ?? throw new NotFoundException("Cliente não encontrado.");
        }

        private static PessoaAutorizadaResponse ParaResponse(PessoaAutorizada p, decimal saldoDevedor) => new()
        {
            PessoaAutorizadaID = p.PessoaAutorizadaID,
            Nome = p.Nome,
            Cpf = p.Cpf,
            Parentesco = (int)p.Parentesco,
            MenorDeIdade = p.MenorDeIdade,
            LimiteCredito = p.LimiteCredito,
            SaldoDevedor = saldoDevedor,
            SaldoDisponivel = p.LimiteCredito.HasValue ? p.LimiteCredito.Value - saldoDevedor : null,
            CriadoEm = p.CriadoEm,
            AutorizadaEm = p.AutorizadaEm,
            RevogacaoSolicitadaEm = p.RevogacaoSolicitadaEm,
            RevogadaEm = p.RevogadaEm,
            Situacao = p.RevogadaEm.HasValue ? "Revogada"
                : p.RevogacaoSolicitadaEm.HasValue ? "Revogação pendente"
                : p.AutorizadaEm.HasValue ? "Autorizada" : "Pendente"
        };
    }
}
