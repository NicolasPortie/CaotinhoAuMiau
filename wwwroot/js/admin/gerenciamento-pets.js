class PetManagementMVC {
    constructor() {
        this.debounceTimers = new Map();
        this.modalInstances = new Map();
        this.init();
    }

    init() {
        this.setupToastr();
        this.setupEventListeners();
        this.setupFormValidation();
        this.initializeLazyLoading();
        this.verificarParametrosURL();
        this.mostrarNotificacoesTempData();
    }

    setupToastr() {
        if (typeof toastr !== 'undefined') {
            toastr.options = {
                "closeButton": true,
                "progressBar": true,
                "positionClass": "toast-top-right",
                "timeOut": "5000",
                "preventDuplicates": true
            };
        }
    }

    setupEventListeners() {
        document.querySelectorAll('.btn-editar').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const petId = btn.dataset.petId || btn.closest('[data-pet-id]')?.dataset.petId;
                if (petId) {
                    this.abrirModalEdicao(petId);
                }
            });
        });

        document.querySelectorAll('.btn-visualizar').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const petId = btn.dataset.petId || btn.closest('[data-pet-id]')?.dataset.petId;
                if (petId) {
                    this.visualizarPet(petId);
                }
            });
        });

        document.querySelectorAll('.btn-excluir').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const petId = btn.dataset.petId || btn.closest('[data-pet-id]')?.dataset.petId;
                if (petId) {
                    this.confirmarExclusao(petId);
                }
            });
        });

        const formPet = document.getElementById('form-pet');
        if (formPet) {
            formPet.addEventListener('submit', (e) => {
                if (!this.validatePetForm(formPet).isValid) {
                    e.preventDefault();
                    return false;
                }
                return true;
            });
        }

        this.setupImageUpload();
        
        this.setupCharacterCounters();
        
        this.setupAgeValidation();
        
        this.setupSalvarRascunho();
    }

    mostrarNotificacoesTempData() {
        const sucessoMsg = document.querySelector('[data-tempdata-sucesso]')?.textContent;
        const erroMsg = document.querySelector('[data-tempdata-erro]')?.textContent;
        
        if (sucessoMsg && typeof toastr !== 'undefined') {
            toastr.success(sucessoMsg);
        }
        
        if (erroMsg && typeof toastr !== 'undefined') {
            toastr.error(erroMsg);
        }
    }

    visualizarPet(petId) {
        try {
            const petData = this.obterDadosPetDoCard(petId);
            if (!petData) {
                return;
            }

            this.preencherModalDetalhes(petData);
            this.showModal('modal-pet-details');

        } catch (error) {
            if (typeof toastr !== 'undefined') {
                toastr.error('Erro ao carregar detalhes do pet.');
            }
        }
    }

    abrirModalEdicao(petId) {
        try {
            const petData = this.obterDadosPetDoCard(petId);
            if (!petData) return;

            if (this.isPetNonEditable(petData.Status || petData.status)) {
                const statusTexto = (petData.Status || petData.status) === 'EmProcesso' ? 'em processo de adoção' : 'já adotado';
                if (typeof toastr !== 'undefined') {
                    toastr.warning(`Não é possível editar um pet ${statusTexto}.`);
                }
                return;
            }

            this.preencherFormularioEdicao(petData);
            this.showModal('modal-pet');

        } catch (error) {
            if (typeof toastr !== 'undefined') {
                toastr.error('Erro ao carregar dados do pet para edição.');
            }
        }
    }

    isPetNonEditable(status) {
        const nonEditableStatuses = ['em processo', 'adotado', 'emprocesso', 'finalizado'];
        return nonEditableStatuses.includes(String(status).toLowerCase().replace(/\s+/g, ''));
    }

    preencherFormularioEdicao(petData) {
        // Mapear enum Especie para valores do select
        let especieValue = '';
        const especieEnum = petData.Especie !== undefined ? petData.Especie : petData.especie;
        
        // Mapear valores do banco para valores do select
        if (especieEnum === 'Cao' || especieEnum === 0) {
            especieValue = 'Cao';
        } else if (especieEnum === 'Felino' || especieEnum === 1) {
            especieValue = 'Felino';
        }

        const formElements = {
            'petId': petData.Id || petData.id,
            'nomePet': petData.Nome || petData.nome,
            'especiePet': especieValue,
            'racaPet': petData.Raca || petData.raca,
            'sexoPet': petData.Sexo || petData.sexo,
            'portePet': petData.Porte || petData.porte,
            'anosPet': petData.Anos || petData.anos || 0,
            'mesesPet': petData.Meses || petData.meses || 0,
            'descricaoPet': petData.Descricao || petData.descricao || '',
            'statusPet': petData.Status || petData.status || 'Disponível'
        };

        Object.entries(formElements).forEach(([id, value]) => {
            const element = document.getElementById(id);
            if (element) {
                element.value = value;
            }
        });

        const titulo = document.getElementById('tituloCadastroEdicao');
        if (titulo) {
            titulo.textContent = 'Editar Pet';
        }

        const nomeImagem = petData.NomeArquivoImagem || petData.nomeArquivoImagem;
        if (nomeImagem) {
            this.preencherImagemPreview(nomeImagem);
        }
    }

    preencherImagemPreview(nomeImagem) {
        const previewImg = document.getElementById('previewImagem');
        const removeBtn = document.getElementById('btnRemoverImagem');
        const dropMessage = document.querySelector('.mensagem-soltar');

        if (previewImg && nomeImagem) {
            previewImg.src = `/imagens/pets/${nomeImagem}`;
            previewImg.style.display = 'block';
            if (removeBtn) removeBtn.style.display = 'block';
            if (dropMessage) dropMessage.style.display = 'none';
        }
    }

    obterDadosPetDoCard(petId) {
        try {
            const petCard = document.querySelector(`.cartao-pet[data-id="${petId}"]`);
            if (!petCard) {
                return null;
            }

            const jsonData = petCard.getAttribute('data-json');
            if (!jsonData) {
                return null;
            }

            const decodedData = this.decodificarHtml(jsonData);
            const petData = JSON.parse(decodedData);
            
            return petData;
            
        } catch (error) {
            return null;
        }
    }

    decodificarHtml(texto) {
        if (!texto) return '';
        try {
            const textarea = document.createElement('textarea');
            textarea.innerHTML = texto;
            const decoded = textarea.value;
            
            if (!decoded || decoded === texto) {
                const parser = new DOMParser();
                const doc = parser.parseFromString(texto, 'text/html');
                return doc.documentElement.textContent || doc.body.textContent || texto;
            }
            
            return decoded;
        } catch (error) {
            return texto;
        }
    }

    preencherModalDetalhes(petData) {
        const detailsElements = {
            'nomeDetalhes': petData.Nome || petData.nome,
            'especieDetalhes': petData.EspecieTexto || 'Não informado', // Agora vem do servidor
            'especieDetalhesInfo': petData.EspecieTexto || 'Não informado',
            'racaDetalhes': petData.Raca || petData.raca,
            'racaDetalhesInfo': petData.Raca || petData.raca,
            'sexoDetalhesInfo': petData.Sexo || petData.sexo,
            'porteDetalhesInfo': petData.Porte || petData.porte,
            'idadeDetalhes': this.formatarIdade(petData),
            'idadeDetalhesInfo': this.formatarIdade(petData),
            'descricaoDetalhes': petData.Descricao || petData.descricao || 'Sem descrição',
            'statusDetalhesInfo': petData.StatusTexto || 'Não informado', // Agora vem do servidor
            'dataCadastroInfo': this.formatarData(petData.DataCriacao || petData.dataCriacao),
            'dataAtualizacaoInfo': this.formatarData(petData.DataAtualizacao || petData.dataAtualizacao)
        };

        Object.entries(detailsElements).forEach(([id, value]) => {
            const element = document.getElementById(id);
            if (element) {
                element.textContent = value;
            }
        });

        const imagemDetalhes = document.getElementById('imgPetDetalhes');
        const nomeImagem = petData.NomeArquivoImagem || petData.nomeArquivoImagem;
        if (imagemDetalhes) {
            imagemDetalhes.src = nomeImagem 
                ? `/imagens/pets/${nomeImagem}` 
                : '/imagens/pets/pet-placeholder.jpg';
            imagemDetalhes.alt = `Foto de ${detailsElements.nomeDetalhes}`;
        }

        const statusBadge = document.getElementById('pet-status-badge');
        if (statusBadge) {
            statusBadge.textContent = petData.StatusTexto || 'Não informado'; // Agora vem do servidor
            statusBadge.className = `pet-status-badge ${petData.StatusCssClass || ''}`;
        }

        const btnEditarPet = document.querySelector('.btnEditarPet');
        if (btnEditarPet) {
            btnEditarPet.onclick = () => {
                this.hideModal('modal-pet-details');
                setTimeout(() => {
                    window.location.href = `/admin/pets/editar/${petData.Id || petData.id}`;
                }, 300);
            };
        }
    }

    formatarIdade(petData) {
        const anos = petData.Anos || petData.anos || 0;
        const meses = petData.Meses || petData.meses || 0;
        
        if (anos > 0 && meses > 0) {
            return `${anos} ano${anos > 1 ? 's' : ''} e ${meses} mês${meses > 1 ? 'es' : ''}`;
        } else if (anos > 0) {
            return `${anos} ano${anos > 1 ? 's' : ''}`;
        } else if (meses > 0) {
            return `${meses} mês${meses > 1 ? 'es' : ''}`;
        }
        return 'Recém-nascido';
    }

    formatarData(data) {
        if (!data) return 'Não informado';
        try {
            const date = new Date(data);
            return date.toLocaleDateString('pt-BR');
        } catch {
            return data;
        }
    }


    confirmarExclusao(petId) {
        try {
            const petData = this.obterDadosPetDoCard(petId);
            if (!petData) return;

            const nomePetExclusao = document.getElementById('nomePetExclusao');
            const petIdParaExcluir = document.getElementById('petIdParaExcluir');
            
            if (nomePetExclusao) {
                nomePetExclusao.textContent = petData.Nome || petData.nome;
            }
            
            if (petIdParaExcluir) {
                petIdParaExcluir.value = petId;
            }

            const confirmBtn = document.getElementById('btnConfirmarExclusao');
            if (confirmBtn) {
                confirmBtn.onclick = () => this.excluirPetMVC(petId);
            }
            
            this.showModal('modal-confirm-delete');
        } catch (error) {
            if (typeof toastr !== 'undefined') {
                toastr.error('Erro ao carregar dados do pet para exclusão.');
            }
        }
    }

    excluirPetMVC(petId) {
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = `/admin/pets/excluir/${petId}`;
        
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (token) {
            const tokenInput = document.createElement('input');
            tokenInput.type = 'hidden';
            tokenInput.name = '__RequestVerificationToken';
            tokenInput.value = token.value;
            form.appendChild(tokenInput);
        }
        
        document.body.appendChild(form);
        form.submit();
    }

    setupImageUpload() {
        const dropZone = document.querySelector('.area-soltar-imagem');
        const fileInput = document.getElementById('fotoPet');
        
        if (!dropZone || !fileInput) return;

        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            dropZone.addEventListener(eventName, this.preventDefaults, false);
        });

        ['dragenter', 'dragover'].forEach(eventName => {
            dropZone.addEventListener(eventName, () => {
                dropZone.classList.add('dragover');
            }, false);
        });

        ['dragleave', 'drop'].forEach(eventName => {
            dropZone.addEventListener(eventName, () => {
                dropZone.classList.remove('dragover');
            }, false);
        });

        dropZone.addEventListener('drop', (e) => {
            const files = e.dataTransfer.files;
            this.handleFiles(files);
        }, false);

        dropZone.addEventListener('click', () => {
            fileInput.click();
        });

        fileInput.addEventListener('change', (e) => {
            this.handleFiles(e.target.files);
        });

        const btnRemover = document.getElementById('btnRemoverImagem');
        if (btnRemover) {
            btnRemover.addEventListener('click', () => {
                this.removerImagemPreview();
            });
        }
    }

    preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }

    handleFiles(files) {
        if (files.length > 0) {
            this.previewImage(files[0]);
        }
    }

    async previewImage(file) {
        if (!file.type.startsWith('image/')) {
            if (typeof toastr !== 'undefined') {
                toastr.error('Por favor, selecione apenas arquivos de imagem.');
            }
            return;
        }

        const maxSize = 5 * 1024 * 1024; // 5MB
        if (file.size > maxSize) {
            if (typeof toastr !== 'undefined') {
                toastr.error('A imagem deve ter no máximo 5MB.');
            }
            return;
        }

        const previewImg = document.getElementById('previewImagem');
        const removeBtn = document.getElementById('btnRemoverImagem');
        const dropMessage = document.querySelector('.mensagem-soltar');

        try {
            const imageUrl = await this.createImageUrl(file);
            
            previewImg.src = imageUrl;
            previewImg.style.display = 'block';
            removeBtn.style.display = 'block';
            dropMessage.style.display = 'none';

        } catch (error) {
            if (typeof toastr !== 'undefined') {
                toastr.error('Erro ao processar a imagem. Tente novamente.');
            }
        }
    }

    createImageUrl(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = e => resolve(e.target.result);
            reader.onerror = reject;
            reader.readAsDataURL(file);
        });
    }

    removerImagemPreview() {
        const previewImg = document.getElementById('previewImagem');
        const removeBtn = document.getElementById('btnRemoverImagem');
        const dropMessage = document.querySelector('.mensagem-soltar');
        const fileInput = document.getElementById('fotoPet');

        previewImg.style.display = 'none';
        removeBtn.style.display = 'none';
        dropMessage.style.display = 'block';
        fileInput.value = '';
    }

    setupCharacterCounters() {
        const textareas = document.querySelectorAll('textarea[maxlength]');
        textareas.forEach(textarea => {
            const maxLength = textarea.getAttribute('maxlength');
            const counter = document.getElementById('contadorCaracteres');
            
            if (counter) {
                textarea.addEventListener('input', () => {
                    const count = textarea.value.length;
                    counter.textContent = count;
                    
                    if (count > maxLength * 0.9) {
                        counter.classList.add('warning');
                    } else {
                        counter.classList.remove('warning');
                    }
                });
            }
        });
    }

    setupAgeValidation() {
        const anosInput = document.getElementById('anosPet');
        const mesesInput = document.getElementById('mesesPet');
        const idadeTotal = document.getElementById('idadeTotal');

        if (anosInput && mesesInput && idadeTotal) {
            const updateIdadeTotal = () => {
                const anos = parseInt(anosInput.value) || 0;
                const meses = parseInt(mesesInput.value) || 0;
                const textoAnos = anos === 1 ? 'ano' : 'anos';
                const textoMeses = meses === 1 ? 'mês' : 'meses';
                idadeTotal.innerHTML = `<i class="fas fa-info-circle me-1"></i>Idade total: ${anos} ${textoAnos} e ${meses} ${textoMeses}`;
            };

            anosInput.addEventListener('input', updateIdadeTotal);
            mesesInput.addEventListener('input', updateIdadeTotal);
        }
    }

    setupSalvarRascunho() {
        const btnSalvarPet = document.getElementById('btnSalvarPet');
        if (btnSalvarPet) {
            btnSalvarPet.addEventListener('click', (e) => {
                e.preventDefault();
                this.salvarPet();
            });
        }

        const btnSalvarRascunho = document.getElementById('btnSalvarRascunho');
        if (btnSalvarRascunho) {
            btnSalvarRascunho.addEventListener('click', (e) => {
                e.preventDefault();
                this.salvarRascunho();
            });
        }
    }

    salvarPet() {
        const form = document.getElementById('form-pet');
        if (!form) {
            if (typeof toastr !== 'undefined') {
                toastr.error('Formulário não encontrado');
            }
            return;
        }

        // Validar formulário completo (TODOS os campos obrigatórios)
        const validationResult = this.validatePetForm(form);
        if (!validationResult.isValid) {
            // Mostrar mensagens específicas de erro
            const errorMessages = [];
            if (validationResult.errors.nome) errorMessages.push('• Nome do pet');
            if (validationResult.errors.especie) errorMessages.push('• Espécie');
            if (validationResult.errors.raca) errorMessages.push('• Raça');
            if (validationResult.errors.sexo) errorMessages.push('• Sexo');
            if (validationResult.errors.porte) errorMessages.push('• Porte');
            if (validationResult.errors.descricao) errorMessages.push('• Descrição');
            if (validationResult.errors.imagem) errorMessages.push('• Imagem do pet');
            
            const errorText = errorMessages.length > 0 
                ? `Preencha os campos obrigatórios:\n${errorMessages.join('\n')}`
                : 'Por favor, corrija os erros no formulário antes de salvar';
            
            if (typeof toastr !== 'undefined') {
                toastr.error(errorText);
            }
            return;
        }

        const statusInput = document.getElementById('statusPet');
        if (statusInput) {
            statusInput.value = 'Disponivel';
        }

        const cadastroCompletoInput = document.getElementById('cadastroCompleto');
        if (cadastroCompletoInput) {
            cadastroCompletoInput.value = 'true';
        }

        form.submit();
    }

    salvarRascunho() {
        const form = document.getElementById('form-pet');
        if (!form) {
            if (typeof toastr !== 'undefined') {
                toastr.error('Formulário não encontrado');
            }
            return;
        }

        // Validar apenas nome para rascunho
        const nomePet = form.querySelector('[name="Nome"], #nomePet');
        if (!nomePet || !nomePet.value.trim()) {
            if (typeof toastr !== 'undefined') {
                toastr.warning('Nome do pet é obrigatório para salvar como rascunho');
            }
            nomePet?.focus();
            return;
        }

        // Usar AJAX para não fechar modal
        const formData = new FormData(form);
        
        fetch('/admin/pets/SalvarRascunho', {
            method: 'POST',
            body: formData
        })
        .then(response => response.json())
        .then(data => {
            if (data.sucesso) {
                if (typeof toastr !== 'undefined') {
                    toastr.success(data.mensagem);
                }
                // Limpar formulário após salvar rascunho
                this.limparFormulario();
            } else {
                if (typeof toastr !== 'undefined') {
                    toastr.error(data.mensagem);
                }
            }
        })
        .catch(error => {
            if (typeof toastr !== 'undefined') {
                toastr.error('Erro ao salvar rascunho');
            }
        });
    }

    limparFormulario() {
        const form = document.getElementById('form-pet');
        if (form) {
            // Resetar todos os campos
            form.reset();
            
            // Limpar imagem preview
            const previewImg = document.getElementById('previewImagem');
            const removeBtn = document.getElementById('btnRemoverImagem');
            const dropMessage = document.querySelector('.mensagem-soltar');
            
            if (previewImg) previewImg.style.display = 'none';
            if (removeBtn) removeBtn.style.display = 'none';
            if (dropMessage) dropMessage.style.display = 'block';
            
            // Remover classes de validação
            form.querySelectorAll('.is-invalid, .is-valid').forEach(el => {
                el.classList.remove('is-invalid', 'is-valid');
            });
        }
    }

    validatePetForm(form) {
        const nomePetField = form.querySelector('[name="Nome"], #nomePet');
        let isValid = true;
        let errors = {};
        
        // Validar nome do pet (obrigatório)
        if (nomePetField && !nomePetField.value.trim()) {
            nomePetField.classList.add('is-invalid');
            errors.nome = 'Nome do pet é obrigatório';
            isValid = false;
        } else if (nomePetField) {
            nomePetField.classList.remove('is-invalid');
        }
        
        // Validar outros campos obrigatórios para cadastro completo
        const especieField = form.querySelector('[name="Especie"], #especiePet');
        if (especieField && !especieField.value.trim()) {
            especieField.classList.add('is-invalid');
            errors.especie = 'Espécie é obrigatória';
            isValid = false;
        } else if (especieField) {
            especieField.classList.remove('is-invalid');
        }
        
        const racaField = form.querySelector('[name="Raca"], #racaPet');
        if (racaField && !racaField.value.trim()) {
            racaField.classList.add('is-invalid');
            errors.raca = 'Raça é obrigatória';
            isValid = false;
        } else if (racaField) {
            racaField.classList.remove('is-invalid');
        }
        
        const sexoField = form.querySelector('[name="Sexo"], #sexoPet');
        if (sexoField && !sexoField.value.trim()) {
            sexoField.classList.add('is-invalid');
            errors.sexo = 'Sexo é obrigatório';
            isValid = false;
        } else if (sexoField) {
            sexoField.classList.remove('is-invalid');
        }
        
        const porteField = form.querySelector('[name="Porte"], #portePet');
        if (porteField && !porteField.value.trim()) {
            porteField.classList.add('is-invalid');
            errors.porte = 'Porte é obrigatório';
            isValid = false;
        } else if (porteField) {
            porteField.classList.remove('is-invalid');
        }

        // Validar descrição obrigatória
        const descricaoField = form.querySelector('[name="Descricao"], #descricaoPet');
        if (descricaoField && !descricaoField.value.trim()) {
            descricaoField.classList.add('is-invalid');
            errors.descricao = 'Descrição é obrigatória';
            isValid = false;
        } else if (descricaoField) {
            descricaoField.classList.remove('is-invalid');
        }

        // Validar imagem obrigatória
        const previewImg = document.getElementById('previewImagem');
        const fileInput = document.getElementById('fotoPet');
        const dropArea = document.querySelector('.area-soltar-imagem');
        const imagemError = dropArea?.parentElement?.querySelector('.mensagem-erro');
        
        // Verificar se há uma imagem selecionada (preview visível) ou arquivo selecionado
        const hasImage = (previewImg && previewImg.style.display !== 'none') || 
                         (fileInput && fileInput.files && fileInput.files.length > 0);
        
        if (!hasImage) {
            if (dropArea) {
                dropArea.classList.add('is-invalid');
            }
            if (imagemError) {
                imagemError.style.display = 'block';
            }
            errors.imagem = 'Imagem do pet é obrigatória';
            isValid = false;
        } else {
            if (dropArea) {
                dropArea.classList.remove('is-invalid');
            }
            if (imagemError) {
                imagemError.style.display = 'none';
            }
        }

        return { isValid, errors };
    }

    setupFormValidation() {
        const requiredFields = ['nomePet'];
        
        requiredFields.forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                field.addEventListener('blur', () => {
                    this.validateField(field);
                });
                
                field.addEventListener('input', () => {
                    if (field.classList.contains('is-invalid')) {
                        this.validateField(field);
                    }
                });
            }
        });
    }

    validateField(field) {
        const value = field.value.trim();
        const isValid = value.length > 0;
        
        field.classList.toggle('is-invalid', !isValid);
        field.classList.toggle('is-valid', isValid);
    }

    showModal(modalId) {
        try {
            const modal = document.getElementById(modalId);
            if (!modal) {
                return;
            }

            if (typeof bootstrap !== 'undefined') {
                setTimeout(() => {
                    try {
                        const bsModal = new bootstrap.Modal(modal);
                        bsModal.show();
                    } catch (bootstrapError) {
                        modal.classList.add('show');
                        modal.style.display = 'block';
                    }
                }, 10);
            } else {
                modal.classList.add('show');
                modal.style.display = 'block';
            }
        } catch (error) {
        }
    }

    hideModal(modalId) {
        const modal = document.getElementById(modalId);
        if (modal && typeof bootstrap !== 'undefined') {
            const bsModal = bootstrap.Modal.getInstance(modal);
            if (bsModal) {
                bsModal.hide();
            }
        }
    }

    initializeLazyLoading() {
        const images = document.querySelectorAll('img[loading="lazy"]');
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries, observer) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        if (img.dataset.src) {
                            img.src = img.dataset.src;
                            img.removeAttribute('data-src');
                        }
                        imageObserver.unobserve(img);
                    }
                });
            });

            images.forEach(img => imageObserver.observe(img));
        }
    }

    verificarParametrosURL() {
        const urlParams = new URLSearchParams(window.location.search);
        const petId = urlParams.get('visualizarPet');

        if (petId) {
            setTimeout(() => {
                this.visualizarPet(petId);
            }, 500);
        }
    }

    // === UTILITÁRIOS DE PERFORMANCE (movidos de performance-utils.js) ===

    // Função de debounce
    debounce(key, func, delay) {
        if (this.debounceTimers.has(key)) {
            clearTimeout(this.debounceTimers.get(key));
        }

        const timer = setTimeout(() => {
            func();
            this.debounceTimers.delete(key);
        }, delay);

        this.debounceTimers.set(key, timer);
    }

    // Loading states para botões
    withLoading(button, asyncFunction) {
        return new Promise(async (resolve, reject) => {
            if (!button) {
                try {
                    const result = await asyncFunction();
                    resolve(result);
                } catch (error) {
                    reject(error);
                }
                return;
            }

            const originalText = button.innerHTML;
            const originalDisabled = button.disabled;

            // Set loading state
            button.disabled = true;
            button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Carregando...';

            try {
                const result = await asyncFunction();
                resolve(result);
            } catch (error) {
                reject(error);
            } finally {
                // Restore original state
                button.disabled = originalDisabled;
                button.innerHTML = originalText;
            }
        });
    }

    // Cleanup ao destruir a classe
    cleanup() {
        this.debounceTimers.forEach(timer => clearTimeout(timer));
        this.debounceTimers.clear();
        this.modalInstances.clear();
    }
}

function resetarFormulario() {
    const form = document.getElementById('form-pet');
    if (form) {
        form.reset();
        document.getElementById('tituloCadastroEdicao').textContent = 'Novo Pet';
        document.getElementById('petId').value = '0';
        
        const previewImg = document.getElementById('previewImagem');
        const removeBtn = document.getElementById('btnRemoverImagem');
        const dropMessage = document.querySelector('.mensagem-soltar');
        
        if (previewImg) previewImg.style.display = 'none';
        if (removeBtn) removeBtn.style.display = 'none';
        if (dropMessage) dropMessage.style.display = 'block';
    }
}

document.addEventListener('DOMContentLoaded', () => {
    window.petManagementMVC = new PetManagementMVC();
});

window.addEventListener('beforeunload', () => {
    if (window.petManagementMVC) {
        window.petManagementMVC.cleanup();
    }
});