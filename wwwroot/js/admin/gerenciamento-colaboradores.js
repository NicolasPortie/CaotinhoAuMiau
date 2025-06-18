$(document).ready(function() {
    
    configurarLimpezaErrosFormulario();
    configurarToastr();
    inicializarBootstrap();
    inicializarModalColaborador();
    
    
    if (typeof $.fn.mask === 'function') {
        $('.mascara-cpf').mask('000.000.000-00');
        $('.mascara-telefone').mask('(00) 00000-0000');
    }
    
    
    preencherDropdownCargos();
    
    
    $('#btnNovoColaborador').on('click', function() {
        abrirModalColaborador('criar');
    });
    
    
    $('#btnFiltrarColaboradores').on('click', filtrarColaboradores);
    $('#btnLimparFiltros').on('click', limparFiltros);
    
    
    $('#formularioColaborador').on('submit', function(e) {
        e.preventDefault();
        if (validarFormularioColaborador()) {
            enviarFormularioColaborador();
        }
    });
    
    
    $('#botaoConfirmarDesativar').on('click', async function() {
        const id = $('#idDesativar').val();
        const token = $('input[name="__RequestVerificationToken"]').val();
        try {
            const response = await fetch(`/Admin/Colaboradores/DesativarColaborador/${id}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({})
            });
            const data = await response.json();
            if (data.sucesso) {
                mostrarNotificacao(data.mensagem, 'success');
                $('#modalDesativar').modal('hide');
                setTimeout(() => window.location.reload(), 1000);
            } else {
                mostrarNotificacao(data.mensagem, 'error');
            }
        } catch (err) {
            console.error(err);
            mostrarNotificacao('Erro na comunicação com o servidor', 'error');
        }
    });
    $('#botaoConfirmarExcluir').on('click', async function() {
        const id = $('#idExcluir').val();
        const token = $('input[name="__RequestVerificationToken"]').val();
        try {
            const response = await fetch(`/Admin/Colaboradores/ExcluirColaborador/${id}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({})
            });
            const data = await response.json();
            if (data.sucesso) {
                mostrarNotificacao(data.mensagem, 'success');
                $('#modalExcluir').modal('hide');
                setTimeout(() => window.location.reload(), 1000);
            } else {
                mostrarNotificacao(data.mensagem, 'error');
            }
        } catch (err) {
            console.error(err);
            mostrarNotificacao('Erro na comunicação com o servidor', 'error');
        }
    });
    
    // Corrigir problema de paginação
    $('.pagination .page-link').on('click', function(e) {
        e.preventDefault();
        var url = $(this).attr('href');
        if (url) {
            window.location.href = url;
        }
    });
});






function configurarLimpezaErrosFormulario() {
    
    $('input, select, textarea').on('input change focus', function() {
        $(this).removeClass('is-invalid');
        const feedbackId = $(this).attr('id') + '-erro';
        $('#' + feedbackId).text('');
    });
    
    $(document).on('click', '.mensagem-erro', function() {
        $(this).text('');
        $(this).siblings('input, select, textarea').removeClass('is-invalid');
    });
}


function configurarToastr() {
    toastr.options = {
        "closeButton": true,
        "positionClass": "toast-top-right",
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };
}


function inicializarBootstrap() {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
}


function inicializarModalColaborador() {
    const $modal = $('#modalColaborador');
    
    
    $modal.on('show.bs.modal', function() {
        
        if (typeof $.fn.mask === 'function') {
            $(this).find('.mascara-cpf').mask('000.000.000-00');
            $(this).find('.mascara-telefone').mask('(00) 00000-0000');
        }
        
        
        $modal.find('.form-floating').each(function(index) {
            const $this = $(this);
            setTimeout(function() {
                $this.css({
                    'opacity': 1,
                    'transform': 'translateY(0)'
                });
            }, index * 50);
        });
    });
    
    
    $modal.on('hidden.bs.modal', function() {
        limparFormularioColaborador();
        
        
        $modal.find('.form-floating').css({
            'opacity': 0,
            'transform': 'translateY(10px)'
        });
    });
    
    
    $('.toggle-password').on('click', function() {
        alternarVisibilidadeSenha($(this).data('target'));
    });
    
    
    $('#colaboradorImagem').on('change', function(e) {
        const file = e.target.files[0];
        const $avatarContainer = $('.admin-avatar-container');
        
        if (file) {
            const reader = new FileReader();
            
            
            $avatarContainer.find('img, .success-overlay').remove();
            $avatarContainer.append('<div class="loading-overlay"><i class="bi bi-arrow-repeat spin"></i></div>');
            
            reader.onload = function(event) {
                setTimeout(() => {
                    
                    $avatarContainer.find('.loading-overlay').remove();
                    
                    
                    const img = $('<img>').attr('src', event.target.result);
                    $avatarContainer.append(img);
                    
                    
                    const $successOverlay = $('<div class="success-overlay"><i class="bi bi-check-lg"></i></div>');
                    $avatarContainer.append($successOverlay);
                    
                    setTimeout(() => $successOverlay.fadeOut(300, function() {
                        $(this).remove();
                    }), 1000);
                }, 500);
            };
            
            reader.readAsDataURL(file);
        }
    });
    
    
    $('#colaboradorAtivo').on('change', function() {
        const $label = $(this).siblings('.form-check-label');
        if ($(this).is(':checked')) {
            $label.text('Colaborador ativo').css('color', 'var(--success)');
        } else {
            $label.text('Colaborador inativo').css('color', '#6c757d');
        }
    });
}






function abrirModalColaborador(modo, id = null) {
    limparFormularioColaborador();
    const $modal = $('#modalColaborador');
    const $header = $modal.find('.modal-header');
    
    $header.css('background', '');
    
    switch (modo) {
        case 'criar':
            
            $header.css('background', getComputedStyle(document.documentElement).getPropertyValue('--primary-gradient'));
            $('#tituloModalColaborador').html('<i class="fas fa-user-plus me-2 text-white"></i> Novo Colaborador');
            $('#colaboradorId').val('0');
            
            
            $('#formularioColaborador').attr('action', '/Admin/Colaboradores/CadastrarColaborador');
            
            
            $('#containerSenhaAtual').hide();
            $('#containerSenha').show();
            $('#linhaConfirmarSenha').show();
            
            
            $('#colaboradorAtivo').prop('checked', true).trigger('change');
            
            
            habilitarCamposFormulario(true);
            
            
            $('.botao-modo-edicao').show();
            $('.botao-modo-visualizacao').hide();
            $('#botaoSalvarColaborador').html('<i class="fas fa-save me-2"></i> Cadastrar');
            break;
            
        case 'editar':
            
            $header.css('background', getComputedStyle(document.documentElement).getPropertyValue('--success-gradient'));
            if (!id) {
                console.error('ID do colaborador não fornecido para edição');
                return;
            }
            
            $('#tituloModalColaborador').html('<i class="fas fa-user-edit me-2 text-white"></i> Editar Colaborador');
            $('#colaboradorId').val(id);
            
            
            $('#formularioColaborador').attr('action', '/Admin/Colaboradores/AtualizarColaborador');
            
            
            $('#containerSenhaAtual').show();
            $('#containerSenha').show();
            $('#linhaConfirmarSenha').show();
            
            
            habilitarCamposFormulario(true);
            
            
            $('.botao-modo-edicao').show();
            $('.botao-modo-visualizacao').hide();
            $('#botaoSalvarColaborador').html('<i class="fas fa-save me-2"></i> Salvar Alterações');
            
            
            carregarDadosColaborador(id);
            break;
            
        case 'visualizar':
            
            $header.css('background', getComputedStyle(document.documentElement).getPropertyValue('--accent-gradient'));
            if (!id) {
                console.error('ID do colaborador não fornecido para visualização');
                return;
            }
            
            $('#tituloModalColaborador').html('<i class="fas fa-eye me-2 text-white"></i> Detalhes do Colaborador');
            $('#colaboradorId').val(id);
            
            
            $('#containerSenhaAtual').hide();
            $('#containerSenha').hide();
            $('#linhaConfirmarSenha').hide();
            
            
            habilitarCamposFormulario(false);
            
            
            $('.botao-modo-edicao').hide();
            $('.botao-modo-visualizacao').hide();
            
            
            carregarDadosColaborador(id);
            break;
            
        default:
            console.error('Modo inválido para o modal de colaborador');
            return;
    }
    
    
    $modal.modal('show');
}


function habilitarCamposFormulario(habilitar) {
    const campos = $('#formularioColaborador').find('input, select, textarea');
    
    campos.each(function() {
        $(this).prop('readonly', !habilitar);
        $(this).prop('disabled', !habilitar);
    });
    
    
    if (habilitar && $('#colaboradorId').val() !== '0') {
        $('#colaboradorEmail').prop('readonly', true); 
    }
}


async function carregarDadosColaborador(id) {
    try {
        
        const response = await fetch(`/Admin/Colaboradores/ObterColaborador/${id}`);
        
        if (!response.ok) {
            throw new Error(`Erro HTTP: ${response.status}`);
        }
        
        const data = await response.json();
        
        if (data.sucesso) {
            
            const colaborador = data.dados;
            
            $('#nomeColaborador').val(colaborador.nome);
            $('#emailColaborador').val(colaborador.email);
            $('#cpfColaborador').val(colaborador.cpf).trigger('input'); 
            $('#telefoneColaborador').val(colaborador.telefone).trigger('input'); 
            $('#cargoColaborador').val(colaborador.cargo);
            $('#colaboradorAtivo').prop('checked', colaborador.ativo).trigger('change');
            
            
            if (colaborador.imagemUrl) {
                const $avatarContainer = $('.admin-avatar-container');
                $avatarContainer.find('img').remove();
                $avatarContainer.append($('<img>').attr('src', colaborador.imagemUrl));
            }
        } else {
            console.error("Erro ao obter dados:", data.mensagem || "Erro desconhecido");
            mostrarNotificacao(`Erro ao carregar dados: ${data.mensagem || 'Erro desconhecido'}`, 'error');
        }
    } catch (error) {
        console.error("Erro ao carregar dados do colaborador:", error);
        mostrarNotificacao(`Erro ao carregar dados: ${error.message}`, 'error');
    }
}


function limparFormularioColaborador() {
    
    $('#formularioColaborador')[0].reset();
    
    
    $('#formularioColaborador').find('input, select, textarea').removeClass('is-valid is-invalid');
    $('.mensagem-erro').text('').hide();
    
    $('.admin-avatar-container').find('img, .success-overlay, .loading-overlay').remove();
}


function alternarVisibilidadeSenha(targetId) {
    const $input = $('#' + targetId);
    const $toggleBtn = $('button[data-target="' + targetId + '"]');
    
    
    const type = $input.attr('type') === 'password' ? 'text' : 'password';
    $input.attr('type', type);
    
    
    $toggleBtn.find('i').toggleClass('bi-eye bi-eye-slash');
    
    
    $toggleBtn.addClass('btn-clicked');
    setTimeout(() => $toggleBtn.removeClass('btn-clicked'), 200);
}






function validarFormularioColaborador() {
    let formValido = true;
    const camposObrigatorios = [
        {id: 'nomeColaborador', min: 3, mensagem: 'O nome deve ter pelo menos 3 caracteres'},
        {id: 'emailColaborador', regex: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, mensagem: 'Digite um email válido'},
        {id: 'cpfColaborador', mensagem: 'CPF inválido'},
        {id: 'telefoneColaborador', mensagem: 'Telefone inválido'},
        {id: 'cargoColaborador', mensagem: 'Selecione um cargo'}
    ];
    
    
    const isCriacao = $('#colaboradorId').val() === '0';
    
    
    if (isCriacao) {
        camposObrigatorios.push(
            {id: 'senhaColaborador', min: 6, mensagem: 'A senha deve ter pelo menos 6 caracteres'}
        );
    }
    
    
    camposObrigatorios.forEach(campo => {
        const $campo = $('#' + campo.id);
        if (!$campo.length) {
            console.warn(`Campo de validação '${campo.id}' não encontrado, pulando.`);
            return;
        }
        let valido = true;
        
        
        if (!$campo.is(':visible')) {
            return;
        }
        
        
        
        const raw = $campo.val();
        const valor = raw != null ? raw.toString() : '';
        if (campo.min && valor.trim().length < campo.min) {
            valido = false;
        }
        
        if (campo.regex && !campo.regex.test(valor.trim())) {
            valido = false;
        }
        
        if (campo.id === 'cpfColaborador') {
            const rawCpf = $campo.val() || '';
            const cpfDigitos = removerMascara(rawCpf);
            if (cpfDigitos.length !== 11) {
                valido = false;
            }
        }
        
        if (campo.id === 'telefoneColaborador' && $campo.val()) {
            const telefone = removerMascara($campo.val());
            valido = telefone.length >= 10;
        }
        
        
        if (campo.id === 'senhaColaborador' && isCriacao) {
            const senha = $campo.val();
            const confirmarSenha = $('#confirmarSenhaColaborador').val();
            
            if (senha !== confirmarSenha) {
                $('#confirmarSenhaColaborador').addClass('is-invalid');
                $('#confirmarSenhaColaborador-erro').text('As senhas não coincidem').show();
                formValido = false;
            } else {
                $('#confirmarSenhaColaborador').removeClass('is-invalid');
                $('#confirmarSenhaColaborador-erro').text('').hide();
            }
        }
        
        if (!valido) {
            $campo.addClass('is-invalid');
            $('#' + campo.id + '-erro').text(campo.mensagem).show();
            formValido = false;
        } else {
            $campo.removeClass('is-invalid');
            $('#' + campo.id + '-erro').text('').hide();
        }
    });
    
    return formValido;
}

function removerMascara(valor) {
    return valor.replace(/[^\d]/g, '');
}

async function enviarFormularioColaborador() {
    try {
        const $form = $('#formularioColaborador');
        
        const $cpf = $('#cpfColaborador');
        if ($cpf.length) {
            $cpf.val(removerMascara($cpf.val()));
        }
        const $telefone = $('#telefoneColaborador');
        if ($telefone.length) {
            $telefone.val(removerMascara($telefone.val()));
        }
        const formData = new FormData($form[0]);
        const url = $form.attr('action');
        const $btnSalvar = $('#botaoSalvarColaborador');
        
        
        const textoOriginal = $btnSalvar.html();
        $btnSalvar.html('<i class="bi bi-arrow-repeat spin me-2"></i>Salvando...').prop('disabled', true);
        
        
        const response = await fetch(url, {
            method: 'POST',
            body: formData
        });
        
        const data = await response.json();
        
        
        $btnSalvar.html(textoOriginal).prop('disabled', false);
                    
                    if (data.sucesso) {
            
            $('#modalColaborador').modal('hide');
            mostrarNotificacao(data.mensagem || 'Operação realizada com sucesso!', 'success');
            
            
                                setTimeout(() => {
                                    window.location.reload();
            }, 1000);
                    } else {
            mostrarNotificacao(data.mensagem || 'Ocorreu um erro ao processar a solicitação', 'error');
            
            
            if (data.erros) {
                Object.keys(data.erros).forEach(campo => {
                    const $campo = $('#' + campo);
                    const mensagem = data.erros[campo];
                    
                    $campo.addClass('is-invalid');
                    $('#' + campo + '-erro').text(mensagem).show();
                });
            }
        }
    } catch (error) {
        console.error("Erro ao enviar formulário:", error);
        mostrarNotificacao('Erro na comunicação com o servidor', 'error');
        $('#botaoSalvarColaborador').html('<i class="fas fa-save me-2"></i>Salvar').prop('disabled', false);
    }
}






function visualizarDetalhesColaborador(id) {
    abrirModalColaborador('visualizar', id);
}


function editarColaborador(id) {
    abrirModalColaborador('editar', id);
}


function confirmarDesativacaoColaborador(id) {
    Swal.fire({
        title: 'Confirmar desativação',
        text: 'Deseja realmente desativar este colaborador?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Sim, desativar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            alterarStatusColaborador(id, false);
        }
    });
}


let statusAction = '';

function abrirModalStatus(id, nome, acao) {
    const $modal = $('#modalStatus');
    
    $modal.attr('data-action', acao);
    statusAction = acao;
    $('#idStatus').val(id);
    $('#nomeColaboradorStatus').text(nome);

    const $header = $modal.find('.modal-header');
    const $titleIcon = $modal.find('.modal-title i');
    const $titleText = $modal.find('#tituloStatus');
    const $confirmBtn = $('#botaoConfirmarStatus');

    if (acao === 'ativar') {
        
        $header.css('background', getComputedStyle(document.documentElement).getPropertyValue('--success-gradient'));
        $titleIcon.attr('class', 'fas fa-user-check me-2');
        $titleText.text('Confirmar Ativação');
        $confirmBtn.attr('class', 'botao-primario').html('<i class="fas fa-user-check me-2"></i>Ativar');
    } else {
        
        $header.css('background', getComputedStyle(document.documentElement).getPropertyValue('--danger-gradient'));
        $titleIcon.attr('class', 'fas fa-user-slash me-2');
        $titleText.text('Confirmar Status');
        $confirmBtn.attr('class', 'botao-perigo').html('<i class="fas fa-user-slash me-2"></i>Inativar');
    }
    $modal.modal('show');
}


$(document).on('click', '#botaoConfirmarStatus', async function() {
    const id = $('#idStatus').val();
    const token = $('input[name="__RequestVerificationToken"]').val();
    const url = statusAction === 'ativar'
        ? `/Admin/Colaboradores/AtivarColaborador/${id}`
        : `/Admin/Colaboradores/DesativarColaborador/${id}`;
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            }
        });
        const data = await response.json();
        if (data.sucesso) {
            mostrarNotificacao(data.mensagem, 'success');
            $('#modalStatus').modal('hide');
            setTimeout(() => window.location.reload(), 1000);
        } else {
            mostrarNotificacao(data.mensagem, 'error');
        }
    } catch (err) {
        console.error(err);
        mostrarNotificacao('Erro na comunicação com o servidor', 'error');
    }
});






function filtrarColaboradores() {
    const filtroNome = $('#filtroNome').val().trim();
    const filtroCargo = $('#filtroCargo').val();
    const filtroStatus = $('#filtroStatus').val();
    
    
    let url = '/Admin/Colaboradores?';
    
    if (filtroNome) {
        url += `nome=${encodeURIComponent(filtroNome)}&`;
    }
    
    if (filtroCargo) {
        url += `cargo=${encodeURIComponent(filtroCargo)}&`;
    }
    
    if (filtroStatus) {
        url += `status=${encodeURIComponent(filtroStatus)}&`;
    }
    
    
    window.location.href = url.slice(0, -1); 
}


function limparFiltros() {
    $('#filtroNome').val('');
    $('#filtroCargo').val('');
    $('#filtroStatus').val('');
    
    window.location.href = '/Admin/Colaboradores';
}


function preencherDropdownCargos() {
    try {
        
        const $dropdownCargo = $('#colaboradorCargo');
        const $filtroDropdownCargo = $('#filtroCargo');
        
        if (!$dropdownCargo.length && !$filtroDropdownCargo.length) {
            return; 
        }
        
        
        fetch('/Admin/Colaboradores/ObterCargos')
            .then(response => {
        if (!response.ok) {
            throw new Error(`Erro HTTP: ${response.status}`);
        }
                return response.json();
            })
            .then(data => {
        if (data.sucesso) {
                    const cargos = data.dados;
                    
                    
                    if ($dropdownCargo.length) {
                        $dropdownCargo.empty();
                        $dropdownCargo.append('<option value="">Selecione um cargo</option>');
                        
                        cargos.forEach(cargo => {
                            $dropdownCargo.append(`<option value="${cargo.id}">${cargo.nome}</option>`);
                        });
                    }
                    
                    
                    if ($filtroDropdownCargo.length) {
                        const valorAtual = $filtroDropdownCargo.val();
                        
                        $filtroDropdownCargo.empty();
                        $filtroDropdownCargo.append('<option value="">Todos os cargos</option>');
                        
                        cargos.forEach(cargo => {
                            $filtroDropdownCargo.append(`<option value="${cargo.id}">${cargo.nome}</option>`);
                        });
                        
                        
                        if (valorAtual) {
                            $filtroDropdownCargo.val(valorAtual);
                        }
                    }
                } else {
                    console.error("Erro ao obter cargos:", data.mensagem || "Erro desconhecido");
                }
            })
            .catch(error => {
                console.error("Erro ao carregar cargos:", error);
            });
    } catch (error) {
        console.error("Erro ao preencher dropdown de cargos:", error);
    }
}






function mostrarNotificacao(mensagem, tipo = 'success') {
    
    const existingNotificacao = document.querySelector('.notificacao');
    if (existingNotificacao) {
        existingNotificacao.remove();
    }
    
    
    const notificacao = document.createElement('div');
    notificacao.className = `notificacao notificacao-${tipo}`;
    
    let icone = tipo === 'success' ? 'bi-check-circle-fill' : 'bi-exclamation-triangle';
    let titulo = tipo === 'success' ? 'Sucesso!' : 'Erro';
    
    
    notificacao.innerHTML = `
        <div class="notificacao-icone">
            <i class="bi ${icone}"></i>
        </div>
        <div class="notificacao-conteudo">
            <h5 class="mb-0 fw-bold">${titulo}</h5>
            <p>${mensagem}</p>
        </div>
        <button class="notificacao-fechar">
            <i class="bi bi-x"></i>
        </button>
    `;
    
    document.body.appendChild(notificacao);
    
    
    setTimeout(() => {
        notificacao.classList.add('notificacao-show');
    }, 10);
    
    
    const closeBtn = notificacao.querySelector('.notificacao-fechar');
    closeBtn.addEventListener('click', function() {
        notificacao.classList.remove('notificacao-show');
        setTimeout(() => {
            notificacao.remove();
        }, 300);
    });
    
    
    setTimeout(() => {
        if (notificacao.parentNode) {
            notificacao.classList.remove('notificacao-show');
            setTimeout(() => {
                if (notificacao.parentNode) {
                    notificacao.remove();
                }
            }, 300);
        }
    }, 5000);
}


function formatarData(dataStr) {
    if (!dataStr) return '';
    
        const data = new Date(dataStr);
    
    if (isNaN(data.getTime())) {
        return '';
    }
    
        return data.toLocaleDateString('pt-BR', {
            day: '2-digit',
            month: '2-digit',
        year: 'numeric'
        });
}


function abrirModalDesativar(id, nome) {
    $('#idDesativar').val(id);
    $('#nomeColaboradorDesativacao').text(nome);
    $('#modalDesativar').modal('show');
}

function abrirModalExcluir(id, nome) {
    $('#idExcluir').val(id);
    $('#nomeColaboradorExclusao').text(nome);
    $('#modalExcluir').modal('show');
}

