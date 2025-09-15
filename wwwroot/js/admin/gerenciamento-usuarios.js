function verDetalhes(usuarioId) {
    const modal = new bootstrap.Modal(document.getElementById('modalDetalhesUsuario'));
    modal.show();
    
    fetch(`/admin/usuarios/detalhes-modal/${usuarioId}`)
        .then(response => response.json())
        .then(data => {
            if (data.sucesso) {
                preencherModalDetalhes(data.dados);
            } else {
                document.getElementById('conteudoDetalhesUsuario').innerHTML = 
                    `<div class="alert alert-danger"><i class="fas fa-exclamation-triangle me-2"></i>${data.mensagem}</div>`;
            }
        })
        .catch(error => {
            document.getElementById('conteudoDetalhesUsuario').innerHTML = 
                '<div class="alert alert-danger"><i class="fas fa-exclamation-triangle me-2"></i>Erro ao carregar detalhes do usuário.</div>';
        });
}

function preencherModalDetalhes(dados) {
    // Primeiro, esconde o loading e mostra o template profissional
    document.querySelector('.profile-loading-state').style.display = 'none';
    document.getElementById('profileDataTemplate').classList.remove('d-none');


    // Preenche estatísticas
    const totalAdoptionsElement = document.getElementById('totalAdoptions');
    if (totalAdoptionsElement) totalAdoptionsElement.textContent = dados.estatisticas?.totalAdocoes || 0;

    const successfulAdoptionsElement = document.getElementById('successfulAdoptions');
    if (successfulAdoptionsElement) successfulAdoptionsElement.textContent = dados.estatisticas?.adocoesFinalizadas || 0;

    const pendingAdoptionsElement = document.getElementById('pendingAdoptions');
    if (pendingAdoptionsElement) pendingAdoptionsElement.textContent = dados.estatisticas?.adocoesPendentes || 0;

    const formsSubmittedElement = document.getElementById('formsSubmitted');
    if (formsSubmittedElement) formsSubmittedElement.textContent = dados.estatisticas?.totalFormularios || 0;

    const violationsElement = document.getElementById('violations');
    if (violationsElement) violationsElement.textContent = dados.administracao?.numeroViolacoes || 0;

    // Preenche dados pessoais - Identificação (apenas se houver dados)
    const fullNameElement = document.getElementById('fullName');
    if (fullNameElement && dados.nome) {
        const valueText = fullNameElement.querySelector('.value-text');
        if (valueText) valueText.textContent = dados.nome;
        fullNameElement.closest('.info-field').style.display = 'flex';
    } else if (fullNameElement) {
        fullNameElement.closest('.info-field').style.display = 'none';
    }

    const cpfElement = document.getElementById('cpf');
    if (cpfElement && dados.cpf) {
        const valueText = cpfElement.querySelector('.value-text');
        const cpfFormatado = dados.cpf.length === 11
            ? `${dados.cpf.substring(0, 3)}.${dados.cpf.substring(3, 6)}.${dados.cpf.substring(6, 9)}-${dados.cpf.substring(9, 11)}`
            : dados.cpf;
        if (valueText) valueText.textContent = cpfFormatado;
        cpfElement.closest('.info-field').style.display = 'flex';
    } else if (cpfElement) {
        cpfElement.closest('.info-field').style.display = 'none';
    }

    const birthDateElement = document.getElementById('birthDate');
    if (birthDateElement && dados.dataNascimento) {
        const valueText = birthDateElement.querySelector('.value-text');
        if (valueText) valueText.textContent = dados.dataNascimento;
        birthDateElement.closest('.info-field').style.display = 'flex';
    } else if (birthDateElement) {
        birthDateElement.closest('.info-field').style.display = 'none';
    }

    const userAgeElement = document.getElementById('userAge');
    if (userAgeElement && dados.dataNascimento) {
        const valueText = userAgeElement.querySelector('.value-text');
        if (valueText) {
            const birthDate = new Date(dados.dataNascimento);
            const today = new Date();
            const age = today.getFullYear() - birthDate.getFullYear();
            valueText.textContent = `${age} anos`;
        }
        userAgeElement.closest('.info-field').style.display = 'flex';
    } else if (userAgeElement) {
        userAgeElement.closest('.info-field').style.display = 'none';
    }

    // Preenche endereço (apenas se houver dados)
    const addressElement = document.getElementById('address');
    if (addressElement && dados.endereco?.completo) {
        const valueText = addressElement.querySelector('.value-text');
        if (valueText) valueText.textContent = dados.endereco.completo;
        addressElement.closest('.info-field').style.display = 'flex';
    } else if (addressElement) {
        addressElement.closest('.info-field').style.display = 'none';
    }

    const cityElement = document.getElementById('city');
    if (cityElement && dados.endereco?.cidade) {
        const valueText = cityElement.querySelector('.value-text');
        if (valueText) valueText.textContent = dados.endereco.cidade;
        cityElement.closest('.info-field').style.display = 'flex';
    } else if (cityElement) {
        cityElement.closest('.info-field').style.display = 'none';
    }

    const stateElement = document.getElementById('state');
    if (stateElement && dados.endereco?.uf) {
        const valueText = stateElement.querySelector('.value-text');
        if (valueText) valueText.textContent = dados.endereco.uf;
        stateElement.closest('.info-field').style.display = 'flex';
    } else if (stateElement) {
        stateElement.closest('.info-field').style.display = 'none';
    }

    const zipCodeElement = document.getElementById('zipCode');
    if (zipCodeElement && dados.endereco?.cep) {
        const valueText = zipCodeElement.querySelector('.value-text');
        if (valueText) valueText.textContent = dados.endereco.cep;
        zipCodeElement.closest('.info-field').style.display = 'flex';
    } else if (zipCodeElement) {
        zipCodeElement.closest('.info-field').style.display = 'none';
    }

    // Preenche contato (apenas se houver dados)
    const primaryEmailElement = document.getElementById('primaryEmail');
    if (primaryEmailElement && dados.email) {
        primaryEmailElement.textContent = dados.email;
        primaryEmailElement.closest('.contact-method').style.display = 'flex';
    } else if (primaryEmailElement) {
        primaryEmailElement.closest('.contact-method').style.display = 'none';
    }

    const phoneNumberElement = document.getElementById('phoneNumber');
    if (phoneNumberElement && dados.telefone) {
        const telefoneFormatado = dados.telefone.length >= 10
            ? dados.telefone.length === 11
                ? `(${dados.telefone.substring(0, 2)}) ${dados.telefone.substring(2, 7)}-${dados.telefone.substring(7, 11)}`
                : `(${dados.telefone.substring(0, 2)}) ${dados.telefone.substring(2, 6)}-${dados.telefone.substring(6, 10)}`
            : dados.telefone;
        phoneNumberElement.textContent = telefoneFormatado;
        phoneNumberElement.closest('.contact-method').style.display = 'flex';
    } else if (phoneNumberElement) {
        phoneNumberElement.closest('.contact-method').style.display = 'none';
    }

    // Carregar linha do tempo de adoções
    carregarTimelineAdocoes(dados.id);

    // Preencher aba administrativa
    preencherAbaAdministrativa(dados);
}

function carregarTimelineAdocoes(usuarioId) {
    const timelineContainer = document.getElementById('adoptionsTimeline');
    if (!timelineContainer) return;

    // Mostrar loading
    timelineContainer.innerHTML = `
        <div class="timeline-loading">
            <i class="fa-solid fa-spinner fa-spin"></i>
            <p>Carregando histórico de adoções...</p>
        </div>
    `;

    fetch(`/admin/usuarios/historico-completo/${usuarioId}`)
        .then(response => response.json())
        .then(data => {
            if (data.success && data.historico && data.historico.length > 0) {
                renderizarTimelineAdocoes(data.historico);
            } else {
                timelineContainer.innerHTML = `
                    <div class="timeline-empty">
                        <i class="fa-solid fa-paw"></i>
                        <p>Nenhuma adoção encontrada para este usuário</p>
                    </div>
                `;
            }
        })
        .catch(error => {
            timelineContainer.innerHTML = `
                <div class="timeline-error">
                    <i class="fa-solid fa-exclamation-triangle"></i>
                    <p>Erro ao carregar histórico de adoções</p>
                </div>
            `;
        });
}

function renderizarTimelineAdocoes(adocoes) {
    const timelineContainer = document.getElementById('adoptionsTimeline');

    let timelineHtml = '';

    adocoes.forEach((adocao, index) => {
        const statusClass = obterClasseStatus(adocao.statusEnum);
        const iconeStatus = obterIconeStatus(adocao.statusEnum);
        const dataFormatada = new Date(adocao.dataEnvio).toLocaleDateString('pt-BR');

        timelineHtml += `
            <div class="timeline-item ${statusClass}">
                <div class="timeline-marker">
                    <i class="${iconeStatus}"></i>
                </div>
                <div class="timeline-content">
                    <div class="timeline-header">
                        <h6 class="timeline-title">
                            ${adocao.petNome} (${adocao.petEspecie})
                        </h6>
                        <span class="timeline-date">${dataFormatada}</span>
                    </div>
                    <div class="timeline-body">
                        <div class="timeline-status">
                            <span class="status-badge ${statusClass}">
                                <i class="${iconeStatus} me-1"></i>
                                ${adocao.status}
                            </span>
                        </div>
                        ${adocao.observacoes ? `
                            <div class="timeline-observations">
                                <small><strong>Observações:</strong> ${adocao.observacoes}</small>
                            </div>
                        ` : ''}
                        ${renderizarDetalhesAdocao(adocao)}
                    </div>
                </div>
            </div>
        `;
    });

    timelineContainer.innerHTML = timelineHtml;
}

function renderizarDetalhesAdocao(adocao) {
    let detalhes = `
        <div class="timeline-details">
            <small class="text-muted">
                <i class="fa-solid fa-calendar me-1"></i>
                Enviado em: ${new Date(adocao.dataEnvio).toLocaleDateString('pt-BR')}
            </small>
    `;

    if (adocao.dataResposta) {
        detalhes += `
            <small class="text-muted">
                <i class="fa-solid fa-reply me-1"></i>
                Respondido em: ${new Date(adocao.dataResposta).toLocaleDateString('pt-BR')}
            </small>
        `;
    }

    if (adocao.dataAssinatura) {
        detalhes += `
            <small class="text-muted">
                <i class="fa-solid fa-file-signature me-1"></i>
                Contrato assinado em: ${new Date(adocao.dataAssinatura).toLocaleDateString('pt-BR')}
            </small>
        `;
    }

    if (adocao.dataFinalizacao) {
        detalhes += `
            <small class="text-muted">
                <i class="fa-solid fa-check-circle me-1"></i>
                Finalizado em: ${new Date(adocao.dataFinalizacao).toLocaleDateString('pt-BR')}
            </small>
        `;
    }

    detalhes += '</div>';
    return detalhes;
}

function obterClasseStatus(statusEnum) {
    switch (statusEnum) {
        case 'Finalizado':
            return 'success';
        case 'AguardandoBuscar':
        case 'AguardandoAssinarContrato':
        case 'ContratoAssinado':
            return 'pending';
        case 'CanceladoPeloCaotinho':
        case 'CanceladoPeloUsuario':
        case 'CanceladoPorNaoAssinarContrato':
        case 'CanceladoPorPrazoVencido':
            return 'cancelled';
        default:
            return 'neutral';
    }
}

function obterIconeStatus(statusEnum) {
    switch (statusEnum) {
        case 'Finalizado':
            return 'fa-solid fa-heart';
        case 'AguardandoBuscar':
            return 'fa-solid fa-clock';
        case 'AguardandoAssinarContrato':
            return 'fa-solid fa-file-contract';
        case 'ContratoAssinado':
            return 'fa-solid fa-file-signature';
        case 'CanceladoPeloCaotinho':
        case 'CanceladoPeloUsuario':
        case 'CanceladoPorNaoAssinarContrato':
        case 'CanceladoPorPrazoVencido':
            return 'fa-solid fa-times-circle';
        default:
            return 'fa-solid fa-paw';
    }
}

function preencherAbaAdministrativa(dados) {
    const administracao = dados.administracao;
    if (!administracao) return;

    // Preencher status do usuário
    preencherStatusUsuario(dados, administracao);

    // Preencher status de quarentena
    preencherStatusQuarentena(administracao);

    // Preencher observações administrativas
    preencherObservacoesAdministrativas(administracao);

    // Preencher estatísticas administrativas
    preencherEstatisticasAdministrativas(dados);
}

function preencherStatusUsuario(dados, administracao) {
    const userStatusElement = document.getElementById('userStatus');
    const activeSinceElement = document.getElementById('activeSince');

    if (userStatusElement) {
        const statusDot = userStatusElement.querySelector('.status-dot');
        const statusText = userStatusElement.querySelector('span');
        const statusDetails = userStatusElement.nextElementSibling?.querySelector('p');

        if (dados.ativo) {
            statusDot.className = 'status-dot active';
            statusText.textContent = 'Ativo';
            if (statusDetails) statusDetails.textContent = 'Usuário habilitado para adoções';
        } else {
            statusDot.className = 'status-dot inactive';
            statusText.textContent = 'Inativo';
            if (statusDetails) statusDetails.textContent = 'Usuário desabilitado para adoções';
        }
    }

    if (activeSinceElement && dados.dataCadastro) {
        activeSinceElement.textContent = dados.dataCadastro;
    }
}

function preencherStatusQuarentena(administracao) {
    const quarantineStatusElement = document.getElementById('quarantineStatus');
    const lastQuarantineCheckElement = document.getElementById('lastQuarantineCheck');

    if (quarantineStatusElement) {
        const statusDots = quarantineStatusElement.querySelectorAll('.status-dot');
        const statusText = quarantineStatusElement.querySelector('span');
        const statusDetails = quarantineStatusElement.nextElementSibling?.querySelector('p');

        if (administracao.quarentenaAtiva) {
            // Mostrar dot de quarentena, esconder o ativo
            statusDots.forEach(dot => {
                if (dot.classList.contains('quarantine')) {
                    dot.style.display = 'block';
                } else {
                    dot.style.display = 'none';
                }
            });

            statusText.textContent = 'Em Quarentena';
            if (statusDetails) {
                const diasRestantes = administracao.diasRestantesQuarentena || 0;
                statusDetails.innerHTML = `
                    <strong>Motivo:</strong> ${administracao.motivoQuarentena || 'Não informado'}<br>
                    <strong>Restam:</strong> ${diasRestantes} dia(s)
                `;
            }
        } else {
            // Mostrar dot ativo, esconder o de quarentena
            statusDots.forEach(dot => {
                if (dot.classList.contains('active')) {
                    dot.style.display = 'block';
                } else {
                    dot.style.display = 'none';
                }
            });

            statusText.textContent = 'Sem Restrições';
            if (statusDetails) statusDetails.textContent = 'Nenhuma quarentena ativa';
        }
    }

    if (lastQuarantineCheckElement) {
        const ultimaVerificacao = administracao.fimQuarentena || 'Nunca';
        lastQuarantineCheckElement.textContent = ultimaVerificacao;
    }
}

function preencherObservacoesAdministrativas(administracao) {
    const adminNotesContentElement = document.getElementById('adminNotesContent');

    if (adminNotesContentElement) {
        if (administracao.observacoesAdministrativas && administracao.observacoesAdministrativas.trim()) {
            adminNotesContentElement.innerHTML = `
                <div class="admin-notes-text">
                    <div class="notes-content">
                        <p>${administracao.observacoesAdministrativas.replace(/\n/g, '<br>')}</p>
                    </div>
                    <div class="notes-metadata">
                        <small class="text-muted">
                            <i class="fa-solid fa-user-shield me-1"></i>
                            Última atualização: ${new Date().toLocaleDateString('pt-BR')}
                        </small>
                    </div>
                </div>
            `;
        } else {
            adminNotesContentElement.innerHTML = `
                <div class="notes-empty">
                    <i class="fa-solid fa-note-sticky"></i>
                    <p>Nenhuma observação administrativa registrada</p>
                    <small>Clique em "Editar" para adicionar observações</small>
                </div>
            `;
        }
    }
}

function preencherEstatisticasAdministrativas(dados) {
    // Preencher número de violações se existir elemento
    const violationsElement = document.getElementById('violations');
    if (violationsElement && dados.administracao) {
        violationsElement.textContent = dados.administracao.numeroViolacoes || 0;
    }

    // Adicionar informações administrativas extras se necessário
    adicionarInformacoesExtras(dados);
}

function adicionarInformacoesExtras(dados) {
    const administracao = dados.administracao;
    if (!administracao) return;

    // Procurar container de informações extras ou criar
    let extraInfoContainer = document.getElementById('adminExtraInfo');

    if (!extraInfoContainer) {
        // Criar container se não existir
        const adminActionsSection = document.querySelector('.admin-actions-section');
        if (adminActionsSection) {
            extraInfoContainer = document.createElement('div');
            extraInfoContainer.id = 'adminExtraInfo';
            extraInfoContainer.className = 'admin-extra-info-section';
            adminActionsSection.parentNode.insertBefore(extraInfoContainer, adminActionsSection);
        }
    }

    if (extraInfoContainer) {
        let extraInfoHtml = `
            <h6 class="admin-section-subtitle">
                <i class="fa-solid fa-info-circle me-2"></i>
                Informações Complementares
            </h6>
            <div class="admin-extra-grid">
        `;

        if (administracao.dataUltimaBloqueio) {
            extraInfoHtml += `
                <div class="extra-info-card">
                    <div class="info-icon warning">
                        <i class="fa-solid fa-ban"></i>
                    </div>
                    <div class="info-content">
                        <h6>Último Bloqueio</h6>
                        <p>${administracao.dataUltimaBloqueio}</p>
                    </div>
                </div>
            `;
        }

        if (administracao.numeroViolacoes > 0) {
            extraInfoHtml += `
                <div class="extra-info-card">
                    <div class="info-icon danger">
                        <i class="fa-solid fa-exclamation-triangle"></i>
                    </div>
                    <div class="info-content">
                        <h6>Violações Registradas</h6>
                        <p>${administracao.numeroViolacoes} violação(ões)</p>
                    </div>
                </div>
            `;
        }

        if (administracao.justificativaRemocaoQuarentena) {
            extraInfoHtml += `
                <div class="extra-info-card">
                    <div class="info-icon info">
                        <i class="fa-solid fa-file-text"></i>
                    </div>
                    <div class="info-content">
                        <h6>Justificativa de Remoção</h6>
                        <p>${administracao.justificativaRemocaoQuarentena}</p>
                    </div>
                </div>
            `;
        }

        extraInfoHtml += '</div>';
        extraInfoContainer.innerHTML = extraInfoHtml;
    }
}

function mostrarModalAplicarQuarentena(usuarioId) {
    document.getElementById('usuarioIdQuarentena').value = usuarioId;
    document.getElementById('diasDuracao').value = '30';
    document.getElementById('motivoQuarentena').value = '';
    
    const modal = new bootstrap.Modal(document.getElementById('modalAplicarQuarentena'));
    modal.show();
}

function confirmarAplicarQuarentena() {
    const usuarioId = document.getElementById('usuarioIdQuarentena').value;
    const diasDuracao = document.getElementById('diasDuracao').value;
    const motivo = document.getElementById('motivoQuarentena').value.trim();
    
    if (!diasDuracao) {
        console.warn('Por favor, selecione a duração da quarentena.');
        return;
    }
    
    if (!motivo) {
        console.warn('Por favor, informe o motivo da quarentena.');
        return;
    }
    
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    
    fetch('/admin/usuarios/aplicar-quarentena', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({
            UsuarioId: parseInt(usuarioId),
            DiasDuracao: parseInt(diasDuracao),
            Motivo: motivo
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.sucesso) {
            bootstrap.Modal.getInstance(document.getElementById('modalAplicarQuarentena')).hide();
            
            if (typeof toastr !== 'undefined') {
                toastr.success(data.mensagem);
            } else {
                console.log(data.mensagem);
            }
            
            setTimeout(() => {
                window.location.reload();
            }, 1500);
        } else {
            console.error(data.mensagem || 'Erro ao aplicar quarentena.');
        }
    })
    .catch(error => {
        console.error('Erro ao processar solicitação.');
    });
}

function mostrarModalRemoverQuarentena(usuarioId) {
    document.getElementById('usuarioIdRemoverQuarentena').value = usuarioId;
    document.getElementById('justificativaRemocao').value = '';
    
    const modal = new bootstrap.Modal(document.getElementById('modalRemoverQuarentena'));
    modal.show();
}

function confirmarRemoverQuarentena() {
    const usuarioId = document.getElementById('usuarioIdRemoverQuarentena').value;
    const justificativa = document.getElementById('justificativaRemocao').value.trim();
    
    if (!justificativa) {
        console.warn('Por favor, informe a justificativa para remoção da quarentena.');
        return;
    }
    
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    
    fetch('/admin/usuarios/remover-quarentena', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({
            UsuarioId: parseInt(usuarioId),
            Justificativa: justificativa
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.sucesso) {
            bootstrap.Modal.getInstance(document.getElementById('modalRemoverQuarentena')).hide();
            
            if (typeof toastr !== 'undefined') {
                toastr.success(data.mensagem);
            } else {
                console.log(data.mensagem);
            }
            
            setTimeout(() => {
                window.location.reload();
            }, 1500);
        } else {
            console.error(data.mensagem || 'Erro ao remover quarentena.');
        }
    })
    .catch(error => {
        console.error('Erro ao processar solicitação.');
    });
}

function alterarStatusUsuario(usuarioId, ativar) {
    const acao = ativar ? 'ativar' : 'desativar';
    const mensagem = `Tem certeza que deseja ${acao} este usuário?`;
    
    // Mostrar modal de confirmação
    mostrarModalConfirmacao(mensagem, () => {
        executarAlterarStatusUsuario(usuarioId, ativar);
    });
}

function executarAlterarStatusUsuario(usuarioId, ativar) {
    
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    
    fetch('/admin/usuarios/ativar-desativar', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({
            UsuarioId: parseInt(usuarioId),
            Ativar: ativar
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.sucesso) {
            if (typeof toastr !== 'undefined') {
                toastr.success(data.mensagem);
            } else {
                console.log(data.mensagem);
            }
            
            setTimeout(() => {
                window.location.reload();
            }, 1500);
        } else {
            console.error(data.mensagem || `Erro ao ${acao} usuário.`);
        }
    })
    .catch(error => {
        console.error('Erro ao processar solicitação.');
    });
}

function mostrarModalObservacoes(usuarioId) {
    document.getElementById('usuarioIdObservacoes').value = usuarioId;
    
    // Carregar observações existentes
    fetch(`/admin/usuarios/detalhes/${usuarioId}`)
        .then(response => {
            if (response.ok) {
                document.getElementById('observacoesTexto').value = '';
            }
        })
        .catch(error => {
        });
    
    const modal = new bootstrap.Modal(document.getElementById('modalObservacoes'));
    modal.show();
}

function salvarObservacoes() {
    const usuarioId = document.getElementById('usuarioIdObservacoes').value;
    const observacoes = document.getElementById('observacoesTexto').value.trim();
    
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    
    fetch('/admin/usuarios/observacoes', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({
            UsuarioId: parseInt(usuarioId),
            Observacoes: observacoes
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.sucesso) {
            bootstrap.Modal.getInstance(document.getElementById('modalObservacoes')).hide();
            
            if (typeof toastr !== 'undefined') {
                toastr.success(data.mensagem);
            } else {
                console.log(data.mensagem);
            }
        } else {
            console.error(data.mensagem || 'Erro ao salvar observações.');
        }
    })
    .catch(error => {
        console.error('Erro ao processar solicitação.');
    });
}

function mostrarModalConfirmacao(mensagem, callbackConfirmar) {
    document.getElementById('mensagemConfirmacao').textContent = mensagem;
    
    const modal = new bootstrap.Modal(document.getElementById('modalConfirmarAcao'));
    modal.show();
    
    // Remover event listeners anteriores
    const btnConfirmar = document.getElementById('btnConfirmarAcao');
    const newBtnConfirmar = btnConfirmar.cloneNode(true);
    btnConfirmar.parentNode.replaceChild(newBtnConfirmar, btnConfirmar);
    
    // Adicionar novo event listener
    newBtnConfirmar.addEventListener('click', function() {
        modal.hide();
        callbackConfirmar();
    });
    
    // Configurar estilo do botão
    newBtnConfirmar.className = 'btn btn-danger';
    newBtnConfirmar.innerHTML = '<i class="fas fa-check me-1"></i>Confirmar';
}

// Navegação entre abas do modal
function initializeTabNavigation() {
    const tabs = document.querySelectorAll('.profile-tab');
    const tabPanes = document.querySelectorAll('.profile-tab-pane');

    tabs.forEach(tab => {
        tab.addEventListener('click', function() {
            const targetTab = this.getAttribute('data-tab');

            // Remove active class from all tabs and panes
            tabs.forEach(t => t.classList.remove('active'));
            tabPanes.forEach(p => p.classList.remove('active'));

            // Add active class to clicked tab
            this.classList.add('active');

            // Show corresponding pane
            const targetPane = document.getElementById(targetTab);
            if (targetPane) {
                targetPane.classList.add('active');
            }
        });
    });
}

document.addEventListener('DOMContentLoaded', function() {
    const filtroStatus = document.querySelector('select[name="filtroStatus"]');
    const filtroTipo = document.querySelector('select[name="filtroTipo"]');

    if (filtroStatus) {
        filtroStatus.addEventListener('change', function() {
            this.form.submit();
        });
    }

    if (filtroTipo) {
        filtroTipo.addEventListener('change', function() {
            this.form.submit();
        });
    }

    // Inicializar navegação entre abas
    initializeTabNavigation();
    
    // Pesquisa com Enter
    const campoPesquisa = document.querySelector('input[name="pesquisa"]');
    if (campoPesquisa) {
        campoPesquisa.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                this.form.submit();
            }
        });
    }
    
    // Validação dos modais
    const formAplicarQuarentena = document.getElementById('formAplicarQuarentena');
    if (formAplicarQuarentena) {
        formAplicarQuarentena.addEventListener('submit', function(e) {
            e.preventDefault();
            confirmarAplicarQuarentena();
        });
    }
    
    const formRemoverQuarentena = document.getElementById('formRemoverQuarentena');
    if (formRemoverQuarentena) {
        formRemoverQuarentena.addEventListener('submit', function(e) {
            e.preventDefault();
            confirmarRemoverQuarentena();
        });
    }
    
    const formObservacoes = document.getElementById('formObservacoes');
    if (formObservacoes) {
        formObservacoes.addEventListener('submit', function(e) {
            e.preventDefault();
            salvarObservacoes();
        });
    }
});