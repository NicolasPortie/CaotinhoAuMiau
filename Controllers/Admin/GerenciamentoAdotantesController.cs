using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Models.ViewModels.Admin;
using CaotinhoAuMiau.Models.Enums;
using CaotinhoAuMiau.Utils;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CaotinhoAuMiau.Controllers.Admin
{
    [Route("admin/usuarios")]
    [Authorize(Roles = "Administrador")]
    public class GerenciamentoAdotantesController : Controller
    {
        private readonly ApplicationDbContext _contexto;
        private readonly ILogger<GerenciamentoAdotantesController> _logger;

        public GerenciamentoAdotantesController(ApplicationDbContext contexto, ILogger<GerenciamentoAdotantesController> logger)
        {
            _contexto = contexto;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string filtroStatus = "todos",
            string filtroTipo = "todos", 
            string pesquisa = "",
            int pagina = 1,
            int itensPorPagina = 20)
        {
            try
            {
                var query = _contexto.Usuarios
                    .Include(u => u.FormulariosAdocao)
                    .Include(u => u.Adocoes)
                    .Where(u => u.FormulariosAdocao.Any(f => f.StatusEnum == StatusFormulario.Aprovado))
                    .AsQueryable();

                if (!string.IsNullOrEmpty(pesquisa))
                {
                    query = query.Where(u => u.Nome.Contains(pesquisa) || 
                                           u.Email.Contains(pesquisa) || 
                                           u.CPF.Contains(pesquisa));
                }

                if (filtroStatus != "todos")
                {
                    switch (filtroStatus)
                    {
                        case "ativo":
                            query = query.Where(u => u.Ativo);
                            break;
                        case "inativo":
                            query = query.Where(u => !u.Ativo);
                            break;
                        case "quarentena":
                            query = query.Where(u => u.EmQuarentena);
                            break;
                        case "violacoes":
                            query = query.Where(u => u.NumeroViolacoes > 0);
                            break;
                    }
                }

                var baseQuery = _contexto.Usuarios.Where(u => u.FormulariosAdocao.Any(f => f.StatusEnum == StatusFormulario.Aprovado));
                
                var totalUsuarios = await baseQuery.CountAsync();
                var usuariosAtivos = await baseQuery.CountAsync(u => u.Ativo);
                var usuariosQuarentena = await baseQuery.CountAsync(u => u.EmQuarentena);
                var usuariosViolacoes = await baseQuery.CountAsync(u => u.NumeroViolacoes > 0);

                var totalItens = await query.CountAsync();
                var totalPaginas = (int)Math.Ceiling((double)totalItens / itensPorPagina);
                
                var usuarios = await query
                    .OrderByDescending(u => u.DataCadastro)
                    .Skip((pagina - 1) * itensPorPagina)
                    .Take(itensPorPagina)
                    .ToListAsync();

                var viewModel = new GerenciamentoUsuariosViewModel
                {
                    Usuarios = usuarios.Select(u => new UsuarioResumoViewModel
                    {
                        Id = u.Id,
                        Nome = u.Nome,
                        Email = u.Email,
                        CPF = u.CPF,
                        Telefone = u.Telefone,
                        Ativo = u.Ativo,
                        DataCadastro = u.DataCadastro,
                        UltimoAcesso = u.UltimoAcesso,
                        FotoPerfil = u.FotoPerfil,
                        EmQuarentena = u.EmQuarentena,
                        FimQuarentena = u.FimQuarentena,
                        NumeroViolacoes = u.NumeroViolacoes,
                        TotalAdocoes = u.Adocoes.Count,
                        AdocoesFinalizadas = u.Adocoes.Count(a => a.Status == StatusAdocao.Finalizado),
                        Cidade = u.Cidade,
                        Estado = u.Estado,
                        EnderecoCompleto = $"{u.Logradouro}, {u.Numero}" + 
                                         (!string.IsNullOrEmpty(u.Complemento) ? $", {u.Complemento}" : "") + 
                                         $" - {u.Bairro}, {u.Cidade}/{u.Estado} - CEP: {u.CEP}"
                    }).ToList(),
                    
                    FiltroStatus = filtroStatus,
                    FiltroTipo = filtroTipo,
                    Pesquisa = pesquisa,
                    PaginaAtual = pagina,
                    TotalPaginas = totalPaginas,
                    TotalItens = totalItens,
                    ItensPorPagina = itensPorPagina,
                    
                    TotalUsuarios = totalUsuarios,
                    UsuariosAtivos = usuariosAtivos,
                    UsuariosQuarentena = usuariosQuarentena,
                    UsuariosViolacoes = usuariosViolacoes
                };

                return View("~/Views/Admin/GerenciamentoAdotantes.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(_logger, ex, "Erro ao carregar gerenciamento de adotantes");
                
                var erroViewModel = new GerenciamentoUsuariosViewModel
                {
                    Usuarios = new List<UsuarioResumoViewModel>(),
                    FiltroStatus = filtroStatus,
                    FiltroTipo = filtroTipo,
                    Pesquisa = pesquisa,
                    PaginaAtual = pagina,
                    TotalPaginas = 1,
                    TotalItens = 0,
                    ItensPorPagina = itensPorPagina,
                    TotalUsuarios = 0,
                    UsuariosAtivos = 0,
                    UsuariosQuarentena = 0,
                    UsuariosViolacoes = 0
                };
                
                TempData["Erro"] = ErrorHandler.GerarMensagemErroUsuario(ex);
                return View(erroViewModel);
            }
        }

        [HttpPost("aplicar-quarentena")]
        public async Task<IActionResult> AplicarQuarentenaAsync([FromBody] SolicitacaoQuarentenaUsuario request)
        {
            try
            {
                var usuario = await _contexto.Usuarios.FindAsync(request.UsuarioId);
                if (usuario == null)
                {
                    return Json(new { sucesso = false, mensagem = "Usuário não encontrado." });
                }

                usuario.EmQuarentena = true;
                usuario.InicioQuarentena = DateTime.Now;
                usuario.FimQuarentena = DateTime.Now.AddDays(request.DiasDuracao);
                usuario.MotivoQuarentena = request.Motivo;
                usuario.NumeroViolacoes += 1;
                usuario.DataUltimaBloqueio = DateTime.Now;

                await _contexto.SaveChangesAsync();

                _logger.LogInformation("Quarentena aplicada ao usuário {UsuarioId} por {Dias} dias. Motivo: {Motivo}", 
                    request.UsuarioId, request.DiasDuracao, request.Motivo);

                return Json(new { sucesso = true, mensagem = "Quarentena aplicada com sucesso!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao aplicar quarentena ao usuário {UsuarioId}", request.UsuarioId);
                return Json(new { sucesso = false, mensagem = "Erro interno no servidor." });
            }
        }

        [HttpPost("remover-quarentena")]
        public async Task<IActionResult> RemoverQuarentenaAsync([FromBody] SolicitacaoRemocaoQuarentena request)
        {
            try
            {
                var usuario = await _contexto.Usuarios.FindAsync(request.UsuarioId);
                if (usuario == null)
                {
                    return Json(new { sucesso = false, mensagem = "Usuário não encontrado." });
                }

                usuario.EmQuarentena = false;
                usuario.FimQuarentena = DateTime.Now;
                usuario.JustificativaRemocaoQuarentena = request.Justificativa;

                await _contexto.SaveChangesAsync();

                _logger.LogInformation("Quarentena removida do usuário {UsuarioId}. Justificativa: {Justificativa}", 
                    request.UsuarioId, request.Justificativa);

                return Json(new { sucesso = true, mensagem = "Quarentena removida com sucesso!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover quarentena do usuário {UsuarioId}", request.UsuarioId);
                return Json(new { sucesso = false, mensagem = "Erro interno no servidor." });
            }
        }

        [HttpPost("ativar-desativar")]
        public async Task<IActionResult> AtivarDesativarUsuarioAsync([FromBody] SolicitacaoAlteracaoStatusUsuario request)
        {
            try
            {
                var usuario = await _contexto.Usuarios.FindAsync(request.UsuarioId);
                if (usuario == null)
                {
                    return Json(new { sucesso = false, mensagem = "Usuário não encontrado." });
                }

                usuario.Ativo = request.Ativar;
                if (!request.Ativar)
                {
                    usuario.DataUltimaBloqueio = DateTime.Now;
                }

                await _contexto.SaveChangesAsync();

                var acao = request.Ativar ? "ativado" : "desativado";
                _logger.LogInformation("Usuário {UsuarioId} foi {Acao}", request.UsuarioId, acao);

                return Json(new { sucesso = true, mensagem = $"Usuário {acao} com sucesso!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar status do usuário {UsuarioId}", request.UsuarioId);
                return Json(new { sucesso = false, mensagem = "Erro interno no servidor." });
            }
        }

        [HttpPost("observacoes")]
        public async Task<IActionResult> SalvarObservacoesAsync([FromBody] SolicitacaoSalvarObservacoes request)
        {
            try
            {
                var usuario = await _contexto.Usuarios.FindAsync(request.UsuarioId);
                if (usuario == null)
                {
                    return Json(new { sucesso = false, mensagem = "Usuário não encontrado." });
                }

                usuario.ObservacoesAdministrativas = request.Observacoes;
                usuario.DataAtualizacao = DateTime.Now;

                await _contexto.SaveChangesAsync();

                _logger.LogInformation("Observações administrativas salvas para usuário {UsuarioId}", request.UsuarioId);

                return Json(new { sucesso = true, mensagem = "Observações salvas com sucesso!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar observações do usuário {UsuarioId}", request.UsuarioId);
                return Json(new { sucesso = false, mensagem = "Erro interno no servidor." });
            }
        }

        [HttpGet("detalhes/{id}")]
        public async Task<IActionResult> DetalhesUsuarioAsync(int id)
        {
            try
            {
                var usuario = await _contexto.Usuarios
                    .Include(u => u.Adocoes)
                        .ThenInclude(a => a.Pet)
                    .Include(u => u.FormulariosAdocao)
                        .ThenInclude(f => f.Pet)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (usuario == null)
                {
                    TempData["Erro"] = "Usuário não encontrado.";
                    return RedirectToAction("Index");
                }

                var viewModel = new DetalhesUsuarioViewModel
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    CPF = usuario.CPF,
                    Telefone = usuario.Telefone,
                    Ativo = usuario.Ativo,
                    EmailVerificado = usuario.EmailVerificado,
                    DataCadastro = usuario.DataCadastro,
                    UltimoAcesso = usuario.UltimoAcesso,
                    DataNascimento = usuario.DataNascimento,
                    FotoPerfil = usuario.FotoPerfil,
                    
                    CEP = usuario.CEP,
                    Logradouro = usuario.Logradouro,
                    Numero = usuario.Numero,
                    Complemento = usuario.Complemento,
                    Bairro = usuario.Bairro,
                    Cidade = usuario.Cidade,
                    Estado = usuario.Estado,
                    
                    EmQuarentena = usuario.EmQuarentena,
                    InicioQuarentena = usuario.InicioQuarentena,
                    FimQuarentena = usuario.FimQuarentena,
                    MotivoQuarentena = usuario.MotivoQuarentena,
                    JustificativaRemocaoQuarentena = usuario.JustificativaRemocaoQuarentena,
                    ObservacoesAdministrativas = usuario.ObservacoesAdministrativas,
                    DataUltimaBloqueio = usuario.DataUltimaBloqueio,
                    NumeroViolacoes = usuario.NumeroViolacoes,
                    RequererAprovacaoManual = usuario.RequererAprovacaoManual,
                    
                    TotalAdocoes = usuario.Adocoes.Count,
                    AdocoesFinalizadas = usuario.Adocoes.Count(a => a.Status == StatusAdocao.Finalizado),
                    AdocoesCanceladas = usuario.Adocoes.Count(a => a.Status.EstaCancelada()),
                    TotalFormularios = usuario.FormulariosAdocao.Count
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes do usuário {UsuarioId}", id);
                TempData["Erro"] = "Erro ao carregar detalhes do usuário.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("detalhes-modal/{id}")]
        public async Task<IActionResult> DetalhesUsuarioModalAsync(int id, [FromQuery] int adocaoPagina = 1)
        {
            try
            {
                var adminId = User.ObterIdUsuario();
                var adminNome = User.ObterValorClaim(System.Security.Claims.ClaimTypes.Name) ?? "Admin";
                _logger.LogInformation("Admin {AdminId} ({AdminNome}) acessou dados pessoais completos do usuário {UsuarioId} em {DataHora}", 
                    adminId, adminNome, id, DateTime.Now);
                
                var usuario = await _contexto.Usuarios
                    .Include(u => u.Adocoes)
                        .ThenInclude(a => a.Pet)
                    .Include(u => u.FormulariosAdocao)
                        .ThenInclude(f => f.Pet)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (usuario == null)
                {
                    return Json(new { sucesso = false, mensagem = "Usuário não encontrado." });
                }

                const int itensPorPaginaAdocao = 5;
                var adocoesQuery = usuario.Adocoes.AsQueryable().OrderByDescending(a => a.DataEnvio);
                var totalAdocoes = adocoesQuery.Count();
                var adocoesPaginadas = adocoesQuery.Skip((adocaoPagina - 1) * itensPorPaginaAdocao).Take(itensPorPaginaAdocao).ToList();

                var dados = new
                {
                    id = usuario.Id,
                    nome = usuario.Nome,
                    email = usuario.Email,
                    cpf = usuario.CPF,
                    telefone = usuario.Telefone ?? "Não informado",
                    ativo = usuario.Ativo,
                    emailVerificado = usuario.EmailVerificado,
                    dataCadastro = usuario.DataCadastro.ToString("dd/MM/yyyy HH:mm"),
                    ultimoAcesso = usuario.UltimoAcesso?.ToString("dd/MM/yyyy HH:mm") ?? "Nunca acessou",
                    dataNascimento = usuario.DataNascimento.ToString("dd/MM/yyyy"),
                    fotoPerfil = usuario.FotoPerfil,
                    
                    endereco = new
                    {
                        cep = usuario.CEP,
                        logradouro = usuario.Logradouro,
                        numero = usuario.Numero,
                        complemento = usuario.Complemento,
                        bairro = usuario.Bairro,
                        cidade = usuario.Cidade,
                        estado = usuario.Estado,
                        completo = $"{usuario.Logradouro}, {usuario.Numero}" + 
                                 (!string.IsNullOrEmpty(usuario.Complemento) ? $", {usuario.Complemento}" : "") + 
                                 $" - {usuario.Bairro}, {usuario.Cidade}/{usuario.Estado} - CEP: {usuario.CEP}"
                    },
                    
                    administracao = new
                    {
                        emQuarentena = usuario.EmQuarentena,
                        inicioQuarentena = usuario.InicioQuarentena?.ToString("dd/MM/yyyy HH:mm"),
                        fimQuarentena = usuario.FimQuarentena?.ToString("dd/MM/yyyy HH:mm"),
                        motivoQuarentena = usuario.MotivoQuarentena,
                        justificativaRemocaoQuarentena = usuario.JustificativaRemocaoQuarentena,
                        observacoesAdministrativas = usuario.ObservacoesAdministrativas,
                        dataUltimaBloqueio = usuario.DataUltimaBloqueio?.ToString("dd/MM/yyyy HH:mm"),
                        numeroViolacoes = usuario.NumeroViolacoes,
                        quarentenaAtiva = usuario.EmQuarentena && usuario.FimQuarentena.HasValue && DateTime.Now < usuario.FimQuarentena.Value,
                        diasRestantesQuarentena = usuario.EmQuarentena && usuario.FimQuarentena.HasValue && DateTime.Now < usuario.FimQuarentena.Value 
                            ? Math.Max(0, (int)(usuario.FimQuarentena.Value - DateTime.Now).TotalDays + 1) 
                            : (int?)null
                    },
                    
                    estatisticas = new
                    {
                        totalAdocoes = usuario.Adocoes.Count,
                        adocoesFinalizadas = usuario.Adocoes.Count(a => a.Status == StatusAdocao.Finalizado),
                        adocoesCanceladas = usuario.Adocoes.Count(a => a.Status.EstaCancelada()),
                        totalFormularios = usuario.FormulariosAdocao.Count,
                        taxaSucesso = usuario.Adocoes.Count > 0 ? (double)usuario.Adocoes.Count(a => a.Status == StatusAdocao.Finalizado) / usuario.Adocoes.Count * 100 : 0
                    }
                };

                return Json(new { sucesso = true, dados });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes do usuário {UsuarioId}", id);
                return Json(new { sucesso = false, mensagem = "Erro ao carregar detalhes do usuário." });
            }
        }

        [HttpGet("historico-completo/{usuarioId}")]
        public async Task<IActionResult> HistoricoCompletoUsuarioAsync(int usuarioId)
        {
            try
            {
                var adocoes = await _contexto.Adocoes
                    .Include(a => a.Pet)
                    .Include(a => a.Contrato)
                    .Where(a => a.UsuarioId == usuarioId)
                    .OrderByDescending(a => a.DataEnvio)
                    .ToListAsync();

                var historico = adocoes.Select(a => new
                {
                    id = a.Id,
                    petNome = a.Pet?.Nome ?? "Pet não informado",
                    petEspecie = a.Pet?.Especie.ObterTexto() ?? "Não informado",
                    petImagem = a.Pet?.NomeArquivoImagem,
                    dataEnvio = a.DataEnvio,
                    dataResposta = a.DataResposta,
                    dataFinalizacao = a.DataFinalizacao,
                    dataAssinatura = a.Contrato?.DataAssinatura,
                    status = a.Status.ObterTexto(),
                    statusEnum = a.Status.ToString(),
                    statusCss = a.Status.ToString().ToLower().Replace(" ", "-"),
                    observacoes = a.ObservacoesCancelamento,
                    contratoAssinado = a.ContratoAssinado,
                    temContrato = a.ContratoId.HasValue
                }).ToList();

                return Json(new { success = true, historico });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar histórico completo do usuário {UsuarioId}", usuarioId);
                return Json(new { success = false, message = "Erro ao buscar histórico completo." });
            }
        }
    }

    public class SolicitacaoQuarentenaUsuario
    {
        public int UsuarioId { get; set; }
        public int DiasDuracao { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }

    public class SolicitacaoRemocaoQuarentena
    {
        public int UsuarioId { get; set; }
        public string Justificativa { get; set; } = string.Empty;
    }

    public class SolicitacaoAlteracaoStatusUsuario
    {
        public int UsuarioId { get; set; }
        public bool Ativar { get; set; }
    }

    public class SolicitacaoSalvarObservacoes
    {
        public int UsuarioId { get; set; }
        public string Observacoes { get; set; } = string.Empty;
    }
}