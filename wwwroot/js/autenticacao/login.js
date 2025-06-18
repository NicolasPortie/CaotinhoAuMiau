

window.addEventListener('scroll', function() {
    const navbar = document.querySelector('.navbar');
    if (window.scrollY > 50) {
        navbar.classList.add('scrolled');
    } else {
        navbar.classList.remove('scrolled');
    }
});


function alternarVisibilidadeSenha(inputId) {
    const senhaInput = document.getElementById(inputId);
    const botao = document.querySelector('.botao-senha');
    const icone = botao.querySelector('i');
    
    if (senhaInput.type === 'password') {
        senhaInput.type = 'text';
        icone.classList.replace('fa-eye', 'fa-eye-slash');
    } else {
        senhaInput.type = 'password';
        icone.classList.replace('fa-eye-slash', 'fa-eye');
    }
}


document.addEventListener('DOMContentLoaded', function() {
    
    const botaoSenha = document.querySelector('.botao-senha');
    if (botaoSenha) {
        botaoSenha.addEventListener('click', function() {
            alternarVisibilidadeSenha('Senha');
        });
    }
    
    
    const alertas = document.querySelectorAll('.alerta');
    if (alertas.length > 0) {
        setTimeout(function() {
            alertas.forEach(alerta => {
                alerta.style.opacity = '0';
                setTimeout(() => {
                    alerta.style.display = 'none';
                }, 500);
            });
        }, 5000);
    }
}); 