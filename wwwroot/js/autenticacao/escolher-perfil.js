

function selecionarPerfil(tipo) {
    
    
    document.querySelectorAll('.cartao-opcao').forEach(card => {
        card.classList.remove('selecionado');
    });
    
    
    const cardSelecionado = document.getElementById('card-' + tipo);
    if (cardSelecionado) {
        cardSelecionado.classList.add('selecionado');
    }
    
    
    const campoTipoPerfil = document.getElementById('tipoPerfil');
    if (campoTipoPerfil) {
        campoTipoPerfil.value = tipo;
    }
    
    
    const botaoContinuar = document.getElementById('btnContinuar');
    if (botaoContinuar) {
        botaoContinuar.disabled = false;
    }
}


document.addEventListener('DOMContentLoaded', function() {
    
    
    const formPerfil = document.getElementById('formPerfil');
    if (formPerfil) {
        formPerfil.addEventListener('submit', function(event) {
            const tipoPerfil = document.getElementById('tipoPerfil').value;
            
            if (!tipoPerfil) {
                event.preventDefault();
                alert('Por favor, selecione um perfil para continuar.');
            } else {
            }
        });
    }
    
    
    const cards = document.querySelectorAll('.cartao-opcao');
    if (cards.length === 1) {
        const unicoTipo = cards[0].id.replace('card-', '');
        selecionarPerfil(unicoTipo);
    }
}); 