var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
/// <reference types="bootstrap" />
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text !== null && text !== void 0 ? text : '';
    return div.innerHTML;
}

function handleImageError(img, userName) {
    const inicial = (userName || 'U').charAt(0).toUpperCase();
    img.style.display = 'none';
    img.parentElement.innerHTML = `<div class="avatar-placeholder-laranja">${inicial}</div>`;
}
let modalDetalhesFormulario = null;
let idFormularioAtual = null;
let observacaoAdminInicial = '';
let statusFormularioAtual = '';
const coresStatus = {
    'Pendente': 'pendente',
    'Aprovado': 'aprovado',
    'Rejeitada': 'rejeitado',
    'Cancelada': 'cancelado',
    'Cancelado pelo Usuario': 'cancelado',
    'Cancelado pelo Admin': 'cancelado',
    'Aguardando Busca': 'aguardando-buscar'
};
function analisarData(textoData) {
    const partes = textoData.split(' ')[0].split('/');
    return new Date(+partes[2], +partes[1] - 1, +partes[0]);
}
function filtrarFormularios() {
    const pesquisa = document.getElementById('search-input').value.toLowerCase();
    const status = document.getElementById('filter-status').value;
    const data = document.getElementById('filter-date').value;
    const hoje = new Date();
    hoje.setHours(0, 0, 0, 0);
    const ultimos7Dias = new Date();
    ultimos7Dias.setDate(ultimos7Dias.getDate() - 7);
    const ultimos30Dias = new Date();
    ultimos30Dias.setDate(ultimos30Dias.getDate() - 30);
    document.querySelectorAll('table tbody tr').forEach(linha => {
        var _a, _b, _c, _d, _e;
        const nome = ((_b = (_a = linha.querySelector('.nome-cliente')) === null || _a === void 0 ? void 0 : _a.textContent) === null || _b === void 0 ? void 0 : _b.toLowerCase()) || '';
        const emailElem = Array.from(linha.querySelectorAll('small')).find(s => { var _a; return (_a = s.textContent) === null || _a === void 0 ? void 0 : _a.includes('@'); });
        const email = emailElem ? emailElem.textContent.toLowerCase() : '';
        const enderecoElem = Array.from(linha.querySelectorAll('small')).find(s => s.innerHTML.includes('fa-map-marker-alt'));
        const endereco = enderecoElem ? enderecoElem.textContent.toLowerCase() : '';
        const statusLinha = ((_d = (_c = linha.querySelector('.indicador-status')) === null || _c === void 0 ? void 0 : _c.textContent) === null || _d === void 0 ? void 0 : _d.toLowerCase()) || '';
        const textoData = ((_e = linha.querySelector('.data')) === null || _e === void 0 ? void 0 : _e.textContent) || '';
        let correspondenciaData = true;
        if (data) {
            const dataFormulario = analisarData(textoData);
            if (data === 'hoje') {
                correspondenciaData = dataFormulario >= hoje;
            }
            else if (data === '7dias') {
                correspondenciaData = dataFormulario >= ultimos7Dias;
            }
            else if (data === '30dias') {
                correspondenciaData = dataFormulario >= ultimos30Dias;
            }
        }
        const correspondenciaNome = nome.includes(pesquisa) || email.includes(pesquisa) || endereco.includes(pesquisa);
        const correspondenciaStatus = !status || statusLinha === status.toLowerCase();
        linha.style.display = (correspondenciaNome && correspondenciaStatus && correspondenciaData) ? '' : 'none';
    });
    verificarResultadosPorSecao();
}
function verificarResultadosPorSecao() {
    const secoes = [
        { container: '.tabela-formularios', mensagem: '.mensagem-sem-formularios' }
    ];
    secoes.forEach(secao => {
        const linhasVisiveis = Array.from(document.querySelectorAll(`${secao.container} tbody tr`)).filter(tr => tr.style.display !== 'none').length;
        const mensagemEl = document.querySelector(secao.mensagem);
        if (!mensagemEl)
            return;
        if (linhasVisiveis === 0) {
            mensagemEl.classList.remove('d-none');
        }
        else {
            mensagemEl.classList.add('d-none');
        }
    });
}
document.querySelectorAll('#campoPesquisaForm, #filtroStatus, #filtroData').forEach(el => {
    el.addEventListener('input', filtrarFormularios);
    el.addEventListener('change', filtrarFormularios);
});
function limparFiltros() {
    document.getElementById('search-input').value = '';
    document.getElementById('filter-status').value = '';
    document.getElementById('filter-date').value = '';
    filtrarFormularios();
    toastr.info('Filtros limpos com sucesso!');
}
function limparAlertasModal() {
    document.querySelectorAll('#modalDetalhesFormulario .alert').forEach(el => el.remove());
}
function visualizarFormulario(id) {
    return __awaiter(this, void 0, void 0, function* () {
        limparAlertasModal();
        document.getElementById('formularioIdAtual').value = id;
        document.getElementById('observacaoAdmin').value = '';
        observacaoAdminInicial = '';
        statusFormularioAtual = '';
        document.getElementById('conteudoDetalhesFormulario').innerHTML = `
        <div class="d-flex justify-content-center my-5">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Carregando...</span>
            </div>
        </div>`;
        resetarPaineisConfirmacao();
        if (!modalDetalhesFormulario) {
            modalDetalhesFormulario = new bootstrap.Modal(document.getElementById('modalDetalhesFormulario'));
        }
        modalDetalhesFormulario.show();
        try {
            const response = yield fetch(`/admin/formularios-adocao/detalhes/${id}`);
            if (!response.ok)
                throw new Error('Falha ao obter detalhes');
            const text = yield response.text();
            let resposta;
            try {
                resposta = JSON.parse(text);
            }
            catch (_a) {
                resposta = { html: text };
            }
            let formulario = resposta.formulario || resposta.data || resposta.resultado;
            if (!formulario && resposta.id !== undefined)
                formulario = resposta;
            if (resposta.html) {
                document.getElementById('conteudoDetalhesFormulario').innerHTML = resposta.html;
            }
            else if (formulario) {
                document.getElementById('conteudoDetalhesFormulario').innerHTML = construirHTMLDetalhesFormulario(formulario);
            }
            const observacao = resposta.observacoesAdmin || (formulario ? (formulario.observacaoAdminFormulario || formulario.observacaoAdmin || formulario.observacoes) : '') || '';
            const obsInput = document.getElementById('observacaoAdmin');
            obsInput.value = observacao;
            observacaoAdminInicial = observacao;

            // Capturar observações de cancelamento
            window.observacoesCancelamentoAtual = resposta.observacoesCancelamento || (formulario ? (formulario.observacoesCancelamento || formulario.ObservacoesCancelamento) : '') || '';
            statusFormularioAtual = resposta.status || (formulario ? formulario.status : '');
            const botoesPrimarios = document.getElementById('botoesAcaoPrimarios');
            if (statusFormularioAtual !== 'Pendente') {
                botoesPrimarios.classList.add('d-none');
                obsInput.readOnly = true;
            }
            else {
                botoesPrimarios.classList.remove('d-none');
                obsInput.readOnly = false;
            }
            const botaoAprovar = document.getElementById('botaoAprovarNoModal');
            const botaoRejeitar = document.getElementById('botaoRejeitarNoModal');
            const botaoCancelaAprov = document.getElementById('botaoCancelarAprovacao');
            const botaoCancelaRej = document.getElementById('botaoCancelarRejeicao');
            const botaoConfAprov = document.getElementById('botaoConfirmarAprovacao');
            const botaoConfRej = document.getElementById('botaoConfirmarRejeicao');
            botaoAprovar.addEventListener('click', exibirConfirmacaoAprovacao, { once: true });
            botaoRejeitar.addEventListener('click', exibirConfirmacaoRejeicao, { once: true });
            botaoCancelaAprov.addEventListener('click', resetarPaineisConfirmacao, { once: true });
            botaoCancelaRej.addEventListener('click', resetarPaineisConfirmacao, { once: true });
            botaoConfAprov.addEventListener('click', aprovarFormularioConfirmado, { once: true });
            botaoConfRej.addEventListener('click', rejeitarFormularioConfirmado, { once: true });
        }
        catch (erro) {
            document.getElementById('conteudoDetalhesFormulario').innerHTML = `<div class="alert alert-danger"><i class="fas fa-times-circle me-2"></i>Ocorreu um erro ao carregar os detalhes do formulário.</div>`;
        }
    });
}
function construirHTMLDetalhesFormulario(formulario) {
    var _a, _b;
    try {
        const s = escapeHtml;
        const dataEnvio = new Date(formulario.dataEnvio || formulario.DataEnvio).toLocaleString('pt-BR');
        const dataResposta = (formulario.dataResposta || formulario.DataResposta) ? 
            new Date(formulario.dataResposta || formulario.DataResposta).toLocaleString('pt-BR') : '-';
        // Acessar dados com diferentes possibilidades de nomenclatura
        const usuario = formulario.usuario || formulario.Usuario || {};
        const pet = formulario.pet || formulario.Pet || {};
        
        // Definir classe do status
        const statusClass = formulario.status?.toLowerCase().replace(/\s+/g, '-') || 'pendente';
        
        let htmlConteudo = `<div class="detalhes-formulario-container" data-timestamp="${new Date().getTime()}">`;
        
        // Cabeçalho com resumo do formulário
        htmlConteudo += `
            <div class="cabecalho-formulario mb-4">
                <div class="row align-items-center">
                    <div class="col-md-8">
                        <div class="info-principal">
                            <div class="usuario-info">
                                <div class="avatar-usuario">
                                    ${usuario.fotoPerfil ? 
                                        `<img src="/imagens/perfil/${usuario.fotoPerfil}" alt="${s(usuario.nome || 'Usuário')}" class="avatar-img" onerror="handleImageError(this, '${s(usuario.nome || 'Usuário')}')">` :
                                        `<div class="avatar-placeholder-laranja">${(usuario.nome || 'U').charAt(0).toUpperCase()}</div>`
                                    }
                                </div>
                                <div class="detalhes-usuario-modal">
                                    <h5 class="nome-usuario mb-1">
                                        ${s(usuario.nome || usuario.nomeCompleto || 'Nome não informado')}
                                    </h5>
                                    <div class="contatos-grid">
                                        <div class="contato-item">
                                            <i class="fas fa-envelope"></i>
                                            <span>${s(usuario.email || 'Email não informado')}</span>
                                        </div>
                                        <div class="contato-item">
                                            <i class="fas fa-phone"></i>
                                            <span>${s(usuario.telefone || 'Telefone não informado')}</span>
                                        </div>
                                        <div class="contato-item">
                                            <i class="fas fa-map-marker-alt"></i>
                                            <span>${s((usuario.cidade || 'Cidade não informada'))} - ${s((usuario.estado || 'Estado não informado'))}</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-4 text-end">
                        <div class="formulario-meta">
                            <div class="status-badge status-${statusClass} mb-3">
                                <i class="fas fa-circle-dot me-1"></i>${s(formulario.status)}
                            </div>
                            <div class="datas-info">
                                <div class="data-item">
                                    <i class="fas fa-calendar-plus text-primary"></i>
                                    <div>
                                        <small class="label">Enviado em</small>
                                        <div class="valor">${s(dataEnvio)}</div>
                                    </div>
                                </div>
                                ${dataResposta !== '-' ? `
                                <div class="data-item">
                                    <i class="fas fa-calendar-check text-success"></i>
                                    <div>
                                        <small class="label">Respondido em</small>
                                        <div class="valor">${s(dataResposta)}</div>
                                    </div>
                                </div>` : ''}
                            </div>
                        </div>
                    </div>
                </div>
            </div>`;

        // Informações do Pet
        htmlConteudo += `
            <div class="secao-pet mb-4">
                <div class="secao-header">
                    <h6 class="secao-titulo">
                        <i class="fas fa-paw me-2 text-primary"></i>Pet de Interesse
                    </h6>
                </div>
                <div class="card-pet-melhorado">
                    <div class="pet-showcase">
                        <div class="pet-imagem-container">
                            ${(pet.nomeArquivoImagem || pet.NomeArquivoImagem) ? 
                                `<img src="/imagens/pets/${pet.nomeArquivoImagem || pet.NomeArquivoImagem}" alt="${s(pet.nome || pet.Nome)}" class="pet-imagem-melhorada">` :
                                `<div class="pet-placeholder-melhorado">
                                    <i class="fas fa-${(pet.especie || pet.Especie) === 'Cachorro' ? 'dog' : 'cat'} fa-3x"></i>
                                    <span class="placeholder-text">Sem foto</span>
                                </div>`
                            }
                        </div>
                        <div class="pet-info-container">
                            <div class="pet-header">
                                <h4 class="pet-nome-principal">${s(pet.nome || pet.Nome || 'Nome não informado')}</h4>
                                <div class="pet-id-badge">#${pet.id || pet.Id || 'N/A'}</div>
                            </div>
                            
                            <div class="pet-caracteristicas">
                                <div class="caracteristica-item especie">
                                    <div class="caracteristica-icon">
                                        <i class="fas fa-${(pet.especie || pet.Especie) === 'Cachorro' ? 'dog' : 'cat'}"></i>
                                    </div>
                                    <div class="caracteristica-info">
                                        <span class="caracteristica-label">Espécie</span>
                                        <span class="caracteristica-valor">${s(pet.especie || pet.Especie || 'Não informado')}</span>
                                    </div>
                                </div>
                                
                                <div class="caracteristica-item porte">
                                    <div class="caracteristica-icon">
                                        <i class="fas fa-ruler-vertical"></i>
                                    </div>
                                    <div class="caracteristica-info">
                                        <span class="caracteristica-label">Porte</span>
                                        <span class="caracteristica-valor">${s(pet.porte || pet.Porte || 'Não informado')}</span>
                                    </div>
                                </div>
                                
                                <div class="caracteristica-item sexo">
                                    <div class="caracteristica-icon">
                                        <i class="fas fa-venus-mars"></i>
                                    </div>
                                    <div class="caracteristica-info">
                                        <span class="caracteristica-label">Sexo</span>
                                        <span class="caracteristica-valor">${s(pet.sexo || pet.Sexo || 'Não informado')}</span>
                                    </div>
                                </div>
                                
                                ${pet.idade ? `
                                <div class="caracteristica-item idade">
                                    <div class="caracteristica-icon">
                                        <i class="fas fa-birthday-cake"></i>
                                    </div>
                                    <div class="caracteristica-info">
                                        <span class="caracteristica-label">Idade</span>
                                        <span class="caracteristica-valor">${s(pet.idade)}</span>
                                    </div>
                                </div>` : ''}
                            </div>
                        </div>
                    </div>
                </div>
            </div>`;

        // Respostas do Formulário organizadas por categoria
        htmlConteudo += `
            <div class="secoes-respostas">
                <div class="secao-header mb-3">
                    <h6 class="secao-titulo">
                        <i class="fas fa-clipboard-list me-2"></i>Respostas do Formulário
                    </h6>
                </div>
                
                <!-- Motivação e Experiência -->
                <div class="categoria-respostas mb-4">
                    <h6 class="categoria-titulo">
                        <i class="fas fa-heart text-danger me-2"></i>Motivação e Experiência
                    </h6>
                    <div class="respostas-grid">
                        <div class="resposta-item">
                            <div class="pergunta">
                                <i class="fas fa-question-circle me-2"></i>
                                Por que você quer adotar especificamente o(a) ${s(pet.nome)}?
                            </div>
                            <div class="resposta">${s(formulario.motivacaoAdocao)}</div>
                        </div>
                        <div class="resposta-item">
                            <div class="pergunta">
                                <i class="fas fa-history me-2"></i>
                                Conte-nos sobre sua experiência com pets
                            </div>
                            <div class="resposta">${s(formulario.experienciaAnterior)}</div>
                        </div>
                    </div>
                </div>

                <!-- Condições de Moradia -->
                <div class="categoria-respostas mb-4">
                    <h6 class="categoria-titulo">
                        <i class="fas fa-home text-success me-2"></i>Condições de Moradia
                    </h6>
                    <div class="respostas-grid">
                        <div class="resposta-item">
                            <div class="pergunta">
                                <i class="fas fa-expand me-2"></i>
                                Qual espaço o pet terá disponível?
                            </div>
                            <div class="resposta">${s(formulario.espacoAdequado)}</div>
                        </div>
                        <div class="resposta-item">
                            <div class="pergunta">
                                <i class="fas fa-building me-2"></i>
                                Descreva sua moradia
                            </div>
                            <div class="resposta">${s(formulario.descricaoMoradia)}</div>
                        </div>
                        <div class="resposta-item">
                            <div class="pergunta">
                                <i class="fas fa-users me-2"></i>
                                Quantas pessoas moram na residência?
                            </div>
                            <div class="resposta">${(_b = formulario.numeroMoradores) !== null && _b !== void 0 ? _b : 'Não informado'}</div>
                        </div>
                    </div>
                </div>

                <!-- Responsabilidade e Cuidados -->
                <div class="categoria-respostas mb-4">
                    <h6 class="categoria-titulo">
                        <i class="fas fa-shield-alt text-warning me-2"></i>Responsabilidade e Cuidados
                    </h6>
                    <div class="respostas-grid">
                        <div class="resposta-item">
                            <div class="pergunta">
                                <i class="fas fa-plane me-2"></i>
                                O que fará com o pet quando precisar viajar?
                            </div>
                            <div class="resposta">${s(formulario.planejamentoViagens)}</div>
                        </div>
                        <div class="resposta-item">
                            <div class="pergunta">
                                <i class="fas fa-dollar-sign me-2"></i>
                                Como planeja arcar com os custos do pet?
                            </div>
                            <div class="resposta">${s(formulario.condicoesFinanceiras)}</div>
                        </div>
                        <div class="resposta-item">
                            <div class="pergunta">
                                <i class="fas fa-money-bill-wave me-2"></i>
                                Renda mensal aproximada
                            </div>
                            <div class="resposta">${(_a = formulario.rendaMensal) !== null && _a !== void 0 ? _a : 'Não informado'}</div>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Seção de Observações de Cancelamento (apenas se existir)
        const observacoesCancelamento = window.observacoesCancelamentoAtual || formulario.observacoesCancelamento || formulario.ObservacoesCancelamento;
        if (observacoesCancelamento && observacoesCancelamento.trim()) {
            htmlConteudo += `
                <div class="secao-cancelamento mb-4">
                    <div class="secao-header mb-3">
                        <h6 class="secao-titulo">
                            <i class="fas fa-comment-alt me-2 text-warning"></i>Observações de Cancelamento do Usuário
                        </h6>
                    </div>
                    <div class="card-observacao-cancelamento">
                        <div class="observacao-content">
                            <i class="fas fa-user-times text-danger me-2"></i>
                            <div class="observacao-texto">${s(observacoesCancelamento)}</div>
                        </div>
                    </div>
                </div>
            `;
        }

        htmlConteudo += `
        </div>`;
        return htmlConteudo;
    }
    catch (erro) {
        return `<div class="alert alert-danger"><i class="fas fa-exclamation-triangle me-2"></i>Erro ao carregar detalhes do formulário. Por favor, tente novamente.</div>`;
    }
}
function exibirConfirmacaoAprovacao() {
    limparAlertasModal();
    document.getElementById('botoesAcaoPrimarios').classList.add('d-none');
    document.getElementById('confirmacaoRejeicao').classList.add('d-none');
    document.getElementById('confirmacaoAprovacao').classList.remove('d-none');
}
function exibirConfirmacaoRejeicao() {
    limparAlertasModal();
    document.getElementById('botoesAcaoPrimarios').classList.add('d-none');
    document.getElementById('confirmacaoAprovacao').classList.add('d-none');
    document.getElementById('confirmacaoRejeicao').classList.remove('d-none');
}
function resetarPaineisConfirmacao() {
    document.getElementById('confirmacaoAprovacao').classList.add('d-none');
    document.getElementById('confirmacaoRejeicao').classList.add('d-none');
    document.getElementById('botoesAcaoPrimarios').classList.remove('d-none');
    limparAlertasModal();
}
function aprovarFormularioConfirmado() {
    return __awaiter(this, void 0, void 0, function* () {
        const formularioId = document.getElementById('formularioIdAtual').value;
        const observacaoAdmin = document.getElementById('observacaoAdmin').value;
        const botao = document.getElementById('botaoConfirmarAprovacao');
        botao.disabled = true;
        botao.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Processando...';
        try {
            const formData = new FormData();
            formData.append('observacao', observacaoAdmin);
            formData.append('__RequestVerificationToken', document.querySelector('input[name="__RequestVerificationToken"]').value);
            const response = yield fetch(`/admin/formularios-adocao/Aprovar/${formularioId}`, { method: 'POST', body: formData });
            const resposta = yield response.json();
            botao.disabled = false;
            botao.innerHTML = '<i class="fas fa-check me-2"></i>Confirmar Aprovação';
            if (response.ok && resposta.sucesso) {
                toastr.success('Formulário aprovado com sucesso!');
                setTimeout(() => location.reload(), 1500);
            }
            else {
                toastr.error(resposta.mensagem || 'Erro ao aprovar formulário.');
                resetarPaineisConfirmacao();
            }
        }
        catch (erro) {
            botao.disabled = false;
            botao.innerHTML = '<i class="fas fa-check me-2"></i>Confirmar Aprovação';
            toastr.error('Erro ao aprovar formulário.');
            resetarPaineisConfirmacao();
        }
    });
}
function rejeitarFormularioConfirmado() {
    return __awaiter(this, void 0, void 0, function* () {
        const formularioId = document.getElementById('formularioIdAtual').value;
        const observacaoAdmin = document.getElementById('observacaoAdmin').value;
        if (!observacaoAdmin || observacaoAdmin.trim() === '') {
            toastr.warning('Por favor, preencha a observação indicando o motivo da rejeição.');
            return;
        }
        try {
            const formData = new FormData();
            formData.append('motivo', observacaoAdmin);
            formData.append('__RequestVerificationToken', document.querySelector('input[name="__RequestVerificationToken"]').value);
            const response = yield fetch(`/admin/formularios-adocao/Rejeitar/${formularioId}`, { method: 'POST', body: formData });
            const resposta = yield response.json();
            if (response.ok && resposta.sucesso) {
                toastr.success('Formulário rejeitado com sucesso!');
                setTimeout(() => location.reload(), 1500);
            }
            else {
                toastr.error(resposta.mensagem || 'Erro ao rejeitar formulário.');
                resetarPaineisConfirmacao();
            }
        }
        catch (_a) {
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
    // Function deprecated - use modal-based approach instead
    console.warn('rejeitarFormulario function is deprecated. Use modal-based approach.');
    return;
}
function processarFormulario(id, acao, observacao = '') {
    toastr.info(`Processando ${acao === 'aprovar' ? 'aprovação' : 'rejeição'} do formulário...`);
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    processarFormularioAcao(id, acao, token, observacao);
}
function processarFormularioAcao(id_1, acao_1, token_1) {
    return __awaiter(this, arguments, void 0, function* (id, acao, token, observacao = '') {
        const url = acao === 'aprovar'
            ? `/admin/formularios-adocao/Aprovar/${id}`
            : `/admin/formularios-adocao/Rejeitar/${id}`;
        const formData = new FormData();
        formData.append('__RequestVerificationToken', token);
        if (acao === 'rejeitar') {
            formData.append('motivo', observacao);
        }
        else {
            formData.append('observacao', observacao);
        }
        try {
            const response = yield fetch(url, { method: 'POST', body: formData });
            const resposta = yield response.json();
            if (response.ok && resposta && resposta.sucesso) {
                const classeStatus = acao === 'aprovar' ? 'aprovado' : 'rejeitado';
                const textoStatus = acao === 'aprovar' ? 'Aprovado' : 'Rejeitada';
                const linha = document.querySelector(`table tbody tr[data-id="${id}"]`);
                if (linha) {
                    const statusEl = linha.querySelector('.indicador-status');
                    statusEl.classList.remove('pendente', 'aprovado', 'rejeitado', 'cancelado', 'aguardando-buscar');
                    statusEl.classList.add(classeStatus);
                    
                    // Usar o mesmo formato do HTML original
                    const iconClass = obterIconeStatus(textoStatus);
                    statusEl.innerHTML = `<i class="${iconClass} me-2"></i>${textoStatus}`;
                    linha.querySelector('.botoes-acao').innerHTML = `
                    <button class="botao-acao botao-visualizar" onclick="visualizarFormulario(${id})">
                        <i class="fas fa-eye"></i>
                    </button>`;
                    linha.setAttribute('data-status', textoStatus);
                }
                atualizarContadoresFormularios();
                toastr.success(`Formulário ${acao === 'aprovar' ? 'aprovado' : 'rejeitado'} com sucesso!`);
                if (modalDetalhesFormulario) {
                    modalDetalhesFormulario.hide();
                }
            }
            else {
                toastr.error((resposta === null || resposta === void 0 ? void 0 : resposta.mensagem) || `Erro ao ${acao === 'aprovar' ? 'aprovar' : 'rejeitar'} formulário`);
            }
        }
        catch (_a) {
            toastr.error(`Erro ao ${acao === 'aprovar' ? 'aprovar' : 'rejeitar'} formulário`);
        }
    });
}
function atualizarContadoresFormularios() {
    const total = document.querySelectorAll('table tbody tr').length;
    const pendentes = document.querySelectorAll('table tbody tr[data-status="Pendente"]').length;
    const aprovados = document.querySelectorAll('table tbody tr[data-status="Aprovado"]').length;
    const rejeitados = document.querySelectorAll('table tbody tr[data-status="Rejeitada"]').length;
    const cancelados = document.querySelectorAll('table tbody tr[data-status^="Cancelad"]').length;
    document.querySelector('.card.resumo.total-formularios .h5').textContent = total.toString();
    document.querySelector('.card.resumo.pendentes .h5').textContent = pendentes.toString();
    document.querySelector('.card.resumo.aprovados .h5').textContent = aprovados.toString();
    document.querySelector('.card.resumo.rejeicoes .h5').textContent = (rejeitados + cancelados).toString();
}
function gerarAvatarInicial(nome) {
    if (!nome || nome.length === 0)
        return '?';
    return nome.charAt(0).toUpperCase();
}
function exibirFotoUsuario(fotoPerfil, nome) {
    if (fotoPerfil && fotoPerfil !== '') {
        return `<img src="/imagens/perfil/${fotoPerfil}" alt="${nome}" class="foto-usuario" onerror="handleImageError(this, '${nome}')">`;
    }
    else {
        return `<div class="avatar-inicial">${gerarAvatarInicial(nome)}</div>`;
    }
}
function obterIconeStatus(status) {
    // GAMBIARRA ELIMINADA - Ícones agora vem do servidor via extension methods
    // Mapeamento simples baseado em palavras-chave
    const statusLower = status.toLowerCase();
    
    if (statusLower.includes('aprovado') || statusLower.includes('check')) return 'fas fa-check-circle';
    if (statusLower.includes('rejeitado') || statusLower.includes('negado')) return 'fas fa-times-circle';
    if (statusLower.includes('cancelado')) return 'fas fa-times-circle';
    if (statusLower.includes('processo') || statusLower.includes('analise')) return 'fas fa-spinner';
    if (statusLower.includes('buscar') || statusLower.includes('paw')) return 'fas fa-paw';
    if (statusLower.includes('finalizado') || statusLower.includes('concluido')) return 'fas fa-check-double';
    
    return 'fas fa-hourglass-half'; // default
}
document.addEventListener('DOMContentLoaded', () => {
    document.addEventListener('click', event => {
        const link = event.target.closest('.pagination .page-link');
        if (link) {
            event.preventDefault();
            const url = link.getAttribute('href');
            if (url) {
                window.location.href = url;
            }
        }
    });
    const tabelaForm = document.querySelector('.tabela-formularios tbody');
    if (tabelaForm) {
        tabelaForm.addEventListener('click', event => {
            const botao = event.target.closest('.botao-visualizar, .botao-contorno-primario');
            if (botao) {
                const linha = botao.closest('tr');
                const id = linha === null || linha === void 0 ? void 0 : linha.getAttribute('data-id');
                if (id) {
                    event.preventDefault();
                    visualizarFormulario(id);
                }
            }
        });
    }
    const modalEl = document.getElementById('modalDetalhesFormulario');
    if (modalEl) {
        modalEl.addEventListener('hide.bs.modal', e => {
            const obsAtual = document.getElementById('observacaoAdmin').value;
            if (statusFormularioAtual === 'Pendente' && obsAtual && obsAtual !== observacaoAdminInicial) {
                // Remove confirm dialog - allow user to exit
                // Consider adding a toastr warning instead
                console.warn('Observações não salvas serão perdidas');
            }
        });
        modalEl.addEventListener('hidden.bs.modal', () => {
            document.getElementById('observacaoAdmin').value = '';
            observacaoAdminInicial = '';
            statusFormularioAtual = '';
        });
    }

    // Event listeners para filtros
    const btnFiltrar = document.getElementById('btnFiltrar');
    const btnLimparFiltros = document.getElementById('btnLimparFiltros');

    if (btnFiltrar) {
        btnFiltrar.addEventListener('click', filtrarFormularios);
    }

    if (btnLimparFiltros) {
        btnLimparFiltros.addEventListener('click', limparFiltros);
    }

    // Event listeners para filtrar automaticamente enquanto digita
    const searchInput = document.getElementById('search-input');
    const filterStatus = document.getElementById('filter-status');
    const filterDate = document.getElementById('filter-date');

    if (searchInput) {
        searchInput.addEventListener('input', filtrarFormularios);
    }

    if (filterStatus) {
        filterStatus.addEventListener('change', filtrarFormularios);
    }

    if (filterDate) {
        filterDate.addEventListener('change', filtrarFormularios);
    }
});
