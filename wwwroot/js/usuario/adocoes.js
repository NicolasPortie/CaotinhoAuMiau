const CONFIG = {
    api: {
        baseUrl: '/usuario/adocao',
        headers: {
            'Content-Type': 'application/json'
        }
    }
};


const STATUS_CONFIG = {
    PENDENTE: 'pendente',
    APROVADO: 'aprovado',
    REJEITADO: 'rejeitado',
    CANCELADO: 'cancelado',
    AGUARDANDO_BUSCAR: 'aguardando-buscar',
    FINALIZADO: 'finalizada'
};


const state = {
    requestToken: null,
    userId: null,
    userCpf: null,
    filtroAtivo: 'all',
    adocaoIdAtual: null
};


const STATUS = {
    'PENDENTE': {
        classe: 'pendente',
        texto: 'Em Análise',
        icone: 'fa-clock'
    },
    'EM_PROCESSO': {
        classe: 'em-processo',
        texto: 'Em Processo',
        icone: 'fa-sync'
    },
    'APROVADO': {
        classe: 'aprovada',
        texto: 'Aprovado',
        icone: 'fa-check-circle'
    },
    'AGUARDANDO_BUSCAR': {
        classe: 'aguardando-buscar',
        texto: 'Aguardando buscar',
        icone: 'fa-hourglass-half'
    },
    'FINALIZADA': {
        classe: 'finalizada',
        texto: 'Finalizada',
        icone: 'fa-heart'
    },
    'REJEITADA': {
        classe: 'cancelada',
        texto: 'Rejeitada',
        icone: 'fa-times-circle'
    },
    'CANCELADA': {
        classe: 'cancelada',
        texto: 'Cancelada',
        icone: 'fa-ban'
    },
    
    'Pendente': {
        classe: 'pendente',
        texto: 'Em Análise',
        icone: 'fa-clock'
    },
    'Em Processo': {
        classe: 'em-processo',
        texto: 'Em Processo',
        icone: 'fa-sync'
    },
    'Aprovado': {
        classe: 'aprovada',
        texto: 'Aprovado',
        icone: 'fa-check-circle'
    },
    'Aguardando buscar': {
        classe: 'aguardando-buscar',
        texto: 'Aguardando buscar',
        icone: 'fa-hourglass-half'
    },
    'Finalizada': {
        classe: 'finalizada',
        texto: 'Finalizada',
        icone: 'fa-heart'
    },
    'Rejeitada': {
        classe: 'cancelada',
        texto: 'Rejeitada',
        icone: 'fa-times-circle'
    },
    'Cancelada': {
        classe: 'cancelada',
        texto: 'Cancelada',
        icone: 'fa-ban'
    }
};


document.addEventListener('DOMContentLoaded', function() {
    inicializarApp();
});


function inicializarApp() {
    inicializarToastr();
    inicializarEstado();
    inicializarUI();
    inicializarOuvinteEventos();
    inicializarBadgesStatus();
}

function inicializarToastr() {
    toastr.options = CONFIG.toastr;
}

function inicializarEstado() {
    state.requestToken = document.getElementById('requestVerificationToken')?.value;
    state.userId = document.getElementById('userId')?.value;
    state.userCpf = document.getElementById('userCpf')?.value;
    mostrarMensagensTempData();
}

function inicializarUI() {
    inicializarTooltips();
    inicializarCards();
    inicializarModais();
}

function inicializarOuvinteEventos() {
    inicializarOuvintesFiltros();
    inicializarBotaoCancelamento();
}


function mostrarMensagensTempData() {
    const tempData = document.getElementById('temp-data');
    if (!tempData) return;

    const { success, error } = tempData.dataset;
    if (success) toastr.success(success);
    if (error) toastr.error(error);
}

function inicializarTooltips(element = document) {
    if (typeof bootstrap === 'undefined') return;

    const tooltips = element.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltips.forEach(el => {
        if (!el._tooltip) {
            new bootstrap.Tooltip(el, {
                container: 'body',
                html: false,
                trigger: 'hover focus'
            });
        }
    });
}


function inicializarOuvintesFiltros() {
    const filtroStatus = document.getElementById('filtroStatus');
    const pesquisaNomePet = document.getElementById('pesquisaNomePet');
    const cardsEstatisticas = document.querySelectorAll('.card-estatistica');

    if (filtroStatus) filtroStatus.addEventListener('change', aplicarFiltros);
    if (pesquisaNomePet) pesquisaNomePet.addEventListener('input', aplicarFiltros);

    
    cardsEstatisticas.forEach(card => {
        card.addEventListener('click', () => {
            
            cardsEstatisticas.forEach(c => c.classList.remove('active'));
            
            
            card.classList.add('active');
            
            
            if (filtroStatus) {
                filtroStatus.value = card.dataset.status;
            }
            
            
            aplicarFiltros();
        });
    });

    
    if (cardsEstatisticas.length > 0 && !document.querySelector('.card-estatistica.active')) {
        cardsEstatisticas[0].classList.add('active');
    }
}

function aplicarFiltros() {
    
    const statusFiltro = document.querySelector('.card-estatistica.active')?.dataset?.status || 'all';
    const termoPesquisa = document.getElementById('pesquisaNomePet')?.value.trim().toLowerCase() || '';
    
    const itensAdocao = document.querySelectorAll('.adocao-item');
    
    
    let contadorVisiveis = 0;
    
    
    itensAdocao.forEach(item => {
        const statusItem = item.dataset.status;
        const nomePet = item.dataset.nome;
        
        
        const correspondeStatus = statusFiltro === 'all' || statusItem === statusFiltro;
        const correspondeNome = !termoPesquisa || nomePet.includes(termoPesquisa);
        const deveExibir = correspondeStatus && correspondeNome;
        
        
        item.style.display = deveExibir ? 'block' : 'none';
        
        
        if (deveExibir) contadorVisiveis++;
    });
    
    
    atualizarFiltroAtivo(statusFiltro);
    
    
    atualizarEstadoVazio(contadorVisiveis);
    
    
    setTimeout(() => {
        inicializarBadgesStatus();
    }, 100);
}

function atualizarEstadoVazio(contadorVisiveis) {
    const estadoVazio = document.querySelector('.estado-vazio');
    if (estadoVazio) {
        estadoVazio.style.display = contadorVisiveis === 0 ? 'flex' : 'none';
    }
}


function atualizarFiltroAtivo(status) {
    const cardsEstatisticas = document.querySelectorAll('.card-estatistica');
    cardsEstatisticas.forEach(card => {
        if (card.dataset.status === status) {
            card.classList.add('active');
        } else {
            card.classList.remove('active');
        }
    });
}


function inicializarCards() {
    const adocaoItens = document.querySelectorAll('.adocao-item');
    adocaoItens.forEach((item, index) => {
        item.style.opacity = '0';
        item.style.transform = 'translateY(20px)';
        setTimeout(() => {
            item.style.opacity = '1';
            item.style.transform = 'translateY(0)';
        }, index * 100);
    });
}


function inicializarModais() {
    inicializarModalCancelamento();
}

function inicializarModalCancelamento() {
    const modal = document.getElementById('modalCancelamento');
    if (!modal) return;

    const form = modal.querySelector('form');
    if (form) {
        form.addEventListener('submit', manipularEnvioCancelamento);
    }
}

function manipularEnvioCancelamento(event) {
    event.preventDefault();
    const motivo = document.getElementById('motivoCancelamento').value.trim();
    const adocaoId = document.getElementById('adocaoIdCancelar').value;

    if (!validarMotivoCancelamento(motivo)) return;

    enviarSolicitacaoCancelamento(adocaoId, motivo);
}

function validarMotivoCancelamento(motivo) {
    const erroMotivo = document.getElementById('erroMotivo');
    const inputMotivo = document.getElementById('motivoCancelamento');

    if (!motivo) {
        mostrarErro(erroMotivo, inputMotivo, 'O motivo do cancelamento é obrigatório.');
        return false;
    }

    if (motivo.length < 10) {
        mostrarErro(erroMotivo, inputMotivo, 'O motivo do cancelamento deve ter pelo menos 10 caracteres.');
        return false;
    }

    limparErro(erroMotivo, inputMotivo);
    return true;
}

function mostrarErro(errorElement, inputElement, message) {
    errorElement.textContent = message;
    errorElement.style.display = 'block';
    inputElement.classList.add('is-invalid');
    inputElement.focus();
}

function limparErro(errorElement, inputElement) {
    errorElement.style.display = 'none';
    inputElement.classList.remove('is-invalid');
}

async function enviarSolicitacaoCancelamento(adocaoId, motivo) {
    const btn = document.getElementById('btnConfirmarCancelamento');
    if (!btn) return;

    try {
        btn.disabled = true;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span> Processando...';

        const response = await fetch(`${CONFIG.api.baseUrl}/cancelar-formulario-adocao/${adocaoId}`, {
            method: 'POST',
            headers: {
                ...CONFIG.api.headers,
                'RequestVerificationToken': state.requestToken
            },
            body: JSON.stringify({ MotivoCancelamento: motivo })
        });

        if (!response.ok) {
            const data = await response.json();
            throw new Error(data.message || 'Erro ao cancelar a adoção');
        }

        const data = await response.json();
        manipularSucessoCancelamento(data);
    } catch (error) {
        manipularErroCancelamento(error, btn);
    }
}

function manipularSucessoCancelamento(data) {
    const modal = bootstrap.Modal.getInstance(document.getElementById('modalCancelamento'));
    if (modal) modal.hide();

    toastr.success(data.message || 'Adoção cancelada com sucesso!');
    setTimeout(() => window.location.reload(), 1500);
}

function manipularErroCancelamento(error, btn) {
    console.error('Erro:', error);
    toastr.error(error.message || 'Erro ao cancelar a adoção. Por favor, tente novamente mais tarde.');
    
    if (btn) {
        btn.disabled = false;
        btn.innerHTML = '<i class="fas fa-check me-2"></i>Sim, Cancelar Adoção';
    }
}

function visualizarFormulario(id) {
    if (!id) {
        toastr.error('ID do formulário inválido');
        return;
    }

    
    const modal = new bootstrap.Modal(document.getElementById('modalFormulario'));
    modal.show();
    
    
    const loadingDetalhes = document.getElementById('loadingDetalhes');
    const formularioDetalhes = document.getElementById('formularioDetalhes');
    
    if (!loadingDetalhes || !formularioDetalhes) {
        console.error("Elementos DOM necessários não encontrados");
        return;
    }
    
    
    loadingDetalhes.style.display = 'flex';
    formularioDetalhes.style.display = 'none';
    
    
    formularioDetalhes.innerHTML = '';
    
    const timeoutId = setTimeout(() => {
        console.warn("Carregamento demorado detectado");
        const spinnerAtual = loadingDetalhes.querySelector('.spinner-border');
        if (spinnerAtual && loadingDetalhes.style.display !== 'none') {
            
            const mensagemCarregamento = document.createElement('div');
            mensagemCarregamento.className = 'mt-3 text-center text-muted';
            mensagemCarregamento.innerHTML = 'Carregamento em andamento, aguarde um momento...';
            loadingDetalhes.appendChild(mensagemCarregamento);
        }
    }, 3000);
    
    fetch(`/usuario/adocao/formulario-detalhes/${id}`)
        .then(response => {
            if (!response.ok) {
                throw new Error(`Erro ao carregar detalhes: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            
            if (data.status === "Aprovado") {
                
                
                renderizarFormulario(data);
                
                
                fetch(`/usuario/adocao/status-adocao/${id}`)
                    .then(response => response.json())
                    .then(statusData => {
                        if (statusData.success && statusData.status) {
                            
                            data.status = statusData.status;
                            
                            renderizarFormulario(data);
                        } else {
                        }
        })
        .catch(error => {
                        console.error("Erro ao buscar status da adoção:", error);
                    })
                    .finally(() => {
                        clearTimeout(timeoutId);
                    });
            } else {
                renderizarFormulario(data);
                clearTimeout(timeoutId);
            }
        })
        .catch(error => {
            console.error('Erro:', error);
            clearTimeout(timeoutId);
            
            
            loadingDetalhes.style.display = 'none';
                formularioDetalhes.innerHTML = `
                    <div class="alert alert-danger">
                        <i class="fas fa-exclamation-circle me-2"></i>
                    Não foi possível carregar os detalhes da adoção. Tente novamente mais tarde.
                    <div class="mt-2 small text-muted">Erro técnico: ${error.message}</div>
                    </div>
                <div class="text-center mt-3">
                    <button class="btn btn-secondary" data-bs-dismiss="modal">Fechar</button>
                    <button class="btn btn-primary ms-2" onclick="visualizarFormulario(${id})">
                        <i class="fas fa-sync-alt me-2"></i>Tentar novamente
                        </button>
                    </div>
                `;
            formularioDetalhes.style.display = 'block';
        });
}

function renderizarFormulario(formulario) {
        
    const formularioDetalhes = document.getElementById('formularioDetalhes');
    const loadingDetalhes = document.getElementById('loadingDetalhes');
    
        if (!formularioDetalhes) {
            throw new Error("Elemento formularioDetalhes não encontrado");
        }
        
    
    
    
    try {
        if (formulario.id && formulario.status) {
            
            atualizarStatusNaListagem(formulario.id, formulario.status, formulario.observacoesCancelamento || formulario.observacaoCancelamento);
        }
    } catch (statusError) {
        console.warn("Não foi possível atualizar o status na listagem:", statusError);
        }
        
        
    if (!formulario || !formulario.pet) {
            formularioDetalhes.innerHTML = `
        <div class="alert alert-warning">
            <i class="fas fa-exclamation-triangle me-2"></i>
            <strong>Dados incompletos</strong>
                    <p>As informações da adoção estão incompletas ou em um formato inesperado.</p>
        </div>
        <div class="text-center mt-3">
            <button class="btn btn-secondary" data-bs-dismiss="modal">Fechar</button>
                </div>
            `;
        
        if (loadingDetalhes) loadingDetalhes.style.display = 'none';
        formularioDetalhes.style.display = 'block';
            return;
        }
        
    
    if (!formulario.status) {
        formulario.status = 'Pendente';
    }
    
    
        
    
    const pet = formulario.pet || {};
    
    
    const statusTimeline = formulario.status || 'Pendente';
        
        
        const obterMotivoCancelamento = () => {
        
        const statusLower = (statusTimeline || '').toLowerCase();
        const isCancelado = statusLower.includes('cancelado') || statusLower === 'cancelada' || statusLower === 'rejeitada';
        if (!isCancelado) {
                return null;
            }
            
            
            const possiveisMotivos = [
                formulario.observacaoCancelamento,
                formulario.observacoesCancelamento,
                formulario.motivoCancelamento,
                formulario.observacaoAdminFormulario
            ];
            
            
        for (const motivo of possiveisMotivos) {
            if (motivo && typeof motivo === 'string' && motivo.trim() !== '') {
                return motivo.trim();
                }
            }
            
            return "Não informado";
        };
        
        const motivoCancelamento = obterMotivoCancelamento();
    
    const formatarData = (dataString) => {
        if (!dataString) return 'Não informado';
        try {
        const data = new Date(dataString);
            return data.toLocaleDateString('pt-BR', {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        } catch (e) {
            console.warn("Erro ao formatar data:", e);
            return dataString;
        }
        };
        
        
        const escapeHTML = (text) => {
            if (!text) return '';
        return String(text)
                .replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;')
                .replace(/"/g, '&quot;')
                .replace(/'/g, '&#039;');
    };
    
    
    const podeCancelar = statusTimeline.toLowerCase() === 'pendente' || 
                         statusTimeline.toLowerCase() === 'em processo' ||
                         statusTimeline.toLowerCase() === 'aprovado';

    
    state.adocaoIdAtual = formulario.id;
    
    
    if (podeCancelar) {
        const modalFooter = document.querySelector('#modalFormulario .modal-footer');
        if (modalFooter) {
            
            const botaoCancelarExistente = modalFooter.querySelector('#botaoCancelarAdocao');
            if (botaoCancelarExistente) botaoCancelarExistente.remove();
            
            
            const botaoCancelar = document.createElement('button');
            botaoCancelar.id = 'botaoCancelarAdocao';
            botaoCancelar.className = 'btn btn-danger';
            botaoCancelar.innerHTML = '<i class="fas fa-times-circle me-2"></i>Cancelar Adoção';
            botaoCancelar.onclick = function() { confirmarCancelamentoAdocao(formulario.id); };
            
            
            const primeiroFilho = modalFooter.firstChild;
            modalFooter.insertBefore(botaoCancelar, primeiroFilho);
        }
    } else {
        
        const botaoCancelarExistente = document.querySelector('#modalFormulario .modal-footer #botaoCancelarAdocao');
        if (botaoCancelarExistente) botaoCancelarExistente.remove();
    }

    
    const organizarRespostas = () => {
        
        const respostas = [];
        
        
        const categoriasPerguntas = {
            "Qual espaço o pet terá disponível?": "residencia",
            "Descreva sua moradia": "residencia",
            "Tipo de residência": "residencia",
            "Pessoas na residência": "residencia",
            "Conte-nos sobre sua experiência com pets": "experiencia",
            "Por que você quer adotar este pet?": "experiencia",
            "Renda mensal aproximada": "financeiro",
            "Como planeja arcar com os custos do pet?": "financeiro",
            "O que fará com o pet quando precisar viajar?": "disponibilidade",
            "Quanto tempo livre você tem para dedicar ao pet diariamente?": "disponibilidade"
        };
        
        
        if (formulario.espacoAdequado) {
            respostas.push({ 
                pergunta: "Qual espaço o pet terá disponível?", 
                resposta: formulario.espacoAdequado,
                categoria: "residencia"
            });
        }
        if (formulario.experienciaAnterior) {
            respostas.push({ 
                pergunta: "Conte-nos sobre sua experiência com pets", 
                resposta: formulario.experienciaAnterior,
                categoria: "experiencia"
            });
        }
        if (formulario.numeroMoradores) {
            respostas.push({ 
                pergunta: "Pessoas na residência", 
                resposta: formulario.numeroMoradores.toString(),
                categoria: "residencia"
            });
        }
        if (formulario.descricaoMoradia) {
            respostas.push({ 
                pergunta: "Descreva sua moradia", 
                resposta: formulario.descricaoMoradia,
                categoria: "residencia"
            });
        }
        if (formulario.tipoResidencia) {
            respostas.push({ 
                pergunta: "Tipo de residência", 
                resposta: formulario.tipoResidencia,
                categoria: "residencia"
            });
        }
        if (formulario.rendaMensal) {
            respostas.push({ 
                pergunta: "Renda mensal aproximada", 
                resposta: `R$ ${parseFloat(formulario.rendaMensal).toLocaleString('pt-BR', {minimumFractionDigits: 2, maximumFractionDigits: 2})}`,
                categoria: "financeiro"
            });
        }
        if (formulario.condicoesFinanceiras) {
            respostas.push({ 
                pergunta: "Como planeja arcar com os custos do pet?", 
                resposta: formulario.condicoesFinanceiras,
                categoria: "financeiro"
            });
        }
        if (formulario.motivacaoAdocao) {
            respostas.push({ 
                pergunta: "Por que você quer adotar este pet?", 
                resposta: formulario.motivacaoAdocao,
                categoria: "experiencia"
            });
        }
        if (formulario.planejamentoViagens) {
            respostas.push({ 
                pergunta: "O que fará com o pet quando precisar viajar?", 
                resposta: formulario.planejamentoViagens,
                categoria: "disponibilidade"
            });
        }
        if (formulario.tempoDisponivel) {
            respostas.push({ 
                pergunta: "Quanto tempo livre você tem para dedicar ao pet diariamente?", 
                resposta: formulario.tempoDisponivel + " horas por dia",
                categoria: "disponibilidade"
            });
        }
        
        
        if (Array.isArray(formulario.respostas)) {
            formulario.respostas.forEach(item => {
                
                const jaExiste = respostas.some(resp => resp.pergunta === item.pergunta);
                if (!jaExiste) {
                    
                    const categoria = categoriasPerguntas[item.pergunta] || "geral";
                    respostas.push({
                        ...item,
                        categoria: categoria
                    });
                }
            });
        }
        
        
        let htmlRespostas = '';
        
        respostas.forEach(item => {
            const respostaVazia = !item.resposta || item.resposta.trim() === '';
            const classeResposta = respostaVazia ? 'resposta nao-respondida' : 'resposta';
            const textoResposta = respostaVazia ? 'Não respondido' : escapeHTML(item.resposta);
            
            htmlRespostas += `
                <div class="detalhe-resposta-item" data-categoria="${escapeHTML(item.categoria || 'geral')}">
                    <div class="pergunta">${escapeHTML(item.pergunta)}</div>
                    <div class="${classeResposta}">${textoResposta}</div>
                </div>
            `;
        });
        
        
        setTimeout(() => {
            const tituloRespostasCard = document.querySelector('.detalhe-respostas-card h3');
            if (tituloRespostasCard) {
                tituloRespostasCard.innerHTML = `
                    <i class="fas fa-clipboard-list"></i> 
                    Respostas do Formulário
                    <span class="contador-perguntas">${respostas.length}</span>
                `;
            }
        }, 200);
        
        return htmlRespostas;
    };

    
    let html = `
        <div class="detalhes-adocao-container">
            <!-- Cabeçalho com Dados do Pet -->
            <div class="detalhe-pet-header">
                <div class="detalhe-pet-info">
                    <div class="detalhe-pet-foto">
                        <img src="${pet.imagemUrl || '/imagens/pets/pet-placeholder.jpg'}" alt="${escapeHTML(pet.nome)}" class="img-fluid">
                    </div>
                    <div class="detalhe-pet-dados">
                        <h2 class="detalhe-pet-nome">${escapeHTML(pet.nome || 'Pet Não Identificado')}</h2>
                        <div class="detalhe-pet-badges">
                            <span class="pet-badge">
                                <i class="fas fa-paw"></i> ${escapeHTML(pet.especie || 'Não informada')}
                            </span>
                            <span class="pet-badge">
                                <i class="fas fa-dog"></i> ${escapeHTML(pet.raca || 'Não informada')}
                            </span>
                            <span class="pet-badge">
                                <i class="fas ${(pet.sexo || '').toLowerCase() === 'macho' ? 'fa-mars' : 'fa-venus'}"></i> ${escapeHTML(pet.sexo || 'Não informado')}
                            </span>
                            <span class="pet-badge">
                                <i class="fas fa-birthday-cake"></i> ${escapeHTML(pet.idade ? `${pet.idade} ano(s)` : 'Idade não informada')}
                            </span>
                        </div>
                    </div>
                </div>
            </div>
            
            <!-- Status da Adoção -->
            <div class="detalhe-status-card">
                <div class="detalhe-status-header">
                    <h3>
                        <i class="fas fa-info-circle"></i>
                        Status da Adoção
                    </h3>
                    <span class="badge-status-detalhes ${normalizarStatusParaClasse(statusTimeline)}" 
                          data-status="${statusTimeline || 'Não definido'}">
                        ${formatarStatus(statusTimeline)}
                    </span>
                </div>
                <div class="detalhe-status-body">
                    <div class="detalhe-data">
                        <div class="data-item">
                            <span class="data-label"><i class="fas fa-calendar-plus"></i> Data de Envio</span>
                            <span class="data-value">${formatarData(formulario.dataEnvio)}</span>
                        </div>
                        ${formulario.dataResposta ? `
                        <div class="data-item">
                            <span class="data-label"><i class="fas fa-calendar-check"></i> Data de Resposta</span>
                            <span class="data-value">${formatarData(formulario.dataResposta)}</span>
                        </div>
                        ` : ''}
                        ${formulario.dataCancelamento ? `
                        <div class="data-item">
                            <span class="data-label"><i class="fas fa-calendar-times"></i> Data de Cancelamento</span>
                            <span class="data-value">${formatarData(formulario.dataCancelamento)}</span>
                        </div>
                        ` : ''}
                    </div>
                    
                    ${formulario.observacaoResposta ? `
                    <div class="detalhe-observacao">
                        <h4><i class="fas fa-comment"></i> Observações da Equipe</h4>
                        <p>${escapeHTML(formulario.observacaoResposta)}</p>
                    </div>
                    ` : ''}
                    
                    ${motivoCancelamento ? `
                    <div class="detalhe-cancelamento">
                        <h4 class="detalhe-cancelamento-titulo"><i class="fas fa-ban"></i> Motivo do Cancelamento</h4>
                        <p class="detalhe-cancelamento-texto">${escapeHTML(motivoCancelamento)}</p>
                        ${formulario.dataCancelamento ? `
                        <div class="detalhe-cancelamento-data">
                            Cancelado em: ${formatarData(formulario.dataCancelamento)}
                        </div>
                        ` : ''}
                    </div>
                    ` : ''}
                </div>
            </div>
            
            <!-- Respostas do Formulário -->
            <div class="detalhe-respostas-card">
                <h3><i class="fas fa-clipboard-list"></i> Respostas do Formulário</h3>
                <div class="detalhe-respostas-grid">
                    ${organizarRespostas()}
                </div>
            </div>
        </div>
    `;
    
    formularioDetalhes.innerHTML = html;
    
            setTimeout(() => {
        try {
            inicializarTooltips(formularioDetalhes);
            
            inicializarBadgesStatus();
        } catch (tooltipError) {
            console.warn("Erro ao inicializar tooltips:", tooltipError);
        }
        
        if (loadingDetalhes) loadingDetalhes.style.display = 'none';
        formularioDetalhes.style.display = 'block';
    }, 100);
}

function atualizarStatusNaListagem(id, novoStatus, observacoesCancelamento) {
    try {
        
    
        
        if (!id) {
            console.warn("ID de adoção inválido");
            return;
        }
        
        
        const adocaoItem = document.querySelector(`.adocao-item[data-id="${id}"]`);
    if (!adocaoItem) {
            
        return;
    }
    
        
        const badgeStatus = adocaoItem.querySelector('.badge-status');
        const cardAdocao = adocaoItem.querySelector('.card-adocao');
        
        if (!badgeStatus || !cardAdocao) {
            console.warn(`Elementos de status não encontrados para adoção ID ${id}`);
            return;
        }
        
        
        const statusFormatado = formatarStatus(novoStatus);
        
        
        let statusClasse = novoStatus.toLowerCase().replace(' ', '-');
        let statusIcone = obterIconeStatus(novoStatus.toLowerCase());
        
        
        badgeStatus.className = `badge-status ${statusClasse}`;
        
        
        badgeStatus.innerHTML = `
            <i class="fas ${statusIcone} me-1"></i>
            ${statusFormatado}
        `;
        
        
        adocaoItem.setAttribute('data-status', novoStatus.toLowerCase());
        
        
        cardAdocao.classList.add('highlight-update');
        setTimeout(() => {
            cardAdocao.classList.remove('highlight-update');
        }, 2000);
        
        
        if (typeof aplicarFiltros === 'function') {
            aplicarFiltros();
        }
        
    } catch (error) {
        console.error(`Erro ao atualizar status na listagem: ${error.message}`);
    }
}


function confirmarCancelamentoAdocao(adocaoId) {
    
    
    document.getElementById('adocaoIdCancelar').value = adocaoId;
    
    
    const motivoCancelamento = document.getElementById('motivoCancelamento');
    if (motivoCancelamento) {
        motivoCancelamento.value = '';
        motivoCancelamento.classList.remove('is-invalid');
    }
    
    
    const erroMotivo = document.getElementById('erroMotivo');
    if (erroMotivo) {
        erroMotivo.style.display = 'none';
    }
    
    
    const btnConfirmarCancelamento = document.getElementById('btnConfirmarCancelamento');
    if (btnConfirmarCancelamento) {
        btnConfirmarCancelamento.disabled = false;
        btnConfirmarCancelamento.innerHTML = '<i class="fas fa-times-circle me-2"></i>Confirmar Cancelamento';
    }
    
    
    const modalCancelamento = new bootstrap.Modal(document.getElementById('modalCancelamento'));
    modalCancelamento.show();
}


function processarCancelamentoAdocao() {
    const adocaoId = document.getElementById('adocaoIdCancelar').value;
    const motivoCancelamento = document.getElementById('motivoCancelamento').value.trim();
    const erroMotivo = document.getElementById('erroMotivo');
    const btnConfirmarCancelamento = document.getElementById('btnConfirmarCancelamento');
    
    
    if (!motivoCancelamento || motivoCancelamento.length < 10) {
        document.getElementById('motivoCancelamento').classList.add('is-invalid');
        erroMotivo.textContent = "O motivo do cancelamento deve ter pelo menos 10 caracteres.";
        erroMotivo.style.display = 'block';
        return;
    }
    
    
    btnConfirmarCancelamento.disabled = true;
    btnConfirmarCancelamento.innerHTML = `
        <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
        Processando...
    `;
    
    
    const dados = {
        motivoCancelamento: motivoCancelamento
    };
    
    
    const token = document.getElementById('requestVerificationToken').value;
    
    
    fetch(`${CONFIG.api.baseUrl}/cancelar-formulario-adocao/${adocaoId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest',
            'RequestVerificationToken': token
        },
        body: JSON.stringify(dados)
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`Erro na requisição: ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        
        const modalCancelamento = bootstrap.Modal.getInstance(document.getElementById('modalCancelamento'));
        modalCancelamento.hide();
        
        if (data.success) {
            
            toastr.success(data.message || 'Adoção cancelada com sucesso!');
            
            
            const item = document.querySelector(`.adocao-item[data-id="${adocaoId}"]`);
            if (item) {
                const statusBadge = item.querySelector('.badge-status');
                if (statusBadge) {
                    statusBadge.className = 'badge-status cancelada';
                    statusBadge.innerHTML = '<i class="fas fa-times-circle me-2"></i>Cancelada';
                }
                
                
                item.setAttribute('data-status', 'cancelada');
                
                
                const botoesAcao = item.querySelector('.acoes-adocao');
                if (botoesAcao) {
                    
                    const botaoDetalhes = botoesAcao.querySelector('.botao-detalhes');
                    if (botaoDetalhes) {
                        botoesAcao.innerHTML = '';
                        botoesAcao.appendChild(botaoDetalhes);
                    }
                }
                
                const infoAdocao = item.querySelector('.info-adocao');
                if (infoAdocao) {
                    
                    const alertasExistentes = infoAdocao.querySelectorAll('.alert');
                    alertasExistentes.forEach(alerta => alerta.remove());
                    
                    
                    const alertaCancelada = document.createElement('div');
                    alertaCancelada.className = 'mt-2 alert alert-danger p-2 d-flex align-items-center animate__animated animate__fadeIn';
                    alertaCancelada.innerHTML = `
                        <i class="fas fa-times-circle me-2"></i>
                        <span>Adoção cancelada. <strong>Clique em "Detalhes"</strong> para mais informações.</span>
                    `;
                    infoAdocao.appendChild(alertaCancelada);
                }
            }
            
            
            if (state.adocaoIdAtual == adocaoId) {
                visualizarFormulario(adocaoId);
            }
        } else {
            
            toastr.error(data.message || 'Ocorreu um erro ao cancelar a adoção. Tente novamente mais tarde.');
            
            
            btnConfirmarCancelamento.disabled = false;
            btnConfirmarCancelamento.innerHTML = '<i class="fas fa-times-circle me-2"></i>Confirmar Cancelamento';
        }
    })
    .catch(error => {
        console.error('Erro:', error);
        toastr.error('Ocorreu um erro ao processar o cancelamento. Por favor, tente novamente mais tarde.');
        
        
        btnConfirmarCancelamento.disabled = false;
        btnConfirmarCancelamento.innerHTML = '<i class="fas fa-times-circle me-2"></i>Confirmar Cancelamento';
    });
}


function gerarRespostasHTML(respostas) {
    if (!respostas || !Array.isArray(respostas) || respostas.length === 0) {
        return `
            <div class="alert alert-info mb-0">
                <div class="d-flex align-items-center">
                    <i class="fas fa-info-circle me-3 fs-4"></i>
                    <span>Não há respostas disponíveis para exibir. As informações do formulário não foram encontradas.</span>
                </div>
            </div>
        `;
    }
    
    
    function agruparRespostas(respostas) {
        if (!respostas || !Array.isArray(respostas) || respostas.length === 0) {
            return {
                'residencia': {
                    titulo: 'Informações da Residência',
                    icone: 'home',
                    respostas: []
                },
                'experiencia': {
                    titulo: 'Experiência com Animais',
                    icone: 'paw',
                    respostas: []
                },
                'disponibilidade': {
                    titulo: 'Disponibilidade e Cuidados',
                    icone: 'clock',
                    respostas: []
                }
            };
        }
        
        const categorias = {
            'residencia': {
                titulo: 'Informações da Residência',
                icone: 'home',
                classe: 'residence',
                respostas: []
            },
            'experiencia': {
                titulo: 'Experiência com Animais',
                icone: 'paw',
                classe: 'experience',
                respostas: []
            },
            'disponibilidade': {
                titulo: 'Disponibilidade e Cuidados',
                icone: 'clock',
                classe: 'availability',
                respostas: []
            }
        };
        
        
        respostas.forEach(resp => {
            if (resp.pergunta.toLowerCase().includes('mora') || 
                resp.pergunta.toLowerCase().includes('residência') || 
                resp.pergunta.toLowerCase().includes('casa') || 
                resp.pergunta.toLowerCase().includes('apartamento') ||
                resp.pergunta.toLowerCase().includes('quintal')) {
                categorias.residencia.respostas.push(resp);
            }
            else if (resp.pergunta.toLowerCase().includes('animal') || 
                    resp.pergunta.toLowerCase().includes('pet') || 
                    resp.pergunta.toLowerCase().includes('cão') || 
                    resp.pergunta.toLowerCase().includes('gato') ||
                    resp.pergunta.toLowerCase().includes('cachorro')) {
                categorias.experiencia.respostas.push(resp);
            }
            else {
                categorias.disponibilidade.respostas.push(resp);
            }
        });
        
        return categorias;
    }
    
    
    const categorias = agruparRespostas(respostas);
    let html = '';
    
    const gerarSecao = (categoria, titulo, icone, classe) => {
        if (categoria.respostas.length === 0) return '';
        
        let secaoHtml = `
            <div class="form-section ${classe}">
                <div class="form-section-title">
                    <div class="section-icon">
                        <i class="fas fa-${icone}"></i>
                    </div>
                    <h4>${titulo}</h4>
                </div>
                <div class="form-answers">
        `;
        
        categoria.respostas.forEach(resp => {
            secaoHtml += `
                <div class="answer-card">
                    <div class="question">${resp.pergunta}</div>
                    <div class="answer">${resp.resposta || '<span style="color: #999; font-style: italic;">Não respondido</span>'}</div>
                </div>
            `;
        });
        
        secaoHtml += `
                </div>
            </div>
        `;
        
        return secaoHtml;
    };
    
    html += gerarSecao(categorias.residencia, categorias.residencia.titulo, categorias.residencia.icone, categorias.residencia.classe);
    html += gerarSecao(categorias.experiencia, categorias.experiencia.titulo, categorias.experiencia.icone, categorias.experiencia.classe);
    html += gerarSecao(categorias.disponibilidade, categorias.disponibilidade.titulo, categorias.disponibilidade.icone, categorias.disponibilidade.classe);
    
    return html || `
        <div class="alert alert-info mb-0">
            <div class="d-flex align-items-center">
                <i class="fas fa-info-circle me-3 fs-4"></i>
                <span>Nenhuma informação categorizada encontrada no formulário.</span>
            </div>
        </div>
    `;
}

function gerarBadgeStatus(status) {
    const statusLower = status.toLowerCase();
    
    let classe = '';
    let icone = '';
    let texto = status;
    
    if (statusLower === 'pendente') {
        classe = 'pendente';
        icone = 'fa-clock';
        texto = 'Em Análise';
    } else if (statusLower === 'em análise') {
        classe = 'em-processo';
        icone = 'fa-sync';
        texto = 'Em Processo';
    } else if (statusLower === 'aprovado') {
        classe = 'aprovada';
        icone = 'fa-check-circle';
        texto = 'Aprovado';
    } else if (statusLower === 'aguardando buscar') {
        classe = 'aguardando-buscar';
        icone = 'fa-hourglass-half';
        texto = 'Aguardando buscar';
    } else if (statusLower === 'finalizada') {
        classe = 'finalizada';
        icone = 'fa-heart';
        texto = 'Finalizada';
    } else if (statusLower === 'cancelada' || statusLower === 'rejeitada') {
        classe = 'cancelada';
        icone = 'fa-times-circle';
        texto = statusLower === 'cancelada' ? 'Cancelada' : 'Rejeitada';
    } else if (statusLower === 'em processo') {
        classe = 'em-processo';
        icone = 'fa-sync';
        texto = 'Em Processo';
    }
    
    
    return `<span class="indicador-status ${classe}"><i class="${icone} me-2"></i>${texto}</span>`;
}


function formatarData(dataString) {
    if (!dataString) return 'Não informada';
    
    try {
        const data = new Date(dataString);
        return data.toLocaleDateString('pt-BR', { 
            day: '2-digit', 
            month: '2-digit', 
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    } catch (e) {
        console.warn("Erro ao formatar data:", e);
        return dataString;
    }
}


async function carregarDetalhesFormulario(idFormulario) {
    try {
        const response = await fetch(`${CONFIG.api.baseUrl}/formulario-detalhes/${idFormulario}`);
        if (!response.ok) throw new Error('Erro ao carregar detalhes do formulário');
        
        const data = await response.json();
        if (!data.success) throw new Error(data.message || 'Erro ao carregar detalhes');
        
        
        const adocaoResponse = await fetch(`${CONFIG.api.baseUrl}/status-adocao/${idFormulario}`);
        const adocaoData = await adocaoResponse.json();
        
        
        const status = adocaoData.success ? adocaoData.status : data.status;
        
        
        const badgeStatus = document.getElementById('badgeStatus');
        if (badgeStatus) {
            badgeStatus.innerHTML = gerarBadgeStatus(status);
        }
        
    } catch (error) {
        console.error('Erro ao carregar detalhes:', error);
        toastr.error('Erro ao carregar detalhes do formulário');
    }
}


function verMaisDetalhes(id, tipo) {
    
    if (tipo === 'pet') {
        
        window.location.href = `/pets/detalhes/${id}`;
    } else if (tipo === 'adocao') {
        
        visualizarFormulario(id);
    }
}

function gerarItemEtapa(titulo, concluida) {
    return `<li class="${concluida ? 'concluido' : ''}">
        <span class="marcador"></span>
        <span class="texto">${titulo}</span>
    </li>`;
}

function obterIconeStatus(status) {
    
    const statusNormalizado = (status || '').toLowerCase().trim();
    
    const icones = {
        'pendente': 'fa-clock',
        'em análise': 'fa-clock',
        'em analise': 'fa-clock',
        'aprovado': 'fa-check-circle',
        'aguardando buscar': 'fa-hourglass-half',
        'aguardando-buscar': 'fa-hourglass-half',
        'finalizada': 'fa-heart',
        'finalizado': 'fa-heart',
        'rejeitada': 'fa-times-circle',
        'rejeitado': 'fa-times-circle',
        'cancelada': 'fa-ban',
        'cancelado': 'fa-ban',
        'em processo': 'fa-sync'
    };
    
    
    if (icones[statusNormalizado]) {
        return icones[statusNormalizado];
    }
    
    
    for (const [key, value] of Object.entries(icones)) {
        if (statusNormalizado.includes(key)) {
            return value;
        }
    }
    
    
    return 'fa-question-circle';
}

function formatarStatus(status) {
    if (!status) return 'Não definido';
    
    
    const statusNormalizado = status.toLowerCase().trim();
    
    const map = {
        'pendente': 'Em Análise',
        'em análise': 'Em Análise',
        'em analise': 'Em Análise',
        'aprovado': 'Aprovado',
        'aguardando buscar': 'Aguardando Buscar',
        'aguardando-buscar': 'Aguardando Buscar',
        'finalizada': 'Finalizada',
        'finalizado': 'Finalizada',
        'rejeitada': 'Rejeitada',
        'rejeitado': 'Rejeitada',
        'cancelada': 'Cancelada',
        'cancelado': 'Cancelada',
        'em processo': 'Em Processo'
    };
    
    
    if (map[statusNormalizado]) {
        return map[statusNormalizado];
    }
    
    
    for (const [key, value] of Object.entries(map)) {
        if (statusNormalizado.includes(key)) {
            return value;
        }
    }
    
    
    return status;
}

function atualizarStatusBadge(status) {
    let classe = '';
    let texto = '';
    
    switch (status.toLowerCase()) {
        case 'em análise':
        case 'pendente':
            classe = 'pendente';
            texto = 'Em Análise';
            break;
        case 'aprovado':
            classe = 'aprovada';
            texto = 'Aprovado';
            break;
        case 'aguardando buscar':
            classe = 'aguardando-buscar';
            texto = 'Aguardando buscar';
            break;
        case 'finalizada':
            classe = 'finalizada';
            texto = 'Finalizada';
            break;
        case 'rejeitada':
            classe = 'cancelada';
            texto = 'Não Aprovada';
            break;
        case 'cancelada':
            classe = 'cancelada';
            texto = 'Cancelada';
            break;
        case 'em processo':
            classe = 'em-processo';
            texto = 'Em Processo';
            break;
        default:
            classe = 'pendente';
            texto = status || 'Pendente';
    }
    
    return { classe, texto };
}

function gerarEtapa(data, status, descricao, dataStatus) {
    try {
        
        
        if (typeof data === 'object' && data !== null) {
        const classeItem = data.status === status || 
                         (status === 'Aprovado' && (data.status === 'Aguardando buscar' || data.status === 'Finalizada')) ? 
                         'active' : 
                         (data.status === 'Rejeitada' || data.status === 'Cancelada' ? 'inactive' : '');
                         
        return `
        <div class="timeline-item ${classeItem}">
                <div class="timeline-date">${dataStatus ? formatarData(dataStatus) : '-'}</div>
            <div class="timeline-title">${status}</div>
                <div class="timeline-desc">${descricao}</div>
        </div>
        `;
    }
    
        console.warn("Parâmetros incorretos para gerarEtapa:", { data, status, descricao, dataStatus });
        return '';
    } catch (error) {
        console.error("Erro ao gerar etapa:", error);
    return '';
    }
}

/**
 * Gera o HTML para o modal de detalhes do formulário
 * @param {Object} data - Objeto contendo os dados do formulário
 * @returns {string} - HTML do formulário
 */
function gerarHTMLDetalhesFormulario(data) {
    
    if (!data) {
        console.error("Dados vazios recebidos para gerarHTMLDetalhesFormulario");
        return '<div class="alert alert-danger">Erro ao carregar dados</div>';
    }
    
    if (!data.status) {
        data.status = 'Pendente';
    }
    
    
    const formatarDataSegura = (dataString) => {
        if (!dataString) return 'Não informada';
        try {
            return formatarData(dataString);
        } catch (e) {
            console.warn("Erro ao formatar data:", e, dataString);
            return dataString || 'Não informada';
        }
    };
    
    let html = '<div class="row animate__animated animate__fadeIn">';
    
    
    html += `
        <div class="col-md-6 mb-4">
            <div class="pet-card">
                <div class="pet-header">
                    <h5 class="mb-0"><i class="fas fa-paw me-2"></i>Informações do Pet</h5>
                </div>
                <div class="pet-body">
                    <div class="data-row">
                        <div class="data-label">Nome:</div>
                        <div class="data-value">${data.pet?.nome || 'Não informado'}</div>
                    </div>
                    <div class="data-row">
                        <div class="data-label">Espécie:</div>
                        <div class="data-value">${data.pet?.especie || 'Não informada'}</div>
                    </div>
                    <div class="data-row">
                        <div class="data-label">Raça:</div>
                        <div class="data-value">${data.pet?.raca || 'Não informada'}</div>
                    </div>
                    <div class="data-row">
                        <div class="data-label">Idade:</div>
                        <div class="data-value">${data.pet?.idade || 'Não informada'} ${data.pet?.idadeTipo || ''}</div>
                    </div>
                    <div class="data-row">
                        <div class="data-label">Porte:</div>
                        <div class="data-value">${data.pet?.porte || 'Não informado'}</div>
                    </div>
                    <div class="data-row">
                        <div class="data-label">Sexo:</div>
                        <div class="data-value">${data.pet?.sexo || 'Não informado'}</div>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    
    html += `
        <div class="col-md-6 mb-4">
            <div class="pet-card">
                <div class="pet-header">
                    <h5 class="mb-0"><i class="fas fa-clipboard-list me-2"></i>Detalhes da Adoção</h5>
                </div>
                <div class="pet-body">
                    <div class="data-row">
                        <div class="data-label">Data do Pedido:</div>
                        <div class="data-value">${formatarDataSegura(data.dataEnvio || data.dataCriacao)}</div>
                    </div>
                    ${data.dataResposta ? `
                    <div class="data-row">
                        <div class="data-label">Data da Resposta:</div>
                        <div class="data-value">${formatarDataSegura(data.dataResposta)}</div>
                    </div>
                    ` : ''}
                    ${data.status === 'Aprovado' || data.status === 'Aguardando buscar' ? `
                    <div class="data-row">
                        <div class="data-label">Prazo para Buscar:</div>
                        <div class="data-value">
                            <span class="fw-bold text-${data.diasRestantes <= 3 ? 'danger' : (data.diasRestantes <= 7 ? 'warning' : 'success')}">
                                ${data.diasRestantes || '?'} dia(s)
                            </span>
                        </div>
                    </div>
                    ` : ''}
                    ${data.dataCancelamento ? `
                    <div class="data-row">
                        <div class="data-label">Data de Cancelamento:</div>
                        <div class="data-value">${formatarDataSegura(data.dataCancelamento)}</div>
                    </div>
                    ` : ''}
                </div>
            </div>
        </div>
    `;
    
    
    html += `
        <div class="col-md-12 mb-4">
            <div class="card">
                <div class="card-header">
                    <h5 class="mb-0"><i class="fas fa-info-circle me-2"></i>Status da Adoção</h5>
                </div>
                <div class="card-body">
                    <div class="status-badge ${data.status.toLowerCase().replace(/\s+/g, '-')}">
                        <i class="fas ${obterIconeStatus(data.status.toLowerCase())} me-2"></i>
                        <span class="fw-bold">${formatarStatus(data.status)}</span>
                    </div>
                    
                    <div class="mt-4">
                        <h6 class="mb-3">Processo de Adoção</h6>
                        <div class="timeline-container">
                            <div class="timeline-item ${data.status.toLowerCase() === 'pendente' || data.status.toLowerCase() === 'em análise' ? 'active' : 'completed'}">
                                <div class="timeline-date">${formatarDataSegura(data.dataEnvio || data.dataCriacao)}</div>
                                <div class="timeline-title">Em Análise</div>
                                <div class="timeline-desc">Formulário em análise pela equipe</div>
                            </div>
                            
                            ${data.status.toLowerCase() !== 'pendente' && data.status.toLowerCase() !== 'em análise' && data.status.toLowerCase() !== 'rejeitada' ? `
                            <div class="timeline-item ${['aprovado', 'aguardando buscar', 'finalizada'].includes(data.status.toLowerCase()) ? 'active' : ''}">
                                <div class="timeline-date">${formatarDataSegura(data.dataResposta)}</div>
                                <div class="timeline-title">Aprovado</div>
                                <div class="timeline-desc">Adoção aprovada</div>
                            </div>
                            ` : ''}
                            
                            ${data.status.toLowerCase() === 'aguardando buscar' || data.status.toLowerCase() === 'aguardando-buscar' ? `
                            <div class="timeline-item active">
                                <div class="timeline-date">${formatarDataSegura(data.dataResposta)}</div>
                                <div class="timeline-title">Aguardando Buscar</div>
                                <div class="timeline-desc">Aguardando busca do pet</div>
                            </div>
                            ` : ''}
                            
                            ${data.status.toLowerCase() === 'finalizada' ? `
                            <div class="timeline-item active">
                                <div class="timeline-date">${formatarDataSegura(data.dataFinalizacao || data.dataResposta)}</div>
                                <div class="timeline-title">Finalizada</div>
                                <div class="timeline-desc">Adoção finalizada com sucesso!</div>
                            </div>
                            ` : ''}
                            
                            ${data.status.toLowerCase() === 'rejeitada' ? `
                            <div class="timeline-item active">
                                <div class="timeline-date">${formatarDataSegura(data.dataResposta)}</div>
                                <div class="timeline-title">Rejeitada</div>
                                <div class="timeline-desc">Adoção não aprovada</div>
                            </div>
                            ` : ''}
                            
                            ${data.status.toLowerCase() === 'cancelada' ? `
                            <div class="timeline-item active">
                                <div class="timeline-date">${formatarDataSegura(data.dataCancelamento)}</div>
                                <div class="timeline-title">Cancelada</div>
                                <div class="timeline-desc">Adoção cancelada</div>
                            </div>
                            ` : ''}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    
    if (data.observacaoResposta) {
        html += `
        <div class="col-12 mb-4 animate__animated animate__fadeIn">
            <div class="pet-card">
                <div class="pet-header">
                    <h5 class="mb-0"><i class="fas fa-comment-alt me-2"></i>Observações da Equipe</h5>
                </div>
                <div class="pet-body p-3">
                    <div class="detalhe-observacao">
                        <p>${data.observacaoResposta}</p>
                    </div>
                </div>
            </div>
        </div>
        `;
    }
    
    
    html += `
    <div class="col-12 animate__animated animate__fadeIn">
        <div class="pet-card">
            <div class="pet-header">
                <h5 class="mb-0"><i class="fas fa-history me-2"></i>Linha do Tempo</h5>
            </div>
            <div class="timeline-container">
                <div class="timeline-item ${data.status === 'Em Análise' ? 'active' : ''}">
                    <div class="timeline-date">${formatarDataSegura(data.dataEnvio || data.dataCriacao)}</div>
                    <div class="timeline-title">Em Análise</div>
                    <div class="timeline-desc">Formulário em análise pela equipe</div>
            </div>
                
                ${data.status !== 'Em Análise' && data.status !== 'Rejeitada' ? `
                <div class="timeline-item ${['Aprovado', 'Aguardando buscar', 'Finalizada'].includes(data.status) ? 'active' : ''}">
                    <div class="timeline-date">${formatarDataSegura(data.dataResposta)}</div>
                    <div class="timeline-title">Aprovado</div>
                    <div class="timeline-desc">Adoção aprovada</div>
                </div>
                ` : ''}
                
                ${data.status === 'Rejeitada' ? `
                <div class="timeline-item active">
                    <div class="timeline-date">${formatarDataSegura(data.dataResposta)}</div>
                    <div class="timeline-title">Rejeitada</div>
                    <div class="timeline-desc">Adoção não aprovada</div>
                </div>
                ` : ''}
                
                ${data.status === 'Aguardando buscar' ? `
                <div class="timeline-item active">
                    <div class="timeline-date">${formatarDataSegura(data.dataResposta)}</div>
                    <div class="timeline-title">Aguardando buscar</div>
                    <div class="timeline-desc">Aguardando busca do pet</div>
                </div>
                ` : ''}
                
                ${data.status === 'Finalizada' ? `
                <div class="timeline-item active">
                    <div class="timeline-date">${formatarDataSegura(data.dataFinalizacao || data.dataResposta)}</div>
                    <div class="timeline-title">Finalizada</div>
                    <div class="timeline-desc">Adoção finalizada com sucesso!</div>
                </div>
                ` : ''}
                
                ${data.status === 'Cancelada' ? `
                <div class="timeline-item active">
                    <div class="timeline-date">${formatarDataSegura(data.dataCancelamento)}</div>
                    <div class="timeline-title">Cancelada</div>
                    <div class="timeline-desc">Adoção cancelada</div>
                </div>
                ` : ''}
            </div>
        </div>
    </div>
    `;
    
    html += '</div>';
    
    return html;
}

function obterDetalhesFormulario(formularioId) {
    
    $('#spinnerDetalhesFormulario').show();
    
    $('#detalhesFormulario').hide();
    
    
    $('#modalDetalhesFormulario').modal('show');
    
    
    $.ajax({
        url: '/Usuario/Adocao/DetalhesFormulario',
        type: 'GET',
        data: { formularioId: formularioId },
        dataType: 'json',
        success: function(response) {
            
            $('#spinnerDetalhesFormulario').hide();
            
            if (response.success) {
                
                const htmlDetalhes = gerarHTMLDetalhesFormulario(response.data);
                
                
                $('#detalhesFormulario').html(htmlDetalhes).show();
            } else {
                
                toastr.error(response.message || 'Não foi possível carregar os detalhes do formulário.');
                $('#modalDetalhesFormulario').modal('hide');
            }
        },
        error: function(xhr, status, error) {
            
            $('#spinnerDetalhesFormulario').hide();
            
            
            toastr.error('Ocorreu um erro ao buscar os detalhes do formulário. Tente novamente mais tarde.');
            $('#modalDetalhesFormulario').modal('hide');
            
            
            console.error('Erro ao buscar detalhes do formulário:', error);
        }
    });
}


function inicializarBadgesStatus() {
    
    const badgeStatuses = document.querySelectorAll('.badge-status, .badge-status-detalhes');
    
    badgeStatuses.forEach(badge => {
        const statusClasses = Array.from(badge.classList)
            .filter(cls => cls !== 'badge-status' && cls !== 'badge-status-detalhes');
        
        if (statusClasses.length > 0) {
            const statusClass = statusClasses[0]; 
            let iconClass = '';
            
            switch(statusClass) {
                case 'pendente':
                    iconClass = 'fa-clock';
                    break;
                case 'aprovada':
                case 'aprovado':
                    iconClass = 'fa-check-circle';
                    break;
                case 'aguardando-buscar':
                    iconClass = 'fa-hourglass-half';
                    break;
                case 'finalizada':
                    iconClass = 'fa-heart';
                    break;
                case 'cancelada':
                    iconClass = 'fa-ban';
                    break;
                case 'rejeitada':
                    iconClass = 'fa-times-circle';
                    break;
                case 'em-processo':
                    iconClass = 'fa-sync';
                    break;
                default:
                    iconClass = 'fa-info-circle';
            }
            
            
            if (!badge.querySelector('i')) {
                
                const texto = badge.textContent.trim();
                badge.innerHTML = '';
                
                const icon = document.createElement('i');
                icon.className = `fas ${iconClass}`;
                icon.style.marginRight = '0.5rem';
                
                
                badge.appendChild(icon);
                badge.appendChild(document.createTextNode(texto));
            }
        }
    });
}

function normalizarStatusParaClasse(status) {
    const statusLower = status.toLowerCase();
    if (statusLower === 'pendente' || statusLower === 'em análise' || statusLower === 'em analise') {
        return 'pendente';
    } else if (statusLower === 'aprovado') {
        return 'aprovado';
    } else if (statusLower === 'aguardando buscar' || statusLower === 'aguardando-buscar') {
        return 'aguardando-buscar';
    } else if (statusLower === 'finalizada' || statusLower === 'finalizado') {
        return 'finalizada';
    } else if (statusLower === 'rejeitada' || statusLower === 'rejeitado') {
        return 'rejeitada';
    } else if (statusLower === 'cancelada' || statusLower === 'cancelado') {
        return 'cancelada';
    } else if (statusLower === 'em processo') {
        return 'em-processo';
    } else {
        return 'pendente';
    }
}

$(document).ready(function() {
    // Manipulador para o seletor de itens por página
    $('#selectItensPorPagina').change(function() {
        var itensPorPagina = $(this).val();
        window.location.href = '/usuario/adocoes?pagina=1&itensPorPagina=' + itensPorPagina;
    });
    
    // Corrigir problema de paginação
    $(document).on('click', '.pagination .page-link', function(e) {
        e.preventDefault();
        var url = $(this).attr('href');
        if (url) {
            window.location.href = url;
        }
        return false;
    });
});