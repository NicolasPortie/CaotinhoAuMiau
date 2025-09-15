using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using CaotinhoAuMiau.Services;
using CaotinhoAuMiau.Utils;
using CaotinhoAuMiau.Models.Enums;
using CaotinhoAuMiau.Models.ViewModels.Usuario;

namespace CaotinhoAuMiau.Controllers.Usuario
{
    [Route("usuario/adocao/contrato")]
    [Authorize(Roles = "Usuario")]
    public class ContratoAdocaoController : Controller
    {
        private readonly ApplicationDbContext _contexto;
        private readonly ContratoService _contratoServico;
        private readonly PdfService _pdfServico;
        private readonly EmailService _emailService;

        public ContratoAdocaoController(ApplicationDbContext contexto, ContratoService contratoServico, PdfService pdfServico, EmailService emailService)
        {
            _contexto = contexto;
            _contratoServico = contratoServico;
            _pdfServico = pdfServico;
            _emailService = emailService;
        }

        [HttpGet("{adocaoId}")]
        public async Task<IActionResult> ExibirContratoAsync(int adocaoId)
        {
            var idUsuario = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(idUsuario))
            {
                return RedirectToAction("ExibirTelaLogin", "Authentication");
            }

            var adocao = await _contexto.Adocoes
                .Include(a => a.Pet)
                .Include(a => a.Usuario)
                .Include(a => a.Contrato)
                .FirstOrDefaultAsync(a => a.Id == adocaoId && a.UsuarioId.ToString() == idUsuario);

            if (adocao == null)
            {
                TempData["Erro"] = "Adoção não encontrada.";
                return Redirect("/usuario/adocao");
            }

            if (adocao.Status != StatusAdocao.Aprovado && adocao.Status != StatusAdocao.ContratoAssinado && adocao.Status != StatusAdocao.AguardandoAssinarContrato && adocao.Status != StatusAdocao.AguardandoBuscar)
            {
                TempData["Erro"] = "Contrato só está disponível após aprovação da adoção.";
                return Redirect("/usuario/adocao");
            }

            var contrato = adocao.Contrato;

            if (contrato == null)
            {
                var conteudoContrato = $@"
CONTRATO DE ADOÇÃO DE ANIMAL

Pelo presente instrumento, a CAOTINHO AU MIAU, estabelece um contrato de adoção do animal:

Nome: {adocao.Pet?.Nome}
Espécie: {adocao.Pet?.Especie}
Sexo: {adocao.Pet?.Sexo}
Idade: {adocao.Pet?.Idade}
Raça: {adocao.Pet?.Raca}

ADOTANTE:
Nome: {adocao.Usuario?.Nome}
CPF: {adocao.Usuario?.CPF}
E-mail: {adocao.Usuario?.Email}
Telefone: {adocao.Usuario?.Telefone}
Endereço: {adocao.Usuario?.Cidade} - {adocao.Usuario?.Estado}

TERMOS E CONDIÇÕES:

1. O adotante se compromete a oferecer cuidados veterinários adequados ao animal.
2. O animal não poderá ser vendido, doado ou abandonado.
3. O adotante permitirá visitas da equipe da CAOTINHO AU MIAU para acompanhamento.
4. Em caso de impossibilidade de manter o animal, este deverá ser devolvido à CAOTINHO AU MIAU.

Data: {DateTime.Now:dd/MM/yyyy}

____________________________
Assinatura do Adotante";

                contrato = new ContratoAdocao
                {
                    AdocaoId = adocaoId,
                    ConteudoContrato = conteudoContrato,
                    StatusContrato = "Pendente",
                    DataCriacao = DateTime.Now
                };

                _contexto.ContratosAdocao.Add(contrato);
                adocao.ContratoId = contrato.Id;
                await _contexto.SaveChangesAsync();
            }

            var viewModel = new ContratoUsuarioViewModel
            {
                ContratoId = contrato.Id,
                AdocaoId = adocaoId,
                StatusContrato = contrato.StatusContrato,
                DataAssinatura = contrato.DataAssinatura,
                DataCriacao = contrato.DataCriacao,
                DataVencimento = contrato.DataCriacao.AddDays(3), // 3 dias para assinar
                EstaAssinado = contrato.EstaAssinado,
                EstaPendente = !contrato.EstaAssinado && !contrato.EstaExpirado,
                EstaExpirado = contrato.EstaExpirado,
                ConteudoContrato = contrato.ConteudoContrato,
                AssinaturaUsuario = contrato.AssinaturaUsuario,
                PodeAssinar = !contrato.EstaAssinado && !contrato.EstaExpirado,
                MotivoNaoPodeAssinar = contrato.EstaExpirado ? "Contrato expirado" : null,
                Pet = new PetContratoUsuarioViewModel
                {
                    Nome = adocao.Pet?.Nome ?? "",
                    Especie = adocao.Pet?.Especie.ToString() ?? "",
                    Raca = adocao.Pet?.Raca ?? "",
                    Anos = adocao.Pet?.Anos ?? 0,
                    Meses = adocao.Pet?.Meses ?? 0,
                    Sexo = adocao.Pet?.Sexo.ToString() ?? "",
                    NomeArquivoImagem = adocao.Pet?.NomeArquivoImagem
                },
                Usuario = new UsuarioContratoLogadoViewModel
                {
                    Nome = adocao.Usuario?.Nome ?? "",
                    Email = adocao.Usuario?.Email ?? "",
                    Telefone = adocao.Usuario?.Telefone,
                    FotoPerfil = adocao.Usuario?.FotoPerfil
                },
                Adocao = new AdocaoContratoViewModel
                {
                    DataEnvio = adocao.DataEnvio,
                    DataResposta = adocao.DataResposta,
                    Status = adocao.Status.ToString()
                }
            };

            return View("~/Views/Usuario/ContratoAdocao.cshtml", viewModel);
        }

        [HttpGet("{adocaoId}/dados")]
        public async Task<IActionResult> ObterDadosContratoAsync(int adocaoId)
        {
            try
            {
                var idUsuario = User.ObterIdUsuario();
                if (string.IsNullOrEmpty(idUsuario))
                {
                    return Json(new { sucesso = false, mensagem = "Usuário não autenticado" });
                }

                var adocao = await _contexto.Adocoes
                    .Include(a => a.Pet)
                    .Include(a => a.Usuario)
                    .Include(a => a.Contrato)
                    .FirstOrDefaultAsync(a => a.Id == adocaoId && a.UsuarioId.ToString() == idUsuario);

                if (adocao == null)
                {
                    return Json(new { sucesso = false, mensagem = "Adoção não encontrada" });
                }

                var contrato = adocao.Contrato;
                if (contrato == null)
                {
                    var conteudoContrato = $@"
CONTRATO DE ADOÇÃO DE ANIMAL

Pelo presente instrumento, a CAOTINHO AU MIAU, estabelece um contrato de adoção do animal:

Nome: {adocao.Pet?.Nome}
Espécie: {adocao.Pet?.Especie}
Sexo: {adocao.Pet?.Sexo}
Idade: {adocao.Pet?.Idade} anos
Raça: {adocao.Pet?.Raca}
Cor: Não informado

ADOTANTE:
Nome: {adocao.Usuario?.Nome}
CPF: {adocao.Usuario?.CPF}
E-mail: {adocao.Usuario?.Email}
Telefone: {adocao.Usuario?.Telefone}
Endereço: {adocao.Usuario?.Cidade} - {adocao.Usuario?.Estado}

TERMOS E CONDIÇÕES:

1. O adotante se compromete a oferecer cuidados veterinários adequados ao animal.
2. O animal não poderá ser vendido, doado ou abandonado.
3. O adotante permitirá visitas da equipe da CAOTINHO AU MIAU para acompanhamento.
4. Em caso de impossibilidade de manter o animal, este deverá ser devolvido à CAOTINHO AU MIAU.

Data: {DateTime.Now:dd/MM/yyyy}

____________________________
Assinatura do Adotante";

                    contrato = new ContratoAdocao
                    {
                        AdocaoId = adocaoId,
                        ConteudoContrato = conteudoContrato,
                        StatusContrato = "Pendente",
                        DataCriacao = DateTime.Now
                    };

                    _contexto.ContratosAdocao.Add(contrato);
                    adocao.ContratoId = contrato.Id;
                    await _contexto.SaveChangesAsync();
                }

                var dados = new
                {
                    sucesso = true,
                    contrato = new
                    {
                        id = contrato.Id,
                        conteudo = contrato.ConteudoContrato,
                        status = contrato.StatusContrato,
                        assinado = contrato.EstaAssinado,
                        dataAssinatura = contrato.DataAssinatura?.ToString("dd/MM/yyyy HH:mm"),
                        expirado = contrato.EstaExpirado
                    },
                    pet = new
                    {
                        nome = adocao.Pet?.Nome,
                        foto = adocao.Pet?.NomeArquivoImagem,
                        idade = adocao.Pet?.Idade,
                        especie = adocao.Pet?.Especie.ToString()
                    },
                    adotante = new
                    {
                        nome = adocao.Usuario?.Nome,
                        email = adocao.Usuario?.Email
                    }
                };

                return Json(dados);
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Erro interno do servidor", erro = ex.Message });
            }
        }

        [HttpPost("{adocaoId}/assinar")]
        public async Task<IActionResult> AssinarContratoDigitalAsync(int adocaoId, [FromBody] AssinaturaRequest request)
        {
            try
            {
                var idUsuario = User.ObterIdUsuario();
                if (string.IsNullOrEmpty(idUsuario))
                {
                    return Json(new { sucesso = false, mensagem = "Usuário não autenticado" });
                }

                if (string.IsNullOrWhiteSpace(request?.Assinatura))
                {
                    return Json(new { sucesso = false, mensagem = "Assinatura é obrigatória" });
                }

                var adocao = await _contexto.Adocoes
                    .Include(a => a.Contrato)
                    .Include(a => a.Pet)
                    .FirstOrDefaultAsync(a => a.Id == adocaoId && a.UsuarioId.ToString() == idUsuario);

                if (adocao == null)
                {
                    return Json(new { sucesso = false, mensagem = "Adoção não encontrada" });
                }

                if (adocao.Contrato == null)
                {
                    return Json(new { sucesso = false, mensagem = "Contrato não encontrado" });
                }

                if (adocao.Contrato.EstaExpirado)
                {
                    return Json(new { sucesso = false, mensagem = "Contrato expirado" });
                }

                adocao.Contrato.AssinaturaUsuario = request.Assinatura;
                adocao.Contrato.DataAssinatura = DateTime.Now;
                adocao.Contrato.StatusContrato = "Assinado";

                adocao.ContratoAssinado = true;
                adocao.Status = StatusAdocao.AguardandoBuscar;

                if (adocao.Pet != null)
                {
                    adocao.Pet.Status = StatusPet.EmProcesso;
                }

                await _contexto.SaveChangesAsync();

                try
                {
                    var usuario = await _contexto.Usuarios.FindAsync(adocao.UsuarioId);
                    var pet = await _contexto.Pets.FindAsync(adocao.PetId);
                    
                    if (usuario != null && pet != null)
                    {
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao enviar email de contrato assinado: {ex.Message}");
                }

                return Json(new { sucesso = true, mensagem = "Contrato assinado com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Erro interno do servidor", erro = ex.Message });
            }
        }

        [HttpGet("{adocaoId}/pdf")]
        public async Task<IActionResult> BaixarContratoPdfAsync(int adocaoId)
        {
            var idUsuario = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(idUsuario))
            {
                return RedirectToAction("ExibirTelaLogin", "Authentication");
            }

            var adocao = await _contexto.Adocoes
                .Include(a => a.Pet)
                .Include(a => a.Usuario)
                .Include(a => a.Contrato)
                .FirstOrDefaultAsync(a => a.Id == adocaoId && a.UsuarioId.ToString() == idUsuario);

            if (adocao?.Contrato == null || !adocao.Contrato.EstaAssinado)
            {
                TempData["Erro"] = "Contrato não encontrado ou não foi assinado.";
                return Redirect("/usuario/adocao");
            }

            try
            {
                var nomeArquivo = $"Contrato_Adocao_{adocao.Pet?.Nome}_{DateTime.Now:yyyyMMdd}.pdf";

                var (sucesso, mensagem, caminhoArquivo) = await _pdfServico.GerarPdfContratoAsync(adocao.Contrato);

                if (sucesso && !string.IsNullOrEmpty(caminhoArquivo))
                {
                    var caminhoCompleto = Path.Combine("wwwroot", caminhoArquivo);

                    if (System.IO.File.Exists(caminhoCompleto))
                    {
                        var pdfBytes = await System.IO.File.ReadAllBytesAsync(caminhoCompleto);

                        try { System.IO.File.Delete(caminhoCompleto); } catch { }

                        return File(pdfBytes, "application/pdf", nomeArquivo);
                    }
                }

                var conteudoContrato = $"{adocao.Contrato.ConteudoContrato}\n\nAssinatura Digital: {adocao.Contrato.AssinaturaUsuario}\nData da Assinatura: {adocao.Contrato.DataAssinatura:dd/MM/yyyy HH:mm}";
                var pdfBytesSimples = _pdfServico.GerarContratoPdf(conteudoContrato, adocao.Pet?.Nome ?? "Pet", adocao.Usuario?.Nome ?? "Adotante");

                return File(pdfBytesSimples, "application/pdf", nomeArquivo);
            }
            catch (Exception ex)
            {
                var nomeArquivo = $"Contrato_Adocao_{adocao.Pet?.Nome}_{DateTime.Now:yyyyMMdd}.txt";
                var conteudoTexto = $"{adocao.Contrato.ConteudoContrato}\n\nAssinatura Digital: {adocao.Contrato.AssinaturaUsuario}\nData da Assinatura: {adocao.Contrato.DataAssinatura:dd/MM/yyyy HH:mm}";

                var bytes = System.Text.Encoding.UTF8.GetBytes(conteudoTexto);

                return File(bytes, "text/plain", nomeArquivo);
            }
        }
    }

    public class AssinaturaRequest
    {
        public string Assinatura { get; set; } = string.Empty;
    }
}