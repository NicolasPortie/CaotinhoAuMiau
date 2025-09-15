class PetExplorer {
    constructor() {
        this.currentFilters = new Map();
        this.carregando = false;
        this.debounceTimers = new Map();
        
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.setupInfiniteScroll();
        this.setupFilterOptimization();
        this.initializeLazyContent();
        this.handleUrlParameters();
        this.preloadAdoptionButtons();
    }

    setupEventListeners() {
        // Optimized filter form submission
        const filterForm = perfUtils.getElementById('formFiltros');
        if (filterForm) {
            filterForm.addEventListener('submit', (e) => {
                e.preventDefault();
                this.applyFilters();
            });
        }

        // Debounced search input
        const searchInput = perfUtils.getElementById('filtroNome');
        if (searchInput) {
            searchInput.addEventListener('input', (e) => {
                this.debounceFilter('search', () => {
                    this.applyFilters();
                }, 500);
            });
        }

        // Optimized filter change handlers
        ['filtroEspecie', 'filtroSexo', 'filtroPorte', 'filtroIdade', 'filtroOrdem'].forEach(filterId => {
            const filterElement = perfUtils.getElementById(filterId);
            if (filterElement) {
                filterElement.addEventListener('change', () => {
                    this.debounceFilter(filterId, () => {
                        this.applyFilters();
                    }, 300);
                });
            }
        });

        // Optimized items per page selector
        const itemsSelector = perfUtils.getElementById('selectItensPorPagina');
        if (itemsSelector) {
            itemsSelector.addEventListener('change', (e) => {
                this.changeItemsPerPage(parseInt(e.target.value, 10));
            });
        }

        // Optimized pagination with event delegation
        perfUtils.delegate(document.body, '.pagination .page-link', 'click', (e) => {
            e.preventDefault();
            const page = parseInt(e.target.dataset.pagina, 10);
            if (page) {
                this.navigateToPage(page);
            }
        });

        // Adoption button optimization
        perfUtils.delegate(document.body, '.adopt-button', 'click', (e) => {
            this.handleAdoptionClick(e);
        });

        const clearButton = perfUtils.getElementById('btnLimparFiltros');
        if (clearButton) {
            clearButton.addEventListener('click', (e) => {
                e.preventDefault();
                this.clearFilters();
            });
        }
    }

    debounceFilter(key, callback, delay = 300) {
        if (this.debounceTimers.has(key)) {
            clearTimeout(this.debounceTimers.get(key));
        }
        
        const timeoutId = setTimeout(() => {
            callback();
            this.debounceTimers.delete(key);
        }, delay);
        
        this.debounceTimers.set(key, timeoutId);
    }

    async applyFilters() {
        if (this.carregando) return;

        const filterForm = perfUtils.getElementById('formFiltros');
        if (!filterForm) return;

        this.carregando = true;

        try {
            this.showLoadingState();

            // Collect filter data
            const formData = new FormData(filterForm);
            const params = new URLSearchParams();
            
            for (const [key, value] of formData.entries()) {
                if (value && value.trim() !== '') {
                    params.append(key, value);
                    this.currentFilters.set(key, value);
                }
            }

            params.append('pagina', '1');
            params.append('fromForm', 'true');

            // Make request with timeout
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 10000);

            const response = await fetch(window.location.pathname + '?' + params.toString(), {
                method: 'GET',
                signal: controller.signal
            });

            clearTimeout(timeoutId);

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            // Parse the response
            const html = await response.text();
            this.updateContent(html);

        } catch (error) {
            
            if (error.name === 'AbortError') {
                toastr.warning('A busca demorou muito. Tente novamente.');
            } else {
                toastr.error('Erro ao aplicar filtros. Tente novamente.');
            }
        } finally {
            this.carregando = false;
            this.hideLoadingState();
        }
    }

    updateContent(html) {
        // Create temporary container
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = html;

        // Extract new content
        const newPetsGrid = tempDiv.querySelector('.pets-grid');
        const newPagination = tempDiv.querySelector('.paginacao');

        if (newPetsGrid) {
            const currentGrid = perfUtils.getElement('.pets-grid');
            if (currentGrid) {
                // Smooth transition
                perfUtils.animate(currentGrid, 'animate-fade-out').then(() => {
                    currentGrid.innerHTML = newPetsGrid.innerHTML;
                    
                    if (window.lazyLoader) {
                        window.lazyLoader.addImages(currentGrid);
                        window.lazyLoader.addContent(currentGrid);
                    }
                    
                    // Animate in
                    perfUtils.animate(currentGrid, 'animate-fade-in');
                });
            }
        }

        if (newPagination) {
            const currentPagination = perfUtils.getElement('.paginacao');
            if (currentPagination) {
                currentPagination.innerHTML = newPagination.innerHTML;
            }
        }

        this.updateUrl();
    }

    updateUrl() {
        const params = new URLSearchParams();
        
        this.currentFilters.forEach((value, key) => {
            if (value && value.trim() !== '') {
                params.set(key, value);
            }
        });

        const newUrl = window.location.pathname + (params.toString() ? '?' + params.toString() : '');
        window.history.replaceState({}, '', newUrl);
    }

    async navigateToPage(page) {
        if (this.carregando) return;

        const pageInput = perfUtils.getElementById('paginaAtual');
        if (pageInput) {
            pageInput.value = page;
        }

        const navForm = perfUtils.getElementById('formNavegacao');
        if (!navForm) return;

        this.carregando = true;

        try {
            this.showLoadingState();

            const formData = new FormData(navForm);
            formData.set('pagina', page.toString());
            formData.set('navegacaoPagina', 'true');

            const params = new URLSearchParams(formData);
            
            const response = await fetch(window.location.pathname + '?' + params.toString());
            if (!response.ok) throw new Error(`HTTP ${response.status}`);

            const html = await response.text();
            this.updateContent(html);

        } catch (error) {
            toastr.error('Erro ao navegar. Tente novamente.');
        } finally {
            this.carregando = false;
            this.hideLoadingState();
        }
    }

    async changeItemsPerPage(itemsPerPage) {
        if (this.carregando) return;

        const itemsInput = perfUtils.getElementById('itensPorPagina');
        const navItemsInput = perfUtils.getElementById('itensPorPaginaNav');
        
        if (itemsInput) itemsInput.value = itemsPerPage;
        if (navItemsInput) navItemsInput.value = itemsPerPage;

        // Reset to first page
        const pageInput = perfUtils.getElementById('paginaAtual');
        if (pageInput) pageInput.value = 1;

        this.currentFilters.set('itensPorPagina', itemsPerPage.toString());
        this.currentFilters.set('pagina', '1');

        await this.applyFilters();
    }

    clearFilters() {
        const filterForm = perfUtils.getElementById('formFiltros');
        if (filterForm) {
            const inputs = filterForm.querySelectorAll('input[type="text"], select');
            inputs.forEach(input => {
                if (input.name !== 'itensPorPagina') {
                    input.value = '';
                }
            });
        }

        this.currentFilters.clear();
        
        // Apply empty filters
        this.applyFilters();
    }

    handleAdoptionClick(e) {
        const button = e.target.closest('.adopt-button');
        if (!button) return;

        const userData = perfUtils.getElementById('dados-usuario');
        const isLoggedIn = userData?.dataset.usuarioLogado === 'true';
        const userType = userData?.dataset.usuarioTipo;

        if (!isLoggedIn) {
            e.preventDefault();
            this.showLoginModal();
            return;
        }

        if (userType === 'Administrador') {
            e.preventDefault();
            this.showAdminModal();
            return;
        }

        // Let the normal navigation proceed
    }


    showLoginModal() {
        perfUtils.showModal('login', 'modalLoginCadastro');
    }

    showAdminModal() {
        perfUtils.showModal('admin', 'modalAdminPrincipal');
    }


    showLoadingState() {
        const grid = perfUtils.getElement('.pets-grid');
        const pagination = perfUtils.getElement('.paginacao');
        
        if (grid) grid.classList.add('loading');
        if (pagination) pagination.classList.add('loading');

        this.showSkeletonLoading();
    }

    hideLoadingState() {
        const grid = perfUtils.getElement('.pets-grid');
        const pagination = perfUtils.getElement('.paginacao');
        
        if (grid) grid.classList.remove('loading');
        if (pagination) pagination.classList.remove('loading');

        this.hideSkeletonLoading();
    }

    showSkeletonLoading() {
        const grid = perfUtils.getElement('.pets-grid');
        if (!grid) return;

        const skeletonCount = 6;
        const skeletonHTML = Array.from({ length: skeletonCount }, () => `
            <div class="pet-card skeleton">
                <div class="image-container skeleton"></div>
                <div class="pet-info">
                    <div class="skeleton" style="height: 24px; margin-bottom: 10px;"></div>
                    <div class="skeleton" style="height: 16px; margin-bottom: 5px;"></div>
                    <div class="skeleton" style="height: 16px; margin-bottom: 5px;"></div>
                    <div class="skeleton" style="height: 16px;"></div>
                </div>
            </div>
        `).join('');

        const skeletonContainer = document.createElement('div');
        skeletonContainer.className = 'skeleton-container';
        skeletonContainer.innerHTML = skeletonHTML;
        skeletonContainer.style.display = 'none';

        grid.appendChild(skeletonContainer);

        requestAnimationFrame(() => {
            skeletonContainer.style.display = 'grid';
            skeletonContainer.style.gridTemplateColumns = 'repeat(auto-fill, minmax(300px, 1fr))';
            skeletonContainer.style.gap = '20px';
        });
    }

    hideSkeletonLoading() {
        const skeletonContainer = perfUtils.getElement('.skeleton-container');
        if (skeletonContainer) {
            perfUtils.animate(skeletonContainer, 'animate-fade-out').then(() => {
                skeletonContainer.remove();
            });
        }
    }

    setupInfiniteScroll() {
        // Optional: Implement infinite scroll for better UX
        let isNearBottom = false;
        
        const checkScroll = perfUtils.debounce('scroll', () => {
            const scrollPosition = window.innerHeight + window.scrollY;
            const documentHeight = document.documentElement.offsetHeight;
            
            isNearBottom = scrollPosition >= documentHeight - 1000;
            
            if (isNearBottom && !this.isLoading) {
            }
        }, 100);

        window.addEventListener('scroll', checkScroll);
    }

    setupFilterOptimization() {
        // Preload desabilitado para evitar consultas desnecessárias
    }

    async preloadFilterCombination(filter) {
        // Função desabilitada
        return;
    }

    initializeLazyContent() {
        if (window.lazyLoader) {
            const petCards = document.querySelectorAll('.pet-card[data-lazy-content]');
            petCards.forEach(card => {
                window.lazyLoader.observeContent(card);
            });
        }
    }

    preloadAdoptionButtons() {
        // Preload desabilitado para melhor performance
    }

    handleUrlParameters() {
        const urlParams = new URLSearchParams(window.location.search);
        
        urlParams.forEach((value, key) => {
            const element = perfUtils.getElementById(key);
            if (element) {
                element.value = value;
                this.currentFilters.set(key, value);
            }
        });
    }

    // Public method for external use
    refreshPets() {
        this.applyFilters();
    }

    // Cleanup method
    cleanup() {
        this.debounceTimers.forEach(timer => clearTimeout(timer));
        this.debounceTimers.clear();
        this.currentFilters.clear();
    }
}

// Função global para abrir modal de detalhes
function abrirModalDetalhes(petId) {
    if (!petId) return;

    const modal = document.getElementById('modalDetalhesPet');
    const modalBody = document.getElementById('modalDetalhesPetBody');
    const modalTitle = document.getElementById('modalDetalhesPetTitulo');
    const btnAdotar = document.getElementById('btnAdotarModal');

    if (!modal || !modalBody) return;

    if (btnAdotar) btnAdotar.style.display = 'none';
    if (modalTitle) modalTitle.textContent = 'Detalhes do Pet';

    // Show modal
    modal.style.display = 'flex';
    modal.classList.add('show');

    // Reset modal body to show loading
    modalBody.innerHTML = `
        <div class="loading-container">
            <div class="loading-spinner">
                <i class="fas fa-spinner fa-spin"></i>
            </div>
            <p class="loading-text">Carregando detalhes...</p>
        </div>
    `;

    // Fetch pet details
    fetch(`/api/pet/detalhes/${petId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Erro ao carregar detalhes');
            }
            return response.json();
        })
        .then(pet => {
            // Update modal title
            if (modalTitle) modalTitle.textContent = `Detalhes - ${pet.nome}`;

            // Build detailed content
            const detalhesHtml = `
                <div class="pet-detalhes-container">
                    <div class="pet-detalhes-imagem">
                        <img src="${pet.nomeArquivoImagem ? `/imagens/pets/${pet.nomeArquivoImagem}` : '/imagens/pets/pet-placeholder.jpg'}"
                             alt="${pet.nome}"
                             onerror="this.src='/imagens/pets/pet-placeholder.jpg'">
                        <div class="pet-tags-modal">
                            <span class="tag-especie ${pet.especie === 'Cao' ? 'cao' : 'felino'}">
                                <i class="fas ${pet.especie === 'Cao' ? 'fa-dog' : 'fa-cat'}"></i>
                                ${pet.especie === 'Cao' ? 'Cão' : 'Felino'}
                            </span>
                            <span class="tag-sexo ${pet.sexo === 'Macho' ? 'macho' : 'femea'}">
                                <i class="fas ${pet.sexo === 'Macho' ? 'fa-mars' : 'fa-venus'}"></i>
                                ${pet.sexo}
                            </span>
                        </div>
                    </div>
                    <div class="pet-detalhes-info">
                        <h3>${pet.nome}</h3>
                        <div class="pet-info-grid">
                            <div class="info-item">
                                <i class="fas fa-paw"></i>
                                <span><strong>Porte:</strong> ${pet.porte || 'Não informado'}</span>
                            </div>
                            <div class="info-item">
                                <i class="fas fa-birthday-cake"></i>
                                <span><strong>Idade:</strong> ${formatarIdade(pet.anos, pet.meses)}</span>
                            </div>
                            <div class="info-item">
                                <i class="fas fa-tag"></i>
                                <span><strong>Raça:</strong> ${pet.raca || 'SRD'}</span>
                            </div>
                        </div>
                        ${pet.descricao ? `
                            <div class="pet-descricao-completa">
                                <h4><i class="fas fa-info-circle"></i> Sobre ${pet.nome}</h4>
                                <p>${pet.descricao}</p>
                            </div>
                        ` : ''}

                    </div>
                </div>
            `;

            modalBody.innerHTML = detalhesHtml;

            // Always show adoption button, but handle authentication
            const userData = document.getElementById('dados-usuario');
            const isLoggedIn = userData?.dataset.usuarioLogado === 'true';
            const userType = userData?.dataset.usuarioTipo;

            if (btnAdotar && userType !== 'Administrador') {
                btnAdotar.style.display = 'inline-flex';
                btnAdotar.onclick = () => {
                    if (isLoggedIn) {
                        // User is logged in, proceed to adoption form
                        window.location.href = `/usuario/adocao/formulario/${petId}`;
                    } else {
                        // User is not logged in, show authentication modal
                        fecharModal('modalDetalhesPet');
                        mostrarModalAutenticacao();
                    }
                };
            }
        })
        .catch(error => {
            modalBody.innerHTML = `
                <div class="detalhes-erro">
                    <i class="fas fa-exclamation-triangle"></i>
                    <p>Erro ao carregar detalhes do pet. Tente novamente.</p>
                    <button class="btn-retry" onclick="abrirModalDetalhes(${petId})">
                        <i class="fas fa-redo"></i>
                        Tentar novamente
                    </button>
                </div>
            `;
        });
}

// Função para mostrar modal de autenticação
function mostrarModalAutenticacao() {
    const modal = new bootstrap.Modal(document.getElementById('modalAutenticacao'));
    modal.show();
}

// Função para lidar com clique no botão Adotar
function handleAdoptClick(petId) {
    const userData = document.getElementById('dados-usuario');
    const isLoggedIn = userData?.dataset.usuarioLogado === 'true';
    const userType = userData?.dataset.usuarioTipo;

    if (!isLoggedIn) {
        // Usuário não logado - mostrar modal de autenticação
        mostrarModalAutenticacao();
        return;
    }

    if (userType === 'Administrador') {
        // Administrador não pode adotar - já tratado em outro lugar
        return;
    }

    // Usuário logado e não administrador - redirecionar para formulário de adoção
    window.location.href = `/usuario/adocao/formulario/${petId}`;
}

// Função auxiliar para formatar idade
function formatarIdade(anos, meses) {
    if (anos > 0) {
        let resultado = `${anos} ${anos === 1 ? 'ano' : 'anos'}`;
        if (meses > 0) {
            resultado += ` e ${meses} ${meses === 1 ? 'mês' : 'meses'}`;
        }
        return resultado;
    } else if (meses > 0) {
        return `${meses} ${meses === 1 ? 'mês' : 'meses'}`;
    } else {
        return 'Recém-nascido';
    }
}

// Função global para fechar modal
function fecharModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.remove('show');
        setTimeout(() => {
            modal.style.display = 'none';
        }, 200); // Aguarda animação terminar
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    // Ensure performance utils are loaded
    if (typeof perfUtils !== 'undefined') {
        window.petExplorer = new PetExplorer();
    } else {
        // Fallback initialization
        setTimeout(() => {
            if (typeof perfUtils !== 'undefined') {
                window.petExplorer = new PetExplorer();
            }
        }, 100);
    }
});

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    if (window.petExplorer) {
        window.petExplorer.cleanup();
    }
});

// Handle browser back/forward buttons
window.addEventListener('popstate', () => {
    if (window.petExplorer) {
        window.petExplorer.handleUrlParameters();
        window.petExplorer.refreshPets();
    }
});