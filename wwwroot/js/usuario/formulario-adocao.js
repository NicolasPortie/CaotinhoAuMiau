toastr.options = {
    closeButton: true,
    debug: false,
    newestOnTop: true,
    progressBar: true,
    positionClass: "toast-top-right",
    preventDuplicates: false,
    onclick: null,
    showDuration: "300",
    hideDuration: "1000",
    timeOut: "5000",
    extendedTimeOut: "1000",
    showEasing: "swing",
    hideEasing: "linear",
    showMethod: "fadeIn",
    hideMethod: "fadeOut"
};
(function() {
    const menuHamburguer = document.getElementById('btnMenuHamburguer');
    if (menuHamburguer) {
        menuHamburguer.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            const menuLateral = document.querySelector('.menu-lateral');
            const menuSobreposicao = document.querySelector('.menu-sobreposicao');
            if (menuLateral && menuSobreposicao) {
                menuLateral.classList.toggle('ativo');
                menuSobreposicao.classList.toggle('ativo');
                this.classList.toggle('ativo');
            }
        });
    }
    const menuSobreposicao = document.querySelector('.menu-sobreposicao');
    if (menuSobreposicao) {
        menuSobreposicao.addEventListener('click', function() {
            const menuLateral = document.querySelector('.menu-lateral');
            const menuHamburguer = document.getElementById('btnMenuHamburguer');
            if (menuLateral) {
                menuLateral.classList.remove('ativo');
                this.classList.remove('ativo');
                if (menuHamburguer) menuHamburguer.classList.remove('ativo');
            }
        });
    }
    const notificacaoIcone = document.getElementById('notificacaoIcone');
    if (notificacaoIcone) {
        notificacaoIcone.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            const painelNotificacoes = document.getElementById('painel-notificacoes');
            if (painelNotificacoes) {
                painelNotificacoes.classList.toggle('ativo');
                painelNotificacoes.style.visibility = painelNotificacoes.classList.contains('ativo') ? 'visible' : 'hidden';
                painelNotificacoes.style.opacity = painelNotificacoes.classList.contains('ativo') ? '1' : '0';
                painelNotificacoes.style.display = painelNotificacoes.classList.contains('ativo') ? 'block' : 'none';
            }
            const menuLateral = document.querySelector('.menu-lateral');
            if (menuLateral && menuLateral.classList.contains('ativo')) {
                menuLateral.classList.remove('ativo');
                document.querySelector('.menu-sobreposicao')?.classList.remove('ativo');
                document.getElementById('btnMenuHamburguer')?.classList.remove('ativo');
            }
        });
    }
})();
const navbar = document.querySelector(".navbar");
const sidebarToggle = document.querySelector("#sidebarToggle");
const sidebar = document.querySelector(".sidebar");
const overlay = document.querySelector(".sidebar-overlay");
const notificationIcon = document.querySelector(".notification-icon");
const notificationPanel = document.querySelector(".notification-panel");
const notificationsCloseBtn = document.querySelector(".botao-fechar-notificacoes");
const markAsReadBtn = document.querySelector(".botao-marcar-lidas");
const mainContent = document.querySelector(".main-content");
const formSteps = document.querySelectorAll(".etapa-formulario");
const form = document.getElementById("formularioAdocao");
const requiredInputs = document.querySelectorAll("input[required], textarea[required], select[required]");
const submitButton = document.querySelector(".submit-btn");
const cancelButton = document.querySelector(".botao-voltar");
const maskMoneyOptions = {
    precision: 2,
    separator: ',',
    delimiter: '.',
    unit: 'R$ '
};
function alternarMenuLateral() {
    if (sidebar && overlay) {
        sidebar.classList.toggle("active");
        overlay.classList.toggle("active");
        document.body.classList.toggle("sidebar-open");
    }
}
function inicializarCampos() {
    const rendaMensal = document.getElementById('rendaMensal');
    if (rendaMensal) {
        if (window.VMasker) {
            VMasker(rendaMensal).maskMoney(maskMoneyOptions);
            rendaMensal.dispatchEvent(new Event('input'));
        }
    }
    animarFormulario();
    requiredInputs.forEach(input => {
        input.addEventListener('focus', function() {
            const grupoFormulario = this.closest('.grupo-formulario');
            if (grupoFormulario) {
                grupoFormulario.classList.add('input-focused');
            }
        });
        input.addEventListener('blur', function() {
            const grupoFormulario = this.closest('.grupo-formulario');
            if (grupoFormulario) {
                grupoFormulario.classList.remove('input-focused');
            }
            if (this.value.trim() === '' || (this.classList.contains('moeda-mask') && this.value === 'R$ 0,00')) {
                this.classList.add('is-invalid');
                this.classList.remove('is-valid');
                if (grupoFormulario) {
                    grupoFormulario.classList.add('erro');
                }
            } else {
                this.classList.remove('is-invalid');
                this.classList.add('is-valid');
                if (grupoFormulario) {
                    grupoFormulario.classList.remove('erro');
                }
            }
        });
    });
    const gruposFormulario = document.querySelectorAll('.grupo-formulario');
    gruposFormulario.forEach(grupo => {
        grupo
        grupo
    });
}
function animarFormulario() {
    garantirVisibilidadeFormulario();
    formSteps.forEach((step, index) => {
        setTimeout(() => {
            step.style.opacity = "1";
            step.style.transform = "translateY(0)";
            step.classList.add('animado');
        }, 200 * (index + 1));
    });
}
function validarFormulario() {
    let formularioValido = true;
    requiredInputs.forEach(input => {
        if (input.classList.contains('moeda-mask')) {
                const valor = input.value.trim();
            if (valor === '' || valor === 'R$ ') {
                marcarErro(input, 'Informe um valor para a renda mensal');
                formularioValido = false;
            } else {
                limparErro(input);
            }
        } 
        else if (input.value.trim() === '') {
            marcarErro(input, 'Este campo Ã© obrigatÃ³rio');
            formularioValido = false;
        } else {
            limparErro(input);
        }
    });
    const qtdMoradores = document.getElementById('qtdMoradores');
    if (qtdMoradores && qtdMoradores.value === '') {
        marcarErro(qtdMoradores, 'Campo obrigatÃ³rio');
        formularioValido = false;
    }
    const termos = document.getElementById('termos');
    if (termos && !termos.checked) {
        marcarErro(termos, 'VocÃª deve aceitar os termos');
        formularioValido = false;
    }
    if (!formularioValido) {
    } else {
    }
    return formularioValido;
}
function marcarErro(input, mensagem) {
    input.classList.add('is-invalid');
    input.classList.remove('is-valid');
    if (input.type === 'checkbox' || input.type === 'radio') {
        const feedbackElement = input.closest('.form-check').querySelector('.invalid-feedback');
        if (feedbackElement) {
            feedbackElement.textContent = mensagem;
        }
    } else {
        const feedbackElement = input.nextElementSibling;
        if (feedbackElement && feedbackElement.classList.contains('invalid-feedback')) {
            feedbackElement.textContent = mensagem;
        }
    }
    const grupoFormulario = input.closest('.grupo-formulario');
    if (grupoFormulario) {
        grupoFormulario.classList.add('erro');
    }
}
function limparErro(input) {
    input.classList.remove('is-invalid');
    input.classList.add('is-valid');
    const grupoFormulario = input.closest('.grupo-formulario');
    if (grupoFormulario) {
        grupoFormulario.classList.remove('erro');
    }
}
function exibirModalConfirmacao() {
    const modalConfirmacao = document.getElementById('modalConfirmacao');
    if (modalConfirmacao) {
        if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
            const modal = new bootstrap.Modal(modalConfirmacao);
            modal.show();
        } else {
            modalConfirmacao.style.display = 'block';
            modalConfirmacao.classList.add('show');
            document.body.classList.add('modal-open');
            // Remover backdrop existente se houver
            const existingBackdrop = document.querySelector('.modal-backdrop');
            if (existingBackdrop) {
                existingBackdrop.remove();
            }
            const backdrop = document.createElement('div');
            backdrop.className = 'modal-backdrop fade show';
            document.body.appendChild(backdrop);
        }
        // Adicionar event listeners nos botÃµes do modal
        const btnEntendi = document.getElementById('btnEntendi');
        const btnMinhasAdocoes = document.getElementById('btnMinhasAdocoes');
        const btnExplorarPets = document.getElementById('btnExplorarPets');
        if (btnEntendi) {
            btnEntendi.addEventListener('click', function() {
                window.location.href = '/usuario/adocoes';
            });
        }
        if (btnMinhasAdocoes) {
            btnMinhasAdocoes.addEventListener('click', function() {
                window.location.href = '/usuario/adocoes';
            });
        }
        if (btnExplorarPets) {
            btnExplorarPets.addEventListener('click', function() {
                window.location.href = '/usuario/pets/explorar';
            });
        }
    }
}
async function enviarFormulario(event) {
    event.preventDefault();
    if (!form.checkValidity()) {
        return false;
    }
    const formData = new FormData(form);
    const petId = formData.get('PetId');
    if (!form.querySelector('#concordaTermos').checked) {
        toastr.error('VocÃª precisa concordar com os termos para enviar a solicitaÃ§Ã£o.');
        return false;
    }
    submitButton.disabled = true;
    submitButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Enviando...';
    const tempoDisponivel = document.getElementById('tempoDisponivel').value;
    formData.set('TempoDisponivel', tempoDisponivel);
    const checkboxes = ['temQuintal', 'temVaranda', 'temRedeProtecao', 'temAreaExterna', 'concordaTermos'];
    checkboxes.forEach(id => {
        const checkbox = document.getElementById(id);
        if (checkbox) {
            const fieldName = id.charAt(0).toUpperCase() + id.slice(1); 
            formData.set(fieldName, checkbox.checked);
        }
    });
    if (document.getElementById('tevePetSim') && document.getElementById('tevePetSim').checked) {
        formData.set('TevePet', 'sim');
    } else if (document.getElementById('tevePetNao') && document.getElementById('tevePetNao').checked) {
        formData.set('TevePet', 'nao');
    } else {
        formData.set('TevePet', '');
    }
    // Corrigir formato da renda mensal - remover R$ e converter vÃ­rgula para ponto
    const rendaMensal = formData.get('RendaMensal');
    if (rendaMensal && typeof rendaMensal === 'string') {
        const rendaLimpa = rendaMensal
            .replace('R$', '')
            .replace(/\s/g, '') // remover espaÃ§os
            .replace(/\./g, '') // remover pontos (separadores de milhar)
            .replace(',', '.'); // trocar vÃ­rgula por ponto decimal
        formData.set('RendaMensal', rendaLimpa);
    }
    try {
        const response = await fetch(`/usuario/formulario-adocao/${petId}`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': formData.get('__RequestVerificationToken'),
                'X-Requested-With': 'XMLHttpRequest',
                'Accept': 'application/json'
            },
            body: formData
        });
        if (!response.ok) {
            throw new Error(`CÃ³digo de status: ${response.status}`);
        }
        const data = await response.json();
        if (data.success) {
            toastr.success(data.message);
            // Aguardar um pouco antes de abrir o modal para garantir que o toastr apareÃ§a
            setTimeout(() => {
                exibirModalConfirmacao();
            }, 500);
        } else {
            toastr.error(data.message);
            if (data.errors && data.errors.length > 0) {
                data.errors.forEach(erro => toastr.warning(erro));
            }
            submitButton.disabled = false;
            submitButton.innerHTML = '<i class="fas fa-paw"></i> Enviar SolicitaÃ§Ã£o';
        }
    } catch (error) {
        toastr.error('Ocorreu um erro ao enviar o formulÃ¡rio. Por favor, tente novamente.');
        submitButton.disabled = false;
        submitButton.innerHTML = '<i class="fas fa-paw"></i> Enviar SolicitaÃ§Ã£o';
    }
    return false;
}
async function manipularEnvio(event) {
    if (event && event.preventDefault) {
        event.preventDefault();
    }
    if (event && event.stopPropagation) {
        event.stopPropagation();
    }
    const formularioValido = validarFormulario();
    if (!formularioValido) {
        const firstInvalidField = form.querySelector('.is-invalid');
        if (firstInvalidField) {
            firstInvalidField.scrollIntoView({ behavior: 'smooth', block: 'center' });
            firstInvalidField.focus();
        }
        toastr.error('Por favor, corrija os erros no formulÃ¡rio antes de enviar.');
        return false;
    }
    form.classList.add('was-validated');
    return await enviarFormulario(event);
}
function exibirModalManualmente() {
    var modal = document.getElementById('modalConfirmacao');
    var backdrop = document.querySelector('.modal-backdrop');
    if (backdrop) backdrop.remove();
    document.body.classList.remove('modal-open');
    modal.style.display = '';
    modal.classList.remove('show');
    document.body.classList.add('modal-open');
    document.body.style.overflow = 'hidden';
    modal.classList.add('show');
    modal.style.display = 'block';
    var novoBackdrop = document.createElement('div');
    novoBackdrop.className = 'modal-backdrop fade show';
    document.body.appendChild(novoBackdrop);
}
function garantirVisibilidadeFormulario() {
    var etapas = document.querySelectorAll('.etapa-formulario');
    etapas.forEach(function(etapa) {
        etapa.style.opacity = '1';
        etapa.style.transform = 'translateY(0)';
        etapa.classList.add('visivel');
    });
    document.body.classList.add('loaded');
}
document.addEventListener("DOMContentLoaded", function() {
    garantirVisibilidadeFormulario();
    if (!form) {
        return;
    }
    const menuHamburguer = document.getElementById('btnMenuHamburguer');
    if (menuHamburguer) {
        menuHamburguer.addEventListener('click', function() {
            if (typeof alternarMenu === 'function') {
                alternarMenu();
            } else {
                const menuLateral = document.querySelector('.menu-lateral');
                const menuSobreposicao = document.querySelector('.menu-sobreposicao');
                if (menuLateral && menuSobreposicao) {
                    menuLateral.classList.toggle('ativo');
                    menuSobreposicao.classList.toggle('ativo');
                    this.classList.toggle('ativo');
                }
            }
        });
    }
    const notificacaoIcone = document.getElementById('notificacaoIcone');
    if (notificacaoIcone) {
        notificacaoIcone.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            if (typeof togglePainelNotificacoes === 'function') {
                togglePainelNotificacoes();
            } else {
                const painelNotificacoes = document.getElementById('painel-notificacoes');
                if (painelNotificacoes) {
                    painelNotificacoes.classList.toggle('ativo');
                    painelNotificacoes.style.display = painelNotificacoes.classList.contains('ativo') ? 'block' : 'none';
                }
            }
        });
    }
    const menuSobreposicao = document.querySelector('.menu-sobreposicao');
    if (menuSobreposicao) {
        menuSobreposicao.addEventListener('click', function() {
            if (typeof fecharMenuLateral === 'function') {
                fecharMenuLateral();
            } else {
                const menuLateral = document.querySelector('.menu-lateral');
                const menuHamburguer = document.getElementById('btnMenuHamburguer');
                if (menuLateral) {
                    menuLateral.classList.remove('ativo');
                    this.classList.remove('ativo');
                    if (menuHamburguer) menuHamburguer.classList.remove('ativo');
                }
            }
        });
    }
    inicializarCampos();
    inicializarNavegacaoPainel();
    inicializarContadoresCaracteres();
    inicializarControlesNumericos();
    inicializarSliderTempo();
    inicializarCheckboxesERadios();
    if (submitButton) {
        submitButton.addEventListener("click", async function(event) {
            event.preventDefault();
            // ValidaÃ§Ã£o bÃ¡sica
            if (!form.checkValidity()) {
                form.reportValidity();
                return;
            }
            // Desabilitar botÃ£o durante envio
            submitButton.disabled = true;
            submitButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Enviando...';
            try {
                const formData = new FormData(form);
                const petId = formData.get('PetId');
                // Corrigir formato da renda mensal - remover R$ e converter vÃ­rgula para ponto
                const rendaMensal = formData.get('RendaMensal');
                if (rendaMensal && typeof rendaMensal === 'string') {
                    const rendaLimpa = rendaMensal
                        .replace('R$', '')
                        .replace(/\s/g, '') // remover espaÃ§os
                        .replace(/\./g, '') // remover pontos (separadores de milhar)
                        .replace(',', '.'); // trocar vÃ­rgula por ponto decimal
                    formData.set('RendaMensal', rendaLimpa);
                            }
                const response = await fetch(`/usuario/formulario-adocao/${petId}`, {
                    method: 'POST',
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest',
                        'Accept': 'application/json'
                    },
                    body: formData
                });
                // Verificar se Ã© JSON vÃ¡lido
                const responseText = await response.text();
                let data;
                try {
                    data = JSON.parse(responseText);
                } catch (parseError) {
                    throw new Error('Resposta nÃ£o Ã© JSON vÃ¡lido: ' + responseText);
                }
                if (data.success) {
                    // Sucesso - abrir modal
                    exibirModalConfirmacao();
                } else {
                    // Erro - mostrar mensagem
                    toastr.error(data.message || 'Erro desconhecido');
                    if (data.errors) {
                        data.errors.forEach(erro => toastr.warning(erro));
                    }
                }
            } catch (error) {
                toastr.error('Erro ao enviar formulÃ¡rio. Tente novamente.');
            } finally {
                // Reabilitar botÃ£o
                submitButton.disabled = false;
                submitButton.innerHTML = '<i class="fas fa-paw"></i> Enviar SolicitaÃ§Ã£o';
            }
        });
    }
    verificarParametrosUrl();
});
function inicializarNavegacaoPainel() {
    const paineis = document.querySelectorAll('.painel-formulario');
    const botaoProximo = document.querySelectorAll('.botao-proximo');
    const botaoAnterior = document.querySelectorAll('.botao-anterior');
    const indicadoresProgresso = document.querySelectorAll('.indicador-progresso');
    let painelAtual = 0;
    if (paineis.length > 0) {
        mostrarPainel(0);
        atualizarProgresso(0);
    }
    botaoProximo.forEach(botao => {
        botao.addEventListener('click', function(e) {
            e.preventDefault();
            if (validarPainelAtual()) {
                if (painelAtual === paineis.length - 1) {
                    form.submit();
                } else {
                    mostrarPainel(painelAtual + 1);
                    atualizarProgresso(painelAtual);
                }
            }
        });
    });
    botaoAnterior.forEach(botao => {
        botao.addEventListener('click', function(e) {
            e.preventDefault();
            if (painelAtual > 0) {
                mostrarPainel(painelAtual - 1);
                atualizarProgresso(painelAtual);
            }
        });
    });
    indicadoresProgresso.forEach((indicador, index) => {
        indicador.addEventListener('click', function() {
            if (index <= painelAtual + 1) {
                mostrarPainel(index);
                atualizarProgresso(index);
            }
        });
    });
    function mostrarPainel(indice) {
        paineis.forEach(painel => {
            painel.style.display = 'none';
        });
        if (paineis[indice]) {
            paineis[indice].style.display = 'block';
            paineis[indice].scrollIntoView({ behavior: 'smooth', block: 'start' });
            painelAtual = indice;
            document.querySelectorAll('.botao-anterior').forEach(btn => {
                btn.style.display = indice === 0 ? 'none' : 'inline-block';
            });
            document.querySelectorAll('.botao-proximo').forEach(btn => {
                btn.textContent = indice === paineis.length - 1 ? 'Enviar' : 'PrÃ³ximo';
            });
        }
    }
    function atualizarProgresso(etapaAtiva) {
        indicadoresProgresso.forEach((indicador, index) => {
            indicador.classList.remove('concluido', 'ativo', 'pendente');
            if (index < etapaAtiva) {
                indicador.classList.add('concluido'); 
            } else if (index === etapaAtiva) {
                indicador.classList.add('ativo'); 
            } else {
                indicador.classList.add('pendente'); 
            }
        });
    }
    function validarPainelAtual() {
        const camposObrigatorios = paineis[painelAtual].querySelectorAll('[required]');
        let valido = true;
        camposObrigatorios.forEach(campo => {
            if (!campo.value.trim()) {
                campo.classList.add('is-invalid');
                valido = false;
            } else {
                campo.classList.remove('is-invalid');
            }
        });
        if (!valido) {
            toastr.error('Por favor, preencha todos os campos obrigatÃ³rios.');
        }
        return valido;
    }
}
function inicializarContadoresCaracteres() {
    document.querySelectorAll('textarea[data-max-length]').forEach(textarea => {
        const maxLength = parseInt(textarea.dataset.maxLength);
        const contador = document.createElement('div');
        contador.className = 'contador-caracteres';
        contador.innerHTML = `<span>0</span>/${maxLength}`;
        textarea.parentNode.insertBefore(contador, textarea.nextSibling);
        textarea.addEventListener('input', function() {
            const caracteresUsados = this.value.length;
            contador.querySelector('span').textContent = caracteresUsados;
            if (caracteresUsados > maxLength) {
                contador.classList.add('excedido');
                this.classList.add('is-invalid');
            } else {
                contador.classList.remove('excedido');
                this.classList.remove('is-invalid');
            }
        });
    });
}
function inicializarControlesNumericos() {
    document.querySelectorAll('input[type="number"]').forEach(input => {
        const container = document.createElement('div');
        container.className = 'controle-numero';
        const btnMenos = document.createElement('button');
        btnMenos.type = 'button';
        btnMenos.className = 'btn-numero btn-menos';
        btnMenos.innerHTML = '<i class="fas fa-minus"></i>';
        const btnMais = document.createElement('button');
        btnMais.type = 'button';
        btnMais.className = 'btn-numero btn-mais';
        btnMais.innerHTML = '<i class="fas fa-plus"></i>';
        input.parentNode.insertBefore(container, input);
        container.appendChild(btnMenos);
        container.appendChild(input);
        container.appendChild(btnMais);
        btnMenos.addEventListener('click', () => {
            const valorAtual = parseInt(input.value) || 0;
            const novoValor = valorAtual - 1;
            if (novoValor >= 0) {
                input.value = novoValor;
                input.dispatchEvent(new Event('change'));
            }
        });
        btnMais.addEventListener('click', () => {
            const valorAtual = parseInt(input.value) || 0;
            const novoValor = valorAtual + 1;
            input.value = novoValor;
            input.dispatchEvent(new Event('change'));
        });
    });
}
function inicializarSliderTempo() {
    const slider = document.getElementById('tempoDisponivel');
    if (!slider) return;
    const output = document.getElementById('valorTempoDisponivel');
    if (output) {
        output.textContent = slider.value + ' horas';
        slider.addEventListener('input', function() {
            output.textContent = this.value + ' horas';
        });
    }
}
function inicializarCheckboxesERadios() {
    document.querySelectorAll('.checkbox-personalizado input[type="checkbox"]').forEach(checkbox => {
        const label = checkbox.parentElement;
        checkbox.addEventListener('change', function() {
            if (this.checked) {
                label.classList.add('checked');
            } else {
                label.classList.remove('checked');
            }
        });
        if (checkbox.checked) {
            label.classList.add('checked');
        }
    });
    document.querySelectorAll('.radio-personalizado input[type="radio"]').forEach(radio => {
        const label = radio.parentElement;
        radio.addEventListener('change', function() {
            document.querySelectorAll(`input[name="${this.name}"]`).forEach(r => {
                r.parentElement.classList.remove('checked');
            });
            if (this.checked) {
                label.classList.add('checked');
            }
        });
        if (radio.checked) {
            label.classList.add('checked');
        }
    });
}
function verificarParametrosUrl() {
    const urlParams = new URLSearchParams(window.location.search);
    const success = urlParams.get('success');
    const error = urlParams.get('error');
    if (success === 'true') {
        toastr.success('FormulÃ¡rio enviado com sucesso! Acompanhe o status da sua adoÃ§Ã£o na Ã¡rea do usuÃ¡rio.');
    } else if (error) {
        toastr.error(decodeURIComponent(error));
    }
}
if (sidebarToggle) {
    sidebarToggle.addEventListener("click", alternarMenuLateral);
}
if (overlay && window.fecharMenuLateral) {
    overlay.addEventListener("click", window.fecharMenuLateral);
}
if (notificationIcon) {
    notificationIcon.addEventListener("click", togglePainelNotificacoes);
}
if (notificationsCloseBtn) {
    notificationsCloseBtn.addEventListener("click", ocultarPainelNotificacoes);
}
if (markAsReadBtn) {
    markAsReadBtn.addEventListener("click", function(e) {
        e.preventDefault();
    });
} 
