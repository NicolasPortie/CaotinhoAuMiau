// Ao rolar a página, adiciona ou remove a classe 'scrolled' na barra de navegação
window.addEventListener('scroll', function () {
    var navbar = document.querySelector('.navbar');
    if (navbar) {
        navbar.classList.toggle('scrolled', window.scrollY > 50);
    }
});
// Alterna a visibilidade do campo de senha
var alternarVisibilidadeSenha = function (inputId) {
    var senhaInput = document.getElementById(inputId);
    var botao = document.querySelector('.botao-senha');
    var icone = botao === null || botao === void 0 ? void 0 : botao.querySelector('i');
    if (!senhaInput || !icone)
        return;
    if (senhaInput.type === 'password') {
        senhaInput.type = 'text';
        icone.classList.replace('fa-eye', 'fa-eye-slash');
    }
    else {
        senhaInput.type = 'password';
        icone.classList.replace('fa-eye-slash', 'fa-eye');
    }
};
// Configurações executadas quando o documento estiver pronto
document.addEventListener('DOMContentLoaded', function () {

    var botaoSenha = document.querySelector('.botao-senha');
    if (botaoSenha) {
        botaoSenha.addEventListener('click', function () {
            alternarVisibilidadeSenha('Senha');
        });
    }

    // Interceptar o envio do formulário para debug
    var formLogin = document.querySelector('form');
    if (formLogin) {
        formLogin.addEventListener('submit', function(e) {
            var email = document.getElementById('Email')?.value;
        });
    }

    var alertas = document.querySelectorAll('.alerta');
    if (alertas.length > 0) {
        setTimeout(function () {
            alertas.forEach(function (alerta) {
                alerta.style.opacity = '0';
                setTimeout(function () {
                    alerta.style.display = 'none';
                }, 500);
            });
        }, 5000);
    }

    // Debug de redirecionamento
    if (window.location.pathname === '/') {
    }
});
