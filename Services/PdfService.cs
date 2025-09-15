using CaotinhoAuMiau.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;

namespace CaotinhoAuMiau.Services
{
    public class PdfService
    {
        private readonly AssinaturaDigitalService _assinaturaService;

        public PdfService(AssinaturaDigitalService assinaturaService)
        {
            _assinaturaService = assinaturaService;
        }

        public async Task<(bool sucesso, string mensagem, string? caminhoArquivo)> GerarPdfContratoAsync(ContratoAdocao contrato)
        {
            try
            {
                if (contrato.Adocao == null || contrato.Adocao.Pet == null || contrato.Adocao.Usuario == null)
                {
                    return (false, "Dados incompletos para gerar PDF.", null);
                }

                if (!contrato.EstaAssinado)
                {
                    return (false, "Contrato deve estar assinado para gerar PDF.", null);
                }

                var diretorioPdfs = Path.Combine("wwwroot", "contratos");
                if (!Directory.Exists(diretorioPdfs))
                {
                    Directory.CreateDirectory(diretorioPdfs);
                }

                var nomeArquivo = $"contrato_{contrato.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var caminhoCompleto = Path.Combine(diretorioPdfs, nomeArquivo);

                await GerarPdfComAssinatura(contrato, caminhoCompleto);

                var caminhoRelativo = Path.Combine("contratos", Path.GetFileName(caminhoCompleto));

                return (true, "PDF gerado com sucesso!", caminhoRelativo);
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao gerar PDF: {ex.Message}", null);
            }
        }

        private Task GerarPdfDoHtml(string htmlContent, string caminhoArquivo)
        {
            using var document = new Document(PageSize.A4, 50, 50, 25, 25);
            using var fs = new FileStream(caminhoArquivo, FileMode.Create);
            using var writer = PdfWriter.GetInstance(document, fs);

            document.Open();

            try
            {
                var styles = new StyleSheet();
                styles.LoadTagStyle("body", "font-family", "Arial, sans-serif");
                styles.LoadTagStyle("h1", "font-size", "18px");
                styles.LoadTagStyle("h2", "font-size", "16px");
                styles.LoadTagStyle("h3", "font-size", "14px");
                styles.LoadTagStyle("p", "font-size", "12px");
                styles.LoadTagStyle("td", "font-size", "11px");
                
                var htmlWorker = new HtmlWorker(document);
                var reader = new StringReader(htmlContent);
                htmlWorker.Parse(reader);
            }
            catch
            {
                var textoLimpo = LimparHtml(htmlContent);
                var paragraph = new Paragraph(textoLimpo);
                paragraph.Font = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                document.Add(paragraph);
            }

            document.Close();
            return Task.CompletedTask;
        }

        private string LimparHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return "";

            var texto = html
                .Replace("<h1>", "\n=== ")
                .Replace("</h1>", " ===\n")
                .Replace("<h2>", "\n== ")
                .Replace("</h2>", " ==\n")
                .Replace("<h3>", "\n= ")
                .Replace("</h3>", " =\n")
                .Replace("<p>", "\n")
                .Replace("</p>", "\n")
                .Replace("<br>", "\n")
                .Replace("<br/>", "\n")
                .Replace("<br />", "\n")
                .Replace("<li>", "• ")
                .Replace("</li>", "\n")
                .Replace("<ul>", "\n")
                .Replace("</ul>", "\n")
                .Replace("<strong>", "")
                .Replace("</strong>", "")
                .Replace("<b>", "")
                .Replace("</b>", "")
                .Replace("<td>", " | ")
                .Replace("</td>", "")
                .Replace("<tr>", "\n")
                .Replace("</tr>", "")
                .Replace("<table>", "\n")
                .Replace("</table>", "\n")
                .Replace("&nbsp;", " ");

            while (texto.Contains("<") && texto.Contains(">"))
            {
                var inicio = texto.IndexOf('<');
                var fim = texto.IndexOf('>', inicio);
                if (fim > inicio)
                {
                    texto = texto.Remove(inicio, fim - inicio + 1);
                }
                else
                {
                    break;
                }
            }

            while (texto.Contains("\n\n\n"))
            {
                texto = texto.Replace("\n\n\n", "\n\n");
            }

            return texto.Trim();
        }

        private Task GerarPdfDoTexto(string conteudo, string caminhoArquivo)
        {
            using var document = new Document(PageSize.A4, 50, 50, 25, 25);
            using var fs = new FileStream(caminhoArquivo, FileMode.Create);
            using var writer = PdfWriter.GetInstance(document, fs);

            document.Open();

            var fonteNormal = FontFactory.GetFont(FontFactory.HELVETICA, 11);
            var fonteTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var fonteSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(230, 126, 34));
            var fontePequena = FontFactory.GetFont(FontFactory.HELVETICA, 9, new BaseColor(128, 128, 128));

            var linhas = conteudo.Split('\n');

            foreach (var linha in linhas)
            {
                if (string.IsNullOrWhiteSpace(linha))
                {
                    document.Add(new Paragraph(" ", fonteNormal));
                    continue;
                }

                var paragraph = new Paragraph();
                
                if (linha.StartsWith("==="))
                {
                    paragraph.Add(new Phrase(linha.Replace("===", "").Trim(), fonteTitulo));
                    paragraph.Alignment = Element.ALIGN_CENTER;
                }
                else if (linha.StartsWith("=="))
                {
                    paragraph.Add(new Phrase(linha.Replace("==", "").Trim(), fonteSubtitulo));
                    paragraph.SpacingBefore = 10;
                }
                else if (linha.StartsWith("="))
                {
                    paragraph.Add(new Phrase(linha.Replace("=", "").Trim(), fonteSubtitulo));
                    paragraph.SpacingBefore = 8;
                }
                else if (linha.StartsWith("•"))
                {
                    paragraph.Add(new Phrase(linha, fonteNormal));
                    paragraph.IndentationLeft = 20;
                }
                else if (linha.Contains("Assinado em:") || linha.Contains("Contrato ID:"))
                {
                    paragraph.Add(new Phrase(linha, fontePequena));
                }
                else
                {
                    paragraph.Add(new Phrase(linha, fonteNormal));
                }

                document.Add(paragraph);
            }

            document.Close();
            return Task.CompletedTask;
        }

        private string GerarConteudoTextoContrato(ContratoAdocao contrato)
        {
            var dataAtual = contrato.DataCriacao.ToString("dd/MM/yyyy");
            var dataAssinatura = contrato.DataAssinatura?.ToString("dd/MM/yyyy HH:mm") ?? "Não informado";

            var conteudo = new StringBuilder();
            
            conteudo.AppendLine("=== CONTRATO DE ADOÇÃO DE ANIMAL ===");
            conteudo.AppendLine("=== CaotinhoAuMiau ===");
            conteudo.AppendLine();
            
            conteudo.AppendLine("== DADOS DO PET ==");
            conteudo.AppendLine($"Nome: {contrato.Adocao?.Pet?.Nome}");
            conteudo.AppendLine($"Espécie: {contrato.Adocao?.Pet?.Especie}");
            conteudo.AppendLine($"Raça: {contrato.Adocao?.Pet?.Raca}");
            conteudo.AppendLine($"Idade: {contrato.Adocao?.Pet?.Anos} anos e {contrato.Adocao?.Pet?.Meses} meses");
            conteudo.AppendLine($"Sexo: {contrato.Adocao?.Pet?.Sexo}");
            conteudo.AppendLine();
            
            conteudo.AppendLine("== DADOS DO ADOTANTE ==");
            conteudo.AppendLine($"Nome: {contrato.Adocao?.Usuario?.Nome}");
            conteudo.AppendLine($"Email: {contrato.Adocao?.Usuario?.Email}");
            conteudo.AppendLine($"Telefone: {contrato.Adocao?.Usuario?.Telefone}");
            conteudo.AppendLine();
            
            conteudo.AppendLine("== TERMOS E CONDIÇÕES ==");
            conteudo.AppendLine();
            conteudo.AppendLine("= 1. RESPONSABILIDADES DO ADOTANTE: =");
            conteudo.AppendLine("• Fornecer alimentação adequada, água fresca e abrigo ao animal;");
            conteudo.AppendLine("• Providenciar cuidados veterinários necessários, incluindo vacinação e vermifugação;");
            conteudo.AppendLine("• Manter o animal em ambiente seguro e adequado;");
            conteudo.AppendLine("• Não abandonar, maltratar ou ceder o animal a terceiros sem autorização;");
            conteudo.AppendLine("• Permitir visitas da equipe CaotinhoAuMiau para acompanhamento, se necessário.");
            conteudo.AppendLine();
            
            conteudo.AppendLine("= 2. COMPROMISSOS: =");
            conteudo.AppendLine("• O adotante se compromete a cuidar do animal com amor e responsabilidade;");
            conteudo.AppendLine("• Em caso de impossibilidade de manter o animal, o adotante deve entrar em contato com o CaotinhoAuMiau;");
            conteudo.AppendLine("• O animal não poderá ser comercializado ou utilizado para reprodução sem autorização;");
            conteudo.AppendLine("• Castração é altamente recomendada e pode ser condição para adoção.");
            conteudo.AppendLine();
            
            conteudo.AppendLine("= 3. RESCISÃO: =");
            conteudo.AppendLine("Este contrato pode ser rescindido em caso de descumprimento das condições estabelecidas, com a devolução do animal ao CaotinhoAuMiau.");
            conteudo.AppendLine();
            
            conteudo.AppendLine($"Data de criação: {dataAtual}");
            conteudo.AppendLine();
            conteudo.AppendLine("Declaro que li e concordo com todos os termos deste contrato.");
            conteudo.AppendLine();
            
            conteudo.AppendLine("== ASSINATURA DIGITAL ==");
            conteudo.AppendLine($"Assinado em: {dataAssinatura}");
            conteudo.AppendLine($"Contrato ID: #{contrato.Id}");
            conteudo.AppendLine("Assinatura digital capturada e autenticada pelo sistema CaotinhoAuMiau");
            conteudo.AppendLine();
            conteudo.AppendLine("Este documento foi gerado digitalmente e possui validade legal.");
            conteudo.AppendLine("CaotinhoAuMiau - Sistema de Adoção de Animais");

            return conteudo.ToString();
        }

        private Task GerarPdfComAssinatura(ContratoAdocao contrato, string caminhoArquivo)
        {
            using var document = new Document(PageSize.A4, 40, 40, 20, 20);
            using var fs = new FileStream(caminhoArquivo, FileMode.Create);
            using var writer = PdfWriter.GetInstance(document, fs);

            document.Open();

            var fonteNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var fonteTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
            var fonteSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, new BaseColor(230, 126, 34));
            var fontePequena = FontFactory.GetFont(FontFactory.HELVETICA, 9, new BaseColor(128, 128, 128));

            var titulo = new Paragraph("CONTRATO DE ADOÇÃO DE ANIMAL", fonteTitulo);
            titulo.Alignment = Element.ALIGN_CENTER;
            titulo.SpacingAfter = 5;
            document.Add(titulo);
            
            var subtitulo = new Paragraph("CaotinhoAuMiau", fonteSubtitulo);
            subtitulo.Alignment = Element.ALIGN_CENTER;
            subtitulo.SpacingAfter = 15;
            document.Add(subtitulo);

            var tabelaInfo = new PdfPTable(2);
            tabelaInfo.WidthPercentage = 100;
            tabelaInfo.SetWidths(new float[] { 50f, 50f });
            tabelaInfo.SpacingAfter = 15;

            var colunaPet = new PdfPCell();
            colunaPet.Border = Rectangle.BOX;
            colunaPet.Padding = 8;
            
            var secaoPet = new Paragraph("DADOS DO PET", fonteSubtitulo);
            secaoPet.SpacingAfter = 8;
            colunaPet.AddElement(secaoPet);
            
            colunaPet.AddElement(new Paragraph($"Nome: {contrato.Adocao?.Pet?.Nome ?? ""}", fonteNormal));
            colunaPet.AddElement(new Paragraph(" ", fonteNormal));
            colunaPet.AddElement(new Paragraph($"Espécie: {contrato.Adocao?.Pet?.Especie.ToString() ?? ""}", fonteNormal));
            colunaPet.AddElement(new Paragraph(" ", fonteNormal));
            colunaPet.AddElement(new Paragraph($"Raça: {contrato.Adocao?.Pet?.Raca ?? ""}", fonteNormal));
            colunaPet.AddElement(new Paragraph(" ", fonteNormal));
            colunaPet.AddElement(new Paragraph($"Idade: {contrato.Adocao?.Pet?.Anos ?? 0} anos e {contrato.Adocao?.Pet?.Meses ?? 0} meses", fonteNormal));
            colunaPet.AddElement(new Paragraph(" ", fonteNormal));
            colunaPet.AddElement(new Paragraph($"Sexo: {contrato.Adocao?.Pet?.Sexo.ToString() ?? ""}", fonteNormal));

            var colunaAdotante = new PdfPCell();
            colunaAdotante.Border = Rectangle.BOX;
            colunaAdotante.Padding = 8;
            
            var secaoAdotante = new Paragraph("DADOS DO ADOTANTE", fonteSubtitulo);
            secaoAdotante.SpacingAfter = 8;
            colunaAdotante.AddElement(secaoAdotante);
            
            colunaAdotante.AddElement(new Paragraph($"Nome: {contrato.Adocao?.Usuario?.Nome ?? ""}", fonteNormal));
            colunaAdotante.AddElement(new Paragraph(" ", fonteNormal));
            colunaAdotante.AddElement(new Paragraph($"Email: {contrato.Adocao?.Usuario?.Email ?? ""}", fonteNormal));
            colunaAdotante.AddElement(new Paragraph(" ", fonteNormal));
            colunaAdotante.AddElement(new Paragraph($"Telefone: {contrato.Adocao?.Usuario?.Telefone ?? ""}", fonteNormal));
            colunaAdotante.AddElement(new Paragraph(" ", fonteNormal));
            colunaAdotante.AddElement(new Paragraph($"Data de criação: {contrato.DataCriacao:dd/MM/yyyy}", fonteNormal));

            tabelaInfo.AddCell(colunaPet);
            tabelaInfo.AddCell(colunaAdotante);
            document.Add(tabelaInfo);

            var secaoTermos = new Paragraph("TERMOS E CONDIÇÕES", fonteSubtitulo);
            secaoTermos.SpacingBefore = 15;
            secaoTermos.SpacingAfter = 10;
            document.Add(secaoTermos);

            var resp = new Paragraph("1. RESPONSABILIDADES DO ADOTANTE:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10));
            resp.SpacingAfter = 5;
            document.Add(resp);
            
            var listaResp = new Paragraph("• Fornecer alimentação adequada, água fresca e abrigo ao animal;\n" +
                                        "• Providenciar cuidados veterinários necessários, incluindo vacinação;\n" +
                                        "• Manter o animal em ambiente seguro e adequado;\n" +
                                        "• Não abandonar, maltratar ou ceder sem autorização.", fonteNormal);
            listaResp.IndentationLeft = 15;
            listaResp.SpacingAfter = 8;
            document.Add(listaResp);

            var comp = new Paragraph("2. COMPROMISSOS:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10));
            comp.SpacingAfter = 5;
            document.Add(comp);
            
            var listaComp = new Paragraph("• Cuidar do animal com amor e responsabilidade;\n" +
                                        "• Contatar CaotinhoAuMiau em caso de necessidade;\n" +
                                        "• Não comercializar o animal; castração recomendada.", fonteNormal);
            listaComp.IndentationLeft = 15;
            listaComp.SpacingAfter = 8;
            document.Add(listaComp);

            var resc = new Paragraph("3. RESCISÃO:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10));
            resc.SpacingAfter = 5;
            document.Add(resc);
            
            var textoResc = new Paragraph("Este contrato pode ser rescindido em caso de descumprimento das condições estabelecidas, com a devolução do animal ao CaotinhoAuMiau.", fonteNormal);
            textoResc.IndentationLeft = 15;
            textoResc.SpacingAfter = 15;
            document.Add(textoResc);

            var declaracao = new Paragraph("Declaro que li e concordo com todos os termos deste contrato.", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11));
            declaracao.Alignment = Element.ALIGN_CENTER;
            declaracao.SpacingAfter = 15;
            document.Add(declaracao);

            var secaoAssinatura = new Paragraph("ASSINATURA DIGITAL", fonteSubtitulo);
            secaoAssinatura.SpacingBefore = 10;
            secaoAssinatura.SpacingAfter = 8;
            document.Add(secaoAssinatura);

            var tabelaAssinatura = new PdfPTable(2);
            tabelaAssinatura.WidthPercentage = 100;
            tabelaAssinatura.SetWidths(new float[] { 60f, 40f });

            var infoAssinatura = new PdfPCell();
            infoAssinatura.Border = Rectangle.BOX;
            infoAssinatura.Padding = 10;
            
            var dataAssinatura = contrato.DataAssinatura?.ToString("dd/MM/yyyy HH:mm") ?? "Não informado";
            var info = new Paragraph();
            info.Add(new Phrase("Assinado digitalmente em:\n", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)));
            info.Add(new Phrase($"{dataAssinatura}\n\n", fonteNormal));
            info.Add(new Phrase($"Contrato ID: #{contrato.Id}\n", fontePequena));
            info.Add(new Phrase("Assinatura verificada e autenticada\npelo sistema CaotinhoAuMiau", fontePequena));
            infoAssinatura.AddElement(info);

            var celulaAssinatura = new PdfPCell();
            celulaAssinatura.Border = Rectangle.BOX;
            celulaAssinatura.Padding = 10;
            celulaAssinatura.HorizontalAlignment = Element.ALIGN_CENTER;
            celulaAssinatura.VerticalAlignment = Element.ALIGN_MIDDLE;

            try
            {
                var assinaturaUsuario = contrato.AssinaturaUsuario ?? "";
                
                if (!string.IsNullOrEmpty(assinaturaUsuario))
                {
                    if (assinaturaUsuario.StartsWith("data:image"))
                    {
                        var base64Data = assinaturaUsuario.Split(',')[1];
                        var imageBytes = Convert.FromBase64String(base64Data);
                        
                        var image = Image.GetInstance(imageBytes);
                        image.ScaleToFit(140f, 70f);
                        celulaAssinatura.AddElement(image);
                    }
                    else
                    {
                        var assinaturaBase64 = _assinaturaService.ExtrairImagemBase64(assinaturaUsuario);
                        
                        if (!string.IsNullOrEmpty(assinaturaBase64) && assinaturaBase64.StartsWith("data:image"))
                        {
                            var base64Data = assinaturaBase64.Split(',')[1];
                            var imageBytes = Convert.FromBase64String(base64Data);
                            
                            var image = Image.GetInstance(imageBytes);
                            image.ScaleToFit(140f, 70f);
                            celulaAssinatura.AddElement(image);
                        }
                        else
                        {
                            celulaAssinatura.AddElement(new Paragraph($"Formato não reconhecido: {assinaturaUsuario.Substring(0, Math.Min(50, assinaturaUsuario.Length))}...", fontePequena));
                        }
                    }
                }
                else
                {
                    celulaAssinatura.AddElement(new Paragraph("Assinatura não encontrada no banco", fontePequena));
                }
            }
            catch (Exception ex)
            {
                celulaAssinatura.AddElement(new Paragraph($"Erro: {ex.Message}", fontePequena));
            }

            tabelaAssinatura.AddCell(infoAssinatura);
            tabelaAssinatura.AddCell(celulaAssinatura);
            document.Add(tabelaAssinatura);

            var infoFinal = new Paragraph("Este documento foi gerado digitalmente e possui validade legal.\nCaotinhoAuMiau - Sistema de Adoção de Animais", fontePequena);
            infoFinal.Alignment = Element.ALIGN_CENTER;
            infoFinal.SpacingBefore = 15;
            document.Add(infoFinal);

            document.Close();
            return Task.CompletedTask;
        }

        private void AdicionarCelulasTabela(PdfPTable tabela, string label, string valor)
        {
            var fonteLabel = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var fonteValor = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            
            var celulaLabel = new PdfPCell(new Phrase(label, fonteLabel));
            celulaLabel.BackgroundColor = new BaseColor(249, 249, 249);
            celulaLabel.Border = Rectangle.BOX;
            celulaLabel.Padding = 8;
            
            var celulaValor = new PdfPCell(new Phrase(valor, fonteValor));
            celulaValor.Border = Rectangle.BOX;
            celulaValor.Padding = 8;
            
            tabela.AddCell(celulaLabel);
            tabela.AddCell(celulaValor);
        }

        private string GerarHtmlContrato(ContratoAdocao contrato)
        {
            var assinaturaBase64 = _assinaturaService.ExtrairImagemBase64(contrato.AssinaturaUsuario ?? "");
            var dataAssinatura = contrato.DataAssinatura?.ToString("dd/MM/yyyy HH:mm") ?? "Não informado";

            return $@"
<!DOCTYPE html>
<html lang='pt-BR'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Contrato de Adoção - {contrato.Adocao?.Pet?.Nome}</title>
    <style>
        body {{
            font-family: 'Arial', sans-serif;
            line-height: 1.6;
            margin: 0;
            padding: 20px;
            color: #333;
        }}
        .container {{
            max-width: 800px;
            margin: 0 auto;
            background: white;
            box-shadow: 0 0 20px rgba(0,0,0,0.1);
            padding: 40px;
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
            border-bottom: 3px solid #E67E22;
            padding-bottom: 20px;
        }}
        .header h1 {{
            color: #E67E22;
            margin: 0;
            font-size: 24px;
        }}
        .header h2 {{
            color: #D35400;
            margin: 10px 0 0 0;
            font-size: 18px;
        }}
        .section {{
            margin-bottom: 25px;
        }}
        .section h3 {{
            color: #E67E22;
            border-bottom: 2px solid #E67E22;
            padding-bottom: 5px;
            margin-bottom: 15px;
        }}
        .info-table {{
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
        }}
        .info-table td {{
            padding: 10px;
            border: 1px solid #ddd;
        }}
        .info-table td:first-child {{
            background-color: #f9f9f9;
            font-weight: bold;
            width: 30%;
        }}
        .terms {{
            text-align: justify;
        }}
        .terms ul {{
            margin-left: 20px;
        }}
        .terms li {{
            margin-bottom: 5px;
        }}
        .signature-section {{
            margin-top: 40px;
            border-top: 2px solid #E67E22;
            padding-top: 20px;
        }}
        .signature-info {{
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
        }}
        .signature-image {{
            border: 2px solid #E67E22;
            padding: 10px;
            border-radius: 5px;
            background: #f9f9f9;
            text-align: center;
            max-width: 300px;
        }}
        .signature-image img {{
            max-width: 100%;
            height: auto;
        }}
        .contract-info {{
            background: #f8f9fa;
            padding: 15px;
            border-radius: 5px;
            margin-top: 20px;
            font-size: 12px;
            color: #666;
        }}
        @media print {{
            body {{ margin: 0; }}
            .container {{ box-shadow: none; }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>CONTRATO DE ADOÇÃO DE ANIMAL</h1>
            <h2>CaotinhoAuMiau</h2>
        </div>

        <div class='section'>
            <h3>DADOS DO PET</h3>
            <table class='info-table'>
                <tr>
                    <td>Nome:</td>
                    <td>{contrato.Adocao?.Pet?.Nome}</td>
                </tr>
                <tr>
                    <td>Espécie:</td>
                    <td>{contrato.Adocao?.Pet?.Especie}</td>
                </tr>
                <tr>
                    <td>Raça:</td>
                    <td>{contrato.Adocao?.Pet?.Raca}</td>
                </tr>
                <tr>
                    <td>Idade:</td>
                    <td>{contrato.Adocao?.Pet?.Anos} anos e {contrato.Adocao?.Pet?.Meses} meses</td>
                </tr>
                <tr>
                    <td>Sexo:</td>
                    <td>{contrato.Adocao?.Pet?.Sexo}</td>
                </tr>
            </table>
        </div>

        <div class='section'>
            <h3>DADOS DO ADOTANTE</h3>
            <table class='info-table'>
                <tr>
                    <td>Nome:</td>
                    <td>{contrato.Adocao?.Usuario?.Nome}</td>
                </tr>
                <tr>
                    <td>Email:</td>
                    <td>{contrato.Adocao?.Usuario?.Email}</td>
                </tr>
                <tr>
                    <td>Telefone:</td>
                    <td>{contrato.Adocao?.Usuario?.Telefone}</td>
                </tr>
            </table>
        </div>

        <div class='section'>
            <h3>TERMOS E CONDIÇÕES</h3>
            <div class='terms'>
                <p><strong>1. RESPONSABILIDADES DO ADOTANTE:</strong></p>
                <ul>
                    <li>Fornecer alimentação adequada, água fresca e abrigo ao animal;</li>
                    <li>Providenciar cuidados veterinários necessários, incluindo vacinação e vermifugação;</li>
                    <li>Manter o animal em ambiente seguro e adequado;</li>
                    <li>Não abandonar, maltratar ou ceder o animal a terceiros sem autorização;</li>
                    <li>Permitir visitas da equipe CaotinhoAuMiau para acompanhamento, se necessário.</li>
                </ul>

                <p><strong>2. COMPROMISSOS:</strong></p>
                <ul>
                    <li>O adotante se compromete a cuidar do animal com amor e responsabilidade;</li>
                    <li>Em caso de impossibilidade de manter o animal, o adotante deve entrar em contato com o CaotinhoAuMiau;</li>
                    <li>O animal não poderá ser comercializado ou utilizado para reprodução sem autorização;</li>
                    <li>Castração é altamente recomendada e pode ser condição para adoção.</li>
                </ul>

                <p><strong>3. RESCISÃO:</strong></p>
                <p>Este contrato pode ser rescindido em caso de descumprimento das condições estabelecidas, com a devolução do animal ao CaotinhoAuMiau.</p>
            </div>
        </div>

        <div class='signature-section'>
            <h3>ASSINATURA DIGITAL</h3>
            <div class='signature-info'>
                <div>
                    <p><strong>Data da Assinatura:</strong> {dataAssinatura}</p>
                    <p><strong>Contrato ID:</strong> #{contrato.Id}</p>
                </div>
            </div>
            
            {(string.IsNullOrEmpty(assinaturaBase64) ? 
                "<p style='color: red;'>Assinatura não disponível</p>" : 
                $@"<div class='signature-image'>
                    <p><strong>Assinatura do Adotante:</strong></p>
                    <img src='{assinaturaBase64}' alt='Assinatura Digital' />
                    <p style='font-size: 12px; color: #666; margin-top: 10px;'>
                        Assinatura digital capturada em {dataAssinatura}
                    </p>
                </div>")}
        </div>

        <div class='contract-info'>
            <p><strong>Informações do Contrato:</strong></p>
            <p>Contrato ID: #{contrato.Id} | Data de Criação: {contrato.DataCriacao:dd/MM/yyyy} | Status: {contrato.StatusContrato}</p>
            <p>Este documento foi gerado digitalmente e possui validade legal.</p>
            <p><strong>CaotinhoAuMiau</strong> - Sistema de Adoção de Animais</p>
        </div>
    </div>
</body>
</html>";
        }

        public byte[] GerarContratoPdf(string conteudoContrato, string nomePet, string nomeAdotante)
        {
            using var memoryStream = new MemoryStream();
            using var document = new Document(PageSize.A4, 40, 40, 20, 20);
            using var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            var fonteNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var fonteTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
            var fonteSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, new BaseColor(230, 126, 34));
            var fontePequena = FontFactory.GetFont(FontFactory.HELVETICA, 9, new BaseColor(128, 128, 128));

            try
            {
                var titulo = new Paragraph("CONTRATO DE ADOÇÃO DE ANIMAL", fonteTitulo);
                titulo.Alignment = Element.ALIGN_CENTER;
                titulo.SpacingAfter = 10;
                document.Add(titulo);

                var subtitulo = new Paragraph("CaotinhoAuMiau", fonteSubtitulo);
                subtitulo.Alignment = Element.ALIGN_CENTER;
                subtitulo.SpacingAfter = 20;
                document.Add(subtitulo);

                if (conteudoContrato.Contains("<") && conteudoContrato.Contains(">"))
                {
                    try
                    {
                        var styles = new StyleSheet();
                        styles.LoadTagStyle("body", "font-family", "Arial, sans-serif");
                        styles.LoadTagStyle("body", "font-size", "10px");
                        styles.LoadTagStyle("h1", "font-size", "14px");
                        styles.LoadTagStyle("h2", "font-size", "12px");
                        styles.LoadTagStyle("h3", "font-size", "11px");
                        styles.LoadTagStyle("p", "font-size", "10px");
                        styles.LoadTagStyle("td", "font-size", "9px");

                        var htmlWorker = new HtmlWorker(document);
                        var reader = new StringReader(conteudoContrato);
                        htmlWorker.Parse(reader);
                    }
                    catch
                    {
                        var textoLimpo = LimparHtml(conteudoContrato);
                        ProcessarTextoFormatado(document, textoLimpo, fonteNormal, fonteSubtitulo);
                    }
                }
                else
                {
                    ProcessarTextoFormatado(document, conteudoContrato, fonteNormal, fonteSubtitulo);
                }
            }
            catch (Exception)
            {
                var paragraph = new Paragraph(conteudoContrato, fonteNormal);
                document.Add(paragraph);
            }

            var infoFinal = new Paragraph("\nEste documento foi gerado digitalmente e possui validade legal.\nCaotinhoAuMiau - Sistema de Adoção de Animais", fontePequena);
            infoFinal.Alignment = Element.ALIGN_CENTER;
            infoFinal.SpacingBefore = 15;
            document.Add(infoFinal);

            document.Close();
            return memoryStream.ToArray();
        }

        private void ProcessarTextoFormatado(Document document, string texto, Font fonteNormal, Font fonteSubtitulo)
        {
            var linhas = texto.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var linha in linhas)
            {
                var linhaTrimmed = linha.Trim();

                if (string.IsNullOrEmpty(linhaTrimmed))
                {
                    document.Add(new Paragraph(" ", fonteNormal));
                    continue;
                }

                var paragraph = new Paragraph();
                paragraph.SpacingAfter = 5;

                if (linhaTrimmed.ToUpper().Contains("CONTRATO") && linhaTrimmed.ToUpper().Contains("ADOÇÃO"))
                {
                    paragraph.Add(new Phrase(linhaTrimmed, fonteSubtitulo));
                    paragraph.Alignment = Element.ALIGN_CENTER;
                    paragraph.SpacingAfter = 15;
                }
                else if (linhaTrimmed.EndsWith(":") && linhaTrimmed.Length < 50)
                {
                    paragraph.Add(new Phrase(linhaTrimmed, fonteSubtitulo));
                    paragraph.SpacingBefore = 8;
                }
                else if (linhaTrimmed.StartsWith("•") || linhaTrimmed.StartsWith("-"))
                {
                    paragraph.Add(new Phrase(linhaTrimmed, fonteNormal));
                    paragraph.IndentationLeft = 15;
                }
                else if (linhaTrimmed.ToLower().Contains("assinatura") && linhaTrimmed.ToLower().Contains("digital"))
                {
                    var fontePequena = FontFactory.GetFont(FontFactory.HELVETICA, 9, new BaseColor(128, 128, 128));
                    paragraph.Add(new Phrase(linhaTrimmed, fontePequena));
                }
                else
                {
                    paragraph.Add(new Phrase(linhaTrimmed, fonteNormal));
                }

                document.Add(paragraph);
            }
        }
    }
}