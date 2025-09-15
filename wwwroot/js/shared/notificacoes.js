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
    const lista = document.getElementById('lista-notificacoes');
    if (lista) {
        lista.addEventListener('click', function(e) {
            const notificacao = e.target.closest('.notificacao');
            if (notificacao && notificacao.dataset.id &&
                notificacao.classList.contains('notificacao-nao-lida')) {
                marcarComoLida(notificacao.dataset.id);
                notificacao.classList.remove('notificacao-nao-lida');
            }
        });
    }
    
    const btnFechar = document.getElementById('fechar-notificacoes');
    if (btnFechar) {
        btnFechar.addEventListener('click', function(e) {
            e.preventDefault();
            ocultarPainelNotificacoes();
        });
    }
    
    const btnMarcarLidas = document.getElementById('marcar-todas-lidas');
    if (btnMarcarLidas) {
        // Remover listeners anteriores para evitar duplicação
        btnMarcarLidas.removeEventListener('click', marcarTodasComoLidas);

        btnMarcarLidas.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();

            // Prevenir múltiplos cliques
            if (btnMarcarLidas.disabled) return;

            btnMarcarLidas.disabled = true;
            btnMarcarLidas.textContent = 'Marcando...';

            marcarTodasComoLidas().finally(() => {
                btnMarcarLidas.disabled = false;
                btnMarcarLidas.innerHTML = '<i class="fas fa-check"></i> Marcar todas como lidas';
            });
        });
    }
    
    verificarNotificacoes();
    
    setInterval(verificarNotificacoes, 60000);
}

function togglePainelNotificacoes() {
    const painel = document.getElementById('painel-notificacoes');
    if (!painel) {
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
        return;
    }
    
    painel.style.display = 'block';
    painel.offsetHeight;
    
    painel.style.visibility = 'visible';
    painel.style.opacity = '1';
    painel.style.transform = 'translateY(0) scale(1)';
    painel.classList.add('ativo');
    
    const carregando = document.getElementById('carregando-notificacoes');
    const lista = document.getElementById('lista-notificacoes');
    const semNotificacoes = document.getElementById('sem-notificacoes');
    
    if (carregando) {
        carregando.style.display = 'flex';
        carregando.style.animation = 'fadeInUp 0.3s ease';
    }
    if (lista) lista.style.display = 'none';
    if (semNotificacoes) semNotificacoes.style.display = 'none';
    
    setTimeout(carregarNotificacoes, 150);
    
    setTimeout(function() {
        document.addEventListener('click', fecharPainelAoClicarFora);
    }, 100);
    
}

function ocultarPainelNotificacoes() {
    const painel = document.getElementById('painel-notificacoes');
    if (!painel) return;
    
    painel.style.opacity = '0';
    painel.style.transform = 'translateY(-10px) scale(0.95)';
    painel.classList.remove('ativo');
    
    setTimeout(() => {
        painel.style.display = 'none';
        painel.style.visibility = 'hidden';
    }, 300);
    
    document.removeEventListener('click', fecharPainelAoClicarFora);
    
}

function fecharPainelAoClicarFora(e) {
    const painel = document.getElementById('painel-notificacoes');
    const icone = document.querySelector('.icone-notificacao');
    
    if (painel && !painel.contains(e.target) && (!icone || !icone.contains(e.target))) {
        ocultarPainelNotificacoes();
    }
}

async function verificarNotificacoes() {
    const usuarioId = document.getElementById('usuarioId')?.value;

    if (!usuarioId || usuarioId === '0') {
        return;
    }

    try {
        const response = await fetch('/api/Notificacao/nao-lidas', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error('Falha ao obter notificações');
        }

        const quantidade = await response.json();
        atualizarContadorNotificacoes(quantidade);
    } catch (error) {
        ocultarContadorNotificacoes();
    }
}

function atualizarContadorNotificacoes(quantidade) {
    contadorNotificacoes = quantidade;
    
    const contadores = document.querySelectorAll('.contador-notificacoes');
    const iconesNotificacao = document.querySelectorAll('.icone-notificacao');
    
    contadores.forEach(contador => {
        if (quantidade > 0) {
            const textoContador = quantidade > 99 ? '99+' : quantidade.toString();
            contador.textContent = textoContador;
            contador.style.display = 'flex';
            
            contador.style.animation = 'none';
            contador.offsetHeight;
            contador.style.animation = 'pulse-contador 2s infinite';
        } else {
            contador.style.display = 'none';
        }
    });
    
    iconesNotificacao.forEach(icone => {
        if (quantidade > 0) {
            icone.classList.add('tem-notificacoes');
            setTimeout(() => {
                icone.classList.remove('tem-notificacoes');
            }, 800);
        }
    });
    
    if (quantidade > 0) {
        document.title = `(${quantidade}) ${document.title.replace(/^\(\d+\) /, '')}`;
    } else {
        document.title = document.title.replace(/^\(\d+\) /, '');
    }
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

async function carregarNotificacoes() {
    const carregando = document.getElementById('carregando-notificacoes');
    const lista = document.getElementById('lista-notificacoes');
    const semNotificacoes = document.getElementById('sem-notificacoes');

    if (!lista) return;

    try {
        const response = await fetch('/api/Notificacao', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error('Falha ao obter notificações');
        }

        const notificacoes = await response.json();
        if (carregando) carregando.style.display = 'none';

        if (notificacoes && notificacoes.length > 0) {
            lista.innerHTML = '';

            notificacoes.forEach(notificacao => {
                lista.appendChild(criarItemNotificacao(notificacao));
            });

            lista.style.display = 'block';
            if (semNotificacoes) semNotificacoes.style.display = 'none';
        } else {
            lista.style.display = 'none';
            if (semNotificacoes) semNotificacoes.style.display = 'flex';
        }
    } catch (error) {
        if (carregando) carregando.style.display = 'none';
        if (lista) lista.style.display = 'none';
        if (semNotificacoes) semNotificacoes.style.display = 'flex';
        semNotificacoes.innerHTML = '<i class="fas fa-exclamation-triangle"></i><p>Erro ao carregar notificações</p>';
    }
}

function criarItemNotificacao(dados) {
    const data = new Date(dados.dataCriacao);
    const dataFormatada = formatarData(data);
    
    const div = document.createElement('div');
    div.className = `notificacao ${!dados.lida ? 'notificacao-nao-lida' : ''}`;
    div.setAttribute('data-id', dados.id);
    div.setAttribute('data-tipo', dados.tipo || 'geral');
    
    const iconeNotificacao = obterIconeNotificacao(dados.tipo);
    
    div.innerHTML = `
        <div class="notificacao-titulo">
            <i class="${iconeNotificacao}"></i>
            ${escapeHtml(dados.titulo || 'Notificação')}
        </div>
        <div class="notificacao-mensagem">${escapeHtml(dados.mensagem || '')}</div>
        <div class="notificacao-data">
            <i class="fas fa-clock"></i>
            Há ${dataFormatada}
        </div>
    `;
    
    div.addEventListener('click', function(e) {
        e.preventDefault();
        
        if (!dados.lida) {
            div.style.transform = 'scale(0.98)';
            setTimeout(() => {
                div.style.transform = 'scale(1)';
                div.classList.remove('notificacao-nao-lida');
            }, 150);
            
            marcarComoLida(dados.id);
        }
        
        if (dados.acao && dados.url) {
            setTimeout(() => {
                window.location.href = dados.url;
            }, 200);
        }
    });
    
    div.style.opacity = '0';
    div.style.transform = 'translateX(-20px)';
    setTimeout(() => {
        div.style.transition = 'all 0.3s ease';
        div.style.opacity = '1';
        div.style.transform = 'translateX(0)';
    }, 50);
    
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

async function marcarComoLida(id) {
    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        const response = await fetch(`/api/Notificacao/marcar-como-lida/${id}`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token
            },
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error('Falha ao marcar notificação como lida');
        }

        verificarNotificacoes();
    } catch (error) {
    }
}

async function marcarTodasComoLidas() {
    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value ||
                     document.querySelector('meta[name="csrf-token"]')?.getAttribute('content');
        
        const headers = {
            'Content-Type': 'application/json'
        };
        
        if (token) {
            headers['RequestVerificationToken'] = token;
        }
        
        const response = await fetch('/api/Notificacao/marcar-todas-como-lidas', {
            method: 'POST',
            headers: headers,
            credentials: 'include'
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Falha ao marcar todas notificações como lidas: ${response.status}`);
        }
        
        const notificacaoNaoLidas = document.querySelectorAll('.notificacao.notificacao-nao-lida');

        notificacaoNaoLidas.forEach(item => {
            item.classList.remove('notificacao-nao-lida');
        });

        ocultarContadorNotificacoes();

        await verificarNotificacoes();

        // Mostrar apenas uma mensagem de sucesso
        if (typeof toastr !== 'undefined') {
            toastr.success('Todas as notificações foram marcadas como lidas');
        } else {
            console.log('Todas as notificações foram marcadas como lidas');
        }
        
    } catch (error) {
        exibirMensagem('Erro ao marcar notificações como lidas', 'error');
    }
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
        console.log(mensagem);
    }
}

function abrirModalNotificacoes() {
    mostrarPainelNotificacoes();
}

function obterIconeNotificacao(tipo) {
    const icones = {
        'adocao': 'fas fa-heart',
        'pet': 'fas fa-paw',
        'sistema': 'fas fa-cog',
        'usuario': 'fas fa-user',
        'alerta': 'fas fa-exclamation-triangle',
        'sucesso': 'fas fa-check-circle',
        'info': 'fas fa-info-circle',
        'geral': 'fas fa-bell'
    };
    return icones[tipo] || icones['geral'];
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