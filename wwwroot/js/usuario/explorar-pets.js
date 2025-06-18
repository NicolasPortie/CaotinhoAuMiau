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



let slideAtual = 0;
let intervaloCarrossel = null;


let usuarioEstaLogado = false;
let usuarioIdAtual = '';
let usuarioNomeAtual = '';
let usuarioTipoAtual = '';

document.addEventListener('DOMContentLoaded', function() {
    
    
    inicializarItensPorPagina();
    
    
    
    
    function garantirCarregamentoDosCards() {
        
        
        const cards = document.querySelectorAll('.pet-card');
        const grid = document.querySelector('.pets-grid');
        
        if (cards.length === 0 && grid) {
            
            
            const formFiltros = document.getElementById('formFiltros');
            
            if (formFiltros) {
                try {
                    formFiltros.submit();
                } catch (e) {
                    console.error("Erro ao enviar formulário:", e);
                    
                    window.location.reload();
                }
            } else {
                
                window.location.href = "/usuario/pets/explorar";
            }
        } else {
            
            
            cards.forEach(card => {
                card.style.display = 'block';
                card.style.opacity = '1';
                card.style.visibility = 'visible';
            });
            
            if (grid) {
                grid.style.display = 'grid'; 
            }
        }
    }
    
    
    const referrer = document.referrer;
    if (referrer && (referrer.includes('/usuario/adocao/formulario/') || referrer.includes('/adocao/formulario/'))) {
        
        setTimeout(garantirCarregamentoDosCards, 500);
    }
    
    
    if (window.temMensagensTempData) {
        if (window.mensagemErro) {
            exibirAlerta('erro', window.mensagemErro);
        }
        
        if (window.mensagemSucesso) {
            exibirAlerta('sucesso', window.mensagemSucesso);
        }
    }
    
    
    const dadosUsuario = document.getElementById('dados-usuario');
    if (dadosUsuario) {
        usuarioEstaLogado = dadosUsuario.dataset.usuarioLogado === 'true';
        usuarioIdAtual = dadosUsuario.dataset.usuarioId || '';
        usuarioNomeAtual = dadosUsuario.dataset.usuarioNome || '';
        usuarioTipoAtual = dadosUsuario.dataset.usuarioTipo || '';
        
            logado: usuarioEstaLogado,
            id: usuarioIdAtual,
            nome: usuarioNomeAtual,
            tipo: usuarioTipoAtual
        });
    }
    
    const carrosselElement = document.getElementById('carouselPets');
    if (carrosselElement) {
        try {
            
            
            if (typeof bootstrap !== 'undefined' && bootstrap.Carousel) {
                var carouselInstance = new bootstrap.Carousel(carrosselElement, {
                    interval: 5000,
                    pause: false,
                    wrap: true
                });
                
                
                
                gerenciarBarrasDeProgresso();
                
                
                carrosselElement.addEventListener('slid.bs.carousel', function() {
                    gerenciarBarrasDeProgresso();
                });
            } else {
                
                iniciarCarrosselAutomatico();
                
                
                const slides = carrosselElement.querySelectorAll('.carousel-item');
                slides.forEach((slide, index) => {
                    slide.style.display = index === 0 ? 'block' : 'none';
                });
                
                
                mudarSlide(0);
            }
        } catch (error) {
            console.error("Erro ao inicializar carrossel:", error);
            
            iniciarCarrosselAutomatico();
        }
    }
    
    
    document.addEventListener('click', fecharSobreposicoesAoClicarFora);
    
    
    document.addEventListener('keydown', fecharModaisComEsc);
    
    
    verificarMensagensURL();
    
    
    filtrarPets();
    
    
    configurarMenuENotificacoes();
    
    
    inicializarFiltros();
    inicializarBotoesAdocao();
    inicializarAnimacoesCards();
    
    
    verificarLayoutEAjustar();

    
    document.querySelectorAll('.botao-adotar, .adopt-button').forEach(botao => {
        botao.addEventListener('click', function(e) {
            if (!usuarioEstaLogado) {
                e.preventDefault();
                const petCard = this.closest('.cartao-pet, .pet-card');
                if (petCard) {
                    const petId = petCard.dataset.id || '';
                    abrirModal('modalLoginCadastro');
                    document.getElementById('petIdAutenticacao').value = petId;
                }
            }
        });
    });

    
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});

function configurarMenuENotificacoes() {
    
    
    const btnMenuHamburguer = document.getElementById('btnMenuHamburguer');
    if (btnMenuHamburguer) {
        
        const novoBotao = btnMenuHamburguer.cloneNode(true);
        btnMenuHamburguer.parentNode.replaceChild(novoBotao, btnMenuHamburguer);
        
        
        novoBotao.addEventListener('click', function(e) {
            e.preventDefault();
            
            
            if (typeof window.alternarMenu === 'function') {
                window.alternarMenu();
            } else {
                
                const menuLateral = document.querySelector('.menu-lateral');
                const menuSobreposicao = document.querySelector('.menu-sobreposicao');
                
                if (menuLateral && menuSobreposicao) {
                    const estaAtivo = menuLateral.classList.contains('ativo');
                    
                    if (estaAtivo) {
                        menuLateral.classList.remove('ativo');
                        menuSobreposicao.classList.remove('ativo');
                        this.classList.remove('ativo');
                    } else {
                        menuLateral.classList.add('ativo');
                        menuSobreposicao.classList.add('ativo');
                        this.classList.add('ativo');
                    }
                    
                }
            }
        });
    }
    
    
    const menuSobreposicao = document.querySelector('.menu-sobreposicao');
    if (menuSobreposicao) {
        menuSobreposicao.addEventListener('click', function() {
            if (typeof window.fecharMenuLateral === 'function') {
                window.fecharMenuLateral();
            } else {
                
                const menuLateral = document.querySelector('.menu-lateral');
                const btnHamburguer = document.getElementById('btnMenuHamburguer');
                
                if (menuLateral) menuLateral.classList.remove('ativo');
                if (btnHamburguer) btnHamburguer.classList.remove('ativo');
                this.classList.remove('ativo');
            }
        });
    }
    
    
    const iconeNotificacao = document.getElementById('notificacaoIcone');
    if (iconeNotificacao) {
        
        const novoIcone = iconeNotificacao.cloneNode(true);
        iconeNotificacao.parentNode.replaceChild(novoIcone, iconeNotificacao);
        
        
        novoIcone.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            
            if (typeof window.togglePainelNotificacoes === 'function') {
                window.togglePainelNotificacoes();
            } else {
            }
        });
    }
}

function inicializarFiltros() {
    
    
    document.querySelector('.botao-limpar')?.addEventListener('click', function(e) {
        e.preventDefault();
        limparFiltros();
    });
    
    
    document.getElementById('filtroNome')?.addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            document.getElementById('formFiltros')?.submit();
        }
    });
}

function inicializarBotoesAdocao() {
    
}





function mudarPagina(numeroPagina) {
    window.location.href = `/usuario/pets/explorar?pagina=${numeroPagina}`;
}


function definirFiltroAtivo(btn) {
    
    document.querySelectorAll('.filtro-btn').forEach(b => {
        b.classList.remove('ativo');
    });
    
    
    btn.classList.add('ativo');
    
    
    filtrarPets();
}

function filtrarPets() {
    
    const form = document.getElementById('formFiltros');
    if (!form) {
        console.error("Formulário de filtros não encontrado");
        return;
    }
    
    
    document.querySelector('.botao-filtrar')?.addEventListener('click', function(e) {
        e.preventDefault();
        form.submit();
    });
    
    
    const todosPets = document.querySelectorAll('.cartao-pet');
    const mensagemSemPets = document.querySelector('.mensagem-sem-pets');
    
    if (todosPets.length === 0 && mensagemSemPets) {
        mensagemSemPets.style.display = 'flex';
    } else if (mensagemSemPets) {
        mensagemSemPets.style.display = 'none';
    }
}


function abrirModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'flex';
        document.body.classList.add('modal-aberto');
    }
}

function fecharModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'none';
        document.body.classList.remove('modal-aberto');
    }
}

function gerarNotificacoesExemplo() {
    const notificacoes = [
        {
            icone: 'fa-heart',
            titulo: 'Adoção em processo',
            mensagem: 'Sua solicitação de adoção foi recebida e está em análise.',
            data: '2 horas atrás',
            lida: false
        },
        {
            icone: 'fa-paw',
            titulo: 'Novo pet disponível',
            mensagem: 'Um novo pet foi adicionado que corresponde às suas preferências!',
            data: 'Ontem',
            lida: true
        },
        {
            icone: 'fa-envelope',
            titulo: 'Mensagem recebida',
            mensagem: 'Você recebeu uma mensagem sobre um de seus processos de adoção.',
            data: '3 dias atrás',
            lida: true
        }
    ];
    
    let html = '';
    
    notificacoes.forEach(n => {
        html += `
            <div class="notificacao-item ${!n.lida ? 'notificacao-nao-lida' : ''}">
                <div class="notificacao-icone">
                    <i class="fas ${n.icone}"></i>
                </div>
                <div class="notificacao-conteudo">
                    <div class="notificacao-titulo">${n.titulo}</div>
                    <div class="notificacao-mensagem">${n.mensagem}</div>
                    <div class="notificacao-data">${n.data}</div>
                </div>
            </div>
        `;
    });
    
    return html;
}

function marcarTodasComoLidas() {
    
    const notificacaoItems = document.querySelectorAll('.notificacao-item.notificacao-nao-lida');
    notificacaoItems.forEach(item => {
        item.classList.remove('notificacao-nao-lida');
    });
    
    
    const contador = document.querySelector('.contador-notificacoes');
    if (contador) contador.style.display = 'none';
    
    
    exibirMensagem('Todas as notificações foram marcadas como lidas', 'success');
}

function abrirModalResponsabilidade(petId) {
    if (!usuarioEstaLogado) {
        abrirModal('modalLoginCadastro');
        document.getElementById('petIdAutenticacao').value = petId || '';
        return;
    }
    
    if (usuarioTipoAtual === 'Admin') {
        abrirModal('modalAdminPrincipal');
        return;
    }
    
    
    fetch('/usuario/adocao/verificar-adocoes-pendentes')
        .then(response => response.json())
        .then(data => {
            if (data.temAdocoesPendentes) {
                
                abrirModal('modalAdocaoPendente');
                return;
            }
            
            
            document.getElementById('petIdResponsabilidade').value = petId || '';
            abrirModal('modalResponsabilidade');
        })
        .catch(error => {
            console.error('Erro ao verificar adoções:', error);
            toastr.error('Ocorreu um erro ao verificar suas adoções. Por favor, tente novamente.');
        });
}

function realizarLogout() {
    window.location.href = '/autenticacao/logout';
}

function fecharSobreposicoesAoClicarFora(event) {
    
    const painelNotificacoes = document.getElementById('painel-notificacoes');
    if (painelNotificacoes && painelNotificacoes.classList.contains('ativo')) {
        const iconeNotificacao = document.querySelector('.icone-notificacao');
        if (iconeNotificacao && !iconeNotificacao.contains(event.target) && !painelNotificacoes.contains(event.target)) {
            painelNotificacoes.classList.remove('ativo');
        }
    }
}

function fecharModaisComEsc(event) {
    if (event.key === 'Escape') {
        
        const modais = document.querySelectorAll('.modal');
        modais.forEach(modal => {
            if (modal.style.display === 'flex') {
                const modalId = modal.id;
                fecharModal(modalId);
            }
        });
    }
}

function verificarMensagensURL() {
    const params = new URLSearchParams(window.location.search);
    const mensagem = params.get('mensagem');
    const tipo = params.get('tipo') || 'info';
    
    if (mensagem) {
        exibirMensagem(mensagem, tipo);
        
        
        const url = new URL(window.location.href);
        url.searchParams.delete('mensagem');
        url.searchParams.delete('tipo');
        window.history.replaceState({}, document.title, url.toString());
    }
}

function exibirMensagem(mensagem, tipo) {
    if (typeof toastr !== 'undefined') {
        if (tipo === 'success') {
            toastr.success(mensagem);
        } else if (tipo === 'error') {
            toastr.error(mensagem);
        } else {
            toastr.info(mensagem);
        }
    } else {
        alert(mensagem);
    }
}

function obterIconeParaTipo(tipo) {
    switch (tipo?.toLowerCase()) {
        case 'adocao': return 'fa-heart';
        case 'formulario': return 'fa-file-alt';
        case 'sistema': return 'fa-cog';
        case 'alerta': return 'fa-exclamation-circle';
        default: return 'fa-bell';
    }
}

function formatarData(dataString) {
    if (!dataString) return 'Sem data';
    
    const data = new Date(dataString);
    const hoje = new Date();
    const ontem = new Date(hoje);
    ontem.setDate(hoje.getDate() - 1);
    
    
    const opcoes = { hour: '2-digit', minute: '2-digit' };
    
    
    if (data.toDateString() === hoje.toDateString()) {
        return `Hoje ${data.toLocaleTimeString([], opcoes)}`;
    }
    
    
    if (data.toDateString() === ontem.toDateString()) {
        return `Ontem ${data.toLocaleTimeString([], opcoes)}`;
    }
    
    
    return data.toLocaleDateString() + ' ' + data.toLocaleTimeString([], opcoes);
}

function limparFiltros() {
    
    
    window.location.href = '/usuario/pets/explorar';
}

function abrirModalNotificacoes() {
    const iconeNotificacao = document.querySelector('.icone-notificacao');
    
    if (!iconeNotificacao) {
        console.error("Ícone de notificação não encontrado");
        return;
    }
    
    const painelNotificacoes = document.getElementById('painel-notificacoes');
    
    if (!painelNotificacoes) {
        console.error("Painel de notificações não encontrado");
        return;
    }
    
    
    const isAtivo = painelNotificacoes.classList.toggle('ativo');
    
    if (isAtivo) {
        
        const rect = iconeNotificacao.getBoundingClientRect();
        painelNotificacoes.style.top = rect.bottom + 10 + 'px';
        painelNotificacoes.style.right = window.innerWidth - rect.right + 'px';
        
        
        const listaNotificacoes = painelNotificacoes.querySelector('.lista-notificacoes');
        if (listaNotificacoes && listaNotificacoes.children.length === 0) {
            gerarNotificacoesExemplo();
        }
        
        
        document.addEventListener('click', function fecharAoClicarFora(e) {
            if (!painelNotificacoes.contains(e.target) && !iconeNotificacao.contains(e.target)) {
                painelNotificacoes.classList.remove('ativo');
                document.removeEventListener('click', fecharAoClicarFora);
            }
        });
    }
}


function prosseguirAdocao() {
    
    const petId = document.getElementById('petIdResponsabilidade').value;
    
    if (!petId) {
        console.error('ID do pet não encontrado');
        if (typeof toastr !== 'undefined') {
            toastr.error('Houve um erro ao iniciar o processo de adoção. Tente novamente.');
        } else {
            alert('Houve um erro ao iniciar o processo de adoção. Tente novamente.');
        }
        return;
    }
    
    try {
        
        const url = `/usuario/adocao/formulario/${petId}`;
        
        
        if (typeof fecharModal === 'function') {
            fecharModal('modalResponsabilidade');
        }
        
        
        setTimeout(function() {
            
            window.location.href = url;
        }, 100);
    } catch (error) {
        console.error('Erro ao prosseguir com adoção:', error);
        if (typeof toastr !== 'undefined') {
            toastr.error('Ocorreu um erro ao iniciar o processo de adoção. Tente novamente.');
        } else {
            alert('Ocorreu um erro ao iniciar o processo de adoção. Tente novamente.');
        }
    }
}


function redirecionarLogin() {
    const petId = document.getElementById('petIdAutenticacao').value;
    window.location.href = `/autenticacao/login?returnUrl=/usuario/pets/explorar&petId=${petId}`;
}


function redirecionarCadastro() {
    const petId = document.getElementById('petIdAutenticacao').value;
    window.location.href = `/autenticacao/cadastro?returnUrl=/usuario/pets/explorar&petId=${petId}`;
}

function exibirAlerta(tipo, mensagem) {
    if (!mensagem) return;
    
    const classeAlerta = tipo === 'erro' ? 'alert-danger' : 'alert-success';
    const icone = tipo === 'erro' ? '<i class="fas fa-exclamation-circle"></i>' : '<i class="fas fa-check-circle"></i>';
    
    const alerta = `
        <div class="alert ${classeAlerta} alert-dismissible fade show d-flex align-items-center" role="alert">
            <div class="me-3 fs-4">${icone}</div>
            <div>${mensagem}</div>
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Fechar"></button>
        </div>
    `;
    
    const alertaContainer = document.getElementById('alertaContainer');
    if (alertaContainer) {
        alertaContainer.innerHTML = alerta;
        
        
        setTimeout(() => {
            const alertas = document.querySelectorAll('.alert');
            alertas.forEach(alerta => {
                if (typeof bootstrap !== 'undefined') {
                    const bsAlert = new bootstrap.Alert(alerta);
                    bsAlert.close();
                } else {
                    alerta.remove();
                }
            });
        }, 5000);
    }
}

function verificarAdocao(petId) {
    
    
    const usuarioId = document.getElementById('usuarioId')?.value || '';
    
    if (!usuarioId || usuarioId === "") {
        localStorage.setItem('petIdParaAdocao', petId);
        window.location.href = '/usuario/login?returnUrl=/usuario/adocao/formulario-adocao/' + petId;
        return false;
    }
    
    
    window.location.href = '/usuario/adocao/formulario-adocao/' + petId;
    return false;
}

function adicionarBarrasDeProgresso() {
    const carousel = document.getElementById('carouselPets');
    if (!carousel) return;
    
    const slides = carousel.querySelectorAll('.carousel-item');
    
    
    carousel.querySelectorAll('.carousel-progress').forEach(bar => bar.remove());
    
    slides.forEach(slide => {
        const progressBar = document.createElement('div');
        progressBar.className = 'carousel-progress';
        slide.appendChild(progressBar);
    });
    
    
    const activeSlide = carousel.querySelector('.carousel-item.active');
    if (activeSlide) {
        const progressBar = activeSlide.querySelector('.carousel-progress');
        if (progressBar) {
            
            setTimeout(() => {
                progressBar.style.width = '100%';
            }, 100);
        }
    }
    
    
    carousel.addEventListener('slide.bs.carousel', function(e) {
        
        const progressBars = carousel.querySelectorAll('.carousel-progress');
        progressBars.forEach(bar => {
            bar.style.transition = 'none';
            bar.style.width = '0%';
        });
    });
    
    carousel.addEventListener('slid.bs.carousel', function(e) {
        
        const activeSlide = carousel.querySelector('.carousel-item.active');
        if (activeSlide) {
            const progressBar = activeSlide.querySelector('.carousel-progress');
            if (progressBar) {
                
                void progressBar.offsetWidth;
                
                
                progressBar.style.transition = 'width 5s linear';
                progressBar.style.width = '100%';
            }
        }
    });
    
    
    carousel.addEventListener('mouseenter', function() {
        
        if (typeof bootstrap !== 'undefined' && bootstrap.Carousel) {
            const bsCarousel = bootstrap.Carousel.getInstance(carousel);
            if (bsCarousel) {
                bsCarousel.pause();
            }
        }
        
        
        const activeSlide = carousel.querySelector('.carousel-item.active');
        if (activeSlide) {
            const progressBar = activeSlide.querySelector('.carousel-progress');
            if (progressBar) {
                progressBar.style.transition = 'none';
                const width = getComputedStyle(progressBar).width;
                progressBar.style.width = width;
            }
        }
    });
    
    
    carousel.addEventListener('mouseleave', function() {
        
        if (typeof bootstrap !== 'undefined' && bootstrap.Carousel) {
            const bsCarousel = bootstrap.Carousel.getInstance(carousel);
            if (bsCarousel) {
                bsCarousel.cycle();
                
                
                const activeSlide = carousel.querySelector('.carousel-item.active');
                if (activeSlide) {
                    const progressBar = activeSlide.querySelector('.carousel-progress');
                    if (progressBar) {
                        
                        void progressBar.offsetWidth;
                        
                        
                        progressBar.style.transition = 'width 5s linear';
                        progressBar.style.width = '100%';
                    }
                }
            }
        }
    });
}

function inicializarAnimacoesCards() {
    
    const petCards = document.querySelectorAll('.pet-card');
    petCards.forEach((card, index) => {
        card.style.setProperty('--animal-index', index % 12);
        
        
        const petItems = card.querySelectorAll('.pet-item');
        petItems.forEach(item => {
            
            const icone = item.querySelector('i');
            if (icone) {
                if (icone.classList.contains('fa-mars') || icone.classList.contains('fa-venus')) {
                    
                    icone.style.transition = 'all 0.3s ease';
                    item.addEventListener('mouseenter', function() {
                        icone.style.transform = 'scale(1.2) rotate(15deg)';
                    });
                    item.addEventListener('mouseleave', function() {
                        icone.style.transform = '';
                    });
                }
                else if (icone.classList.contains('fa-ruler')) {
                    
                    icone.style.transition = 'all 0.3s ease';
                    item.addEventListener('mouseenter', function() {
                        icone.style.transform = 'scale(1.2) translateY(-2px)';
                    });
                    item.addEventListener('mouseleave', function() {
                        icone.style.transform = '';
                    });
                }
                else if (icone.classList.contains('fa-calendar-alt')) {
                    
                    icone.style.transition = 'all 0.3s ease';
                    item.addEventListener('mouseenter', function() {
                        icone.style.transform = 'scale(1.2) rotate(-10deg)';
                    });
                    item.addEventListener('mouseleave', function() {
                        icone.style.transform = '';
                    });
                }
                else if (icone.classList.contains('fa-paw')) {
                    
                    icone.style.transition = 'all 0.3s ease';
                    item.addEventListener('mouseenter', function() {
                        icone.style.transform = 'scale(1.2) rotate(10deg)';
                    });
                    item.addEventListener('mouseleave', function() {
                        icone.style.transform = '';
                    });
                }
            }
            
            item.addEventListener('mouseenter', function() {
                this.style.transform = 'translateY(-5px)';
                this.style.boxShadow = '0 8px 15px rgba(0, 0, 0, 0.1)';
            });
            
            item.addEventListener('mouseleave', function() {
                this.style.transform = '';
                this.style.boxShadow = '';
            });
        });
        
        const description = card.querySelector('.pet-description');
        if (description) {
            description.addEventListener('mouseenter', function() {
                this.style.borderTopColor = '#ccc';
                this.style.borderBottomColor = '#ccc';
                this.style.transition = 'all 0.3s ease';
                
                const icone = this.querySelector('.description-icon');
                if (icone) {
                    icone.style.transform = 'scale(1.2)';
                    icone.style.transition = 'transform 0.3s ease';
                }
            });
            
            description.addEventListener('mouseleave', function() {
                this.style.borderTopColor = '';
                this.style.borderBottomColor = '';
                
                const icone = this.querySelector('.description-icon');
                if (icone) {
                    icone.style.transform = '';
                }
            });
        }
    });
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
                observer.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.1,
        rootMargin: '0px 0px -100px 0px'
    });
    
    
    petCards.forEach(card => {
        observer.observe(card);
    });
}


function iniciarCarrosselAutomatico() {
    
    if (intervaloCarrossel) {
        clearInterval(intervaloCarrossel);
    }
    
    
    intervaloCarrossel = setInterval(function() {
        proximoSlide();
    }, 5000);
}

function pararCarrosselAutomatico() {
    if (intervaloCarrossel) {
        clearInterval(intervaloCarrossel);
    }
}

function mudarSlide(indice) {
    
    const slides = document.querySelectorAll('.carousel-item');
    if (slides.length === 0) return;
    
    
    if (indice === slideAtual) return;
    
    
    slides[slideAtual].classList.remove('active');
    
    
    slideAtual = indice;
    
    
    if (slideAtual < 0) {
        slideAtual = slides.length - 1;
    } else if (slideAtual >= slides.length) {
        slideAtual = 0;
    }
    
    
    slides[slideAtual].classList.add('active');
}

function slideAnterior() {
    mudarSlide(slideAtual - 1);
}

function proximoSlide() {
    mudarSlide(slideAtual + 1);
}


function calcularItensPorPagina() {
    
    const cardsPerRow = obterCardsPorLinha();
    
    
    
    const linhasPorPagina = 3;
    const valorBase = Math.ceil((cardsPerRow * linhasPorPagina) / 4) * 4;
    
    
    
    const valorFinal = valorBase + 8;
    
    
    const valorMinimo = Math.max(valorFinal, 12);
    
    
    return valorMinimo;
}


function copiarFiltrosParaFormularioNavegacao() {
    
    const formFiltros = document.getElementById('formFiltros');
    const formNavegacao = document.getElementById('formNavegacao');
    
    if (!formFiltros || !formNavegacao) return;
    
    
    if (document.getElementById('filtroNome') && document.getElementById('filtroNomeNav')) {
        document.getElementById('filtroNomeNav').value = document.getElementById('filtroNome').value;
    }
    
    if (document.getElementById('filtroEspecie') && document.getElementById('filtroEspecieNav')) {
        document.getElementById('filtroEspecieNav').value = document.getElementById('filtroEspecie').value;
    }
    
    if (document.getElementById('filtroSexo') && document.getElementById('filtroSexoNav')) {
        document.getElementById('filtroSexoNav').value = document.getElementById('filtroSexo').value;
    }
    
    if (document.getElementById('filtroPorte') && document.getElementById('filtroPorteNav')) {
        document.getElementById('filtroPorteNav').value = document.getElementById('filtroPorte').value;
    }
    
    if (document.getElementById('filtroOrdem') && document.getElementById('filtroOrdemNav')) {
        document.getElementById('filtroOrdemNav').value = document.getElementById('filtroOrdem').value;
    }
    
    
    if (document.getElementById('filtroIdade') && document.getElementById('filtroIdadeNav')) {
        document.getElementById('filtroIdadeNav').value = document.getElementById('filtroIdade').value;
    }
    
    
}


function verificarLayoutEAjustar() {
    const cards = document.querySelectorAll('.pet-card');
    if (cards.length === 0) {
        const mensagemSemPets = document.querySelector('.no-pets-message');
        if (mensagemSemPets) {
            mensagemSemPets.style.display = 'flex';
            mensagemSemPets.innerHTML = `
                <div class="text-center w-100">
                    <i class="fas fa-search fa-4x mb-3 text-muted"></i>
                    <h3>Nenhum pet encontrado</h3>
                    <p>Tente modificar os filtros ou voltar mais tarde.</p>
                    <a href="/usuario/pets/explorar" class="btn btn-primary mt-3">
                        <i class="fas fa-sync-alt me-2"></i> Limpar filtros
                    </a>
                </div>
            `;
        }
        return;
    }
    
    
    let cardsPerRow = obterCardsPorLinha();
    
    
    const paginaAtualElement = document.getElementById('paginaAtual');
    let paginaAtual = 1;
    if (paginaAtualElement) {
        paginaAtual = parseInt(paginaAtualElement.value) || 1;
    }
    
    
    
    
    if (paginaAtual > 1 && cards.length < cardsPerRow) {
        navegarParaPagina(paginaAtual - 1);
        return; 
    }
    
    
    const totalPets = cards.length;
    const itensPorPagina = calcularItensPorPagina();
    
    
    
    
    ajustarCardsParaTresLinhas();
    
    
    window.addEventListener('resize', function() {
        
        const newCardsPerRow = obterCardsPorLinha();
        
        
        if (newCardsPerRow !== cardsPerRow) {
            
            cardsPerRow = newCardsPerRow;
            
            ajustarCardsParaTresLinhas();
        }
    });
    
    
    
    
    const btnLimparFiltros = document.querySelector('.clear-button');
    if (btnLimparFiltros) {
        btnLimparFiltros.addEventListener('click', function(e) {
            e.preventDefault();
            limparFiltros();
        });
    }
}


function obterCardsPorLinha() {
    
    const grid = document.querySelector('.pets-grid');
    
    if (!grid || !grid.children || grid.children.length === 0) {
        
        
        
        if (window.innerWidth <= 576) {
            return 1; 
        } else if (window.innerWidth <= 768) {
            return 2; 
        } else if (window.innerWidth <= 1200) {
            return 3; 
        } else if (window.innerWidth <= 1600) {
            return 4; 
        } else {
            return 5; 
        }
    }
    
    try {
        
        const firstCard = grid.children[0];
        if (!firstCard) return 4; 
        
        const firstCardRect = firstCard.getBoundingClientRect();
        const firstCardTop = firstCardRect.top;
        
        
        let cardsInFirstRow = 0;
        const tolerance = 5; 
        
        for (let i = 0; i < Math.min(grid.children.length, 12); i++) {
            const rect = grid.children[i].getBoundingClientRect();
            if (Math.abs(rect.top - firstCardTop) <= tolerance) {
                cardsInFirstRow++;
            } else {
                
                break;
            }
        }
        
        
        
        if (cardsInFirstRow <= 0 || cardsInFirstRow > 8) {
            
            const gridStyle = window.getComputedStyle(grid);
            const gridTemplateColumns = gridStyle.getPropertyValue('grid-template-columns');
            
            
            if (gridTemplateColumns.includes('repeat')) {
                const matches = gridTemplateColumns.match(/repeat\((\d+)/);
                if (matches && matches[1]) {
                    const explicitColumns = parseInt(matches[1]);
                    return explicitColumns;
                }
            }
            
            
            if (window.innerWidth <= 576) {
                return 1;
            } else if (window.innerWidth <= 768) {
                return 2;
            } else if (window.innerWidth <= 1200) {
                return 3;
            } else if (window.innerWidth <= 1600) {
                return 4;
            } else {
                return 5;
            }
        }
        
        return cardsInFirstRow;
    } catch (e) {
        console.error("Erro ao calcular colunas do grid:", e);
        
        
        if (window.innerWidth <= 576) {
            return 1;
        } else if (window.innerWidth <= 768) {
            return 2;
        } else if (window.innerWidth <= 1200) {
            return 3;
        } else if (window.innerWidth <= 1600) {
            return 4;
        } else {
            return 5;
        }
    }
}


function ajustarCardsParaTresLinhas() {
    const cards = document.querySelectorAll('.pet-card');
    if (cards.length === 0) return;
    
    
    let cardsPerRow = obterCardsPorLinha();
    
    
    const cardsToShow = cardsPerRow * 3;
    
    
    
    const paginaAtualElement = document.getElementById('paginaAtual');
    const paginaAtual = paginaAtualElement ? (parseInt(paginaAtualElement.value) || 1) : 1;
    
    if (paginaAtual === 1 && cards.length < cardsToShow) {
    }
    
    
    
    const valorAjustado = calcularItensPorPagina();
    
    
    if (document.getElementById('itensPorPagina')) {
        document.getElementById('itensPorPagina').value = valorAjustado;
    }
    
    if (document.getElementById('itensPorPaginaNav')) {
        document.getElementById('itensPorPaginaNav').value = valorAjustado;
    }
    
    
    if (cards.length <= cardsToShow) {
        cards.forEach(card => {
            card.style.display = 'block';
            card.style.opacity = '1';
            card.style.visibility = 'visible';
        });
        
        
        atualizarContadorPets(cards.length);
        return cards.length;
    }
    
    
    cards.forEach((card, index) => {
        if (index < cardsToShow) {
            card.style.display = 'block';
            card.style.opacity = '1';
            card.style.visibility = 'visible';
        } else {
            
            card.style.display = 'none';
        }
    });
    
    
    atualizarContadorPets(Math.min(cardsToShow, cards.length));
    
    
    
    if (paginaAtualElement) {
        
        
        if (paginaAtual > 1 && cards.length < cardsPerRow) {
            navegarParaPagina(paginaAtual - 1);
        }
    }
    
    return cardsToShow;
}


function atualizarContadorPets(cardsVisiveis) {
    const totalCards = document.querySelectorAll('.pet-card').length;
    const contadorTexto = document.querySelector('.contador-texto');
    
    if (contadorTexto) {
        
        const exibindo = Math.min(cardsVisiveis, totalCards);
        
        contadorTexto.innerHTML = `
            <span>Exibindo</span>
            <strong>${exibindo}</strong>
            <span>de</span>
            <strong>${totalCards}</strong>
            <span>pets</span>
        `;
    }
}


function navegarParaPagina(pagina) {
    
    document.getElementById('paginaAtual').value = pagina;
    
    
    copiarFiltrosParaFormularioNavegacao();
    
    
    const valorFinal = calcularItensPorPagina();
    
    
    document.getElementById('itensPorPaginaNav').value = valorFinal;
    
    
    const petsGrid = document.querySelector('.pets-grid');
    if (petsGrid) {
        
        const loadingOverlay = document.createElement('div');
        loadingOverlay.className = 'loading-overlay';
        loadingOverlay.innerHTML = '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Carregando...</span></div>';
        loadingOverlay.style.position = 'absolute';
        loadingOverlay.style.top = '0';
        loadingOverlay.style.left = '0';
        loadingOverlay.style.width = '100%';
        loadingOverlay.style.height = '100%';
        loadingOverlay.style.background = 'rgba(255,255,255,0.8)';
        loadingOverlay.style.display = 'flex';
        loadingOverlay.style.justifyContent = 'center';
        loadingOverlay.style.alignItems = 'center';
        loadingOverlay.style.zIndex = '10';
        
        
        const container = petsGrid.parentElement;
        if (container) {
            container.style.position = 'relative';
            container.appendChild(loadingOverlay);
        }
    }
    
    
    
    document.getElementById('formNavegacao').submit();
}


function gerenciarBarrasDeProgresso() {
    
    document.querySelectorAll('.carousel-progress-bar').forEach(function(bar) {
        bar.style.transition = 'none';
        bar.style.width = '0%';
    });
    
    
    setTimeout(function() {
        const activeSlide = document.querySelector('.carousel-item.active');
        if (activeSlide) {
            const progressBar = activeSlide.querySelector('.carousel-progress-bar');
            if (progressBar) {
                
                void progressBar.offsetWidth;
                
                
                progressBar.style.transition = 'width 5s linear';
                progressBar.style.width = '100%';
            }
        }
    }, 50);
}


function inicializarItensPorPagina() {
    
    const valorFinal = calcularItensPorPagina();
    
    
    
    if (document.getElementById('itensPorPagina')) {
        document.getElementById('itensPorPagina').value = valorFinal;
    }
    
    if (document.getElementById('itensPorPaginaNav')) {
        document.getElementById('itensPorPaginaNav').value = valorFinal;
    }
    
    return valorFinal;
}

$(document).ready(function() {
    // Manipulador para o seletor de itens por página
    $('#selectItensPorPagina').change(function() {
        var itensPorPagina = $(this).val();
        $('#itensPorPagina').val(itensPorPagina);
        $('#itensPorPaginaNav').val(itensPorPagina);
        $('#paginaAtual').val(1); // Volta para a primeira página ao mudar itens por página
        $('#formNavegacao').submit();
    });
});


