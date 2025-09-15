

window.abrirMenuLateral = function() {
    const menuLateral = document.querySelector('.menu-lateral');
    const menuSobreposicao = document.querySelector('.menu-sobreposicao');
    const menuHamburguer = document.getElementById('btnMenuHamburguer');
    
    if (menuLateral) {
        menuLateral.classList.add('ativo');
        
    }
    
    if (menuSobreposicao) {
        menuSobreposicao.classList.add('ativo');
    }
    
    if (menuHamburguer) {
        menuHamburguer.classList.add('ativo');
    }
    
    
    window.fecharPainelNotificacoesSeAberto();
    
}


window.fecharMenuLateral = function() {
    const menuLateral = document.querySelector('.menu-lateral');
    const menuSobreposicao = document.querySelector('.menu-sobreposicao');
    const menuHamburguer = document.getElementById('btnMenuHamburguer');
    
    if (menuLateral) {
        menuLateral.classList.remove('ativo');
        
    }
    
    if (menuSobreposicao) {
        menuSobreposicao.classList.remove('ativo');
    }
    
    if (menuHamburguer) {
        menuHamburguer.classList.remove('ativo');
    }
    
}


window.fecharPainelNotificacoesSeAberto = function() {
    const painelNotificacoes = document.getElementById('painel-notificacoes');
    if (painelNotificacoes && (painelNotificacoes.classList.contains('ativo') || painelNotificacoes.style.display === 'block')) {
        if (typeof ocultarPainelNotificacoes === 'function') {
            ocultarPainelNotificacoes();
        } else {
            painelNotificacoes.style.display = 'none';
            painelNotificacoes.style.visibility = 'hidden';
            painelNotificacoes.style.opacity = '0';
            painelNotificacoes.classList.remove('ativo');
        }
    }
}


async function realizarLogout() {
    try {
        const response = await fetch('/Autenticacao/Logout', {
            method: 'GET',
            credentials: 'include'
        });

        if (response.ok || response.redirected) {
            window.location.href = '/Home/Index';
        } else {
            console.error('Ocorreu um erro ao fazer logout. Por favor, tente novamente.');
        }
    } catch (error) {
        console.error('Ocorreu um erro ao fazer logout. Por favor, tente novamente.');
    }
}


document.addEventListener('DOMContentLoaded', function() {
    
    
    const iconeNotificacao = document.querySelector('.icone-notificacao');
    if (iconeNotificacao) {
        iconeNotificacao.addEventListener('click', function() {
            
        });
    }

    
    const logoutLink = document.querySelector('a[href="/Autenticacao/Logout"]');
    if (logoutLink) {
        logoutLink.addEventListener('click', function(e) {
            e.preventDefault();
            realizarLogout();
        });
    }
    
});


function alternarBarraLateral() {
    const sidebar = document.querySelector('.sidebar');
    if (sidebar) {
        sidebar.classList.toggle('active');
    }
}


document.addEventListener('click', function(event) {
    const sidebar = document.querySelector('.sidebar');
    const sidebarToggle = document.querySelector('.navbar-toggler');
    
    if (sidebar && sidebar.classList.contains('active') && 
        !sidebar.contains(event.target) && 
        !sidebarToggle.contains(event.target)) {
        sidebar.classList.remove('active');
    }
});


document.addEventListener('DOMContentLoaded', function() {
    const navbar = document.querySelector('.navbar');
    if (navbar) {
        const toggleButton = document.createElement('button');
        toggleButton.className = 'btn btn-link d-lg-none';
        toggleButton.innerHTML = '<i class="fas fa-bars"></i>';
        toggleButton.onclick = alternarBarraLateral;
        
        const navbarToggler = navbar.querySelector('.navbar-toggler');
        if (navbarToggler) {
            navbarToggler.parentNode.insertBefore(toggleButton, navbarToggler);
        }
    }
});


window.addEventListener('resize', function() {
    const sidebar = document.querySelector('.sidebar');
    if (sidebar && window.innerWidth >= 992) {
        sidebar.classList.remove('active');
    }
});


window.abrirMenuLateral = abrirMenuLateral;
window.fecharMenuLateral = fecharMenuLateral;
window.realizarLogout = realizarLogout; 