if (typeof toastr !== 'undefined') {
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": true,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "preventDuplicates": false,
        "onclick": null,
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

let ultimaAtualizacaoNotificacoes = null;
let intervaloAtualizacao = null;
let contadorNotificacoes = 0;

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text ?? '';
    return div.innerHTML;
}

function inicializarComponenteNotificacoes() {
    
    const iconesNotificacao = document.querySelectorAll('.icone-notificacao');
    iconesNotificacao.forEach(icone => {
        icone.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            const menuLateral = document.querySelector('.menu-lateral');
            if (menuLateral && menuLateral.classList.contains('ativo')) {
                if (typeof fecharMenuLateral === 'function') {
                    fecharMenuLateral();
                }
            }
            
            togglePainelNotificacoes();
        });
    });
    
    const btnFechar = document.getElementById('fechar-notificacoes');
    if (btnFechar) {
        btnFechar.addEventListener('click', function(e) {
            e.preventDefault();
            ocultarPainelNotificacoes();
        });
    }
    
    const btnMarcarLidas = document.getElementById('marcar-todas-lidas');
    if (btnMarcarLidas) {
        btnMarcarLidas.addEventListener('click', function(e) {
            e.preventDefault();
            marcarTodasComoLidas();
        });
    }
    
    verificarNotificacoes();
    
    setInterval(verificarNotificacoes, 60000);
}

function togglePainelNotificacoes() {
    const painel = document.getElementById('painel-notificacoes');
    if (!painel) {
        console.error('Painel de notificações não encontrado!');
        return;
    }
    
    
    if (painel.classList.contains('ativo')) {
        ocultarPainelNotificacoes();
    } else {
        mostrarPainelNotificacoes();
    }
}

function mostrarPainelNotificacoes() {
    const painel = document.getElementById('painel-notificacoes');
    if (!painel) {
        console.error('Painel de notificações não encontrado!');
        return;
    }
    
    
    painel.style.display = 'block';
    painel.style.visibility = 'visible';
    painel.style.opacity = '1';
    painel.classList.add('ativo');
    
    const carregando = document.getElementById('carregando-notificacoes');
    const lista = document.getElementById('lista-notificacoes');
    const semNotificacoes = document.getElementById('sem-notificacoes');
    
    if (carregando) carregando.style.display = 'flex';
    if (lista) lista.style.display = 'none';
    if (semNotificacoes) semNotificacoes.style.display = 'none';
    
    setTimeout(carregarNotificacoes, 100);
    
    setTimeout(function() {
        document.addEventListener('click', fecharPainelAoClicarFora);
    }, 100);
}

function ocultarPainelNotificacoes() {
    const painel = document.getElementById('painel-notificacoes');
    if (!painel) return;
    
    
    painel.style.display = 'none';
    painel.style.visibility = 'hidden';
    painel.style.opacity = '0';
    painel.classList.remove('ativo');
    
    document.removeEventListener('click', fecharPainelAoClicarFora);
}

function fecharPainelAoClicarFora(e) {
    const painel = document.getElementById('painel-notificacoes');
    const icone = document.querySelector('.icone-notificacao');
    
    if (painel && !painel.contains(e.target) && (!icone || !icone.contains(e.target))) {
        ocultarPainelNotificacoes();
    }
}

function verificarNotificacoes() {
    const usuarioId = document.getElementById('usuarioId')?.value;
    
    if (!usuarioId || usuarioId === '0') {
        return;
    }
    
    
    fetch('/api/Notificacao/nao-lidas', {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        credentials: 'include'
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Falha ao obter notificações');
        }
        return response.json();
    })
    .then(quantidade => {
        atualizarContadorNotificacoes(quantidade);
    })
    .catch(error => {
        console.error('Erro ao verificar notificações:', error);
        ocultarContadorNotificacoes();
    });
}

function atualizarContadorNotificacoes(quantidade) {
    contadorNotificacoes = quantidade;
    
    const contadores = document.querySelectorAll('.contador-notificacoes');
    contadores.forEach(contador => {
        if (quantidade > 0) {
            contador.textContent = quantidade;
            contador.style.display = 'flex';
            
            const sinos = document.querySelectorAll('.sino-svg');
            sinos.forEach(sino => {
                sino.classList.add('animacao-sino');
            });
        } else {
            contador.style.display = 'none';
            
            const sinos = document.querySelectorAll('.sino-svg');
            sinos.forEach(sino => {
                sino.classList.remove('animacao-sino');
            });
        }
    });
}

function ocultarContadorNotificacoes() {
    const contadores = document.querySelectorAll('.contador-notificacoes');
    contadores.forEach(contador => {
        contador.style.display = 'none';
    });
    
    const sinos = document.querySelectorAll('.sino-svg');
    sinos.forEach(sino => {
        sino.classList.remove('animacao-sino');
    });
}

function carregarNotificacoes() {
    const carregando = document.getElementById('carregando-notificacoes');
    const lista = document.getElementById('lista-notificacoes');
    const semNotificacoes = document.getElementById('sem-notificacoes');
    
    if (!lista) return;
    
    fetch('/api/Notificacao', {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        credentials: 'include'
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Falha ao obter notificações');
        }
        return response.json();
    })
    .then(notificacoes => {
        if (carregando) carregando.style.display = 'none';
        
        if (notificacoes && notificacoes.length > 0) {
            lista.innerHTML = '';
            
            notificacoes.forEach(notificacao => {
                lista.appendChild(criarItemNotificacao(notificacao));
            });
            
            lista.style.display = 'block';
            if (semNotificacoes) semNotificacoes.style.display = 'none';
        } else {
            if (lista) lista.style.display = 'none';
            if (semNotificacoes) semNotificacoes.style.display = 'flex';
        }
    })
    .catch(error => {
        console.error('Erro ao carregar notificações:', error);
        if (carregando) carregando.style.display = 'none';
        if (lista) lista.style.display = 'none';
        if (semNotificacoes) semNotificacoes.style.display = 'flex';
        semNotificacoes.innerHTML = '<i class="fas fa-exclamation-triangle"></i><p>Erro ao carregar notificações</p>';
    });
}

function criarItemNotificacao(dados) {
    const data = new Date(dados.dataCriacao);
    const dataFormatada = formatarData(data);
    
    const div = document.createElement('div');
    div.className = `notificacao ${!dados.lida ? 'notificacao-nao-lida' : ''}`;
    div.setAttribute('data-id', dados.id);
    div.style.padding = "15px 20px";
    div.style.borderBottom = "1px solid rgba(0,0,0,0.05)";
    div.style.cursor = "pointer";
    div.style.transition = "all 0.2s ease";
    div.style.backgroundColor = "white";
    div.style.width = "100%";
    div.style.boxSizing = "border-box";
    div.style.position = "relative";
    
    div.innerHTML = `
        <h4 style="margin: 0; font-size: 1rem; color: #333; font-weight: 600; margin-bottom: 5px;">${escapeHtml(dados.titulo || 'Notificação')}</h4>
        <p style="margin: 0 0 5px 0; font-size: 0.9rem; color: #555; line-height: 1.5;">${escapeHtml(dados.mensagem || '')}</p>
        <small style="color: #999; font-size: 0.8rem;">Há ${dataFormatada}</small>
    `;
    
    div.addEventListener('click', function() {
        const id = this.getAttribute('data-id');
        if (id && !dados.lida) {
            marcarComoLida(id);
            this.classList.remove('notificacao-nao-lida');
        }
    });
    
    return div;
}

function formatarData(data) {
    const agora = new Date();
    const diff = agora - data;
    const segundos = Math.floor(diff / 1000);
    const minutos = Math.floor(segundos / 60);
    const horas = Math.floor(minutos / 60);
    const dias = Math.floor(horas / 24);
    
    if (segundos < 60) return 'agora';
    if (minutos < 60) return `${minutos} ${minutos === 1 ? 'minuto' : 'minutos'}`;
    if (horas < 24) return `${horas} ${horas === 1 ? 'hora' : 'horas'}`;
    if (dias < 7) return `${dias} ${dias === 1 ? 'dia' : 'dias'}`;
    
    return `${data.getDate().toString().padStart(2, '0')}/${(data.getMonth() + 1).toString().padStart(2, '0')}/${data.getFullYear()}`;
}

function marcarComoLida(id) {
    fetch(`/api/Notificacao/marcar-como-lida/${id}`, {
        method: 'POST',
        credentials: 'include'
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Falha ao marcar notificação como lida');
        }
        
        verificarNotificacoes();
    })
    .catch(error => {
        console.error('Erro ao marcar notificação como lida:', error);
    });
}

function marcarTodasComoLidas() {
    fetch('/api/Notificacao/marcar-todas-como-lidas', {
        method: 'POST',
        credentials: 'include'
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Falha ao marcar todas notificações como lidas');
        }
        
        const itensNotificacao = document.querySelectorAll('.item-notificacao');
        itensNotificacao.forEach(item => {
            item.classList.remove('notificacao-nao-lida');
        });
        
        ocultarContadorNotificacoes();
        verificarNotificacoes();
    })
    .catch(error => {
        console.error('Erro ao marcar todas notificações como lidas:', error);
        exibirMensagem('Erro ao marcar notificações como lidas', 'error');
    });
}

function exibirMensagem(mensagem, tipo) {
    if (typeof toastr !== 'undefined') {
        switch (tipo) {
            case 'success':
                toastr.success(mensagem);
                break;
            case 'error':
                toastr.error(mensagem);
                break;
            default:
                toastr.info(mensagem);
        }
    } else {
        alert(mensagem);
    }
}

function abrirModalNotificacoes() {
    mostrarPainelNotificacoes();
}

window.inicializarComponenteNotificacoes = inicializarComponenteNotificacoes;
window.togglePainelNotificacoes = togglePainelNotificacoes;
window.mostrarPainelNotificacoes = mostrarPainelNotificacoes;
window.ocultarPainelNotificacoes = ocultarPainelNotificacoes;
window.carregarNotificacoes = carregarNotificacoes;
window.marcarTodasComoLidas = marcarTodasComoLidas;

document.addEventListener('DOMContentLoaded', function() {
    inicializarComponenteNotificacoes();
}); 