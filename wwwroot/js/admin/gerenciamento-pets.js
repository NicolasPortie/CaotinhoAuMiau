let modalPet;
let modalDetalhesPet;
let modalConfirmacaoExclusao;


function limparBackdrops() {
    document.querySelectorAll('.modal-backdrop').forEach(backdrop => {
        backdrop.remove();
    });
    document.body.classList.remove('modal-open');
    document.body.style.overflow = '';
    document.body.style.paddingRight = '';
}

function verificarParametrosURL() {
    const urlParams = new URLSearchParams(window.location.search);
    
    const petId = urlParams.get('visualizarPet');
    if (petId) {
        
        setTimeout(() => {
            visualizarPet(petId);
        }, 500);
    }
}


toastr.options = {
    "closeButton": true,
    "progressBar": true,
    "positionClass": "toast-top-right",
    "timeOut": "5000"
};


document.addEventListener('DOMContentLoaded', function() {
    
    // Inicializar modais
    try {
        const modalPetEl = document.getElementById('modalPet');
        const modalDetalhesEl = document.getElementById('modalDetalhesPet');
        const modalConfirmacaoEl = document.getElementById('modalConfirmacaoExclusao');
        
        if (modalPetEl) {
            modalPet = new bootstrap.Modal(modalPetEl);
        }
        
        if (modalDetalhesEl) {
            modalDetalhesPet = new bootstrap.Modal(modalDetalhesEl);
        }
        
        if (modalConfirmacaoEl) {
            modalConfirmacaoExclusao = new bootstrap.Modal(modalConfirmacaoEl);
        }
    } catch (error) {
        console.error('Erro ao inicializar modais:', error);
    }
    
    // Limpar backdrops desnecessários
    limparBackdrops();
    
    // Verificar se Bootstrap está disponível
    if (typeof bootstrap === 'undefined') {
        console.error('Bootstrap não está carregado. Os modais podem não funcionar.');
        toastr.error('Erro ao carregar recursos da página. Por favor, atualize a página.');
    }
    
    // Inicializar componentes de interface
    preCarregarImagens();
    configurarTratamentoErroImagens();
    garantirCarregamentoImagens();
    inicializarUploadImagem();
    
    // Configurar tooltips e modais
    configurarTooltipsBotoesDesabilitados();
    configurarComportamentoModais();
    
    // Adicionar manipuladores de eventos
    inicializarManipuladoresEventos();
    
    // Verificar parâmetros de URL
    verificarParametrosURL();
    
    // Inicializar validação de nome duplicado
    inicializarValidacaoNomeDuplicado();
});

// Função para gerenciar exibição e comportamento do seletor de itens por página
function gerenciarItensPorPagina() {
    // Forçar exibição dos elementos de paginação
    $('.paginacao-container').css('display', 'block');
    $('.paginacao-info').css('display', 'flex');
    $('.itens-por-pagina').css('display', 'flex');
    
    // Carregar preferências do usuário
    const urlParams = new URLSearchParams(window.location.search);
    const itensPorPaginaUrl = urlParams.get('itensPorPagina');
    const itensPorPaginaLocalStorage = localStorage.getItem('adminPetsItensPorPagina');
    
    if (itensPorPaginaUrl) {
        $('#selectItensPorPagina').val(itensPorPaginaUrl);
    } else if (itensPorPaginaLocalStorage) {
        $('#selectItensPorPagina').val(itensPorPaginaLocalStorage);
    }
    
    // Atualizar o atributo data-itens do grid
    const itensPorPagina = $('#selectItensPorPagina').val();
    $('#listaPets').attr('data-itens', itensPorPagina);
    
    // Configurar o manipulador de eventos para o seletor de itens por página
    $('#selectItensPorPagina').off('change').on('change', function() {
        const itensPorPagina = $(this).val();
        const urlAtual = new URL(window.location.href);
        
        // Salvar a preferência no localStorage
        localStorage.setItem('adminPetsItensPorPagina', itensPorPagina);
        
        // Atualizar a URL e redirecionar
        urlAtual.searchParams.set('itensPorPagina', itensPorPagina);
        urlAtual.searchParams.set('pagina', 1); // Volta para a primeira página
        
        // Mostrar indicador de carregamento
        mostrarIndicadorCarregamento();
        
        // Fazer cópia das imagens atuais para restaurá-las após o carregamento
        const imagensAtuais = {};
        document.querySelectorAll('.imagem-pet').forEach(img => {
            if (img.src && img.complete && !img.naturalWidth == 0) {
                const cardId = img.closest('.cartao-pet')?.getAttribute('data-id');
                if (cardId) {
                    imagensAtuais[cardId] = img.src;
                }
            }
        });
        
        // Salvar imagens atuais no sessionStorage
        sessionStorage.setItem('imagensPets', JSON.stringify(imagensAtuais));
        
        // Aplicar classe de transição aos cards
        $('.cartao-pet').addClass('mudando-layout');
        
        // Redirecionar após um pequeno delay para mostrar o indicador de carregamento
        setTimeout(() => {
            window.location.href = urlAtual.toString();
        }, 300);
    });
}

// Função para restaurar imagens pré-carregadas
function restaurarImagensPreCarregadas() {
    try {
        // Verificar se temos imagens salvas no sessionStorage
        const imagensSalvas = sessionStorage.getItem('imagensPets');
        if (!imagensSalvas) return;
        
        const imagensAtuais = JSON.parse(imagensSalvas);
        
        // Para cada card de pet, verificar se temos a imagem salva
        document.querySelectorAll('.cartao-pet').forEach(card => {
            const id = card.getAttribute('data-id');
            if (!id || !imagensAtuais[id]) return;
            
            const imgElement = card.querySelector('.imagem-pet');
            if (!imgElement) return;
            
            // Se a imagem estiver com erro ou sem src, restaurar
            if (!imgElement.src || imgElement.style.display === 'none') {
                imgElement.src = imagensAtuais[id];
                imgElement.style.display = 'block';
                
                // Esconder elemento .sem-imagem se existir
                const semImagem = card.querySelector('.sem-imagem');
                if (semImagem) {
                    semImagem.style.display = 'none';
                }
            }
        });
    } catch (err) {
        console.error('Erro ao restaurar imagens:', err);
    }
}

// Garantir o carregamento de imagens dos pets
function garantirCarregamentoImagens() {
    // Primeiro tentar restaurar imagens salvas da sessão anterior
    restaurarImagensPreCarregadas();
    
    // Forçar recarregamento das imagens que ainda não apareceram
    document.querySelectorAll('.imagem-pet').forEach(img => {
        // Se já estiver visível, não fazer nada
        if (img.complete && img.naturalWidth > 0 && img.style.display !== 'none') {
            return;
        }
        
        // Forçar recarregamento da imagem
        const src = img.getAttribute('src');
        if (!src) return;
        
        // Setar para vazio e depois restaurar para forçar novo carregamento
        img.removeAttribute('src');
        
        // Pequeno delay para garantir recarregamento
        setTimeout(() => {
            img.setAttribute('src', src);
        }, 50);
        
        // Configurar eventos de carregamento
        img.onload = function() {
            img.style.display = 'block';
            const container = img.closest('.container-imagem');
            if (container && container.querySelector('.sem-imagem')) {
                container.querySelector('.sem-imagem').style.display = 'none';
            }
            
            // Salvar na cache de imagens
            const cardId = img.closest('.cartao-pet')?.getAttribute('data-id');
            if (cardId) {
                try {
                    const imagensCache = JSON.parse(sessionStorage.getItem('imagensPets') || '{}');
                    imagensCache[cardId] = img.src;
                    sessionStorage.setItem('imagensPets', JSON.stringify(imagensCache));
                } catch (e) {}
            }
        };
        
        img.onerror = function() {
            img.style.display = 'none';
            const container = img.closest('.container-imagem');
            if (container) {
                if (!container.querySelector('.sem-imagem')) {
                    const semImagem = document.createElement('div');
                    semImagem.className = 'sem-imagem';
                    container.appendChild(semImagem);
                } else {
                    container.querySelector('.sem-imagem').style.display = 'flex';
                }
            }
        };
    });
}

// Função para mostrar indicador de carregamento
function mostrarIndicadorCarregamento() {
    // Remover qualquer overlay existente primeiro
    $('.carregando-overlay').remove();
    
    // Adicionar novo overlay
    $('body').append('<div class="carregando-overlay"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Carregando...</span></div></div>');
    
    // Timeout de segurança - remover o indicador após 8 segundos
    setTimeout(() => {
        removerIndicadorCarregamento();
    }, 8000);
}

// Função para remover o indicador de carregamento
function removerIndicadorCarregamento() {
    $('.carregando-overlay').fadeOut(300, function() {
        $(this).remove();
    });
}

// Função para inicializar a página
function inicializarPagina() {
    // Configurar elementos do DOM
    configElementosDOM();
    
    // Verificar se estamos em uma página de detalhes
    if (window.location.pathname.includes('/Admin/GerenciamentoPets/Details/')) {
        // Inicializar carrossel na página de detalhes
        $('.carousel').carousel();
        return;
    }
    
    // Verificar se tem grid de pets
    if ($('.pets-grid').length === 0) return;
    
    // Carregar dados iniciais
    carregarPets();
    
    // Configurar eventos
    configurarEventos();
    
    // Pré-carregar imagens
    preCarregarImagens();
    
    // Garantir que não fique preso em tela de carregamento
    setTimeout(removerTodosIndicadoresCarregamento, 10000);
}

// Função para remover qualquer indicador de carregamento
function removerTodosIndicadoresCarregamento() {
    $('.carregando-overlay').fadeOut(300, function() {
        $(this).remove();
    });
    $('.pets-grid').addClass('layout-pronto');
}

// Inicializar quando o documento estiver pronto
$(document).ready(function() {
    inicializarPagina();
    
    // Timeout de segurança para não ficar em carregamento infinito
    setTimeout(removerTodosIndicadoresCarregamento, 5000);
});

async function salvarPetComoRascunho() {
    
    
    const campoObrigatorio = document.querySelector('[data-required-for-draft="true"]');
    if (!campoObrigatorio || !campoObrigatorio.value.trim()) {
        toastr.error('Por favor, informe o nome do pet para salvar como rascunho.');
        return;
    }
    
    try {
        
        document.getElementById('statusPet').value = 'Rascunho';
        
        
        const camposApenasCadastroCompleto = document.querySelectorAll('[data-required-for-complete="true"]');
        
        
        const estadoOriginal = new Map();
        camposApenasCadastroCompleto.forEach(element => {
            estadoOriginal.set(element, element.hasAttribute('required'));
            element.removeAttribute('required');
        });
        
        
        const formData = new FormData(document.getElementById('formPet'));
        
        
        formData.append('CadastroCompleto', 'false');
        
        
        const camposTexto = ['Especie', 'Raca', 'Sexo', 'Porte', 'Descricao'];
        camposTexto.forEach(campo => {
            if (!formData.has(campo) || formData.get(campo) === null || formData.get(campo) === 'null') {
                formData.set(campo, '');
            }
        });
        
        if (!formData.has('Anos') || formData.get('Anos') === null || isNaN(formData.get('Anos'))) {
            formData.set('Anos', '0');
        }
        
        if (!formData.has('Meses') || formData.get('Meses') === null || isNaN(formData.get('Meses'))) {
            formData.set('Meses', '0');
        }
        
        
        const petId = document.getElementById('petId').value;
        if (petId && petId !== '0') {
            formData.append('ManterImagemAtual', 'true');
        }
        
        
        if (!formData.has('UsuarioId')) {
            formData.append('UsuarioId', '1'); 
        }
        
        
        await enviarViaXHR(formData, true);
        
        
        camposApenasCadastroCompleto.forEach(element => {
            if (estadoOriginal.get(element)) {
                element.setAttribute('required', '');
            }
        });
    } catch (error) {
        console.error('Erro ao salvar rascunho:', error);
        toastr.error('Ocorreu um erro ao salvar o rascunho. Por favor, tente novamente.');
    }
}

async function salvarPet() {
    
    const form = document.getElementById('formPet');
    
    
    const petId = document.getElementById('petId').value;
    const isEdicao = petId && petId !== '0';
    
    
    if (!validarFormulario()) {
        toastr.error('Por favor, preencha todos os campos obrigatórios.');
        return;
    }
    
    try {
        
        const formData = new FormData(form);
        
        
        formData.append('CadastroCompleto', 'true');
        
        
        if (isEdicao) {
            
            const previewImg = document.getElementById('previewImagem');
            const inputFile = document.getElementById('fotoPet');
            
            if (previewImg.style.display !== 'none' && (!inputFile.files || inputFile.files.length === 0)) {
                formData.append('ManterImagemAtual', 'true');
            }
        }
        
        
        if (!formData.has('UsuarioId')) {
            formData.append('UsuarioId', '1'); 
        }
        
        
        const camposTexto = ['Especie', 'Raca', 'Sexo', 'Porte', 'Descricao'];
        camposTexto.forEach(campo => {
            if (!formData.has(campo) || formData.get(campo) === null || formData.get(campo) === 'null') {
                formData.set(campo, '');
            }
        });
        
        if (!formData.has('Anos') || formData.get('Anos') === null || isNaN(formData.get('Anos'))) {
            formData.set('Anos', '0');
        }
        
        if (!formData.has('Meses') || formData.get('Meses') === null || isNaN(formData.get('Meses'))) {
            formData.set('Meses', '0');
        }
        
        
        await enviarViaXHR(formData, false);
    } catch (error) {
        console.error('Erro ao salvar pet:', error);
        toastr.error('Ocorreu um erro ao salvar o pet. Por favor, tente novamente.');
    }
}

async function validarFormulario() {
    const form = document.getElementById('formPet');
    let valido = true;
    
    
    const petId = document.getElementById('petId').value;
    const isEdicao = petId && petId !== '0';
    
    
    document.querySelectorAll('.mensagem-erro').forEach(elem => {
        elem.style.display = 'none';
    });
    
    
    const campoNome = document.getElementById('nomePet');
    if (campoNome && campoNome.value.trim().length > 0) {
        try {
            const response = await fetch(`/admin/pets/verificar-nome?nome=${encodeURIComponent(campoNome.value.trim())}&id=${petId}`);
            const resultado = await response.json();
            
            if (!resultado.disponivel) {
                
                campoNome.classList.add('is-invalid');
                
                
                const mensagemErro = campoNome.parentElement.querySelector('.mensagem-erro');
                if (mensagemErro) {
                    mensagemErro.textContent = resultado.mensagem;
                    mensagemErro.style.display = 'block';
                }
                
                valido = false;
            }
        } catch (error) {
            console.error('Erro ao verificar nome:', error);
        }
    }
    
    
    const camposObrigatorios = [
        'especiePet', 'racaPet', 'sexoPet', 'portePet', 'descricaoPet'
    ];
    
    camposObrigatorios.forEach(campo => {
        const element = document.getElementById(campo);
        if (!element || !element.value.trim()) {
            valido = false;
            
            const msgErro = element.parentElement.querySelector('.mensagem-erro');
            if (msgErro) {
                msgErro.style.display = 'block';
            }
        }
    });
    
    
    const anos = parseInt(document.getElementById('anosPet').value) || 0;
    const meses = parseInt(document.getElementById('mesesPet').value) || 0;
    
    if (anos < 0 || anos > 30) {
        valido = false;
        document.getElementById('anosPet').parentElement.parentElement.querySelector('.mensagem-erro').style.display = 'block';
    }
    
    if (meses < 0 || meses > 11) {
        valido = false;
        document.getElementById('mesesPet').parentElement.parentElement.querySelector('.mensagem-erro').style.display = 'block';
    }
    
    if (anos === 0 && meses === 0) {
        valido = false;
        toastr.error('A idade do pet deve ser maior que zero.');
    }
    
    
    
    const previewImg = document.getElementById('previewImagem');
    const inputFile = document.getElementById('fotoPet');
    const temImagemPreview = previewImg && previewImg.style.display !== 'none' && previewImg.src;
    const temNovaImagem = inputFile && inputFile.files && inputFile.files.length > 0;
    
    
    if (!temImagemPreview && !temNovaImagem) {
        valido = false;
        const areaImagem = document.querySelector('.pet-drop-container');
        if (areaImagem) {
            const msgErro = areaImagem.querySelector('.mensagem-erro');
            if (msgErro) {
                msgErro.style.display = 'block';
            }
        }
    }
    
    return valido;
}

function enviarViaXHR(formData, isRascunho = false) {
    return new Promise((resolve, reject) => {
        
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenElement ? tokenElement.value : '';
        
        const xhr = new XMLHttpRequest();
        xhr.open('POST', '/admin/pets/SalvarPet');
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
        
        
        xhr.upload.onprogress = function(e) {
            if (e.lengthComputable) {
                const percentComplete = (e.loaded / e.total) * 100;
            }
        };
        
        
        xhr.onload = function() {
            if (xhr.status >= 200 && xhr.status < 300) {
                try {
                    const response = JSON.parse(xhr.responseText);
                    
                    if (response.sucesso) {
                        if (isRascunho) {
                            toastr.success('Rascunho salvo com sucesso!');
                        } else {
                            toastr.success('Pet salvo com sucesso!');
                        }
                        
                        
                        resetarFormulario();
                        $('#modalPet').modal('hide');
                        
                        
                        setTimeout(() => {
                            window.location.reload();
                        }, 1500);
                        
                        resolve(response);
                    } else {
                        console.error('Erro ao salvar pet:', response);
                        
                        if (response.erros) {
                            
                            Object.keys(response.erros).forEach(campo => {
                                const mensagem = response.erros[campo];
                                const elemento = document.getElementById(campo);
                                
                                if (elemento) {
                                    const containerErro = elemento.nextElementSibling;
                                    if (containerErro && containerErro.classList.contains('mensagem-erro')) {
                                        containerErro.textContent = mensagem;
                                        containerErro.style.display = 'block';
                                    }
                                }
                                
                                toastr.error(mensagem);
                            });
                        } else {
                            
                            toastr.error(response.mensagem || 'Erro ao salvar o pet');
                        }
                        
                        reject(new Error(response.mensagem || 'Erro ao salvar'));
                    }
                } catch (error) {
                    console.error('Erro ao analisar resposta:', error);
                    
                    
                    let mensagemErro = 'Falha ao processar a resposta do servidor';
                    
                    try {
                        const errorResponse = JSON.parse(xhr.responseText);
                        if (errorResponse.mensagem) {
                            mensagemErro = errorResponse.mensagem;
                        }
                    } catch (e) {
                        console.error('Erro ao analisar resposta de erro:', e);
                    }
                    
                    reject(new Error(mensagemErro));
                }
            } else {
                console.error('Erro HTTP:', xhr.status, xhr.responseText);
                let mensagemErro = 'Erro ao salvar. Por favor, tente novamente.';
                
                try {
                    
                    const errorResponse = JSON.parse(xhr.responseText);
                    if (errorResponse.message) {
                        mensagemErro = errorResponse.message;
                    }
                } catch (e) {
                    
                }
                
                toastr.error(mensagemErro);
                reject(new Error(`Erro HTTP: ${xhr.status} - ${mensagemErro}`));
            }
        };
        
        xhr.onerror = function() {
            toastr.error('Erro de conexão. Por favor, verifique sua internet e tente novamente.');
            reject(new Error('Erro de conexão'));
        };
        
        
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }
        
        
        xhr.send(formData);
    });
}

function inicializarUploadImagem() {
    const dropArea = document.getElementById('dropArea');
    const inputFoto = document.getElementById('fotoPet');
    const previewImg = document.getElementById('previewImagem');
    const btnRemoverImagem = document.getElementById('btnRemoverImagem');
    const mensagemSoltar = document.querySelector('.mensagem-soltar');
    const progressoUpload = document.querySelector('.progresso-upload');
    
    if (!dropArea || !inputFoto || !previewImg) {
        console.error('Elementos de upload não encontrados');
        return;
    }

    
    function prevenirPadroes(e) {
        e.preventDefault();
        e.stopPropagation();
    }
    
    
    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        dropArea.addEventListener(eventName, prevenirPadroes, false);
    });
    
    
    ['dragenter', 'dragover'].forEach(eventName => {
        dropArea.addEventListener(eventName, function() {
            dropArea.classList.add('drag-over');
        }, false);
    });
    
    
    ['dragleave', 'drop'].forEach(eventName => {
        dropArea.addEventListener(eventName, function() {
            dropArea.classList.remove('drag-over');
        }, false);
    });
    
    
    dropArea.addEventListener('drop', function(e) {
        const dt = e.dataTransfer;
        const files = dt.files;
        
        if (files.length) {
            inputFoto.files = files;
            lidarComSelecaoArquivo(files[0]);
        }
    }, false);
    
    
    dropArea.addEventListener('click', function() {
        inputFoto.click();
    });
    
    
    inputFoto.addEventListener('change', function() {
        if (this.files && this.files[0]) {
            lidarComSelecaoArquivo(this.files[0]);
        }
    });
    
    
    async function lidarComSelecaoArquivo(arquivo) {
        
        
        const tamanhoMaximo = 10 * 1024 * 1024; 
        if (arquivo.size > tamanhoMaximo) {
            toastr.error('O arquivo é muito grande. O tamanho máximo é 10MB.', 'Erro de upload');
            return;
        }
        
        
        if (!arquivo.type.match('image.*')) {
            toastr.error('O arquivo selecionado não é uma imagem.', 'Erro de upload');
            return;
        }
        
        try {
            
            mensagemSoltar.style.display = 'none';
            progressoUpload.style.display = 'flex';
            
            
            const reader = new FileReader();
            reader.onload = function(e) {
                
                progressoUpload.style.display = 'none';
                previewImg.src = e.target.result;
                previewImg.style.display = 'block';
                btnRemoverImagem.style.display = 'block';
            };
            reader.readAsDataURL(arquivo);
        } catch (error) {
            console.error('Erro ao processar a imagem:', error);
            progressoUpload.style.display = 'none';
            mensagemSoltar.style.display = 'flex';
            toastr.error('Erro ao processar a imagem. Por favor, tente novamente.', 'Erro');
        }
    }
    
    
    if (btnRemoverImagem) {
        btnRemoverImagem.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation(); 
            
            
            inputFoto.value = '';
            
            previewImg.style.display = 'none';
            previewImg.src = '';
            btnRemoverImagem.style.display = 'none';
            
            
            mensagemSoltar.style.display = 'flex';
        });
    }
}


function abrirModalEdicao(petId) {
    
    resetarFormulario();
    
    
    const petCard = document.querySelector(`.cartao-pet[data-id="${petId}"]`);
    if (petCard) {
        
        const petData = JSON.parse(petCard.getAttribute('data-json'));
        
        
        const status = petData.Status?.toLowerCase();
        if (status === 'em processo' || status === 'adotado') {
            toastr.warning(`Não é possível editar um pet ${status === 'em processo' ? 'em processo de adoção' : 'já adotado'}.`);
            return;
        }
        
        
        document.getElementById('petId').value = petData.Id;
        document.getElementById('nomePet').value = petData.Nome || '';
        document.getElementById('especiePet').value = petData.Especie || '';
        document.getElementById('racaPet').value = petData.Raca || '';
        document.getElementById('sexoPet').value = petData.Sexo || '';
        document.getElementById('portePet').value = petData.Porte || '';
        document.getElementById('anosPet').value = petData.Anos || 0;
        document.getElementById('mesesPet').value = petData.Meses || 0;
        document.getElementById('descricaoPet').value = petData.Descricao || '';
        
        document.getElementById('statusPet').value = petData.Status || 'Disponível';
        
        
        const descricao = petData.Descricao || '';
        document.getElementById('contadorCaracteres').textContent = descricao.length;
        
        
        if (petData.NomeArquivoImagem) {
            const previewImg = document.getElementById('previewImagem');
            previewImg.src = `/imagens/pets/${petData.NomeArquivoImagem}`;
            previewImg.style.display = 'block';
            document.getElementById('btnRemoverImagem').style.display = 'block';
            document.querySelector('.mensagem-soltar').style.display = 'none';
        }
        
        
        atualizarIdadeTotal();
        
        
        document.getElementById('tituloCadastroEdicao').textContent = 'Editar Pet';
        
        
        if (modalPet) {
            modalPet.show();
        } else {
            console.error('Modal não inicializado');
        }
    } else {
        console.error(`Pet card não encontrado para ID: ${petId}`);
        toastr.error('Não foi possível carregar os dados do pet para edição.');
    }
}


function aplicarEstilosModalDetalhesPet() {
    
    let styleEl = document.getElementById('estilos-detalhes-pet');
    if (!styleEl) {
        
        styleEl = document.createElement('style');
        styleEl.id = 'estilos-detalhes-pet';
        document.head.appendChild(styleEl);
    }
    
    
    styleEl.textContent = `
        /* Estilo geral do modal */
        #modalDetalhesPet .modal-content {
            border: none;
            border-radius: 1rem;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15);
            overflow: hidden;
        }
        
        #modalDetalhesPet .modal-header {
            background: linear-gradient(135deg, #4e73df 0%, #224abe 100%);
            color: white;
            padding: 1.5rem;
            border-bottom: none;
        }
        
        #modalDetalhesPet .modal-title {
            font-weight: 600;
            display: flex;
            align-items: center;
        }
        
        #modalDetalhesPet .modal-title i {
            margin-right: 0.5rem;
            font-size: 1.2rem;
            filter: drop-shadow(0 2px 3px rgba(0, 0, 0, 0.2));
        }
        
        #modalDetalhesPet .btn-close-white {
            filter: brightness(0) invert(1);
            opacity: 0.8;
            transition: all 0.2s;
        }
        
        #modalDetalhesPet .btn-close-white:hover {
            opacity: 1;
            transform: rotate(90deg);
        }
        
        #modalDetalhesPet .modal-body {
            background-color: #f8f9fc;
            padding: 1.5rem;
        }
        
        #modalDetalhesPet .modal-footer {
            background-color: #f8f9fc;
            border-top: 1px solid #e3e6f0;
            padding: 1rem 1.5rem;
        }
        
        /* Melhoria no layout */
        #modalDetalhesPet .row {
            margin-bottom: 1.5rem;
        }
        
        #modalDetalhesPet .col-md-5 {
            display: flex;
            flex-direction: column;
            justify-content: center;
        }
        
        /* Estilos para a imagem do pet */
        #imgPetDetalhes {
            width: 100%;
            height: 280px;
            object-fit: cover;
            border-radius: 0.5rem;
            box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
            border: 3px solid white;
        }
        
        /* Estilos para os campos de informação */
        #modalDetalhesPet .rotulo-form {
            font-size: 0.8rem;
            font-weight: 600;
            color: #6c757d;
            display: block;
            margin-bottom: 0.25rem;
        }
        
        #modalDetalhesPet .rotulo-form i {
            color: #4e73df;
            margin-right: 0.25rem;
            width: 18px;
            text-align: center;
        }
        
        #modalDetalhesPet .texto-estatico {
            font-size: 1rem;
            font-weight: 500;
            color: #333;
            background-color: white;
            padding: 0.75rem;
            border-radius: 0.5rem;
            border: 1px solid #e3e6f0;
            margin-bottom: 0;
            transition: all 0.2s ease;
            box-shadow: 0 2px 5px rgba(0, 0, 0, 0.02);
        }
        
        #modalDetalhesPet .col-md-6:hover .texto-estatico,
        #modalDetalhesPet .col-md-7:hover .texto-estatico {
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.05);
            transform: translateY(-2px);
            border-color: #d1d3e2;
        }
        
        /* Estilo para a descrição */
        #modalDetalhesPet #descricaoDetalhes {
            min-height: 100px;
            background-color: white;
            border: 1px solid #e3e6f0;
            border-radius: 0.5rem;
            padding: 1rem;
            color: #333;
            line-height: 1.6;
            box-shadow: 0 2px 5px rgba(0, 0, 0, 0.02);
            transition: all 0.2s ease;
        }
        
        #modalDetalhesPet #descricaoDetalhes:hover {
            box-shadow: 0 4px 10px rgba(0, 0, 0, 0.05);
            border-color: #d1d3e2;
        }
        
        /* Estilo para badges de status */
        #statusDetalhes {
            position: relative;
            padding-left: 1.5rem !important;
            font-weight: 600 !important;
        }
        
        #statusDetalhes::before {
            content: "";
            display: block;
            width: 12px;
            height: 12px;
            border-radius: 50%;
            position: absolute;
            left: 0.75rem;
            top: 50%;
            transform: translateY(-50%);
            background-color: #4e73df;
            transition: all 0.3s ease;
            box-shadow: 0 0 0 2px rgba(78, 115, 223, 0.2);
        }
        
        #statusDetalhes.status-disponivel {
            color: #1cc88a !important;
        }
        
        #statusDetalhes.status-disponivel::before {
            background-color: #1cc88a;
            box-shadow: 0 0 0 2px rgba(28, 200, 138, 0.2);
        }
        
        #statusDetalhes.status-adotado {
            color: #4e73df !important;
        }
        
        #statusDetalhes.status-adotado::before {
            background-color: #4e73df;
            box-shadow: 0 0 0 2px rgba(78, 115, 223, 0.2);
        }
        
        #statusDetalhes.status-processo {
            color: #f6c23e !important;
        }
        
        #statusDetalhes.status-processo::before {
            background-color: #f6c23e;
            box-shadow: 0 0 0 2px rgba(246, 194, 62, 0.2);
        }
        
        #statusDetalhes.status-indisponivel {
            color: #e74a3b !important;
        }
        
        #statusDetalhes.status-indisponivel::before {
            background-color: #e74a3b;
            box-shadow: 0 0 0 2px rgba(231, 74, 59, 0.2);
        }
        
        /* Botões do footer */
        #modalDetalhesPet .botao-contorno-secundario {
            color: #6c757d;
            border: 1px solid #6c757d;
            background-color: transparent;
            padding: 0.5rem 1rem;
            border-radius: 0.35rem;
            transition: all 0.2s;
        }
        
        #modalDetalhesPet .botao-contorno-secundario:hover {
            background-color: #6c757d;
            color: white;
        }
        
        #modalDetalhesPet .botao-contorno-primario {
            color: #4e73df;
            border: 1px solid #4e73df;
            background-color: transparent;
            padding: 0.5rem 1rem;
            border-radius: 0.35rem;
            transition: all 0.2s;
        }
        
        #modalDetalhesPet .botao-contorno-primario:hover {
            background-color: #4e73df;
            color: white;
        }
        
        /* Responsividade */
        @media (max-width: 768px) {
            #imgPetDetalhes {
                height: 200px;
                margin-bottom: 1rem;
            }
            
            #modalDetalhesPet .modal-body {
                padding: 1rem;
            }
            
            #modalDetalhesPet .row {
                margin-bottom: 0.75rem;
            }
            
            #modalDetalhesPet .col-md-6 {
                margin-bottom: 0.75rem;
            }
            
            #modalDetalhesPet .texto-estatico {
                padding: 0.5rem;
                font-size: 0.9rem;
            }
            
            #modalDetalhesPet .rotulo-form {
                font-size: 0.75rem;
            }
        }
    `;
}


function visualizarPet(petId) {
    const petCard = document.querySelector(`.cartao-pet[data-id="${petId}"]`);
    if (petCard) {
        
        const petData = JSON.parse(petCard.getAttribute('data-json'));
        
        
        document.getElementById('detalhePetId').value = petData.Id;
        
        
        document.getElementById('nomeDetalhes').textContent = petData.Nome || 'Nome não informado';
        document.getElementById('especieDetalhes').textContent = petData.Especie || 'Não informado';
        document.getElementById('racaDetalhes').textContent = petData.Raca || 'Não informado';
        
        
        let idadeTexto = '';
        if (petData.Anos > 0 && petData.Meses > 0) {
            idadeTexto = `${petData.Anos} ano(s) e ${petData.Meses} mês(es)`;
        } else if (petData.Anos > 0) {
            idadeTexto = `${petData.Anos} ano(s)`;
        } else if (petData.Meses > 0) {
            idadeTexto = `${petData.Meses} mês(es)`;
        } else {
            idadeTexto = "0 ano(s)";
        }
        document.getElementById('idadeDetalhes').textContent = idadeTexto;
        
        
        const statusBadge = document.getElementById('pet-status-badge');
        const statusLower = petData.Status ? petData.Status.toLowerCase().replace(' ', '_') : 'indisponivel';
        
        statusBadge.textContent = petData.Status || 'Indisponível';
        statusBadge.className = 'pet-status-badge';
        statusBadge.classList.add(statusLower);
        
        
        document.getElementById('especieDetalhesInfo').textContent = petData.Especie || 'Não informado';
        document.getElementById('racaDetalhesInfo').textContent = petData.Raca || 'Não informado';
        document.getElementById('idadeDetalhesInfo').textContent = idadeTexto;
        document.getElementById('statusDetalhesInfo').textContent = petData.Status || 'Não informado';
        
        
        const sexoContainer = document.getElementById('sexoDetalhesContainer');
        const porteContainer = document.getElementById('porteDetalhesContainer');
        
        if (petData.Sexo) {
            document.getElementById('sexoDetalhesInfo').textContent = petData.Sexo;
            sexoContainer.style.display = 'flex';
        } else {
            sexoContainer.style.display = 'none';
        }
        
        if (petData.Porte) {
            document.getElementById('porteDetalhesInfo').textContent = petData.Porte;
            porteContainer.style.display = 'flex';
        } else {
            porteContainer.style.display = 'none';
        }
        
        
        document.getElementById('descricaoDetalhes').textContent = petData.Descricao || 'Sem descrição disponível';
        
        
        const datasCadastroContainer = document.getElementById('datasCadastroContainer');
        const dataAtualizacaoContainer = document.getElementById('dataAtualizacaoContainer');
        
        
        const formatarData = (dataString) => {
            if (!dataString) return 'Não disponível';
            
            try {
                const data = new Date(dataString);
                return data.toLocaleDateString('pt-BR', { 
                    day: '2-digit', 
                    month: '2-digit', 
                    year: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit'
                });
            } catch (e) {
                console.error('Erro ao formatar data:', e);
                return 'Formato inválido';
            }
        };
        
        if (petData.DataCadastro) {
            document.getElementById('dataCadastroInfo').textContent = formatarData(petData.DataCadastro);
            datasCadastroContainer.style.display = 'block';
        } else {
            datasCadastroContainer.style.display = 'none';
        }
        
        if (petData.DataAtualizacao) {
            document.getElementById('dataAtualizacaoInfo').textContent = formatarData(petData.DataAtualizacao);
            dataAtualizacaoContainer.style.display = 'flex';
        } else {
            dataAtualizacaoContainer.style.display = 'none';
        }
        
        
        const imgPet = document.getElementById('imgPetDetalhes');
        if (petData.NomeArquivoImagem) {
            imgPet.src = `/imagens/pets/${petData.NomeArquivoImagem}`;
        } else {
            imgPet.src = '/imagens/pets/pet-placeholder.jpg';
        }
        
        
        if (modalDetalhesPet) {
            modalDetalhesPet.show();
        } else {
            console.error('Modal de detalhes não inicializado');
        }
    } else {
        console.error(`Não foi possível encontrar o pet com ID ${petId}`);
        
        
        fetch(`/admin/pets/detalhes/${petId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`Erro ao carregar detalhes do pet: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                if (data && data.sucesso) {
                    const petData = data.dados || data;
                    
                    
                    visualizarPetComDados(petData);
                } else {
                    toastr.error('Não foi possível carregar os detalhes do pet.');
                }
            })
            .catch(error => {
                console.error('Erro ao buscar dados do pet:', error);
                toastr.error('Erro ao carregar detalhes do pet. Por favor, tente novamente.');
            });
    }
}


function visualizarPetComDados(petData) {
    
    document.getElementById('detalhePetId').value = petData.Id;
    
    
    document.getElementById('nomeDetalhes').textContent = petData.Nome || 'Nome não informado';
    document.getElementById('especieDetalhes').textContent = petData.Especie || 'Não informado';
    document.getElementById('racaDetalhes').textContent = petData.Raca || 'Não informado';
    
    
    let idadeTexto = '';
    if (petData.Anos > 0 && petData.Meses > 0) {
        idadeTexto = `${petData.Anos} ano(s) e ${petData.Meses} mês(es)`;
    } else if (petData.Anos > 0) {
        idadeTexto = `${petData.Anos} ano(s)`;
    } else if (petData.Meses > 0) {
        idadeTexto = `${petData.Meses} mês(es)`;
    } else {
        idadeTexto = "0 ano(s)";
    }
    document.getElementById('idadeDetalhes').textContent = idadeTexto;
    
    
    const statusBadge = document.getElementById('pet-status-badge');
    const statusLower = petData.Status ? petData.Status.toLowerCase().replace(' ', '_') : 'indisponivel';
    
    statusBadge.textContent = petData.Status || 'Indisponível';
    statusBadge.className = 'pet-status-badge';
    statusBadge.classList.add(statusLower);
    
    
    document.getElementById('especieDetalhesInfo').textContent = petData.Especie || 'Não informado';
    document.getElementById('racaDetalhesInfo').textContent = petData.Raca || 'Não informado';
    document.getElementById('idadeDetalhesInfo').textContent = idadeTexto;
    document.getElementById('statusDetalhesInfo').textContent = petData.Status || 'Não informado';
    
    
    const sexoContainer = document.getElementById('sexoDetalhesContainer');
    const porteContainer = document.getElementById('porteDetalhesContainer');
    
    if (petData.Sexo) {
        document.getElementById('sexoDetalhesInfo').textContent = petData.Sexo;
        sexoContainer.style.display = 'flex';
    } else {
        sexoContainer.style.display = 'none';
    }
    
    if (petData.Porte) {
        document.getElementById('porteDetalhesInfo').textContent = petData.Porte;
        porteContainer.style.display = 'flex';
    } else {
        porteContainer.style.display = 'none';
    }
    
    
    document.getElementById('descricaoDetalhes').textContent = petData.Descricao || 'Sem descrição disponível';
    
    
    const datasCadastroContainer = document.getElementById('datasCadastroContainer');
    const dataAtualizacaoContainer = document.getElementById('dataAtualizacaoContainer');
    
    
    const formatarData = (dataString) => {
        if (!dataString) return 'Não disponível';
        
        try {
            const data = new Date(dataString);
            return data.toLocaleDateString('pt-BR', { 
                day: '2-digit', 
                month: '2-digit', 
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        } catch (e) {
            console.error('Erro ao formatar data:', e);
            return 'Formato inválido';
        }
    };
    
    if (petData.DataCadastro) {
        document.getElementById('dataCadastroInfo').textContent = formatarData(petData.DataCadastro);
        datasCadastroContainer.style.display = 'block';
    } else {
        datasCadastroContainer.style.display = 'none';
    }
    
    if (petData.DataAtualizacao) {
        document.getElementById('dataAtualizacaoInfo').textContent = formatarData(petData.DataAtualizacao);
        dataAtualizacaoContainer.style.display = 'flex';
    } else {
        dataAtualizacaoContainer.style.display = 'none';
    }
    
    
    const imgPet = document.getElementById('imgPetDetalhes');
    if (petData.NomeArquivoImagem) {
        imgPet.src = `/imagens/pets/${petData.NomeArquivoImagem}`;
    } else {
        imgPet.src = '/imagens/pets/pet-placeholder.jpg';
    }
    
    
    if (modalDetalhesPet) {
        modalDetalhesPet.show();
    } else {
        console.error('Modal de detalhes não inicializado');
    }
}


function atualizarContador() {
    const descricao = document.getElementById('descricaoPet');
    const contador = document.getElementById('contadorCaracteres');
    
    if (descricao && contador) {
        const textoAtual = descricao.value || '';
        contador.textContent = textoAtual.length;
        
        if (textoAtual.length > 450) {
            contador.classList.add('text-warning');
        } else {
            contador.classList.remove('text-warning');
        }
    }
}


function atualizarIdadeTotal() {
    const anos = parseInt(document.getElementById('anosPet').value) || 0;
    const meses = parseInt(document.getElementById('mesesPet').value) || 0;
    const idadeTotalElem = document.getElementById('idadeTotal');
    
    if (idadeTotalElem) {
        let textoIdade = `<i class="fas fa-info-circle me-1"></i>Idade total: ${anos} ano(s)`;
        
        if (meses > 0) {
            textoIdade += ` e ${meses} mês(es)`;
        }
        
        idadeTotalElem.innerHTML = textoIdade;
    }
}


function configurarTratamentoErroImagens() {
    document.querySelectorAll('.imagem-pet').forEach(img => {
        img.addEventListener('error', function() {
            // Esconder a imagem que falhou ao carregar
            this.style.display = 'none';
            
            // Mostrar a div "sem-imagem" no lugar
            const container = this.closest('.container-imagem');
            if (container) {
                if (!container.querySelector('.sem-imagem')) {
                    const semImagem = document.createElement('div');
                    semImagem.className = 'sem-imagem';
                    container.appendChild(semImagem);
                } else {
                    container.querySelector('.sem-imagem').style.display = 'flex';
                }
            }
        });
    });
}

// Pré-carregar imagens para melhorar desempenho
function preCarregarImagens() {
    // Delay para garantir DOM carregado
    setTimeout(() => {
        const imagens = document.querySelectorAll('.imagem-pet');
        if (imagens.length === 0) {
            // Se não há imagens, remover qualquer indicador de carregamento
            removerIndicadorCarregamento();
            return;
        }
        
        // Verificar se há imagens salvas no sessionStorage
        let imagensCache = {};
        try {
            const imagensSalvas = sessionStorage.getItem('imagensPets');
            if (imagensSalvas) {
                imagensCache = JSON.parse(imagensSalvas);
                
                // Restaurar imagens do cache imediatamente
                for (const cardId in imagensCache) {
                    const card = document.querySelector(`.cartao-pet[data-id="${cardId}"]`);
                    if (!card) continue;
                    
                    const img = card.querySelector('.imagem-pet');
                    if (!img) continue;
                    
                    img.src = imagensCache[cardId];
                    img.setAttribute('data-loaded', 'true');
                    img.style.display = 'block';
                    
                    // Esconder elemento .sem-imagem se existir
                    const semImagem = card.querySelector('.sem-imagem');
                    if (semImagem) {
                        semImagem.style.display = 'none';
                    }
                }
            }
        } catch (e) {
            console.error('Erro ao recuperar cache de imagens:', e);
        }
        
        // Mostrar indicador de carregamento para muitas imagens
        let carregando = null;
        if (imagens.length > 10) {
            carregando = document.createElement('div');
            carregando.className = 'carregando-overlay';
            carregando.innerHTML = '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Carregando imagens...</span></div>';
            document.body.appendChild(carregando);
            
            // Timeout de segurança - remover o indicador após 5 segundos independentemente
            setTimeout(() => {
                if (carregando) {
                    carregando.style.opacity = '0';
                    setTimeout(() => {
                        if (carregando && carregando.parentNode) {
                            carregando.remove();
                        }
                    }, 300);
                }
            }, 5000);
        }
        
        // Contador para rastrear carregamento
        let carregadas = 0;
        const total = imagens.length;
        
        // Função para finalizar o carregamento
        const finalizarCarregamento = () => {
            // Aplicar classe para indicar que o layout está pronto
            $('.pets-grid').addClass('layout-pronto');
            
            // Remover indicador quando todas carregadas
            if (carregando) {
                setTimeout(() => {
                    carregando.style.opacity = '0';
                    setTimeout(() => {
                        if (carregando && carregando.parentNode) {
                            carregando.remove();
                        }
                    }, 300);
                }, 500);
            }
            
            // Remover qualquer outro overlay
            removerIndicadorCarregamento();
        };
        
        // Timeout para finalizar o carregamento mesmo se alguma imagem falhar
        setTimeout(finalizarCarregamento, 3000);
        
        imagens.forEach(img => {
            // Se já tiver sido carregada e estiver visível, não precisamos fazer nada
            if (img.complete && img.naturalWidth > 0 && img.style.display !== 'none') {
                carregadas++;
                img.setAttribute('data-loaded', 'true');
                
                // Se todas as imagens estão carregadas, finalizar
                if (carregadas >= total) {
                    finalizarCarregamento();
                }
                return;
            }
            
            // Criar nova imagem para pré-carregamento
            const novaImg = new Image();
            
            // Timeout para cada imagem individual
            const timeoutId = setTimeout(() => {
                // Se a imagem não carregar em 3 segundos, consideramos como erro
                carregadas++;
                
                // Exibir elemento de placeholder
                img.style.display = 'none';
                const container = img.closest('.container-imagem');
                if (container) {
                    if (!container.querySelector('.sem-imagem')) {
                        const semImagem = document.createElement('div');
                        semImagem.className = 'sem-imagem';
                        container.appendChild(semImagem);
                    } else {
                        container.querySelector('.sem-imagem').style.display = 'flex';
                    }
                }
                
                // Se todas as imagens foram processadas, finalizar
                if (carregadas >= total) {
                    finalizarCarregamento();
                }
            }, 3000);
            
            novaImg.onload = function() {
                clearTimeout(timeoutId);
                carregadas++;
                img.style.display = 'block';
                img.setAttribute('data-loaded', 'true');
                
                // Salvar na cache de imagens
                const cardId = img.closest('.cartao-pet')?.getAttribute('data-id');
                if (cardId) {
                    try {
                        imagensCache[cardId] = img.src;
                        sessionStorage.setItem('imagensPets', JSON.stringify(imagensCache));
                    } catch (e) {}
                }
                
                // Se todas as imagens estão carregadas, finalizar
                if (carregadas >= total) {
                    finalizarCarregamento();
                }
            };
            
            novaImg.onerror = function() {
                clearTimeout(timeoutId);
                carregadas++;
                
                // Tratar erro
                img.style.display = 'none';
                const container = img.closest('.container-imagem');
                if (container) {
                    if (!container.querySelector('.sem-imagem')) {
                        const semImagem = document.createElement('div');
                        semImagem.className = 'sem-imagem';
                        container.appendChild(semImagem);
                    } else {
                        container.querySelector('.sem-imagem').style.display = 'flex';
                    }
                }
                
                // Se todas as imagens foram processadas, finalizar
                if (carregadas >= total) {
                    finalizarCarregamento();
                }
            };
            
            // Iniciar carregamento
            novaImg.src = img.src;
        });
    }, 100);
}


function limparFiltros() {
    
    const campoPesquisa = document.getElementById('pesquisaPet');
    const filtroEspecie = document.getElementById('filtroEspecie');
    const filtroStatus = document.getElementById('filtroStatus');
    
    if (campoPesquisa) campoPesquisa.value = '';
    if (filtroEspecie) filtroEspecie.value = '';
    if (filtroStatus) filtroStatus.value = '';
    
    filtrarPets();
    
    toastr.info('Filtros limpos com sucesso!');
}

function filtrarPets() {
    const termoPesquisa = (document.getElementById('pesquisaPet')?.value || '').toLowerCase();
    const especieFiltro = (document.getElementById('filtroEspecie')?.value || '').toLowerCase();
    const statusFiltro = (document.getElementById('filtroStatus')?.value || '').toLowerCase().replace(' ', '_');
    
    const cardsPets = document.querySelectorAll('.cartao-pet');
    let petsVisiveis = 0;
    
    cardsPets.forEach(card => {
        const nomePet = card.querySelector('.nome-pet')?.textContent.toLowerCase() || '';
        const descricaoPet = card.getAttribute('data-json') ? 
            JSON.parse(card.getAttribute('data-json')).Descricao?.toLowerCase() || '' : '';
        const especiePet = card.getAttribute('data-especie')?.toLowerCase() || '';
        const statusPet = card.getAttribute('data-status')?.toLowerCase() || '';
        const racaPet = card.getAttribute('data-json') ? 
            JSON.parse(card.getAttribute('data-json')).Raca?.toLowerCase() || '' : '';
        
        const atendePesquisa = termoPesquisa === '' || 
            nomePet.includes(termoPesquisa) || 
            descricaoPet.includes(termoPesquisa) ||
            racaPet.includes(termoPesquisa);
            
        const atendeEspecie = especieFiltro === '' || especiePet === especieFiltro;
        const atendeStatus = statusFiltro === '' || statusPet === statusFiltro;
        
        if (atendePesquisa && atendeEspecie && atendeStatus) {
            card.style.display = '';
            petsVisiveis++;
        } else {
            card.style.display = 'none';
        }
    });
    
    // Atualizar mensagem de sem pets
    const mensagemSemPets = document.querySelector('.mensagem-sem-pets');
    if (mensagemSemPets) {
        mensagemSemPets.style.display = petsVisiveis === 0 && cardsPets.length > 0 ? 'block' : 'none';
    }
    
    // Atualizar a informação de paginação
    const infoPaginacao = document.querySelector('.info-paginacao');
    if (infoPaginacao) {
        infoPaginacao.innerHTML = `Exibindo <span>${petsVisiveis}</span> de <span>${cardsPets.length}</span> pets`;
    }
    
    // Garantir que a paginação e o seletor de itens por página sejam exibidos
    $('.paginacao-container').css('display', 'block');
    $('.paginacao-info').css('display', 'flex');
    $('.itens-por-pagina').css('display', 'flex');
}


function resetarFormulario() {
    
    const form = document.getElementById('formPet');
    if (form) {
        form.reset();
    }
    
    
    const idInput = document.getElementById('petId');
    if (idInput) {
        idInput.value = '0';
    }
    
    
    const previewImg = document.getElementById('previewImagem');
    const btnRemoverImagem = document.getElementById('btnRemoverImagem');
    const mensagemSoltar = document.querySelector('.mensagem-soltar');
    
    if (previewImg) {
        previewImg.style.display = 'none';
        previewImg.src = '';
    }
    
    if (btnRemoverImagem) {
        btnRemoverImagem.style.display = 'none';
    }
    
    if (mensagemSoltar) {
        mensagemSoltar.style.display = 'flex';
    }
    
    
    const contadorCaracteres = document.getElementById('contadorCaracteres');
    if (contadorCaracteres) {
        contadorCaracteres.textContent = '0';
    }
    
    
    atualizarIdadeTotal();
    
    
    const tituloCadastro = document.getElementById('tituloCadastroEdicao');
    if (tituloCadastro) {
        tituloCadastro.textContent = 'Novo Pet';
    }
    
    
    const statusPet = document.getElementById('statusPet');
    if (statusPet) {
        statusPet.value = 'Disponível';
    }
    
    
    document.querySelectorAll('.mensagem-erro').forEach(mensagem => {
        mensagem.style.display = 'none';
    });
}


function confirmarExclusao(petId) {
    const petCard = document.querySelector(`.cartao-pet[data-id="${petId}"]`);
    if (petCard) {
        
        const petData = JSON.parse(petCard.getAttribute('data-json'));
        
        
        const status = petData.Status?.toLowerCase();
        if (status === 'em processo' || status === 'adotado') {
            toastr.warning(`Não é possível excluir um pet ${status === 'em processo' ? 'em processo de adoção' : 'já adotado'}.`);
            return;
        }
        
        
        document.getElementById('petIdParaExcluir').value = petId;
        document.getElementById('nomePetExclusao').textContent = petData.Nome;
        
        
        if (modalConfirmacaoExclusao) {
            modalConfirmacaoExclusao.show();
        } else {
            console.error('Modal de confirmação não inicializado');
        }
    }
}


function excluirPet(petId) {
    if (!petId) {
        toastr.error("ID do pet inválido");
        return;
    }
    
    
    if (modalConfirmacaoExclusao) {
        modalConfirmacaoExclusao.hide();
    }
    
    
    const xhr = new XMLHttpRequest();
    xhr.open('POST', `/admin/pets/excluir/${petId}`);
    xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
    
    
    const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
    const token = tokenElement ? tokenElement.value : '';
    if (token) {
        xhr.setRequestHeader('RequestVerificationToken', token);
    }
    
    xhr.onload = function() {
        if (xhr.status >= 200 && xhr.status < 300) {
            try {
                const response = JSON.parse(xhr.responseText);
                if (response.sucesso) {
                    toastr.success('Pet excluído com sucesso!');
                    
                    
                    const cardPet = document.querySelector(`.cartao-pet[data-id="${petId}"]`);
                    if (cardPet) {
                        cardPet.remove();
                    }
                    
                    
                    setTimeout(() => {
                        window.location.reload();
                    }, 1500);
                } else {
                    toastr.error(response.mensagem || 'Erro ao excluir pet');
                }
            } catch (error) {
                console.error('Erro ao processar resposta:', error);
                toastr.error('Erro ao processar resposta do servidor');
            }
        } else {
            toastr.error(`Erro ao excluir pet: ${xhr.status}`);
        }
    };
    
    xhr.onerror = function() {
        toastr.error('Erro de conexão ao tentar excluir o pet');
    };
    
    xhr.send();
}


function visualizarDetalhesPet(petId) {
    visualizarPet(petId);
}


function inicializarValidacaoNomeDuplicado() {
    const campoNome = document.getElementById('nomePet');
    const petId = document.getElementById('petId');
    
    if (campoNome) {
        let timeoutId;
        
        campoNome.addEventListener('blur', async function() {
            const nome = campoNome.value.trim();
            const id = petId ? petId.value : 0;
            
            if (nome.length > 0) {
                try {
                    const response = await fetch(`/admin/pets/verificar-nome?nome=${encodeURIComponent(nome)}&id=${id}`);
                    const resultado = await response.json();
                    
                    if (!resultado.disponivel) {
                        
                        campoNome.classList.add('is-invalid');
                        
                        
                        const mensagemErro = campoNome.parentElement.querySelector('.mensagem-erro');
                        if (mensagemErro) {
                            mensagemErro.textContent = resultado.mensagem;
                            mensagemErro.style.display = 'block';
                        }
                    } else {
                        campoNome.classList.remove('is-invalid');
                        
                        
                        const mensagemErro = campoNome.parentElement.querySelector('.mensagem-erro');
                        if (mensagemErro) {
                            mensagemErro.style.display = 'none';
                        }
                    }
                } catch (error) {
                    console.error('Erro ao verificar nome:', error);
                }
            }
        });
        
        
        campoNome.addEventListener('input', function() {
            clearTimeout(timeoutId);
            timeoutId = setTimeout(async function() {
                const nome = campoNome.value.trim();
                const id = petId ? petId.value : 0;
                
                if (nome.length > 2) {
                    try {
                        const response = await fetch(`/admin/pets/verificar-nome?nome=${encodeURIComponent(nome)}&id=${id}`);
                        const resultado = await response.json();
                        
                        if (!resultado.disponivel) {
                            
                            campoNome.classList.add('is-invalid');
                            
                            
                            const mensagemErro = campoNome.parentElement.querySelector('.mensagem-erro');
                            if (mensagemErro) {
                                mensagemErro.textContent = resultado.mensagem;
                                mensagemErro.style.display = 'block';
                            }
                        } else {
                            campoNome.classList.remove('is-invalid');
                            
                            
                            const mensagemErro = campoNome.parentElement.querySelector('.mensagem-erro');
                            if (mensagemErro) {
                                mensagemErro.style.display = 'none';
                            }
                        }
                    } catch (error) {
                        console.error('Erro ao verificar nome:', error);
                    }
                }
            }, 500); 
        });
    }
}

// Inicializar manipuladores de eventos para filtros e botões
function inicializarManipuladoresEventos() {
    // Botão limpar filtros
    const btnLimparFiltros = document.getElementById('btnLimparFiltros');
    if (btnLimparFiltros) {
        btnLimparFiltros.addEventListener('click', limparFiltros);
    }

    // Configurar eventos para campos de pesquisa e filtros
    const campoPesquisa = document.getElementById('pesquisaPet');
    const filtroEspecie = document.getElementById('filtroEspecie');
    const filtroStatus = document.getElementById('filtroStatus');

    if (campoPesquisa) {
        campoPesquisa.addEventListener('input', filtrarPets);
    }

    if (filtroEspecie) {
        filtroEspecie.addEventListener('change', filtrarPets);
    }

    if (filtroStatus) {
        filtroStatus.addEventListener('change', filtrarPets);
    }
    
    // Botões de ação nos cards de pets
    document.querySelectorAll('.botao-acao').forEach(btn => {
        btn.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            const id = this.getAttribute('data-pet-id');
            if (!id) return;
            
            // Efeito de clique
            this.classList.add('clicked');
            setTimeout(() => this.classList.remove('clicked'), 200);
            
            // Executar ação baseada na classe
            if (this.classList.contains('visualizar')) {
                visualizarPet(id);
            } else if (this.classList.contains('editar')) {
                abrirModalEdicao(id);
            } else if (this.classList.contains('excluir')) {
                confirmarExclusao(id);
            }
        });
    });
    
    // Cards de pets clicáveis
    document.querySelectorAll('.cartao-pet').forEach(card => {
        card.addEventListener('click', function(e) {
            // Não fazer nada se o clique foi em um botão de ação
            if (e.target.closest('.botao-acao')) return;
            
            const id = this.getAttribute('data-id');
            if (id) {
                visualizarPet(id);
            }
        });
    });
    
    // Botões dos modais
    const btnConfirmarExclusao = document.getElementById('btnConfirmarExclusao');
    if (btnConfirmarExclusao) {
        btnConfirmarExclusao.addEventListener('click', function() {
            const idElement = document.getElementById('petIdParaExcluir');
            if (idElement && idElement.value) {
                excluirPet(parseInt(idElement.value));
            } else {
                console.error("ID não encontrado para exclusão");
                toastr.error("Erro: ID do pet não encontrado");
            }
        });
    }
    
    // Botão de editar no modal de detalhes
    const btnEditarPet = document.querySelector('.btnEditarPet');
    if (btnEditarPet) {
        btnEditarPet.addEventListener('click', function() {
            const petId = document.getElementById('detalhePetId').value;
            if (petId) {
                modalDetalhesPet.hide();
                setTimeout(() => {
                    abrirModalEdicao(petId);
                }, 500);
            }
        });
    }
    
    // Botões de salvar pet
    const btnSalvarPet = document.getElementById('btnSalvarPet');
    const btnSalvarRascunho = document.getElementById('btnSalvarRascunho');
    
    if (btnSalvarPet) {
        btnSalvarPet.addEventListener('click', salvarPet);
    }
    
    if (btnSalvarRascunho) {
        btnSalvarRascunho.addEventListener('click', salvarPetComoRascunho);
    }
    
    // Campo de descrição - contador de caracteres
    const descricaoPet = document.getElementById('descricaoPet');
    if (descricaoPet) {
        descricaoPet.addEventListener('input', atualizarContador);
    }
    
    // Campos de idade - atualização automática
    const anosPet = document.getElementById('anosPet');
    const mesesPet = document.getElementById('mesesPet');
    
    if (anosPet) {
        anosPet.addEventListener('input', atualizarIdadeTotal);
    }
    
    if (mesesPet) {
        mesesPet.addEventListener('input', atualizarIdadeTotal);
    }
}

// Configurar tooltips para cards com status especial
function configurarTooltipsBotoesDesabilitados() {
    document.querySelectorAll('.cartao-pet').forEach(card => {
        const status = card.getAttribute('data-status');
        if (status === 'em_processo' || status === 'adotado') {
            const btnEditar = card.querySelector('.botao-acao.editar');
            const btnExcluir = card.querySelector('.botao-acao.excluir');
            
            if (btnEditar) {
                const tooltipMensagem = status === 'em_processo' ? 
                    'Não é possível editar um pet em processo de adoção' : 
                    'Não é possível editar um pet já adotado';
                
                btnEditar.removeAttribute('title');
                btnEditar.disabled = true;
                btnEditar.classList.add('desabilitado');
                
                const wrapper = document.createElement('div');
                wrapper.className = 'tooltip-personalizado';
                
                const tooltipText = document.createElement('span');
                tooltipText.className = 'tooltip-texto';
                tooltipText.textContent = tooltipMensagem;
                
                btnEditar.parentNode.insertBefore(wrapper, btnEditar);
                wrapper.appendChild(btnEditar);
                wrapper.appendChild(tooltipText);
            }
            
            if (btnExcluir) {
                const tooltipMensagem = status === 'em_processo' ? 
                    'Não é possível excluir um pet em processo de adoção' : 
                    'Não é possível excluir um pet já adotado';
                
                btnExcluir.removeAttribute('title');
                btnExcluir.disabled = true;
                btnExcluir.classList.add('desabilitado');
                
                const wrapper = document.createElement('div');
                wrapper.className = 'tooltip-personalizado';
                
                const tooltipText = document.createElement('span');
                tooltipText.className = 'tooltip-texto';
                tooltipText.textContent = tooltipMensagem;
                
                btnExcluir.parentNode.insertBefore(wrapper, btnExcluir);
                wrapper.appendChild(btnExcluir);
                wrapper.appendChild(tooltipText);
            }
        }
    });
}

// Configurar comportamento de modais
function configurarComportamentoModais() {
    document.querySelectorAll('.modal').forEach(modal => {
        modal.addEventListener('show.bs.modal', function() {
            limparBackdrops();
        });
        
        modal.addEventListener('hidden.bs.modal', function() {
            limparBackdrops();
            
            const modaisVisiveis = document.querySelectorAll('.modal.show');
            if (modaisVisiveis.length > 0) {
                document.body.classList.add('modal-open');
            }
        });
    });
    
    // Configurar botão novo pet
    const btnNovoPet = document.getElementById('btnNovoPet');
    if (btnNovoPet) {
        btnNovoPet.addEventListener('click', resetarFormulario);
    }
}

// Função para garantir que elementos de paginação estejam sempre visíveis
function garantirVisibilidadeElementosPaginacao() {
    // Adicionar um MutationObserver para garantir que o seletor de itens por página esteja sempre visível
    const observer = new MutationObserver(function() {
        $('.paginacao-container').css('display', 'block');
        $('.paginacao-info').css('display', 'flex');
        $('.itens-por-pagina').css('display', 'flex');
    });
    
    // Observar mudanças na página
    observer.observe(document.body, { childList: true, subtree: true });
    
    // Corrigir comportamento dos links de paginação
    document.querySelectorAll('.pagination .page-link').forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            var url = this.getAttribute('href');
            if (url) {
                window.location.href = url;
            }
            return false;
        });
    });
    
    // Delegação de eventos para links de paginação que podem ser adicionados dinamicamente
    document.addEventListener('click', function(e) {
        if (e.target.matches('.pagination .page-link') || e.target.closest('.pagination .page-link')) {
            e.preventDefault();
            var link = e.target.matches('.pagination .page-link') ? e.target : e.target.closest('.pagination .page-link');
            var url = link.getAttribute('href');
            if (url) {
                window.location.href = url;
            }
            return false;
        }
    });
}

// Função para configurar elementos do DOM
function configElementosDOM() {
    // Inicializar gerenciamento de itens por página
    gerenciarItensPorPagina();
    
    // Garantir visibilidade de elementos de paginação
    garantirVisibilidadeElementosPaginacao();
    
    // Imediatamente tentar restaurar imagens da sessão anterior
    restaurarImagensPreCarregadas();
    
    // Garantir que as imagens sejam carregadas corretamente
    $(window).on('load', function() {
        if (typeof configurarTratamentoErroImagens === 'function') {
            configurarTratamentoErroImagens();
        }
        if (typeof garantirCarregamentoImagens === 'function') {
            garantirCarregamentoImagens();
        }
        
        // Aplicar classe para indicar que o layout está pronto
        setTimeout(() => {
            $('.pets-grid').addClass('layout-pronto');
        }, 200);
        
        // Remover overlay de carregamento se existir
        removerIndicadorCarregamento();
    });
    
    // Adicionar um atributo data-loaded=true nas imagens após carregarem
    $('.imagem-pet').on('load', function() {
        $(this).attr('data-loaded', 'true');
        $(this).css('display', 'block');
        
        // Esconder o elemento .sem-imagem se existir
        const container = $(this).closest('.container-imagem');
        if (container.find('.sem-imagem').length) {
            container.find('.sem-imagem').hide();
        }
        
        // Salvar na cache de imagens
        const cardId = $(this).closest('.cartao-pet').attr('data-id');
        if (cardId) {
            try {
                const imagensCache = JSON.parse(sessionStorage.getItem('imagensPets') || '{}');
                imagensCache[cardId] = $(this).attr('src');
                sessionStorage.setItem('imagensPets', JSON.stringify(imagensCache));
            } catch (e) {}
        }
    });
}

// Função para configurar eventos
function configurarEventos() {
    // Verificar e configurar eventos de filtro
    if (typeof configurarEventosFiltro === 'function') {
        configurarEventosFiltro();
    }
    
    // Verificar e configurar tooltips para botões desabilitados
    if (typeof configurarTooltipsBotoesDesabilitados === 'function') {
        configurarTooltipsBotoesDesabilitados();
    }
}
