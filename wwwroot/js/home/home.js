toastr.options = {
    "closeButton": true,
    "progressBar": true,
    "positionClass": "toast-bottom-right",
    "showDuration": "300",
    "hideDuration": "1000",
    "timeOut": "5000",
    "extendedTimeOut": "1000",
    "showEasing": "swing",
    "hideEasing": "linear",
    "showMethod": "fadeIn",
    "hideMethod": "fadeOut"
};
document.addEventListener('DOMContentLoaded', () => {
    animarAoScrollar();
    document.querySelectorAll('.cartao-motivo, .cartao-passo').forEach(cartao => {
        cartao
    const formularioNewsletter = document.getElementById('formulario-newsletter');
    if (formularioNewsletter) {
        formularioNewsletter.addEventListener('submit', (e) => {
            e.preventDefault();
            const inputEmail = formularioNewsletter.querySelector('input[type="email"]');
            const email = inputEmail ? inputEmail.value : '';
            if (email && email.indexOf('@') > 0) {
                if (inputEmail) inputEmail.value = '';
                mostrarMensagem('Obrigado por se inscrever!', 'success');
            } else {
                mostrarMensagem('Por favor, insira um email vÃ¡lido.', 'error');
            }
        });
    }
});
function mostrarMensagem(mensagem, tipo) {
    if (typeof toastr !== 'undefined') {
        switch(tipo) {
            case 'success':
                toastr.success(mensagem);
                break;
            case 'error':
                toastr.error(mensagem);
                break;
            case 'warning':
                toastr.warning(mensagem);
                break;
            case 'info':
            default:
                toastr.info(mensagem);
                break;
        }
    } else {
    }
}
function animarAoScrollar() {
    const animarElementos = () => {
        document.querySelectorAll('.cartao-motivo, .cartao-passo, .item-contador').forEach(el => {
            const elementoTopo = el.getBoundingClientRect().top + window.scrollY;
            const scrollPos = window.scrollY;
            const windowHeight = window.innerHeight;
            if (scrollPos + windowHeight > elementoTopo + 100) {
                el.classList.add('animated');
            }
        });
    };
    animarElementos();
    window.addEventListener('scroll', animarElementos);
}
