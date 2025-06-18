
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

$(document).ready(function() {
    
    animarAoScrollar();
    
    $('.cartao-motivo, .cartao-passo').hover(
        function() {
            $(this).addClass('hover');
        },
        function() {
            $(this).removeClass('hover');
        }
    );
    
    $('#formulario-newsletter').submit(function(e) {
        e.preventDefault();
        const email = $(this).find('input[type="email"]').val();
        
        if (email && email.indexOf('@') > 0) {
            $(this).find('input[type="email"]').val('');
            mostrarMensagem('Obrigado por se inscrever!', 'success');
        } else {
            mostrarMensagem('Por favor, insira um email válido.', 'error');
        }
    });
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
        alert(mensagem);
    }
}

function animarAoScrollar() {
    const animarElementos = function() {
        $('.cartao-motivo, .cartao-passo, .item-contador').each(function() {
            const elementoTopo = $(this).offset().top;
            const scrollPos = $(window).scrollTop();
            const windowHeight = $(window).height();
            
            if (scrollPos + windowHeight > elementoTopo + 100) {
                $(this).addClass('animated');
            }
        });
    };
    
    animarElementos();
    $(window).scroll(animarElementos);
} 