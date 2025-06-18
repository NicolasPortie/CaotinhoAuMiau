


document.addEventListener('DOMContentLoaded', function() {
    
    
    const elementos = {
        menuToggle: document.querySelector('.usuario-menu-toggle'),
        dropdownMenu: document.getElementById('dropdownUsuario'),
        notificacoesIcone: document.querySelector('.icone-notificacao'),
        painelNotificacoes: document.getElementById('painel-notificacoes'),
        footer: document.querySelector('.footer'),
        navbar: document.querySelector('.navbar')
    };
    
    
    function toggleDropdownUsuario() {
        if (!elementos.dropdownMenu) return;
        
        const estaVisivel = elementos.dropdownMenu.style.display === 'block';
        
        if (estaVisivel) {
            ocultarDropdownUsuario();
        } else {
            mostrarDropdownUsuario();
        }
    }
    
    
    function mostrarDropdownUsuario() {
        if (!elementos.dropdownMenu) return;
        
        
        if (elementos.painelNotificacoes && elementos.painelNotificacoes.classList.contains('ativo')) {
            fecharPainelNotificacoes();
        }
        
        elementos.dropdownMenu.style.display = 'block';
        elementos.dropdownMenu.classList.add('visivel');
        document.body.classList.add('dropdown-aberto');
        
        
        setTimeout(function() {
            document.addEventListener('click', fecharDropdownAoClicarFora);
        }, 100);
    }
    
    
    function ocultarDropdownUsuario() {
        if (!elementos.dropdownMenu) return;
        
        elementos.dropdownMenu.classList.remove('visivel');
        document.body.classList.remove('dropdown-aberto');
        
        
        setTimeout(function() {
            elementos.dropdownMenu.style.display = 'none';
        }, 300);
    }
    
    
    if (elementos.menuToggle && elementos.dropdownMenu) {
        
        elementos.dropdownMenu.style.display = 'none';
        
        
        elementos.dropdownMenu.addEventListener('mouseover', function(e) {
            const link = e.target.closest('a');
            if (link) {
                link.style.backgroundColor = '#505050';
            }
        });
        
        elementos.dropdownMenu.addEventListener('mouseout', function(e) {
            const link = e.target.closest('a');
            if (link) {
                link.style.backgroundColor = '#3A3A3A';
            }
        });
        
        
        elementos.menuToggle.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            toggleDropdownUsuario();
        });
    }
    
    
    function fecharDropdownAoClicarFora(e) {
        if (elementos.dropdownMenu && !elementos.dropdownMenu.contains(e.target) && 
            (!elementos.menuToggle || !elementos.menuToggle.contains(e.target))) {
            ocultarDropdownUsuario();
            document.removeEventListener('click', fecharDropdownAoClicarFora);
        }
    }
    
    
    const inicializarEstilos = (function() {
        
        document.body.classList.add('pagina-sobre');
        
        
        const btnTopo = document.createElement('button');
        btnTopo.innerHTML = '<i class="fas fa-arrow-up"></i>';
        btnTopo.className = 'btn-voltar-topo';
        document.body.appendChild(btnTopo);
        
        
        const style = document.createElement('style');
        style.innerHTML = `
            .btn-voltar-topo {
                position: fixed;
                bottom: 20px;
                right: 20px;
                width: 50px;
                height: 50px;
                background-color: var(--primary-color);
                color: white;
                border: none;
                border-radius: 50%;
                cursor: pointer;
                font-size: 20px;
                display: flex;
                align-items: center;
                justify-content: center;
                opacity: 0;
                visibility: hidden;
                transition: all 0.3s ease;
                z-index: 999;
                box-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
            }
            
            .btn-voltar-topo.mostrar {
                opacity: 1;
                visibility: visible;
            }
            
            .btn-voltar-topo:hover {
                background-color: var(--secondary-color);
                transform: translateY(-5px);
                box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
            }
            
            /* Estilos corrigidos para o painel de notificações */
            #painel-notificacoes {
                position: absolute;
                z-index: 9999; 
                top: 45px; 
                right: 0; 
                width: 350px; 
                min-width: 350px; 
                max-width: 90vw; 
                background-color: white !important; 
                border-radius: 8px; 
                box-shadow: 0 8px 25px rgba(0,0,0,0.25);
                overflow: hidden;
                border: none;
                transition: all 0.3s ease;
            }
            
            .notificacoes-cabecalho {
                background-color: #FF6B00 !important;
                color: white;
                border-radius: 8px 8px 0 0;
                padding: 15px 20px;
                display: flex;
                justify-content: space-between;
                align-items: center;
                width: 100%;
            }
            
            .carregando-notificacoes {
                padding: 30px;
                display: flex;
                flex-direction: column;
                align-items: center;
                justify-content: center;
                background-color: white !important;
                width: 100%;
            }
            
            .lista-notificacoes {
                max-height: 400px;
                overflow-y: auto;
                background-color: white !important;
                width: 100%;
                scrollbar-width: thin;
                scrollbar-color: var(--primary-color) #f0f0f0;
            }
        `;
        document.head.appendChild(style);
        
        return { btnTopo };
    })();
    
    
    let scrollTimeout = null;
    const throttleScroll = function(callback) {
        return function() {
            if (!scrollTimeout) {
                scrollTimeout = setTimeout(() => {
                    callback();
                    scrollTimeout = null;
                }, 50); 
            }
        };
    };
    
    
    let animationEnabled = true;
    
    
    const tratarRolagem = throttleScroll(function() {
        
        const painelAberto = elementos.painelNotificacoes?.classList.contains('ativo') || 
                            (elementos.dropdownMenu && elementos.dropdownMenu.style.display === 'block');
                           
        if (painelAberto) {
            
            if (animationEnabled) {
                animationEnabled = false;
            }
            
            
            if (inicializarEstilos.btnTopo) {
                if (window.pageYOffset > 300) {
                    inicializarEstilos.btnTopo.classList.add('mostrar');
                } else {
                    inicializarEstilos.btnTopo.classList.remove('mostrar');
                }
            }
            
            return; 
        } else if (!animationEnabled) {
            
            animationEnabled = true;
        }
        
        
        if (elementos.navbar) {
            if (window.scrollY > 50) {
                elementos.navbar.classList.add('scrolled');
            } else {
                elementos.navbar.classList.remove('scrolled');
            }
        }
        
        
        if (inicializarEstilos.btnTopo) {
            if (window.pageYOffset > 300) {
                inicializarEstilos.btnTopo.classList.add('mostrar');
            } else {
                inicializarEstilos.btnTopo.classList.remove('mostrar');
            }
        }
        
        
        const banner = document.querySelector('.banner');
        if (banner && animationEnabled) {
            const scrollPos = window.pageYOffset;
            banner.style.backgroundPositionY = `calc(50% + ${scrollPos * 0.4}px)`;
            
            const conteudoBanner = document.querySelector('.conteudo-banner');
            if (conteudoBanner && scrollPos < 500) {
                conteudoBanner.style.opacity = Math.max(0, 1 - (scrollPos * 0.002));
                conteudoBanner.style.transform = `translateY(${scrollPos * 0.2}px)`;
            }
        }
        
        
        if (animationEnabled) {
            verificarScroll();
        }
    });
    
    
    window.addEventListener('scroll', tratarRolagem);
    
    
    inicializarEstilos.btnTopo.addEventListener('click', function() {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    });
    
    
    function verificarScroll() {
        const contadores = document.querySelectorAll('.contador-numero');
        let animacaoIniciada = false;
        
        if (!animacaoIniciada && contadores.length > 0) {
            const rect = contadores[0].getBoundingClientRect();
            const windowHeight = window.innerHeight || document.documentElement.clientHeight;
            
            
            if (rect.top <= windowHeight * 0.8 && rect.bottom >= windowHeight * 0.2) {
                contadores.forEach(contador => {
                    animarContador(contador);
                });
                animacaoIniciada = true;
            }
        }
    }
    
    
    function animarContador(elemento) {
        const valor = parseInt(elemento.getAttribute('data-contador'));
        let atual = 0;
        const incremento = valor > 100 ? 5 : 1;
        const duracao = 2000; 
        const intervalo = duracao / (valor / incremento);
        
        const timer = setInterval(() => {
            atual += incremento;
            if (atual > valor) {
                elemento.textContent = valor;
                clearInterval(timer);
            } else {
                elemento.textContent = atual;
            }
        }, intervalo);
    }
    

    
    if (elementos.notificacoesIcone && elementos.painelNotificacoes) {
        
        elementos.notificacoesIcone.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            const estaAtivo = elementos.painelNotificacoes.classList.contains('ativo');
            
            if (!estaAtivo) {
                
                requestAnimationFrame(() => {
                    fecharTodosPaineis();
                    
                    
                    elementos.painelNotificacoes.classList.add('ativo');
                    elementos.painelNotificacoes.style.display = 'block';
                    elementos.painelNotificacoes.style.visibility = 'visible';
                    
                    
                    document.body.classList.add('painel-aberto');
                    
                    
                    carregarNotificacoes();
                });
            } else {
                fecharPainelNotificacoes();
            }
        });
        
        
        const botaoFechar = document.getElementById('fechar-notificacoes');
        if (botaoFechar) {
            botaoFechar.addEventListener('click', function(e) {
                e.preventDefault();
                fecharPainelNotificacoes();
            });
        }
        
        
        const marcarLidasBtn = document.getElementById('marcar-todas-lidas');
        if (marcarLidasBtn) {
            marcarLidasBtn.addEventListener('click', function(e) {
                e.preventDefault();
                if (typeof window.marcarTodasComoLidas === "function") {
                    window.marcarTodasComoLidas();
                }
            });
        }
    }
    
    
    let clickTimeout = null;
    document.addEventListener('click', function(e) {
        clearTimeout(clickTimeout);
        clickTimeout = setTimeout(() => {
            
            if (elementos.menuToggle && elementos.dropdownMenu && elementos.painelNotificacoes) {
                const clickedOnMenu = 
                    elementos.menuToggle.contains(e.target) || 
                    elementos.dropdownMenu.contains(e.target) ||
                    (elementos.notificacoesIcone && elementos.notificacoesIcone.contains(e.target)) ||
                    elementos.painelNotificacoes.contains(e.target);
                
                if (!clickedOnMenu) {
                    fecharTodosPaineis();
                }
            }
        }, 10);
    });
    
    
    const styleOptimization = document.createElement('style');
    styleOptimization.innerHTML = `
        /* CSS para otimizar performance quando painéis estão abertos */
        body.painel-aberto .banner {
            transition: none !important;
            animation: none !important;
        }
        
        body.painel-aberto .pata {
            animation-play-state: paused !important;
        }
        
        body.painel-aberto .scroll-btn {
            pointer-events: none;
        }
        
        body.painel-aberto .equipe-card {
            transform: none !important;
            transition: none !important;
        }
        
        /* Melhorar desempenho do painel de notificações */
        #painel-notificacoes {
            will-change: transform;
            transform: translateZ(0);
            backface-visibility: hidden;
        }
        
        /* Melhorar desempenho do dropdown menu */
        #dropdownUsuario {
            will-change: transform;
            transform: translateZ(0);
            backface-visibility: hidden;
        }
        
        /* Otimizar lista de notificações */
        .lista-notificacoes {
            will-change: transform;
            transform: translateZ(0);
            backface-visibility: hidden;
        }
        
        /* Estilos para o avatar do usuário */
        .usuario-avatar {
            width: 40px;
            height: 40px;
            border-radius: 50%;
            overflow: hidden;
            display: flex;
            align-items: center;
            justify-content: center;
            background-color: #3A3A3A;
            color: white;
            font-weight: bold;
            font-size: 1.2rem;
        }
        
        .usuario-avatar img {
            width: 100%;
            height: 100%;
            object-fit: cover;
        }
        
        .avatar-inicial {
            width: 100%;
            height: 100%;
            display: flex;
            align-items: center;
            justify-content: center;
            background-color: #FF6B00;
            color: white;
            font-weight: bold;
            font-size: 1.2rem;
        }
    `;
    document.head.appendChild(styleOptimization);
    
    
    function fecharTodosPaineis() {
        ocultarDropdownUsuario();
        fecharPainelNotificacoes();
        
        document.body.classList.remove('painel-aberto');
    }

    
    function inicializarImagensEquipe() {
        const equipeCards = document.querySelectorAll('.equipe-card');
        const equipeImagens = document.querySelectorAll('.equipe-imagem');
        const equipeInfo = document.querySelectorAll('.equipe-info');
        
        
        document.querySelectorAll('.equipe-imagem img').forEach(img => {
            img.onerror = function() {
                console.error('Erro ao carregar imagem:', this.src);
                
                this.src = '/imagens/paw-icon.png'; 
                this.style.objectFit = 'contain';
                this.style.padding = '20px';
                this.style.backgroundColor = '#f0f0f0';
                this.parentElement.classList.add('erro-carregamento');
            };
            
            img.onload = function() {
                this.style.opacity = '1';
                this.parentElement.classList.add('fade-in');
            };
            
            
            const currentSrc = img.src;
            img.src = '';
            img.src = currentSrc;
        });
        
        setTimeout(() => {
            equipeCards.forEach((card, index) => {
                setTimeout(() => {
                    card.classList.add('fade-active');
                }, index * 150);
            });
            
            equipeInfo.forEach((info, index) => {
                setTimeout(() => {
                    info.classList.add('animate-fadeIn');
                }, index * 150 + 300);
            });
        }, 500);
    }
    
    
    inicializarImagensEquipe();
}); 
