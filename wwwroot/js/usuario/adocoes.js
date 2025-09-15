document.addEventListener('DOMContentLoaded', function() {
    initializePage();
});

function initializePage() {
    initializeFilters();
    initializeSearch();
    initializeSort();
    initializeModal();
    adjustButtonSizes();
    
}

function initializeFilters() {
    const filterButtons = document.querySelectorAll('.filter-pill');
    const petCards = document.querySelectorAll('.adoption-card');
    
    filterButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            
            filterButtons.forEach(btn => btn.classList.remove('active'));
            
            this.classList.add('active');
            
            const filterStatus = this.dataset.status;
            
            filterCards(filterStatus, petCards);
        });
    });
}

function filterCards(filterStatus, cards) {
    cards.forEach(card => {
        const cardStatus = card.dataset.status;
        
        if (filterStatus === 'all' || cardStatus === filterStatus) {
            card.style.display = 'flex';
            card.style.animation = 'fadeIn 0.3s ease';
        } else {
            card.style.display = 'none';
        }
    });
}

function initializeSearch() {
    const searchInput = document.getElementById('searchInput');
    const petCards = document.querySelectorAll('.adoption-card');
    
    if (searchInput) {
        searchInput.addEventListener('input', debounce(function() {
            const searchTerm = this.value.toLowerCase().trim();
            searchCards(searchTerm, petCards);
        }, 300));
    }
}

function searchCards(searchTerm, cards) {
    cards.forEach(card => {
        const petName = card.dataset.petName || '';
        const cardText = card.textContent.toLowerCase();
        
        if (searchTerm === '' || petName.includes(searchTerm) || cardText.includes(searchTerm)) {
            card.style.display = 'flex';
            card.style.animation = 'fadeIn 0.3s ease';
        } else {
            card.style.display = 'none';
        }
    });
}

function initializeSort() {
    const sortSelect = document.getElementById('sortSelect');
    
    if (sortSelect) {
        sortSelect.addEventListener('change', function() {
            const sortValue = this.value;
            sortCards(sortValue);
        });
    }
}

function sortCards(sortValue) {
    const grid = document.getElementById('petsGrid');
    const cards = Array.from(document.querySelectorAll('.adoption-card'));
    
    cards.sort((a, b) => {
        switch (sortValue) {
            case 'recent':
                return 0;
            
            case 'oldest':
                return -1;
            
            case 'name':
                const nameA = a.dataset.petName || '';
                const nameB = b.dataset.petName || '';
                return nameA.localeCompare(nameB);
            
            case 'status':
                const statusA = a.dataset.status || '';
                const statusB = b.dataset.status || '';
                return statusA.localeCompare(statusB);
            
            default:
                return 0;
        }
    });
    
    if (sortValue === 'oldest') {
        cards.reverse();
    }
    
    cards.forEach(card => {
        grid.appendChild(card);
    });
}

let currentAdoptionId = null;

function initializeModal() {
    const modal = document.getElementById('modalCancelar');
    if (!modal) return;
    
    const closeBtn = modal.querySelector('.modal-close');
    const textarea = document.getElementById('cancelReason');
    const charCounter = document.getElementById('charCounter');
    
    if (closeBtn) {
        closeBtn.addEventListener('click', fecharModalCancelar);
    }
    
    modal.addEventListener('click', function(e) {
        if (e.target === modal) {
            fecharModalCancelar();
        }
    });
    
    if (textarea && charCounter) {
        textarea.addEventListener('input', function() {
            const count = this.value.length;
            charCounter.textContent = `${count}/300 caracteres`;
            
            if (count > 250) {
                charCounter.style.color = '#E74C3C';
            } else {
                charCounter.style.color = '#5D6D7E';
            }
        });
    }
}

function abrirModalCancelar(adocaoId, petNome, dataSolicitacao, imagemPet) {
    currentAdoptionId = adocaoId;
    
    const modal = document.getElementById('modalCancelar');
    const petImage = document.getElementById('cancelPetImage');
    const petName = document.getElementById('cancelPetName');
    const cancelDate = document.getElementById('cancelDate');
    const textarea = document.getElementById('cancelReason');
    
    if (petImage) {
        petImage.src = `/imagens/pets/${imagemPet}`;
        petImage.alt = petNome;
    }
    
    if (petName) {
        petName.textContent = petNome;
    }
    
    if (cancelDate) {
        cancelDate.textContent = dataSolicitacao;
    }
    
    if (textarea) {
        textarea.value = '';
        textarea.dispatchEvent(new Event('input'));
    }
    
    modal.classList.add('active');
    document.body.style.overflow = 'hidden';
    
    setTimeout(() => {
        if (textarea) {
            textarea.focus();
        }
    }, 100);
}

function fecharModalCancelar() {
    const modal = document.getElementById('modalCancelar');
    const textarea = document.getElementById('cancelReason');
    
    modal.classList.remove('active');
    document.body.style.overflow = '';
    
    currentAdoptionId = null;
    if (textarea) {
        textarea.value = '';
    }
}

function confirmarCancelamento() {
    if (!currentAdoptionId) {
        showNotification('Erro: ID da adoção não encontrado', 'error');
        return;
    }
    
    const motivo = document.getElementById('cancelReason')?.value.trim() || '';
    const userId = document.getElementById('userId')?.value;
    const userCpf = document.getElementById('userCpf')?.value;
    
    if (!userId || !userCpf) {
        showNotification('Erro: Dados do usuário não encontrados', 'error');
        return;
    }
    
    const confirmBtn = document.querySelector('.modal-footer .btn-danger');
    if (confirmBtn) {
        confirmBtn.disabled = true;
        confirmBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Cancelando...';
    }
    
    const dados = {
        adocaoId: currentAdoptionId,
        motivo: motivo,
        usuarioId: userId,
        cpf: userCpf
    };
    
    fetch('/usuario/adocao/cancelar', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': getAntiForgeryToken()
        },
        body: JSON.stringify(dados)
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        if (data.success) {
            showNotification('Solicitação cancelada com sucesso', 'success');
            
            const card = document.querySelector(`[data-id="${currentAdoptionId}"]`);
            if (card) {
                card.style.animation = 'fadeOut 0.3s ease';
                setTimeout(() => {
                    card.remove();
                }, 300);
            }
            
            fecharModalCancelar();
            
            setTimeout(() => {
                window.location.reload();
            }, 1000);
        } else {
            throw new Error(data.message || 'Erro ao cancelar solicitação');
        }
    })
    .catch(error => {
        showNotification(error.message || 'Erro ao cancelar solicitação', 'error');
        
        if (confirmBtn) {
            confirmBtn.disabled = false;
            confirmBtn.innerHTML = '<i class="fas fa-times-circle"></i> Confirmar Cancelamento';
        }
    });
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

function getAntiForgeryToken() {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value ||
                  document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') ||
                  '';
    return token;
}

function showNotification(message, type = 'info') {
    if (typeof window.showNotification === 'function') {
        window.showNotification(message, type);
        return;
    }
    
    const typeMap = {
        success: '[OK]',
        error: '[ERRO]',
        warning: '[AVISO]',
        info: '[INFO]'
    };
    
    console.log(`${typeMap[type] || '[INFO]'} ${message}`);
}

function visualizarAgendamento(adocaoId) {
    if (adocaoId) {
        window.location.href = `/admin/adocoes/agendamento/${adocaoId}`;
    } else {
        showNotification('ID da adoção não encontrado', 'error');
    }
}

const style = document.createElement('style');
style.textContent = `
    @keyframes fadeIn {
        from { opacity: 0; transform: translateY(20px); }
        to { opacity: 1; transform: translateY(0); }
    }
    
    @keyframes fadeOut {
        from { opacity: 1; transform: translateY(0); }
        to { opacity: 0; transform: translateY(-20px); }
    }
`;
document.head.appendChild(style);

function visualizarContrato() {
    const currentModal = document.getElementById('modalDetalhes');
    const adocaoId = currentModal ? currentModal.dataset.adocaoId : null;

    if (adocaoId) {
        window.open(`/usuario/adocao/contrato/${adocaoId}`, '_blank');
    } else {
        showNotification('ID da adoção não encontrado', 'error');
    }
}

function baixarContrato() {
    // Buscar o ID diretamente do modal de detalhes carregado
    const adocaoIdElement = document.getElementById('detalhesAdocaoId');
    const adocaoId = adocaoIdElement ? adocaoIdElement.textContent.replace('#', '') : null;

    if (adocaoId) {
        const link = document.createElement('a');
        link.href = `/usuario/adocao/contrato/${adocaoId}/pdf`;
        link.download = `Contrato_Adocao_${adocaoId}.pdf`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        showNotification('Download do contrato iniciado', 'success');
    } else {
        showNotification('ID da adoção não encontrado', 'error');
    }
}


function adjustButtonSizes() {
    const cardFooterActions = document.querySelectorAll('.card-footer-actions');
    
    cardFooterActions.forEach(container => {
        const buttons = container.querySelectorAll('.btn');
        const verDetalhesBtn = container.querySelector('.btn-ver-detalhes');
        
        buttons.forEach(btn => btn.classList.remove('btn-solo'));
        
        if (buttons.length === 1 && verDetalhesBtn) {
            verDetalhesBtn.classList.add('btn-solo');
        }
    });
}

function submitFormWithSearch(input) {
    const form = input.closest('form');
    if (form) {
        form.submit();
    }
}

function submitFormWithOrder(select) {
    const form = select.closest('form');
    if (form) {
        const selectedValue = select.value;
        
        let ordenarPorField = form.querySelector('input[name="ordenarPor"]');
        let direcaoOrdemField = form.querySelector('input[name="direcaoOrdem"]');
        
        if (!ordenarPorField) {
            ordenarPorField = document.createElement('input');
            ordenarPorField.type = 'hidden';
            ordenarPorField.name = 'ordenarPor';
            form.appendChild(ordenarPorField);
        }
        
        if (!direcaoOrdemField) {
            direcaoOrdemField = document.createElement('input');
            direcaoOrdemField.type = 'hidden';
            direcaoOrdemField.name = 'direcaoOrdem';
            form.appendChild(direcaoOrdemField);
        }
        
        switch (selectedValue) {
            case 'recent':
                ordenarPorField.value = 'DataEnvio';
                direcaoOrdemField.value = 'Desc';
                break;
            case 'oldest':
                ordenarPorField.value = 'DataEnvio';
                direcaoOrdemField.value = 'Asc';
                break;
            case 'name':
                ordenarPorField.value = 'PetNome';
                direcaoOrdemField.value = 'Asc';
                break;
            case 'status':
                ordenarPorField.value = 'Status';
                direcaoOrdemField.value = 'Asc';
                break;
            default:
                ordenarPorField.value = 'DataEnvio';
                direcaoOrdemField.value = 'Desc';
        }
        
        select.name = '';
        form.submit();
    }
}

