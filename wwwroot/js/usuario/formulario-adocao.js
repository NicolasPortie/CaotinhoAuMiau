
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
            } else {
                console.error("Elementos do menu lateral não encontrados");
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
            } else {
                console.error("Painel de notificações não encontrado");
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
const submitButton = document.querySelector(".botao-enviar");
const cancelButton = document.querySelector(".botao-voltar");


document.addEventListener('DOMContentLoaded', function() {
    
    
    const menuHamburguer = document.getElementById('btnMenuHamburguer');
    if (menuHamburguer) {
        menuHamburguer.addEventListener('click', function() {
            if (typeof alternarMenu === 'function') {
                alternarMenu();
            } else {
                console.error("Função alternarMenu não está disponível");
                
                const menuLateral = document.querySelector('.menu-lateral');
                const menuSobreposicao = document.querySelector('.menu-sobreposicao');
                if (menuLateral && menuSobreposicao) {
                    menuLateral.classList.toggle('ativo');
                    menuSobreposicao.classList.toggle('ativo');
                    this.classList.toggle('ativo');
                }
            }
        });
    } else {
        console.warn("Botão de menu hamburger não encontrado");
    }
    
    
    const notificacaoIcone = document.getElementById('notificacaoIcone');
    if (notificacaoIcone) {
        notificacaoIcone.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            if (typeof togglePainelNotificacoes === 'function') {
                togglePainelNotificacoes();
            } else {
                console.error("Função togglePainelNotificacoes não está disponível");
                
                const painelNotificacoes = document.getElementById('painel-notificacoes');
                if (painelNotificacoes) {
                    painelNotificacoes.classList.toggle('ativo');
                    painelNotificacoes.style.display = painelNotificacoes.classList.contains('ativo') ? 'block' : 'none';
                }
            }
        });
    } else {
        console.warn("Ícone de notificação não encontrado");
    }
    
    
    const menuSobreposicao = document.querySelector('.menu-sobreposicao');
    if (menuSobreposicao) {
        menuSobreposicao.addEventListener('click', function() {
            if (typeof fecharMenuLateral === 'function') {
                fecharMenuLateral();
            } else {
                console.error("Função fecharMenuLateral não está disponível");
                
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
    
    
    verificarParametrosUrl();
    
    
    if (form) {
        form.addEventListener('submit', manipularEnvio);
    }
});






function alternarMenuLateral() {
    sidebar.classList.toggle("active");
    overlay.classList.toggle("active");
    document.body.classList.toggle("sidebar-open");
}


function abrirMenuLateral() {
    sidebar.classList.add("active");
    overlay.classList.add("active");
    document.body.classList.add("sidebar-open");
}


function fecharMenuLateral() {
    sidebar.classList.remove("active");
    overlay.classList.remove("active");
    document.body.classList.remove("sidebar-open");
}


function abrirPainelNotificacoes(event) {
    event.preventDefault();
    event.stopPropagation();
    
    
    const iconRect = notificationIcon.getBoundingClientRect();
    notificationPanel.style.top = `${iconRect.bottom + window.scrollY + 10}px`;
    notificationPanel.style.right = `${window.innerWidth - iconRect.right - 10}px`;
    
    notificationPanel.classList.toggle("show");
    
    
    if (sidebar.classList.contains("active")) {
        fecharMenuLateral();
    }
}


function fecharPainelNotificacoes() {
    notificationPanel.classList.remove("show");
}






function inicializarCampos() {
    
    
    const rendaMensal = document.getElementById('rendaMensal');
    if (rendaMensal) {
        $(rendaMensal).maskMoney({
            prefix: 'R$ ',
            thousands: '.',
            decimal: ',',
            allowZero: false,
            allowNegative: false
        });
        
        
        $(rendaMensal).trigger('mask.maskMoney');
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
        grupo.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-2px)';
            this.style.backgroundColor = 'rgba(255, 107, 0, 0.03)';
        });
        
        grupo.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
            this.style.backgroundColor = '';
        });
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
    let isValid = true;
    
    
    requiredInputs.forEach(input => {
        
        if (input.classList.contains('moeda-mask')) {
            const valor = input.value.trim();
            if (valor === '' || valor === 'R$ 0,00' || valor === 'R$ 0,0' || valor === 'R$ 0' || valor === 'R$ ') {
                marcarErro(input, 'Informe um valor válido para a renda mensal');
                isValid = false;
            } else {
                
                const valorNumerico = parseFloat(valor.replace('R$ ', '').replace(/\./g, '').replace(',', '.'));
                if (isNaN(valorNumerico) || valorNumerico <= 0) {
                    marcarErro(input, 'O valor da renda mensal deve ser maior que zero');
                    isValid = false;
                } else {
                    
                    limparErro(input);
                }
            }
        } 
        
        else if (input.value.trim() === '') {
            marcarErro(input, 'Este campo é obrigatório');
            isValid = false;
        } else {
            limparErro(input);
        }
    });
    
    
    const qtdMoradores = document.getElementById('qtdMoradores');
    if (qtdMoradores && (qtdMoradores.value === '' || parseInt(qtdMoradores.value) <= 0)) {
        marcarErro(qtdMoradores, 'Deve ser maior que zero');
        isValid = false;
    }
    
    
    const termos = document.getElementById('termos');
    if (termos && !termos.checked) {
        marcarErro(termos, 'Você deve aceitar os termos');
        isValid = false;
    }
    
    if (!isValid) {
    } else {
    }
    
    return isValid;
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
            
            
            const backdrop = document.createElement('div');
            backdrop.className = 'modal-backdrop fade show';
            document.body.appendChild(backdrop);
        }
    } else {
        console.error("Modal de confirmação não encontrado");
    }
}






function enviarFormulario(event) {
    event.preventDefault();
    
    
    if (!form.checkValidity()) {
        return false;
    }
    
    
    const formData = new FormData(form);
    const petId = formData.get('PetId');
    
    
    if (!form.querySelector('#concordaTermos').checked) {
        toastr.error('Você precisa concordar com os termos para enviar a solicitação.');
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
    
    if (formData.has('RendaMensal')) {
        let rendaMensal = formData.get('RendaMensal');
        
        rendaMensal = rendaMensal.replace('R$ ', '').replace(/\./g, '').replace(',', '.');
        const rendaNumero = parseFloat(rendaMensal);
        if (!isNaN(rendaNumero)) {
            formData.set('RendaMensal', rendaNumero.toString());
        }
    }
    
    
    $.ajax({
        url: `/usuario/adocao/formulario/${petId}`,
        type: 'POST',
        data: new URLSearchParams(formData),
        processData: false,
        contentType: 'application/x-www-form-urlencoded',
        headers: {
            'RequestVerificationToken': formData.get('__RequestVerificationToken')
        },
        success: function(response) {
            
            if (response.success) {
                
                toastr.success(response.message);
                
                
                exibirModalConfirmacao();
            } else {
                
                toastr.error(response.message);
                
                
                if (response.errors && response.errors.length > 0) {
                    response.errors.forEach(function(erro) {
                        toastr.warning(erro);
                    });
                }
                
                
                submitButton.disabled = false;
                submitButton.innerHTML = '<i class="fas fa-paw"></i> Enviar Solicitação';
            }
        },
        error: function(xhr, status, error) {
            console.error("Erro na requisição:", error);
            console.error("Status:", status);
            console.error("Resposta:", xhr.responseText);
            
            toastr.error("Ocorreu um erro ao enviar o formulário. Por favor, tente novamente.");
            
            
            submitButton.disabled = false;
            submitButton.innerHTML = '<i class="fas fa-paw"></i> Enviar Solicitação';
        }
    });
    
    return false;
}


function manipularEnvio(event) {
    
    
    if (event && event.preventDefault) {
        event.preventDefault();
    }
    
    if (event && event.stopPropagation) {
        event.stopPropagation();
    }
    
    
    const isValid = validarFormulario();
    if (!isValid) {
        
        
        const firstInvalidField = form.querySelector('.is-invalid');
        if (firstInvalidField) {
            firstInvalidField.scrollIntoView({ behavior: 'smooth', block: 'center' });
            firstInvalidField.focus();
        }
        
        toastr.error('Por favor, corrija os erros no formulário antes de enviar.');
        return false;
    }
    
    form.classList.add('was-validated');
    
    
    return enviarFormulario(event);
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
        console.warn("Formulário não encontrado. Esta página pode não conter um formulário de adoção.");
        return;
    }
    
    
    
    inicializarCampos();
    
    
    inicializarNavegacaoPainel();
    
    
    inicializarContadoresCaracteres();
    
    
    inicializarControlesNumericos();
    
    
    inicializarSliderTempo();
    
    
    inicializarCheckboxesERadios();
    
    
    if (form) {
        form.addEventListener("submit", function(event) {
            return manipularEnvio(event);
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
                btn.textContent = indice === paineis.length - 1 ? 'Enviar' : 'Próximo';
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
            toastr.error('Por favor, preencha todos os campos obrigatórios.');
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
            const novoValor = parseInt(input.value) - 1;
            if (novoValor >= parseInt(input.min || 0)) {
                input.value = novoValor;
                input.dispatchEvent(new Event('change'));
            }
        });
        
        btnMais.addEventListener('click', () => {
            const novoValor = parseInt(input.value) + 1;
            if (!input.max || novoValor <= parseInt(input.max)) {
                input.value = novoValor;
                input.dispatchEvent(new Event('change'));
            }
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
        toastr.success('Formulário enviado com sucesso! Acompanhe o status da sua adoção na área do usuário.');
    } else if (error) {
        toastr.error(decodeURIComponent(error));
    }
}





if (sidebarToggle) {
    sidebarToggle.addEventListener("click", alternarMenuLateral);
}


if (overlay) {
    overlay.addEventListener("click", fecharMenuLateral);
}


if (notificationIcon) {
    notificationIcon.addEventListener("click", abrirPainelNotificacoes);
}


if (notificationsCloseBtn) {
    notificationsCloseBtn.addEventListener("click", fecharPainelNotificacoes);
}


if (markAsReadBtn) {
    markAsReadBtn.addEventListener("click", function(e) {
        e.preventDefault();
    });
} 