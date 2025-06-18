using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Models.ViewModels;
using CaotinhoAuMiau.Models.ViewModels.Admin;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using CaotinhoAuMiau.Services;
using CaotinhoAuMiau.Utils;
using System.Text.Json;

namespace CaotinhoAuMiau.Controllers.Admin
{
    [Route("admin/pets")]
    [Authorize(Roles = "Administrador")]
    [RequestFormLimits(MultipartBodyLengthLimit = 52428800)] // 50 MB
    [RequestSizeLimit(52428800)] // 50 MB
    public class GerenciamentoPetController : Controller
    {
        private readonly ApplicationDbContext _contexto;
        private readonly IWebHostEnvironment _ambiente;
        private readonly NotificacaoServico _servicoNotificacao;

        public GerenciamentoPetController(ApplicationDbContext contexto, IWebHostEnvironment ambiente, NotificacaoServico servicoNotificacao)
        {
            _contexto = contexto;
            _ambiente = ambiente;
            _servicoNotificacao = servicoNotificacao;
        }

        [HttpGet]
        public async Task<IActionResult> Listar(int pagina = 1, int itensPorPagina = 14)
        {
            var adminId = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction("ExibirTelaLogin", "Autenticacao");
            }

            // Validar e ajustar os parâmetros de paginação
            if (pagina < 1) pagina = 1;
            
            // Garantir que itensPorPagina seja um dos valores permitidos
            if (itensPorPagina != 14 && itensPorPagina != 24 && itensPorPagina != 48)
            {
                itensPorPagina = 14; // Valor padrão
            }

            var query = _contexto.Pets
                .OrderByDescending(p => p.CadastroCompleto == false)
                .ThenByDescending(p => p.DataCriacao);

            var totalItens = await query.CountAsync();
            
            // Garantir que sempre haja pelo menos uma página
            int totalPaginas = Math.Max(1, (int)Math.Ceiling(totalItens / (double)itensPorPagina));
            
            // Ajustar a página atual se for maior que o total de páginas
            if (pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }
            
            var pets = await query
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToListAsync();

            ViewBag.PaginaAtual = pagina;
            ViewBag.ItensPorPagina = itensPorPagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalItens = totalItens;

            return View("~/Views/Admin/GerenciamentoPet.cshtml", pets);
        }

        [HttpGet("criar")]
        public IActionResult ExibirFormularioCriacao()
        {
            return View("~/Views/Admin/GerenciamentoPet.cshtml");
        }

        [HttpPost("SalvarPet")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarPet([FromForm]Pet pet, IFormFile foto, bool RemoverImagem = false, bool CadastroCompleto = true, bool ManterImagemAtual = false)
        {
            try
            {
                Console.WriteLine("=========== INÍCIO DO LOG DE SALVAMENTO DE PET ===========");
                Console.WriteLine($"ID do Pet: {pet.Id}, Nome: {pet.Nome}, CadastroCompleto: {CadastroCompleto}");
                Console.WriteLine($"Imagem recebida: {(foto != null ? $"Sim, nome: {foto.FileName}, tamanho: {foto.Length / 1024} KB" : "Não")}");
                Console.WriteLine($"RemoverImagem: {RemoverImagem}, ManterImagemAtual: {ManterImagemAtual}");
                
                bool novoPet = pet.Id == 0;
                
                if (!novoPet)
                {
                    var petExistente = await _contexto.Pets.FindAsync(pet.Id);
                    if (petExistente != null)
                    {
                        if (petExistente.Status == "Em Processo" || petExistente.Status == "Adotado")
                        {
                            return Json(new { 
                                sucesso = false, 
                                mensagem = $"Não é possível editar um pet com status '{petExistente.Status}'. Pets em processo de adoção ou já adotados não podem ser modificados." 
                            });
                        }
                    }
                }
                
                if (!string.IsNullOrWhiteSpace(pet.Nome))
                {
                    var nomePetTrim = pet.Nome.Trim();
                    var petExistenteComMesmoNome = await _contexto.Pets
                        .Where(p => p.Nome.ToLower() == nomePetTrim.ToLower() 
                                 && p.Id != pet.Id
                                 && p.Status != "Finalizado" 
                                 && p.Status != "Adotado")
                        .FirstOrDefaultAsync();
                    
                    if (petExistenteComMesmoNome != null)
                    {
                        return Json(new { 
                            sucesso = false, 
                            mensagem = $"Já existe um pet ativo com o nome '{pet.Nome}'. Por favor, escolha um nome diferente ou adicione um detalhe para diferenciar." 
                        });
                    }
                }

                if (!CadastroCompleto)
                {
                    ModelState.Clear();
                    
                    if (string.IsNullOrEmpty(pet.Nome))
                    {
                        return Json(new { sucesso = false, mensagem = "O nome do pet é obrigatório mesmo para rascunhos." });
                    }
                }
                else
                {
                    if (ManterImagemAtual && !novoPet)
                    {
                        if (ModelState.ContainsKey("foto"))
                        {
                            ModelState.Remove("foto");
                        }
                        
                        if (ModelState.ContainsKey("NomeArquivoImagem"))
                        {
                            ModelState.Remove("NomeArquivoImagem");
                        }
                    }
                    
                    var errosValidacao = new Dictionary<string, string>();
                    
                    if (string.IsNullOrWhiteSpace(pet.Especie))
                        errosValidacao.Add("especie", "A espécie do pet é obrigatória.");
                    
                    if (string.IsNullOrWhiteSpace(pet.Raca))
                        errosValidacao.Add("raca", "A raça do pet é obrigatória.");
                        
                    if (string.IsNullOrWhiteSpace(pet.Sexo))
                        errosValidacao.Add("sexo", "O sexo do pet é obrigatório.");
                        
                    if (string.IsNullOrWhiteSpace(pet.Porte))
                        errosValidacao.Add("porte", "O porte do pet é obrigatório.");
                        
                    if (string.IsNullOrWhiteSpace(pet.Descricao))
                        errosValidacao.Add("descricao", "A descrição do pet é obrigatória.");
                    
                    if (string.IsNullOrWhiteSpace(pet.NomeArquivoImagem) && foto == null && !ManterImagemAtual)
                        errosValidacao.Add("foto", "A foto do pet é obrigatória.");
                    
                    if (errosValidacao.Count > 0)
                    {
                        return Json(new { sucesso = false, mensagem = "Dados inválidos para cadastro completo", erros = errosValidacao });
                    }
                }

                if (!ModelState.IsValid)
                {
                    var erros = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
                    );
                    
                    return Json(new { sucesso = false, mensagem = "Dados inválidos", erros });
                }
                
                if (!novoPet)
                {
                    Console.WriteLine($"Atualizando pet existente com ID {pet.Id}");
                    var petExistente = await _contexto.Pets.FindAsync(pet.Id);
                    
                    if (petExistente == null)
                    {
                        Console.WriteLine($"Pet com ID {pet.Id} não encontrado no banco de dados");
                        return Json(new { sucesso = false, mensagem = "Pet não encontrado" });
                    }
                    
                    petExistente.Nome = pet.Nome?.Trim() ?? "";
                    petExistente.Status = CadastroCompleto ? "Disponível" : "Rascunho";
                    petExistente.CadastroCompleto = CadastroCompleto;
                    petExistente.DataAtualizacao = DateTime.Now;
                    
                    if (CadastroCompleto)
                    {
                        petExistente.Especie = pet.Especie?.Trim() ?? "";
                        petExistente.Raca = pet.Raca?.Trim() ?? "";
                        petExistente.Sexo = pet.Sexo?.Trim() ?? "";
                        petExistente.Porte = pet.Porte?.Trim() ?? "";
                        petExistente.Anos = pet.Anos;
                        petExistente.Meses = pet.Meses;
                        petExistente.Descricao = pet.Descricao?.Trim() ?? "";
                    }
                    if (!CadastroCompleto)
                    {
                        petExistente.Nome = pet.Nome?.Trim() ?? "";
                        petExistente.Especie = pet.Especie?.Trim() ?? "";
                        petExistente.Raca = pet.Raca?.Trim() ?? "";
                        petExistente.Sexo = pet.Sexo?.Trim() ?? "";
                        petExistente.Porte = pet.Porte?.Trim() ?? "";
                        petExistente.Descricao = pet.Descricao?.Trim() ?? "";
                        
                        if (pet.Anos < 0) pet.Anos = 0;
                        if (pet.Meses < 0 || pet.Meses > 11) pet.Meses = 0;
                        
                        petExistente.Anos = pet.Anos;
                        petExistente.Meses = pet.Meses;
                    }
                    
                    try
                    {
                        if (foto != null && foto.Length > 0)
                        {
                            Console.WriteLine($"Processando nova imagem: {foto.FileName}, tamanho: {foto.Length / 1024} KB");
                            
                            if (!foto.ContentType.StartsWith("image/"))
                            {
                                Console.WriteLine($"Tipo de arquivo inválido: {foto.ContentType}");
                                return Json(new { sucesso = false, mensagem = "O arquivo enviado não é uma imagem válida." });
                            }
                            
                            if (foto.Length > 50 * 1024 * 1024)
                            {
                                Console.WriteLine($"Imagem muito grande: {foto.Length / (1024 * 1024)} MB");
                                return Json(new { sucesso = false, mensagem = "A imagem é muito grande. O tamanho máximo permitido é 50MB." });
                            }
                            
                            if (!string.IsNullOrEmpty(petExistente.NomeArquivoImagem))
                            {
                                var caminhoImagemAntiga = Path.Combine(_ambiente.WebRootPath, "imagens", "pets", petExistente.NomeArquivoImagem);
                                if (System.IO.File.Exists(caminhoImagemAntiga))
                                {
                                    try
                                    {
                                        System.IO.File.Delete(caminhoImagemAntiga);
                                        Console.WriteLine($"Imagem antiga excluída: {caminhoImagemAntiga}");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Erro ao excluir imagem antiga: {ex.Message}");
                                    }
                                }
                            }
                            
                            string nomeArquivoImagem = await ProcessarImagemAsync(foto);
                            if (nomeArquivoImagem != null)
                            {
                                petExistente.NomeArquivoImagem = nomeArquivoImagem;
                                Console.WriteLine($"Nova imagem salva com sucesso: {nomeArquivoImagem}");
                            }
                            else
                            {
                                Console.WriteLine("Falha ao processar a imagem");
                                return Json(new { sucesso = false, mensagem = "Falha ao processar a imagem do pet." });
                            }
                        }
                        else if (RemoverImagem)
                        {
                            Console.WriteLine("Removendo imagem existente conforme solicitado");
                            if (!string.IsNullOrEmpty(petExistente.NomeArquivoImagem))
                            {
                                var caminhoImagem = Path.Combine(_ambiente.WebRootPath, "imagens", "pets", petExistente.NomeArquivoImagem);
                                if (System.IO.File.Exists(caminhoImagem))
                                {
                                    try
                                    {
                                        System.IO.File.Delete(caminhoImagem);
                                        Console.WriteLine($"Imagem excluída: {caminhoImagem}");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Erro ao excluir imagem: {ex.Message}");
                                    }
                                }
                            }
                            
                            petExistente.NomeArquivoImagem = null;
                        }
                        else if (ManterImagemAtual)
                        {
                            Console.WriteLine("Mantendo a imagem atual conforme solicitado");
                        }
                        else if (CadastroCompleto && string.IsNullOrEmpty(petExistente.NomeArquivoImagem) && !RemoverImagem)
                        {
                            Console.WriteLine("Verificando necessidade de imagem para cadastro completo");
                            
                            if (!ManterImagemAtual)
                            {
                                Console.WriteLine("Imagem obrigatória para cadastro completo - validando");
                                return Json(new { 
                                    sucesso = false, 
                                    mensagem = "É necessário fornecer uma imagem para o pet", 
                                    erros = new Dictionary<string, string[]> { 
                                        { "foto", new[] { "É necessário fornecer uma imagem para o pet" } } 
                                    } 
                                });
                            }
                            else
                            {
                                Console.WriteLine("ManterImagemAtual=true, pulando validação de imagem obrigatória");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao processar imagem: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    }
                    
                    _contexto.Update(petExistente);
                }
                else
                {
                    Console.WriteLine("Criando novo pet");
                    pet.DataCriacao = DateTime.Now;
                    pet.Status = CadastroCompleto ? "Disponível" : "Rascunho";
                    pet.CadastroCompleto = CadastroCompleto;
                    
                    if (CadastroCompleto)
                    {
                        pet.Nome = pet.Nome?.Trim() ?? "";
                        pet.Especie = pet.Especie?.Trim() ?? "";
                        pet.Raca = pet.Raca?.Trim() ?? "";
                        pet.Sexo = pet.Sexo?.Trim() ?? "";
                        pet.Porte = pet.Porte?.Trim() ?? "";
                        pet.Descricao = pet.Descricao?.Trim() ?? "";
                    }
                    else
                    {
                        pet.Nome = pet.Nome?.Trim() ?? "";
                        
                        pet.Especie = pet.Especie?.Trim() ?? "";
                        pet.Raca = pet.Raca?.Trim() ?? "";
                        pet.Sexo = pet.Sexo?.Trim() ?? "";
                        pet.Porte = pet.Porte?.Trim() ?? "";
                        pet.Descricao = pet.Descricao?.Trim() ?? "";
                        
                        if (pet.Anos < 0) pet.Anos = 0;
                        if (pet.Meses < 0 || pet.Meses > 11) pet.Meses = 0;
                    }
                    
                    try
                    {
                        if (foto != null && foto.Length > 0)
                        {
                            Console.WriteLine($"Processando imagem para novo pet: {foto.FileName}, tamanho: {foto.Length / 1024} KB");
                            
                            if (!foto.ContentType.StartsWith("image/"))
                            {
                                Console.WriteLine($"Tipo de arquivo inválido: {foto.ContentType}");
                                return Json(new { sucesso = false, mensagem = "O arquivo enviado não é uma imagem válida." });
                            }
                            
                            if (foto.Length > 50 * 1024 * 1024)
                            {
                                Console.WriteLine($"Imagem muito grande: {foto.Length / (1024 * 1024)} MB");
                                return Json(new { sucesso = false, mensagem = "A imagem é muito grande. O tamanho máximo permitido é 50MB." });
                            }
                            
                            string nomeArquivoImagem = await ProcessarImagemAsync(foto);
                            if (nomeArquivoImagem != null)
                            {
                                pet.NomeArquivoImagem = nomeArquivoImagem;
                                Console.WriteLine($"Imagem salva com sucesso: {nomeArquivoImagem}");
                            }
                            else
                            {
                                Console.WriteLine("Falha ao processar a imagem");
                                return Json(new { sucesso = false, mensagem = "Falha ao processar a imagem do pet." });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao processar imagem: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    }
                    
                    _contexto.Add(pet);
                }
                
                try
                {
                    await _contexto.SaveChangesAsync();
                    Console.WriteLine("Alterações salvas no banco de dados com sucesso");
                    
                    string mensagem = CadastroCompleto 
                        ? (novoPet ? "Pet cadastrado com sucesso!" : "Pet atualizado com sucesso!")
                        : (novoPet ? "Rascunho criado com sucesso!" : "Rascunho atualizado com sucesso!");
                    
                    return Json(new { 
                        sucesso = true, 
                        mensagem = mensagem,
                        petId = pet.Id
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao salvar no banco de dados: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    
                    Console.WriteLine($"Detalhes adicionais: ID do Pet: {pet?.Id}, Nome: {pet?.Nome}");
                    Console.WriteLine($"Possui foto: {(foto != null ? "Sim" : "Não")}, RemoverImagem: {RemoverImagem}, ManterImagemAtual: {ManterImagemAtual}");
                    
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                        Console.WriteLine($"Inner Exception Stack Trace: {ex.InnerException.StackTrace}");
                    }
                    
                    return Json(new { 
                        sucesso = false, 
                        mensagem = $"Erro ao processar solicitação: {ex.Message}",
                        details = ex.InnerException?.Message ?? "Sem detalhes adicionais"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro geral ao salvar pet: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                Console.WriteLine($"Detalhes adicionais: ID do Pet: {pet?.Id}, Nome: {pet?.Nome}");
                Console.WriteLine($"Possui foto: {(foto != null ? "Sim" : "Não")}, RemoverImagem: {RemoverImagem}, ManterImagemAtual: {ManterImagemAtual}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner Exception Stack Trace: {ex.InnerException.StackTrace}");
                }
                
                return Json(new { 
                    sucesso = false, 
                    mensagem = $"Erro ao processar solicitação: {ex.Message}",
                    details = ex.InnerException?.Message ?? "Sem detalhes adicionais"
                });
            }
            finally
            {
                Console.WriteLine("=========== FIM DO LOG DE SALVAMENTO DE PET ===========");
            }
        }

        [HttpGet("editar/{id}")]
        public async Task<IActionResult> ExibirFormularioEdicao(int id)
        {
            var pet = await _contexto.Pets
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pet == null)
            {
                return NotFound();
            }

            var viewModel = new PetViewModel
            {
                Id = pet.Id,
                Nome = pet.Nome,
                Descricao = pet.Descricao,
                NomeArquivoImagem = pet.NomeArquivoImagem,
                Especie = pet.Especie,
                Raca = pet.Raca,
                Anos = pet.Anos,
                Meses = pet.Meses,
                Sexo = pet.Sexo,
                Porte = pet.Porte,
                Status = pet.Status,
                CadastroCompleto = pet.CadastroCompleto
            };

            return View("~/Views/Admin/GerenciamentoPet.cshtml", viewModel);
        }

        [HttpPost("excluir/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirPet(int id)
        {
            var pet = await _contexto.Pets
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pet == null)
            {
                return Json(new { sucesso = false, mensagem = "Pet não encontrado." });
            }

            if (pet.Status == "Adotado" || pet.Status == "Em Processo")
            {
                return Json(new { sucesso = false, mensagem = "Não é possível excluir um pet que está em processo de adoção ou já foi adotado." });
            }

            try
            {
                if (!string.IsNullOrEmpty(pet.NomeArquivoImagem))
                {
                    var caminhoArquivo = Path.Combine(_ambiente.WebRootPath, "imagens", "pets", pet.NomeArquivoImagem);
                    if (System.IO.File.Exists(caminhoArquivo))
                    {
                        System.IO.File.Delete(caminhoArquivo);
                    }
                }

                _contexto.Pets.Remove(pet);
                await _contexto.SaveChangesAsync();

                return Json(new { sucesso = true, mensagem = "Pet excluído com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Erro ao excluir pet: " + ex.Message });
            }
        }

        [HttpPost("alterar-status/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarStatusPet(int id, [FromBody] JsonElement modelo)
        {
            try
            {
                if (!modelo.TryGetProperty("novoStatus", out JsonElement novoStatusElement))
                {
                    return Json(new { sucesso = false, mensagem = "O status do pet não foi informado." });
                }
                
                string novoStatus = novoStatusElement.GetString();
                
                if (string.IsNullOrEmpty(novoStatus))
                {
                    return Json(new { sucesso = false, mensagem = "O status do pet não foi informado." });
                }
                
                var pet = await _contexto.Pets.FindAsync(id);
                
                if (pet == null)
                {
                    return Json(new { sucesso = false, mensagem = "Pet não encontrado." });
                }
                
                pet.Status = novoStatus;
                pet.DataAtualizacao = DateTime.Now;
                
                await _contexto.SaveChangesAsync();
                
                return Json(new { sucesso = true, mensagem = $"Status do pet alterado para {novoStatus} com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao alterar status do pet: {ex.Message}" });
            }
        }

        [HttpGet("ObterPet/{id}")]
        public async Task<IActionResult> ObterDadosPet(int id)
        {
            try
            {
                var pet = await _contexto.Pets
                    .FirstOrDefaultAsync(p => p.Id == id);
                
                if (pet == null)
                {
                    return Json(new { sucesso = false, mensagem = "Pet não encontrado." });
                }

                var resultado = new
                {
                    id = pet.Id,
                    nome = pet.Nome,
                    especie = pet.Especie,
                    raca = pet.Raca,
                    anos = pet.Anos,
                    meses = pet.Meses,
                    sexo = pet.Sexo == "M" ? "Macho" : "Fêmea",
                    porte = pet.Porte,
                    status = pet.Status,
                    descricao = pet.Descricao,
                    nomeArquivoImagem = pet.NomeArquivoImagem,
                    cadastroCompleto = pet.CadastroCompleto,
                    dataCriacao = pet.DataCriacao,
                    dataAtualizacao = pet.DataAtualizacao
                };
                
                return Json(new { sucesso = true, dados = resultado });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao obter pet: {ex.Message}" });
            }
        }

        [HttpPost("CadastrarPet")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CadastrarPetAjax(PetViewModel? modelo, IFormFile? imagemUpload)
        {
            try
            {
                Console.WriteLine("=========== INÍCIO DO LOG DE CADASTRO DE PET ===========");
                Console.WriteLine($"Recebendo cadastro de pet: {modelo?.Nome ?? "null"}, Espécie: {modelo?.Especie ?? "null"}");
                Console.WriteLine($"É rascunho: {modelo?.CadastroCompleto == false}");
                Console.WriteLine($"Imagem recebida: {(imagemUpload != null ? $"Sim, nome: {imagemUpload.FileName}, tamanho: {imagemUpload.Length} bytes" : "Não")}");
                
                if (modelo == null)
                {
                    return Json(new { sucesso = false, erros = new Dictionary<string, string> { { "Geral", "Dados do pet não fornecidos." } } });
                }
                
                if (string.IsNullOrEmpty(modelo.Nome))
                {
                    return Json(new { sucesso = false, mensagem = "O nome do pet é obrigatório." });
                }

                if (modelo.CadastroCompleto && string.IsNullOrEmpty(modelo.NomeArquivoImagem))
                {
                    return Json(new { sucesso = false, mensagem = "A imagem do pet é obrigatória para cadastro completo." });
                }

                if (modelo.CadastroCompleto)
                {
                    if (string.IsNullOrEmpty(modelo.Especie) || string.IsNullOrEmpty(modelo.Raca) ||
                        string.IsNullOrEmpty(modelo.Sexo) || string.IsNullOrEmpty(modelo.Porte) ||
                        string.IsNullOrEmpty(modelo.Descricao))
                    {
                        return Json(new { sucesso = false, mensagem = "Todos os campos são obrigatórios para cadastro completo." });
                    }
                }

                var pet = new Pet
                {
                    Nome = modelo.Nome,
                    Especie = modelo.Especie,
                    Raca = modelo.Raca,
                    Anos = modelo.Anos,
                    Meses = modelo.Meses,
                    Sexo = modelo.Sexo,
                    Porte = modelo.Porte,
                    Descricao = modelo.Descricao,
                    Status = modelo.CadastroCompleto ? "Disponível" : "Rascunho",
                    DataCriacao = DateTime.Now,
                    CadastroCompleto = modelo.CadastroCompleto,
                    UsuarioId = 0,
                    NomeArquivoImagem = null
                };
                
                if (imagemUpload != null && imagemUpload.Length > 0)
                {
                    pet.NomeArquivoImagem = await ProcessarImagemAsync(imagemUpload);
                }
                else
                {
                    pet.NomeArquivoImagem = null;
                }
                
                _contexto.Pets.Add(pet);
                await _contexto.SaveChangesAsync();
                
                return Json(new { 
                    sucesso = true, 
                    mensagem = modelo.CadastroCompleto ? "Pet cadastrado com sucesso!" : "Rascunho salvo com sucesso!",
                    pet = new { 
                        id = pet.Id, 
                        nome = pet.Nome,
                        nomeArquivoImagem = pet.NomeArquivoImagem,
                        cadastroCompleto = pet.CadastroCompleto
                    } 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao cadastrar pet: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { sucesso = false, mensagem = $"Erro ao processar cadastro: {ex.Message}" });
            }
        }

        [HttpGet("ObterDetalhesPet/{id}")]
        public async Task<IActionResult> ObterDetalhesPet(int id)
        {
            try
            {
                var pet = await _contexto.Pets.FirstOrDefaultAsync(p => p.Id == id);
                
                if (pet == null)
                {
                    return Json(new { success = false, message = "Pet não encontrado" });
                }
                
                string idadeFormatada = $"{pet.Anos} ano(s) e {pet.Meses} mês(es)";
                
                var petDto = new
                {
                    id = pet.Id,
                    nome = pet.Nome,
                    especie = pet.Especie,
                    raca = pet.Raca,
                    sexo = pet.Sexo,
                    porte = pet.Porte,
                    status = pet.Status,
                    idade = idadeFormatada,
                    anos = pet.Anos,
                    meses = pet.Meses,
                    descricao = pet.Descricao,
                    nomeArquivoImagem = pet.NomeArquivoImagem,
                    cadastroCompleto = pet.CadastroCompleto
                };
                
                return Json(new { success = true, pet = petDto });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("ObterListaPets")]
        public async Task<IActionResult> ObterListaPets()
        {
            try
            {
                var adminId = User.ObterIdUsuario();
                if (string.IsNullOrEmpty(adminId))
                {
                    return Json(new { success = false, message = "Usuário não autenticado" });
                }
                
                var pets = await _contexto.Pets
                    .OrderByDescending(p => p.CadastroCompleto == false) // Rascunhos primeiro
                    .ThenByDescending(p => p.DataCriacao) // Depois ordena por data de criação
                    .ToListAsync();

                var petsFormatados = pets.Select(p => new
                {
                    p.Id,
                    p.Nome,
                    p.Especie,
                    p.Raca,
                    p.Sexo,
                    p.Porte,
                    p.Anos,
                    p.Meses,
                    p.Status,
                    p.Descricao,
                    p.NomeArquivoImagem,
                    DataCadastro = p.DataCriacao.ToString("dd/MM/yyyy"),
                    p.CadastroCompleto
                });
                
                return Json(new { sucesso = true, pets = petsFormatados });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao obter pets: {ex.Message}" });
            }
        }

        private async Task<string> ProcessarImagemAsync(IFormFile imagem, string? nomeArquivoAtual = null)
        {
            if (imagem == null || imagem.Length <= 0)
                return null;
                
            try
            {
                Console.WriteLine($"Iniciando processamento de imagem: {imagem.FileName}, tipo: {imagem.ContentType}, tamanho: {imagem.Length / 1024} KB");
                
                if (!string.IsNullOrEmpty(nomeArquivoAtual))
                {
                    var caminhoImagemAntiga = Path.Combine(_ambiente.WebRootPath, "imagens", "pets", nomeArquivoAtual);
                    if (System.IO.File.Exists(caminhoImagemAntiga))
                    {
                        try
                        {
                            System.IO.File.Delete(caminhoImagemAntiga);
                            Console.WriteLine($"Imagem antiga excluída: {caminhoImagemAntiga}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao excluir imagem antiga: {ex.Message}");
                        }
                    }
                }

                var caminhoUpload = Path.Combine(_ambiente.WebRootPath, "imagens", "pets");
                if (!Directory.Exists(caminhoUpload))
                {
                    Directory.CreateDirectory(caminhoUpload);
                    Console.WriteLine($"Diretório de upload criado: {caminhoUpload}");
                }
                
                var nomeArquivo = Guid.NewGuid().ToString() + Path.GetExtension(imagem.FileName);
                var caminhoArquivo = Path.Combine(caminhoUpload, nomeArquivo);
                
                Console.WriteLine($"Salvando arquivo em: {caminhoArquivo}");
                
                using (var stream = new FileStream(caminhoArquivo, FileMode.Create))
                {
                    await imagem.CopyToAsync(stream);
                }
                
                Console.WriteLine($"Arquivo salvo com sucesso: {nomeArquivo}");
                return nomeArquivo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar upload de imagem: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [HttpGet("verificar-nome")]
        public async Task<IActionResult> VerificarNome(string nome, int id = 0)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                return Json(new { disponivel = false, mensagem = "O nome do pet é obrigatório." });
            }
            
            var nomeTrim = nome.Trim();
            var petExistente = await _contexto.Pets
                .Where(p => p.Nome.ToLower() == nomeTrim.ToLower() 
                         && p.Id != id
                         && p.Status != "Finalizado" 
                         && p.Status != "Adotado")
                .FirstOrDefaultAsync();
            
            return Json(new { 
                disponivel = petExistente == null,
                mensagem = petExistente == null ? "" : $"Este nome já está sendo usado por outro pet ativo no sistema."
            });
        }

    }
} 