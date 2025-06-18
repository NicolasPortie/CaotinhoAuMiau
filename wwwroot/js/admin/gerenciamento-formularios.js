function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text ?? '';
    return div.innerHTML;
}

// Corrigir problema de paginação
$(document).ready(function() {
    // Manipulador para links de paginação existentes
    $('.pagination .page-link').on('click', function(e) {
        e.preventDefault();
        var url = $(this).attr('href');
        if (url) {
            window.location.href = url;
        }
        return false;
    });
    
    // Delegação de eventos para links de paginação que podem ser adicionados dinamicamente
    $(document).on('click', '.pagination .page-link', function(e) {
        e.preventDefault();
        var url = $(this).attr('href');
        if (url) {
            window.location.href = url;
        }
        return false;
    });
});

function filtrarFormularios() {
    const pesquisa = $('#campoPesquisaForm').val().toLowerCase();
    const status = $('#filtroStatus').val();
    const data = $('#filtroData').val();
    
    
    const hoje = new Date();
    hoje.setHours(0, 0, 0, 0);
    
    const ultimos7Dias = new Date();
    ultimos7Dias.setDate(ultimos7Dias.getDate() - 7);
    
    const ultimos30Dias = new Date();
    ultimos30Dias.setDate(ultimos30Dias.getDate() - 30);
    
    $('table tbody tr').each(function() {
        const linha = $(this);
        const nome = linha.find('.nome-cliente').text().toLowerCase();
        const email = linha.find('small:contains("@")').text().toLowerCase();
        const endereco = linha.find('small:contains("fa-map-marker-alt")').text().toLowerCase();
        const statusLinha = linha.find('.indicador-status').text().toLowerCase();
        const textoData = linha.find('.data').text();
        let correspondenciaData = true;
        
        if (data) {
            const dataFormulario = analisarData(textoData);
            if (data === 'hoje') {
                correspondenciaData = dataFormulario >= hoje;
            } else if (data === '7dias') {
                correspondenciaData = dataFormulario >= ultimos7Dias;
            } else if (data === '30dias') {
                correspondenciaData = dataFormulario >= ultimos30Dias;
            }
        }
        
        const correspondenciaNome = nome.includes(pesquisa) || email.includes(pesquisa) || endereco.includes(pesquisa);
        const correspondenciaStatus = !status || statusLinha === status.toLowerCase();
        
        if (correspondenciaNome && correspondenciaStatus && correspondenciaData) {
            linha.show();
        } else {
            linha.hide();
        }
    });
    
    verificarResultadosPorSecao();
}

function analisarData(textoData) {
    const partes = textoData.split(' ')[0].split('/');
    return new Date(partes[2], partes[1] - 1, partes[0]);
}


function verificarResultadosPorSecao() {
    const secoes = [
        { container: '.tabela-formularios', mensagem: '.mensagem-sem-formularios' }
    ];
    
    secoes.forEach(secao => {
        const linhasVisiveis = $(`${secao.container} tbody tr:visible`).length;
        if (linhasVisiveis === 0) {
            $(secao.mensagem).show();
        } else {
            $(secao.mensagem).hide();
        }
    });
}


$('#campoPesquisaForm, #filtroStatus, #filtroData').on('input change', filtrarFormularios);


function limparFiltros() {
    
    $('#campoPesquisaForm').val('');
    $('#filtroStatus').val('');
    $('#filtroData').val('');
    
    filtrarFormularios();
    
    toastr.info('Filtros limpos com sucesso!');
}

let modalDetalhesFormulario;
let idFormularioAtual;

const coresStatus = {
    'Pendente': 'pendente',
    'Aprovado': 'aprovado',
    'Rejeitada': 'rejeitado',
    'Cancelada': 'cancelado',
    'Cancelado pelo Usuario': 'cancelado',
    'Cancelado pelo Admin': 'cancelado',
    'Aguardando Busca': 'aguardando-buscar'
};


function limparAlertasModal() {
    $('#modalDetalhesFormulario .alert').remove();
}


function visualizarFormulario(id) {
    
    
    limparAlertasModal();
    
    
    $('#formularioIdAtual').val(id);
    
    
    $('#conteudoDetalhesFormulario').html(`
        <div class="d-flex justify-content-center my-5">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Carregando...</span>
            </div>
        </div>
    `);
    
    
    resetarPaineisConfirmacao();
    
    
    $('#modalDetalhesFormulario').modal('show');
    
    
    $.ajax({
        url: `/admin/formularios-adocao/detalhes/${id}`,
        type: 'GET',
        success: function(resposta) {
            
            
            if (typeof resposta === 'string' && resposta.trim().startsWith('{')) {
                try {
                    resposta = JSON.parse(resposta);
                } catch (e) {
                    console.error("Erro ao converter resposta de string para objeto:", e);
                }
            }
            
            
            if (resposta) {
                if (resposta.formulario) {
                }
            }
            
            
            if (resposta.sucesso !== undefined) {
                
                if (resposta.sucesso) {
                    
                    if (resposta.html) {
                        $('#conteudoDetalhesFormulario').html(resposta.html);
                    } else if (resposta.formulario) {
                        
                        const htmlConteudo = construirHTMLDetalhesFormulario(resposta.formulario);
                        $('#conteudoDetalhesFormulario').html(htmlConteudo);
                    }
                    
                    
                    if (resposta.observacoesAdmin) {
                        $('#observacaoAdmin').val(resposta.observacoesAdmin);
                    } else if (resposta.formulario && resposta.formulario.observacaoAdminFormulario) {
                        $('#observacaoAdmin').val(resposta.formulario.observacaoAdminFormulario);
                    }
                    
                    
                    const status = resposta.status || (resposta.formulario ? resposta.formulario.status : null);
                    
                    if (status !== 'Pendente') {
                        
                        $('#botoesAcaoPrimarios').addClass('d-none');
                        $('#observacaoAdmin').prop('readonly', true);
                    } else {
                        
                        $('#botoesAcaoPrimarios').removeClass('d-none');
                        $('#observacaoAdmin').prop('readonly', false);
                    }
                } else {
                    $('#conteudoDetalhesFormulario').html(`
                        <div class="alert alert-warning">
                            <i class="fas fa-exclamation-triangle me-2"></i>
                            ${resposta.mensagem || 'Erro ao carregar detalhes do formulário'}
                        </div>
                    `);
                }
            } else if (resposta.formulario !== undefined) {
                
                const htmlConteudo = construirHTMLDetalhesFormulario(resposta.formulario);
                $('#conteudoDetalhesFormulario').html(htmlConteudo);
                
                
                $('#observacaoAdmin').val(resposta.formulario.observacaoAdminFormulario || '');
                
                if (resposta.formulario.status !== 'Pendente') {
                    
                    $('#botoesAcaoPrimarios').addClass('d-none');
                    $('#observacaoAdmin').prop('readonly', true);
                } else {
                    
                    $('#botoesAcaoPrimarios').removeClass('d-none');
                    $('#observacaoAdmin').prop('readonly', false);
                }
            } else {
                
                try {
                    
                    
                    let objFormulario = null;
                    
                    if (resposta.data && typeof resposta.data === 'object') {
                        
                        objFormulario = resposta.data;
                    } else if (resposta.resultado && typeof resposta.resultado === 'object') {
                        
                        objFormulario = resposta.resultado;
                    } else if (resposta.id !== undefined && resposta.status !== undefined) {
                        
                        objFormulario = resposta;
                    } else {
                        
                        for (const key in resposta) {
                            if (
                                resposta[key] && 
                                typeof resposta[key] === 'object' && 
                                resposta[key].id !== undefined && 
                                resposta[key].status !== undefined
                            ) {
                                objFormulario = resposta[key];
                                break;
                            }
                        }
                        
                        
                        if (!objFormulario) {
                            objFormulario = resposta;
                        }
                    }
                    
                    
                    const htmlConteudo = construirHTMLDetalhesFormulario(objFormulario);
                    $('#conteudoDetalhesFormulario').html(htmlConteudo);
                    
                    
                    const observacao = objFormulario.observacaoAdminFormulario || 
                                      objFormulario.observacaoAdmin || 
                                      objFormulario.observacoes || 
                                      '';
                    $('#observacaoAdmin').val(observacao);
                    
                    
                    if (objFormulario.status !== 'Pendente') {
                        
                        $('#botoesAcaoPrimarios').addClass('d-none');
                        $('#observacaoAdmin').prop('readonly', true);
                    } else {
                        
                        $('#botoesAcaoPrimarios').removeClass('d-none');
                        $('#observacaoAdmin').prop('readonly', false);
                    }
                } catch (erro) {
                    console.error("Erro ao processar resposta:", erro);
                    $('#conteudoDetalhesFormulario').html(`
                        <div class="alert alert-danger">
                            <i class="fas fa-times-circle me-2"></i>
                            Ocorreu um erro ao processar os detalhes do formulário.
                        </div>
                    `);
                }
            }
            
            
            $('#botaoAprovarNoModal').off('click').on('click', function() {
                exibirConfirmacaoAprovacao();
            });
            
            $('#botaoRejeitarNoModal').off('click').on('click', function() {
                exibirConfirmacaoRejeicao();
            });

            
            $('#botaoCancelarAprovacao, #botaoCancelarRejeicao').off('click').on('click', function() {
                resetarPaineisConfirmacao();
            });

            
            $('#botaoConfirmarAprovacao').off('click').on('click', function() {
                aprovarFormularioConfirmado();
            });

            $('#botaoConfirmarRejeicao').off('click').on('click', function() {
                rejeitarFormularioConfirmado();
            });
        },
        error: function(xhr, status, erro) {
            $('#conteudoDetalhesFormulario').html(`
                <div class="alert alert-danger">
                    <i class="fas fa-times-circle me-2"></i>
                    Ocorreu um erro ao carregar os detalhes do formulário.
                </div>
            `);
            console.error("Erro na requisição:", status, erro);
        }
    });
}


function construirHTMLDetalhesFormulario(formulario) {
    try {

        const s = escapeHtml;
        
        
        if (!formulario || typeof formulario !== 'object') {
            console.error("Objeto de formulário inválido:", formulario);
            return `
                <div class="alert alert-danger">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    <strong>Erro ao processar formulário</strong>
                    <p>Dados do formulário inválidos ou incompletos.</p>
                </div>
            `;
        }
        
        
        
        
        const dataEnvio = new Date(formulario.dataEnvio);
        const dataFormatada = dataEnvio.toLocaleString('pt-BR');
        const dataResposta = formulario.dataResposta ? new Date(formulario.dataResposta).toLocaleString('pt-BR') : '-';
        
        
        let htmlConteudo = `
            <div class="container-fluid p-0">
                <!-- Cabeçalho com informações básicas -->
                <div class="card shadow-sm mb-4">
                    <div class="card-header bg-light">
                        <div class="d-flex align-items-center">
                            <h4 class="mb-0 fw-bold text-primary fs-5">
                                <i class="fas fa-file-alt me-2"></i>
                                Formulário #${s(formulario.id)}
                            </h4>
                        </div>
                    </div>
                    <div class="card-body">
                        <div class="row small">
                            <div class="col-md-6">
                                <div class="mb-2">
                                    <i class="fas fa-calendar-alt text-primary me-2"></i>
                                    <strong>Data de envio:</strong> ${dataFormatada}
                                </div>
                            </div>
                            ${formulario.dataResposta ? `
                            <div class="col-md-6">
                                <div class="mb-2">
                                    <i class="fas fa-check-circle text-primary me-2"></i>
                                    <strong>Data de resposta:</strong> ${dataResposta}
                                </div>
                            </div>` : ''}
                        </div>
                    </div>
                </div>
        `;

        
        if ((formulario.status === 'Cancelada' || formulario.status === 'Cancelado pelo Admin') && formulario.observacaoAdminFormulario) {
            htmlConteudo += `
                <div class="alert alert-warning mb-4">
                    <div class="d-flex">
                        <div class="me-3">
                            <i class="fas fa-exclamation-triangle fa-2x text-warning"></i>
                        </div>
                        <div>
                            <h5 class="mb-1 fw-bold">Motivo do Cancelamento (Admin)</h5>
                            <p class="mb-0">${s(formulario.observacaoAdminFormulario || 'Não informado')}</p>
                        </div>
                    </div>
                </div>
            `;
        }

        
        if ((formulario.status === 'Cancelada' || formulario.status === 'Cancelado pelo Usuário') && formulario.observacoesCancelamento) {
            htmlConteudo += `
                <div class="alert alert-info mb-4">
                    <div class="d-flex">
                        <div class="me-3">
                            <i class="fas fa-comment-alt fa-2x text-info"></i>
                        </div>
                        <div>
                            <h5 class="mb-1 fw-bold">Motivo do Cancelamento (Usuário)</h5>
                            <p class="mb-0">${s(formulario.observacoesCancelamento || 'Não informado')}</p>
                        </div>
                    </div>
                </div>
            `;
        }
        
        
        htmlConteudo += `
            <div class="row mb-4">
                <!-- Dados do Interessado -->
                <div class="col-md-6 mb-3">
                    <div class="card h-100 shadow-sm">
                        <div class="card-header bg-light">
                            <h5 class="mb-0 fw-bold text-primary">
                                <i class="fas fa-user me-2"></i>Dados do Interessado
                            </h5>
                        </div>
                        <div class="card-body">
                            <div class="d-flex mb-3">
                                <div class="me-3">
                                    <div class="container-foto-usuario ${!formulario.usuario?.fotoPerfil ? 'sem-foto' : ''}" style="width: 64px; height: 64px; border-radius: 50%; display: flex; align-items: center; justify-content: center; background-color: ${!formulario.usuario?.fotoPerfil ? '#F59E0B' : 'transparent'}; color: white; font-weight: bold; font-size: 1.5rem;">
                                        ${!formulario.usuario?.fotoPerfil 
                                            ? `<span class="iniciais">${formulario.usuario?.nome?.charAt(0) || '?'}</span>` 
                                            : `<img src="/imagens/perfil/${formulario.usuario.fotoPerfil}" alt="${s(formulario.usuario.nome || '')}" style="width: 100%; height: 100%; object-fit: cover; border-radius: 50%;" />`
                                        }
                                    </div>
                                </div>
                                <div>
                                    <h6 class="mb-1 fw-bold fs-6">${s(formulario.usuario?.nome || 'Nome não informado')}</h6>
                                    <div class="text-muted small">
                                        <div><i class="fas fa-envelope me-1"></i>${s(formulario.usuario?.email || 'Email não informado')}</div>
                                        <div><i class="fas fa-phone me-1"></i>${s(formulario.usuario?.telefone || 'Telefone não informado')}</div>
                                        <div><i class="fas fa-map-marker-alt me-1"></i>${s(formulario.usuario?.cidade || 'Cidade não informada')}${formulario.usuario?.estado ? ` - ${s(formulario.usuario.estado)}` : ''}</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                
                <!-- Pet de Interesse -->
                <div class="col-md-6 mb-3">
                    <div class="card h-100 shadow-sm">
                        <div class="card-header bg-light">
                            <h5 class="mb-0 fw-bold text-primary">
                                <i class="fas fa-paw me-2"></i>Pet de Interesse
                            </h5>
                        </div>
                        <div class="card-body">
                            <div class="d-flex mb-3">
                                <div class="me-3">
                                    <div style="width: 64px; height: 64px; border-radius: 8px; overflow: hidden;">
                                        <img src="${formulario.pet?.nomeArquivoImagem 
                                            ? `/imagens/pets/${formulario.pet.nomeArquivoImagem}` 
                                            : `/imagens/pets/pet-placeholder.jpg`}" 
                                            alt="${s(formulario.pet?.nome || 'Pet')}" style="width: 100%; height: 100%; object-fit: cover;">
                                    </div>
                                </div>
                                <div>
                                    <h6 class="mb-1 fw-bold fs-6">${s(formulario.pet?.nome || 'Nome não informado')}</h6>
                                    <div class="text-muted small">
                                        <div>
                                            <i class="${formulario.pet?.especie === 'Cachorro' ? 'fas fa-dog' : 'fas fa-cat'} me-1"></i>
                                            ${s(formulario.pet?.especie || 'Espécie não informada')}
                                        </div>
                                        <div>
                                            <i class="fas fa-ruler me-1"></i>
                                            ${s(formulario.pet?.porte || 'Porte não informado')}
                                        </div>
                                        <div>
                                            <i class="fas fa-venus-mars me-1"></i>
                                            ${s(formulario.pet?.sexo || 'Sexo não informado')}
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            
            <!-- Informações do Formulário -->
            <div class="card shadow-sm mb-4">
                <div class="card-header bg-light">
                    <h5 class="mb-0 fw-bold text-primary">
                        <i class="fas fa-clipboard-list me-2"></i>Informações do Formulário
                    </h5>
                </div>
                <div class="card-body p-0">
                    <div class="table-responsive">
                        <table class="table table-hover mb-0">
                            <tbody>
                                <tr>
                                    <th style="width: 30%">Por que você quer adotar este pet?</th>
                                    <td>${s(formulario.motivacaoAdocao || 'Não informado')}</td>
                                </tr>
                                <tr>
                                    <th>Conte-nos sobre sua experiência com pets</th>
                                    <td>${s(formulario.experienciaAnterior || 'Não informado')}</td>
                                </tr>
                                <tr>
                                    <th>Qual espaço o pet terá disponível?</th>
                                    <td>${s(formulario.espacoAdequado || 'Não informado')}</td>
                                </tr>
                                <tr>
                                    <th>O que fará com o pet quando precisar viajar?</th>
                                    <td>${s(formulario.planejamentoViagens || 'Não informado')}</td>
                                </tr>
                                <tr>
                                    <th>Como planeja arcar com os custos do pet?</th>
                                    <td>${s(formulario.condicoesFinanceiras || 'Não informado')}</td>
                                </tr>
                                <tr>
                                    <th>Descreva sua moradia</th>
                                    <td>${s(formulario.descricaoMoradia || 'Não informado')}</td>
                                </tr>
                                <tr>
                                    <th>Tipo de residência</th>
                                    <td>${s(formulario.tipoResidencia || 'Não informado')}</td>
                                </tr>
                                <tr>
                                    <th>Renda mensal aproximada</th>
                                    <td>R$ ${typeof formulario.rendaMensal === 'number' ? formulario.rendaMensal.toLocaleString('pt-BR', { minimumFractionDigits: 2 }) : '0,00'}</td>
                                </tr>
                                <tr>
                                    <th>Pessoas na residência</th>
                                    <td>${s(formulario.numeroMoradores || '0')}</td>
                                </tr>
                                ${formulario.tempoDisponivel ? `
                                <tr>
                                    <th>Quanto tempo livre você tem para dedicar ao pet diariamente?</th>
                                    <td>${s(formulario.tempoDisponivel)} horas por dia</td>
                                </tr>
                                ` : ''}
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        `;
        return htmlConteudo;
    } catch (erro) {
        console.error('Erro ao construir detalhes do formulário:', erro);
        return `<div class="alert alert-danger">
            <i class="fas fa-exclamation-triangle me-2"></i>
            Erro ao carregar detalhes do formulário. Por favor, tente novamente.
        </div>`;
    }
}

function exibirConfirmacaoAprovacao() {
    
    limparAlertasModal();
    
    $('#botoesAcaoPrimarios').addClass('d-none');
    
    
    $('#confirmacaoRejeicao').addClass('d-none');
    
    
    $('#confirmacaoAprovacao').removeClass('d-none');
    
}

function exibirConfirmacaoRejeicao() {
    
    
    limparAlertasModal();
    
    
    $('#botoesAcaoPrimarios').addClass('d-none');
    
    
    $('#confirmacaoAprovacao').addClass('d-none');
    
    
    $('#confirmacaoRejeicao').removeClass('d-none');
    
}

function resetarPaineisConfirmacao() {
    
    
    $('#confirmacaoAprovacao').addClass('d-none');
    $('#confirmacaoRejeicao').addClass('d-none');
    
    
    $('#botoesAcaoPrimarios').removeClass('d-none');
    
    
    limparAlertasModal();
    
}

function aprovarFormularioConfirmado() {
    const formularioId = $('#formularioIdAtual').val();
    const observacaoAdmin = $('#observacaoAdmin').val();
    
    
    
    $('#botaoConfirmarAprovacao').prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>Processando...');
    
    $.ajax({
        url: `/admin/formularios-adocao/Aprovar/${formularioId}`,
        type: 'POST',
        data: {
            observacao: observacaoAdmin,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(resposta) {
            
            
            $('#botaoConfirmarAprovacao').prop('disabled', false).html('<i class="fas fa-check me-2"></i>Confirmar Aprovação');
            
            if (resposta.sucesso) {
                toastr.success('Formulário aprovado com sucesso!');
                setTimeout(function() {
                    location.reload();
                }, 1500);
            } else {
                
                toastr.error(resposta.mensagem || 'Erro ao aprovar formulário.');
                resetarPaineisConfirmacao();
                
                
                if (resposta.mensagem && resposta.mensagem.includes("pet")) {
                    $('#modalDetalhesFormulario .modal-body').prepend(`
                        <div class="alert alert-danger alert-dismissible fade show" role="alert">
                            <strong>Erro!</strong> ${resposta.mensagem}
                            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Fechar"></button>
                        </div>
                    `);
                }
            }
        },
        error: function(xhr, status, erro) {
            console.error("Erro ao aprovar formulário:", erro, xhr);
            
            
            $('#botaoConfirmarAprovacao').prop('disabled', false).html('<i class="fas fa-check me-2"></i>Confirmar Aprovação');
            
            
            let mensagemErro = 'Erro ao aprovar formulário.';
            
            try {
                if (xhr.responseText) {
                    const respostaObj = JSON.parse(xhr.responseText);
                    if (respostaObj && respostaObj.mensagem) {
                        mensagemErro = respostaObj.mensagem;
                    }
                }
            } catch (e) {
                console.error("Erro ao processar resposta:", e);
            }
            
            toastr.error(mensagemErro);
            resetarPaineisConfirmacao();
            
            
            $('#modalDetalhesFormulario .modal-body').prepend(`
                <div class="alert alert-danger alert-dismissible fade show" role="alert">
                    <strong>Erro!</strong> ${mensagemErro}
                    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Fechar"></button>
                </div>
            `);
        }
    });
}

function rejeitarFormularioConfirmado() {
    const formularioId = $('#formularioIdAtual').val();
    const observacaoAdmin = $('#observacaoAdmin').val();
    
    
    if (!observacaoAdmin || observacaoAdmin.trim() === '') {
        toastr.warning('Por favor, preencha a observação indicando o motivo da rejeição.');
        return;
    }
    
    
    $.ajax({
        url: `/admin/formularios-adocao/Rejeitar/${formularioId}`,
        type: 'POST',
        data: {
            motivo: observacaoAdmin,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(resposta) {
            if (resposta.sucesso) {
                toastr.success('Formulário rejeitado com sucesso!');
                setTimeout(function() {
                    location.reload();
                }, 1500);
            } else {
                toastr.error(resposta.mensagem || 'Erro ao rejeitar formulário.');
                resetarPaineisConfirmacao();
            }
        },
        error: function() {
            toastr.error('Ocorreu um erro ao processar sua solicitação.');
            resetarPaineisConfirmacao();
        }
    });
}


function aprovarFormulario(id) {
    idFormularioAtual = id;
    processarFormulario(id, 'aprovar');
}


function rejeitarFormulario(id) {
    
    const motivo = prompt('Por favor, indique o motivo da rejeição:');
    
    
    if (!motivo || motivo.trim() === '') {
        alert('É necessário informar um motivo para rejeitar o formulário.');
        return;
    }
    
    idFormularioAtual = id;
    processarFormulario(id, 'rejeitar', motivo);
}


function processarFormulario(id, acao, observacao = '') {
    toastr.info(`Processando ${acao === 'aprovar' ? 'aprovação' : 'rejeição'} do formulário...`);
    
    const token = $('input[name="__RequestVerificationToken"]').val();
    
    processarFormularioAcao(id, acao, token, observacao);
}

function processarFormularioAcao(id, acao, token, observacao = '') {
    const url = acao === 'aprovar' 
        ? `/admin/formularios-adocao/Aprovar/${id}`
        : `/admin/formularios-adocao/Rejeitar/${id}`;
    
    const data = {
        __RequestVerificationToken: token
    };
    
    if (acao === 'rejeitar') {
        data.motivo = observacao;
    } else {
        data.observacao = observacao;
    }
    
    $.ajax({
        url: url,
        method: 'POST',
        data: data,
        dataType: 'json',
        success: function(resposta) {
            
            if (resposta && resposta.sucesso) {
                const classeStatus = acao === 'aprovar' ? 'aprovado' : 'rejeitado';
                const textoStatus = acao === 'aprovar' ? 'Aprovado' : 'Rejeitada';
                const iconeStatus = acao === 'aprovar' ? 'check-circle' : 'times-circle';
                
                const linha = $(`table tbody tr[data-id="${id}"]`);
                    
                    linha.find('.indicador-status')
                    .removeClass('pendente aprovado rejeitado cancelado aguardando-buscar')
                    .addClass(classeStatus)
                    .html(`${obterIconeStatus(textoStatus)} ${textoStatus}`);
                
                linha.find('.botoes-acao').html(`
                    <button class="botao-acao botao-visualizar" onclick="visualizarFormulario(${id})">
                        <i class="fas fa-eye"></i>
                    </button>
                `);
                
                linha.attr('data-status', textoStatus);
                
                atualizarContadoresFormularios();
                
                toastr.success(`Formulário ${acao === 'aprovar' ? 'aprovado' : 'rejeitado'} com sucesso!`);
                
                if (modalDetalhesFormulario) {
                    modalDetalhesFormulario.hide();
                }
            } else {
                toastr.error(resposta?.mensagem || `Erro ao ${acao === 'aprovar' ? 'aprovar' : 'rejeitar'} formulário`);
            }
        },
        error: function(xhr, status, erro) {
            console.error(`Erro ao ${acao} formulário:`, erro);
            
            
            let mensagemErro = `Erro ao ${acao === 'aprovar' ? 'aprovar' : 'rejeitar'} formulário`;
            
            try {
                const respostaJson = JSON.parse(xhr.responseText);
                if (respostaJson && respostaJson.mensagem) {
                    mensagemErro = respostaJson.mensagem;
                }
            } catch (e) {
                console.error('Erro ao processar resposta de erro:', e);
            }
            
            
            toastr.error(mensagemErro);
        }
    });
}


function atualizarContadoresFormularios() {
    
    const total = $('table tbody tr').length;
    const pendentes = $('table tbody tr[data-status="Pendente"]').length;
    const aprovados = $('table tbody tr[data-status="Aprovado"]').length;
    const rejeitados = $('table tbody tr[data-status="Rejeitada"]').length;
    const cancelados = $('table tbody tr[data-status^="Cancelad"]').length;
    
    
    $('.card.resumo.total-formularios .h5').text(total);
    $('.card.resumo.pendentes .h5').text(pendentes);
    $('.card.resumo.aprovados .h5').text(aprovados);
    $('.card.resumo.rejeicoes .h5').text(rejeitados + cancelados);
}


function gerarAvatarInicial(nome) {
    if (!nome || nome.length === 0) return '?';
    return nome.charAt(0).toUpperCase();
}


function exibirFotoUsuario(fotoPerfil, nome) {
    if (fotoPerfil && fotoPerfil !== '') {
        return `<img src="/imagens/perfil/${fotoPerfil}" alt="${nome}" class="foto-usuario">`;
    } else {
        return `<div class="avatar-inicial">${gerarAvatarInicial(nome)}</div>`;
    }
}

function obterIconeStatus(status) {
    switch (status.toLowerCase()) {
        case 'pendente':
            return 'fas fa-clock text-warning';
        case 'aprovado':
            return 'fas fa-check-circle text-success';
        case 'rejeitado':
            return 'fas fa-times-circle text-danger';
        case 'cancelado':
            return 'fas fa-ban text-secondary';
        case 'em processo':
            return 'fas fa-spinner text-primary';
        case 'aguardando buscar':
            return 'fas fa-truck-pickup text-warning';
        case 'finalizado':
            return 'fas fa-check-double text-success';
        default:
            return 'fas fa-question-circle text-muted';
    }
}