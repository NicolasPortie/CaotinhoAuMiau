let toastrConfig = {
    closeButton: true,
    progressBar: true,
    positionClass: "toast-top-right",
    timeOut: 5000
};


const URL_API_DETALHES_ADOCAO = "/admin/adocoes/DetalhesAdocao/";
const URL_API_APROVAR_ADOCAO = "/admin/adocoes/aprovar/";
const URL_API_REJEITAR_ADOCAO = "/admin/adocoes/rejeitar/";
const URL_API_AGUARDANDO_BUSCAR = "/admin/adocoes/aguardando-buscar/";
const URL_API_FINALIZAR_ADOCAO = "/admin/adocoes/finalizar/";
const URL_API_CANCELAR_ADOCAO = "/admin/adocoes/Cancelar/";
const URL_API_OBTER_PERFIL_USUARIO = "/admin/adocoes/ObterPerfilUsuario";
const URL_API_ESTATISTICAS_ADOCAO_USUARIO = "/admin/adocoes/ObterEstatisticasAdocaoUsuario";
const URL_API_HISTORICO_ADOCOES_USUARIO = "/admin/adocoes/ObterHistoricoAdocoesUsuario";


$(document).ready(function() {
    
    
    toastr.options = toastrConfig;
    
    
    inicializarEventos();
    
    
    aplicarFormatacaoTabela();
    
    // Corrigir problema de paginação - implementação robusta
    $(document).on('click', '.pagination .page-link', function(e) {
        e.preventDefault();
        var url = $(this).attr('href');
        if (url) {
            window.location.href = url;
        }
        return false;
    });
});


function inicializarEventos() {
    
    $('#pesquisaAdocao').on('keyup', function() {
        filtrarTabela();
    });
    
    
    $('#filtroStatus').on('change', function() {
        filtrarTabela();
    });
    
    
    $('#filtroData').on('change', function() {
        filtrarTabela();
    });
    
    
    $('#btnLimparFiltros').on('click', function() {
        limparFiltros();
    });
    
    
    $('.botao-ver-perfil').on('click', function() {
        const usuarioId = $(this).data('usuario-id');
        abrirPerfilUsuario(usuarioId);
    });
    
    
    $('#modalDetalhesAdocao').on('hidden.bs.modal', function() {
        
        $('.carregando-detalhes').show();
        $('.detalhes-adocao-container').hide();
        
        
        $('#detalhesNomePet, #detalhesEspeciePet, #detalhesRacaPet, #detalhesIdadePet, #detalhesSexoPet, #detalhesPortePet').text('-');
        $('#detalhesStatusAdocao, #detalhesDataEnvio, #detalhesDataResposta').text('-');
        $('#detalhesNomeAdotante, #detalhesEmailAdotante, #detalhesTelefoneAdotante, #detalhesCpfAdotante').text('-');
        $('#detalhesObservacoes').html('');
        
        $('.timeline-progresso-item').removeClass('ativo concluido atual rejeitado');
        $('.timeline-progresso-linha').removeClass('ativa');
        $('#timelineDataSolicitacao, #timelineDataAprovacao, #timelineDataBusca, #timelineDataFinalizacao').text('');
        
        $('#detalhesDataBuscaContainer, #detalhesDataFinalizacaoContainer').hide();
        
        
        $('#detalhesPrazoBusca').hide();
        
        
        $('#detalhesPetImagem').html('<i class="fas fa-paw"></i>');
        $('#detalhesAdotanteAvatar').html('<i class="fas fa-user"></i>');
    });
    
    
    $('#modalDetalhesAdocao').on('shown.bs.modal', function() {
        
        setTimeout(function() {
            configurarNavegacaoAbas();
        }, 200);
    });
    
    
    $(document).on('click', '.detalhes-aba', function(e) {
        e.preventDefault();
        
        const painelAlvo = $(this).data('painel');
        
        
        $('.detalhes-aba').removeClass('ativa');
        $(this).addClass('ativa');
        
        
        $('.detalhes-painel').removeClass('ativo');
        $('#' + painelAlvo).addClass('ativo');
    });
}


function aplicarFormatacaoTabela() {
    
    $('.tabela tr').each(function() {
        const status = $(this).find('.indicador-status').text().trim();
        formatarIndicadorStatus($(this).find('.indicador-status'), status);
    });
}


function formatarIndicadorStatus(elemento, status) {
    elemento.removeClass('pendente aprovado finalizada rejeitada cancelada em-processo aguardando-buscar');
    
    
    switch(status.toLowerCase()) {
        case 'pendente':
            elemento.addClass('pendente');
            break;
        case 'aprovado':
            elemento.addClass('aprovado');
            break;
        case 'finalizada':
            elemento.addClass('finalizada');
            break;
        case 'rejeitada':
            elemento.addClass('rejeitada');
            break;
        case 'cancelada':
            elemento.addClass('cancelada');
            break;
        case 'em processo':
            elemento.addClass('em-processo');
            break;
        case 'aguardando buscar':
            elemento.addClass('aguardando-buscar');
            break;
    }
}


function filtrarTabela() {
    const textoPesquisa = $('#pesquisaAdocao').val().toLowerCase();
    const statusFiltro = $('#filtroStatus').val();
    const dataFiltro = $('#filtroData').val();
    
    let temRegistros = false;
    
    $('.tabela tbody tr').each(function() {
        const linha = $(this);
        const conteudoLinha = linha.text().toLowerCase();
        const statusLinha = linha.data('status');
        const dataAdocao = new Date(linha.find('td:nth-child(3) span:first').text().split('/').reverse().join('-'));
        
        let mostrarPorTexto = conteudoLinha.indexOf(textoPesquisa) > -1;
        let mostrarPorStatus = statusFiltro === 'Todos' || statusLinha === statusFiltro;
        let mostrarPorData = verificarFiltroData(dataAdocao, dataFiltro);
        
        if (mostrarPorTexto && mostrarPorStatus && mostrarPorData) {
            linha.show();
            temRegistros = true;
        } else {
            linha.hide();
        }
    });
    
    
    if (temRegistros) {
        $('.tabela').show();
        $('.mensagem-sem-adocoes').hide();
    } else {
        $('.tabela').hide();
        $('.mensagem-sem-adocoes').show();
    }
}


function verificarFiltroData(data, filtro) {
    if (!filtro) return true;
    
    const hoje = new Date();
    
    switch(filtro) {
        case 'hoje':
            return data.toDateString() === hoje.toDateString();
        case '7dias':
            const seteDiasAtras = new Date();
            seteDiasAtras.setDate(hoje.getDate() - 7);
            return data >= seteDiasAtras;
        case '30dias':
            const trintaDiasAtras = new Date();
            trintaDiasAtras.setDate(hoje.getDate() - 30);
            return data >= trintaDiasAtras;
        default:
            return true;
    }
}


function limparFiltros() {
    $('#pesquisaAdocao').val('');
    $('#filtroStatus').val('Todos');
    $('#filtroData').val('');
    
    filtrarTabela();
}

function formatarTelefone(telefone) {
    if (!telefone) return "-";
    telefone = telefone.replace(/\D/g, '');
    if (telefone.length === 11) {
        return telefone.replace(/^(\d{2})(\d{5})(\d{4})$/, '($1) $2-$3');
    }
    return telefone.replace(/^(\d{2})(\d{4})(\d{4})$/, '($1) $2-$3');
}

function formatarCPF(cpf) {
    if (!cpf) return "-";
    cpf = cpf.replace(/\D/g, '');
    return cpf.replace(/^(\d{3})(\d{3})(\d{3})(\d{2})$/, '$1.$2.$3-$4');
}

function formatarCEP(cep) {
    if (!cep) return "-";
    cep = cep.replace(/\D/g, '');
    return cep.replace(/^(\d{5})(\d{3})$/, '$1-$2');
}


function verDetalhes(id, fromProfile = false) {
    
    
    if (fromProfile) {
        $('#perfilUsuarioModal').modal('hide');
    }
    
    
    const modal = $('#modalDetalhesAdocao');
    modal.modal('show');
    
    
    $('.carregando-detalhes').show();
    $('.detalhes-adocao-container').hide();
    
    
    $.ajax({
        url: URL_API_DETALHES_ADOCAO + id,
        type: 'GET',
        success: function(resposta) {
            if (resposta && resposta.id) {
                preencherDetalhesAdocao(resposta, fromProfile);
                
                
                setTimeout(function() {
                    configurarNavegacaoAbas();
                }, 100);
            } else {
                $('.carregando-detalhes').html(`
                    <div class="alert alert-danger m-3">
                        <i class="fas fa-exclamation-triangle me-2"></i>
                        Não foi possível carregar os detalhes da adoção. ${resposta.mensagem || 'Tente novamente mais tarde.'}
                    </div>
                `);
            }
        },
        error: function(xhr, status, error) {
            $('.carregando-detalhes').html(`
                <div class="alert alert-danger m-3">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    Erro ao carregar detalhes: ${error}
                </div>
            `);
        }
    });
}


function preencherDetalhesAdocao(adocao, fromProfile = false) {
    
    $('.carregando-detalhes').hide();
    $('.detalhes-adocao-container').show();
    
    
    const statusElement = $('#detalhesStatusAdocao');
    statusElement.text(adocao.status);
    
    
    statusElement.removeClass('sucesso alerta perigo destaque');
    switch(adocao.status) {
        case 'Finalizada':
            statusElement.addClass('sucesso');
            break;
        case 'Aguardando buscar':
            statusElement.addClass('alerta');
            break;
        case 'Rejeitada':
        case 'Cancelada':
            statusElement.addClass('perigo');
            break;
        case 'Em Processo':
        case 'Aprovado':
        case 'Pendente':
        default:
            statusElement.addClass('destaque');
            break;
    }
    
    
    const prazoElement = $('#detalhesPrazoBusca');
    
    if (adocao.status === 'Aguardando buscar' && adocao.dataResposta) {
        const dataLimite = new Date(adocao.dataResposta);
        dataLimite.setDate(dataLimite.getDate() + 10);
        const hoje = new Date();
        const diasRestantes = Math.ceil((dataLimite - hoje) / (1000 * 60 * 60 * 24));
        
        
        prazoElement.removeClass('sucesso alerta perigo');
        
        
        if (diasRestantes <= 0) {
            prazoElement.text('Prazo expirado');
            prazoElement.addClass('perigo');
        } else if (diasRestantes <= 2) {
            prazoElement.text(`${diasRestantes} dias`);
            prazoElement.addClass('perigo');
        } else if (diasRestantes <= 5) {
            prazoElement.text(`${diasRestantes} dias`);
            prazoElement.addClass('alerta');
        } else {
            prazoElement.text(`${diasRestantes} dias`);
            prazoElement.addClass('sucesso');
        }
        
        prazoElement.show();
    } else {
        prazoElement.hide();
    }
    
    
    configurarTimelineProgresso(adocao);
    
    
    $('#detalhesNomePet').text(adocao.pet ? adocao.pet.nome : 'Não disponível');
    $('#detalhesEspeciePet').text(adocao.pet ? adocao.pet.especie : '-');
    $('#detalhesRacaPet').text(adocao.pet ? adocao.pet.raca : '-');
    $('#detalhesIdadePet').text(adocao.pet ? adocao.pet.idade : '-');
    $('#detalhesSexoPet').text(adocao.pet ? adocao.pet.sexo : '-');
    $('#detalhesPortePet').text(adocao.pet ? adocao.pet.porte : '-');
    
    
    const petImagemContainer = $('#detalhesPetImagem');
    petImagemContainer.empty();
    
    if (adocao.pet && adocao.pet.imagem) {
        const img = $('<img>').attr('src', `/imagens/pets/${adocao.pet.imagem}`).attr('alt', adocao.pet.nome);
        petImagemContainer.append(img);
    } else {
        petImagemContainer.html('<i class="fas fa-paw"></i>');
    }
    
    
    $('#detalhesNomeAdotante').text(adocao.usuario ? adocao.usuario.nome : 'Não disponível');
    $('#detalhesEmailAdotante').text(adocao.usuario ? adocao.usuario.email : '-');
    $('#detalhesTelefoneAdotante').text(adocao.usuario && adocao.usuario.telefone ? formatarTelefone(adocao.usuario.telefone) : '-');
    $('#detalhesCpfAdotante').text(adocao.usuario && adocao.usuario.cpf ? formatarCPF(adocao.usuario.cpf) : '-');
    
    
    const adotanteAvatarContainer = $('#detalhesAdotanteAvatar');
    adotanteAvatarContainer.empty();
    
    if (adocao.usuario && adocao.usuario.fotoPerfil) {
        const img = $('<img>').attr('src', `/imagens/perfil/${adocao.usuario.fotoPerfil}`).attr('alt', adocao.usuario.nome);
        adotanteAvatarContainer.append(img);
    } else if (adocao.usuario && adocao.usuario.nome) {
        adotanteAvatarContainer.text(adocao.usuario.nome.charAt(0).toUpperCase());
    } else {
        adotanteAvatarContainer.html('<i class="fas fa-user"></i>');
    }
    
    
    if (adocao.usuario) {
        $('#botaoVerPerfilCompleto').show().off('click').on('click', function() {
            $('#modalDetalhesAdocao').modal('hide');
            setTimeout(() => {
                abrirPerfilUsuario(adocao.usuario.id);
            }, 500);
        });
    } else {
        $('#botaoVerPerfilCompleto').hide();
    }
    
    
    $('#detalhesDataEnvio').text(new Date(adocao.dataEnvio).toLocaleString('pt-BR'));
    $('#detalhesDataResposta').text(adocao.dataResposta ? new Date(adocao.dataResposta).toLocaleString('pt-BR') : 'Pendente');
    
    
    const dataBuscaContainer = $('#detalhesDataBuscaContainer');
    const dataFinalizacaoContainer = $('#detalhesDataFinalizacaoContainer');
    
    
    if (adocao.status === 'Aguardando buscar' && adocao.dataResposta) {
        const dataBusca = new Date(adocao.dataResposta).toLocaleString('pt-BR');
        $('#detalhesDataBusca').text(dataBusca);
        dataBuscaContainer.show();
    } else {
        dataBuscaContainer.hide();
    }
    
    
    if (adocao.status === 'Finalizada' && adocao.dataFinalizacao) {
        const dataFinalizacao = new Date(adocao.dataFinalizacao).toLocaleString('pt-BR');
        $('#detalhesDataFinalizacao').text(dataFinalizacao);
        dataFinalizacaoContainer.show();
    } else {
        dataFinalizacaoContainer.hide();
    }
    
    
    if (adocao.observacoes) {
        $('#detalhesObservacoes').html(adocao.observacoes).show();
        $('#detalhesObservacoesProcesso').html(adocao.observacoes).show();
    } else {
        $('#detalhesObservacoes').html('<div class="sem-observacoes">Sem observações adicionais.</div>').show();
        $('#detalhesObservacoesProcesso').html('<div class="sem-observacoes">Sem observações adicionais.</div>').show();
    }
    
    
    if (fromProfile) {
        $('#modalDetalhesAdocao .modal-footer').prepend(`
            <button type="button" class="botao-secundario me-auto" onclick="voltarParaPerfil()">
                <i class="fas fa-arrow-left me-2"></i>Voltar ao perfil
            </button>
        `);
    } else {
        $('#modalDetalhesAdocao .modal-footer .botao-secundario').remove();
    }
    
    
    configurarNavegacaoAbas();
}


function configurarTimelineProgresso(adocao) {
    
    $('.timeline-progresso-item').removeClass('ativo concluido atual rejeitado');
    $('.timeline-progresso-linha').removeClass('ativa');
    
    
    $('#timelineDataSolicitacao').text(new Date(adocao.dataEnvio).toLocaleDateString('pt-BR'));
    
    
    $('#timelineSolicitacao').addClass('concluido');
    
    
    switch(adocao.status) {
        case 'Pendente':
            
            break;
            
        case 'Rejeitada':
            
            $('#timelineAprovacao').addClass('rejeitado');
            $('#timelineDataAprovacao').text(adocao.dataResposta ? new Date(adocao.dataResposta).toLocaleDateString('pt-BR') : '');
            break;
            
        case 'Cancelada':
            
            if (adocao.dataResposta) {
                $('#timelineAprovacao').addClass('concluido');
                $('#timelineDataAprovacao').text(new Date(adocao.dataResposta).toLocaleDateString('pt-BR'));
                
                
                if (adocao.status === 'Aguardando buscar') {
                    $('#timelineBusca').addClass('rejeitado');
                    $('#timelineDataBusca').text(adocao.dataResposta ? new Date(adocao.dataResposta).toLocaleDateString('pt-BR') : '');
                } else {
                    $('#timelineBusca').addClass('rejeitado');
                }
            } else {
                $('#timelineAprovacao').addClass('rejeitado');
            }
            break;
            
        case 'Aprovado':
        case 'Em Processo':
            
            $('#timelineAprovacao').addClass('concluido');
            $('#timelineDataAprovacao').text(adocao.dataResposta ? new Date(adocao.dataResposta).toLocaleDateString('pt-BR') : '');
            
            $('.timeline-progresso-linha:eq(0)').addClass('ativa');
            break;
            
        case 'Aguardando buscar':
            
            $('#timelineAprovacao').addClass('concluido');
            $('#timelineDataAprovacao').text(adocao.dataResposta ? new Date(adocao.dataResposta).toLocaleDateString('pt-BR') : '');
            
            $('#timelineBusca').addClass('atual');
            $('#timelineDataBusca').text(adocao.dataResposta ? new Date(adocao.dataResposta).toLocaleDateString('pt-BR') : '');
            
            $('.timeline-progresso-linha:eq(0), .timeline-progresso-linha:eq(1)').addClass('ativa');
            break;
            
        case 'Finalizada':
            
            $('#timelineAprovacao').addClass('concluido');
            $('#timelineDataAprovacao').text(adocao.dataResposta ? new Date(adocao.dataResposta).toLocaleDateString('pt-BR') : '');
            
            $('#timelineBusca').addClass('concluido');
            $('#timelineDataBusca').text(adocao.dataResposta ? new Date(adocao.dataResposta).toLocaleDateString('pt-BR') : '');
            
            $('#timelineFinalizacao').addClass('concluido');
            $('#timelineDataFinalizacao').text(adocao.dataFinalizacao ? new Date(adocao.dataFinalizacao).toLocaleDateString('pt-BR') : '');
            
            $('.timeline-progresso-linha').addClass('ativa');
            break;
    }
}


function voltarParaPerfil() {
    
    $('#modalDetalhesAdocao').modal('hide');
    
    
    $('#modalDetalhesAdocao').on('hidden.bs.modal', function (e) {
        $('#perfilUsuarioModal').modal('show');
        
        $('#modalDetalhesAdocao').off('hidden.bs.modal');
    });
}


function mostrarModalAcao(id, acao) {
    
    
    $('#idAdocaoAcao').val(id);
    $('#tipoAcao').val(acao);
    $('#observacaoAcao').val('');
    $('#erroObservacao').hide();
    
    
    const precisaObservacao = (acao === 'rejeitar' || acao === 'cancelar');
    $('#campoObservacao').toggle(precisaObservacao);
    
    
    let titulo = "Confirmar Ação";
    let mensagem = "Você está prestes a realizar uma ação. Deseja continuar?";
    let corCabecalho = "bg-primary text-white";
    
    switch(acao) {
        case 'aprovar':
            titulo = "Aprovar Adoção";
            mensagem = "Tem certeza que deseja aprovar esta adoção?";
            corCabecalho = "bg-success text-white";
            break;
        case 'rejeitar':
            titulo = "Rejeitar Adoção";
            mensagem = "Tem certeza que deseja rejeitar esta adoção? Por favor, informe o motivo.";
            corCabecalho = "bg-danger text-white";
            break;
        case 'aguardandoBuscar':
            titulo = "Marcar como Aguardando Retirada";
            mensagem = "Tem certeza que deseja marcar esta adoção como aguardando retirada pelo adotante?";
            corCabecalho = "bg-warning text-dark";
            break;
        case 'finalizar':
            titulo = "Finalizar Adoção";
            mensagem = "Tem certeza que deseja finalizar esta adoção, confirmando que o pet foi retirado pelo adotante?";
            corCabecalho = "bg-success text-white";
            break;
        case 'cancelar':
            titulo = "Cancelar Adoção";
            mensagem = "Tem certeza que deseja cancelar esta adoção? Por favor, informe o motivo.";
            corCabecalho = "bg-danger text-white";
            break;
    }
    
    
    $('#tituloModalAcaoAdocao').text(titulo);
    $('#textoConfirmacaoAcao').text(mensagem);
    $('#modalAcaoHeader').removeClass().addClass('modal-header ' + corCabecalho);
    
    
    $('#modalAcaoAdocao').modal('show');
}


function executarAcao() {
    const id = $('#idAdocaoAcao').val();
    const acao = $('#tipoAcao').val();
    const observacao = $('#observacaoAcao').val();
    
    
    if ((acao === 'rejeitar' || acao === 'cancelar') && !observacao.trim()) {
        $('#erroObservacao').show();
        return;
    }
    
    
    $('#botaoConfirmarAcao').prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>Processando...');
    
    
    const token = $('input[name="__RequestVerificationToken"]').val();
    
    
    let url = '';
    let metodo = 'POST';
    let dados = { __RequestVerificationToken: token };
    
    switch(acao) {
        case 'aprovar':
            url = URL_API_APROVAR_ADOCAO + id;
            break;
        case 'rejeitar':
            url = URL_API_REJEITAR_ADOCAO + id;
            dados.motivo = observacao;
            break;
        case 'aguardandoBuscar':
            url = URL_API_AGUARDANDO_BUSCAR + id;
            break;
        case 'finalizar':
            url = URL_API_FINALIZAR_ADOCAO + id;
            if (observacao) {
                dados.observacao = observacao;
            }
            break;
        case 'cancelar':
            url = URL_API_CANCELAR_ADOCAO + id;
            dados.motivo = observacao;
            break;
    }
    
    
    $.ajax({
        url: url,
        type: metodo,
        data: dados,
        success: function(resposta) {
            
            $('#botaoConfirmarAcao').prop('disabled', false).html('Confirmar');
            
            
            $('#modalAcaoAdocao').modal('hide');
            
            if (resposta.sucesso) {
                
                toastr.success(resposta.mensagem);
                
                
                setTimeout(function() {
                    window.location.reload();
                }, 1500);
            } else {
                
                toastr.error(resposta.mensagem);
            }
        },
        error: function(xhr, status, error) {
            
            $('#botaoConfirmarAcao').prop('disabled', false).html('Confirmar');
            
            
            toastr.error("Ocorreu um erro: " + error);
        }
    });
}


function abrirPerfilUsuario(usuarioId) {
    
    
    $('#spinnerPerfilUsuario').show();
    $('#conteudoPerfilUsuario').hide();
    
    
    $.ajax({
        url: URL_API_OBTER_PERFIL_USUARIO,
        type: 'GET',
        data: { id: usuarioId },
        success: function(usuario) {
            if (usuario) {
                renderizarPerfilUsuario(usuario);
            } else {
                toastr.error("Não foi possível carregar os dados do usuário");
            }
        },
        error: function(xhr, status, error) {
            toastr.error("Erro ao carregar perfil: " + error);
        }
    });
    
    
    $.ajax({
        url: URL_API_ESTATISTICAS_ADOCAO_USUARIO,
        type: 'GET',
        data: { id: usuarioId },
        success: function(estatisticas) {
            preencherEstatisticasUsuario(estatisticas);
        }
    });
    
    
    $.ajax({
        url: URL_API_HISTORICO_ADOCOES_USUARIO,
        type: 'GET',
        data: { id: usuarioId },
        success: function(historico) {
            preencherHistoricoAdocoesUsuario(historico, usuarioId);
        }
    });
}


function renderizarPerfilUsuario(usuario) {
    const nomeModal = document.getElementById('perfilUsuarioNome');
    const emailModal = document.getElementById('perfilUsuarioEmail');
    const inicialModal = document.getElementById('perfilUsuarioInicial');
    const containerFoto = document.querySelector('.perfil-foto-container');
    
    nomeModal.textContent = usuario.nome || 'Nome não disponível';
    emailModal.textContent = usuario.email || 'Email não disponível';
    
    
    const imgAnterior = containerFoto.querySelector('img');
    if (imgAnterior) {
        imgAnterior.remove();
    }
    
    
    if (usuario.fotoPerfil) {
        inicialModal.style.display = 'none';
        const img = document.createElement('img');
        img.src = `/imagens/perfil/${usuario.fotoPerfil}`;
        img.alt = usuario.nome;
        img.className = 'perfil-foto';
        containerFoto.appendChild(img);
    } else {
        inicialModal.style.display = 'flex';
        inicialModal.textContent = usuario.nome ? usuario.nome.charAt(0) : 'U';
    }
    
    
    $('#nomeCompletoUsuario').text(usuario.nome || '-');
    $('#emailUsuario').text(usuario.email || '-');
    $('#cpfUsuario').text(formatarCPF(usuario.cpf));
    $('#telefoneUsuario').text(formatarTelefone(usuario.telefone));
    
    
    if (usuario.dataNascimento) {
        const dataNascimento = new Date(usuario.dataNascimento);
        $('#dataNascimentoUsuario').text(dataNascimento.toLocaleDateString('pt-BR'));
        
        
        const hoje = new Date();
        let idade = hoje.getFullYear() - dataNascimento.getFullYear();
        const m = hoje.getMonth() - dataNascimento.getMonth();
        if (m < 0 || (m === 0 && hoje.getDate() < dataNascimento.getDate())) {
            idade--;
        }
        $('#idadeUsuario').text(idade + ' anos');
    } else {
        $('#dataNascimentoUsuario').text('-');
        $('#idadeUsuario').text('-');
    }
    
    
    if (usuario.dataCadastro) {
        const dataCadastro = new Date(usuario.dataCadastro);
        $('#dataCadastroUsuario').text(dataCadastro.toLocaleDateString('pt-BR'));
    } else {
        $('#dataCadastroUsuario').text('-');
    }
    
    
    $('#logradouroUsuario').text(usuario.logradouro || '-');
    $('#numeroUsuario').text(usuario.numero || '-');
    $('#bairroUsuario').text(usuario.bairro || '-');
    $('#complementoUsuario').text(usuario.complemento || '-');
    $('#cepUsuario').text(formatarCEP(usuario.cep));
    $('#cidadeUsuario').text(usuario.cidade || '-');
    $('#estadoUsuario').text(usuario.estado || '-');
    
    
    $('#spinnerPerfilUsuario').hide();
    $('#conteudoPerfilUsuario').show();
    
    
    const modal = new bootstrap.Modal(document.getElementById('perfilUsuarioModal'));
    modal.show();
}


function preencherEstatisticasUsuario(estatisticas) {
    $('#estatisticaTotal').text(estatisticas.total || 0);
    $('#estatisticaAprovadas').text(estatisticas.aprovadas || 0);
    $('#estatisticaRejeitadas').text(estatisticas.rejeitadas || 0);
    $('#estatisticaCanceladas').text(estatisticas.canceladas || 0);
}


function preencherHistoricoAdocoesUsuario(historico, usuarioId) {
    const container = $('#historicoAdocoesUsuarioContainer');
    
    
    container.empty();
    
    if (!historico || historico.length === 0) {
        container.html(`
            <div class="text-center py-4">
                <i class="fas fa-heart-broken fa-3x text-muted mb-3"></i>
                <p class="mb-0">Este usuário ainda não realizou nenhuma adoção.</p>
            </div>
        `);
        return;
    }
    
    
    const historicoContainer = $('<div class="historico-adocoes-container"></div>');
    
    
    historico.forEach(function(item) {
        const dataAdocao = new Date(item.dataAdocao).toLocaleDateString('pt-BR');
        
        
        let classeStatus = "";
        switch(item.status.toLowerCase()) {
            case 'aprovado':
            case 'finalizada':
            case 'aguardando buscar':
                classeStatus = "aprovado";
                break;
            case 'rejeitada':
                classeStatus = "rejeitada";
                break;
            case 'cancelada':
                classeStatus = "cancelada";
                break;
            default:
                classeStatus = "pendente";
        }
        
        
        const itemHistorico = $(`
            <div class="historico-adocao-item">
                <div class="historico-pet-img-container">
                    ${item.imagemPet ? 
                        `<img src="${item.imagemPet}" alt="${item.nomePet}" class="historico-pet-img">` : 
                        `<div class="historico-default-img"><i class="fas fa-paw"></i></div>`
                    }
                </div>
                <div class="historico-pet-info">
                    <div class="historico-pet-nome">${item.nomePet || 'Pet não identificado'}</div>
                    <div class="historico-pet-data">
                        <span class="badge ${classeStatus}">${item.status}</span>
                        <span><i class="far fa-calendar-alt me-1"></i>${dataAdocao}</span>
                    </div>
                </div>
                <div class="historico-acoes">
                    <button class="btn btn-sm btn-outline-primary" 
                            onclick="verDetalhes(${item.id}, true)" 
                            title="Ver detalhes da adoção">
                        <i class="fas fa-eye me-1"></i> Detalhes
                    </button>
                </div>
            </div>
        `);
        
        historicoContainer.append(itemHistorico);
    });
    
    
    container.append(historicoContainer);
}


function configurarNavegacaoAbas() {
    
    
    $('.detalhes-aba').off('click');
    
    
    $('.detalhes-aba').on('click', function(e) {
        e.preventDefault();
        e.stopPropagation();
        
        const painelAlvo = $(this).data('painel');
        
        
        $('.detalhes-aba').removeClass('ativa');
        $(this).addClass('ativa');
        
        
        $('.detalhes-painel').removeClass('ativo').hide();
        $('#' + painelAlvo).addClass('ativo').show();
    });
    
    
    if ($('.detalhes-aba.ativa').length === 0 && $('.detalhes-aba').length > 0) {
        $('.detalhes-aba').first().addClass('ativa');
        const primeiroPainel = $('.detalhes-aba').first().data('painel');
        $('.detalhes-painel').removeClass('ativo').hide();
        $('#' + primeiroPainel).addClass('ativo').show();
    }
    
    
    setTimeout(function() {
        $('.detalhes-aba.ativa').trigger('click');
    }, 50);
} 