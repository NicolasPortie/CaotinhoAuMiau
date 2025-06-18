
document.addEventListener('DOMContentLoaded', function() {
    
    
    configurarNavbar();
    
    
    if (typeof $ === 'undefined') {
        console.error("jQuery não está carregado!");
        return;
    }
    
    
    if (typeof VMasker === 'undefined') {
        console.error("Vanilla Masker não está carregado! Tentando carregar dinamicamente...");
        
        
        var script = document.createElement('script');
        script.src = '/lib/vanilla-masker/vanilla-masker.js';
        script.onload = function() {
            aplicarMascaras();
        };
        document.head.appendChild(script);
    } else {
        aplicarMascaras();
    }
    
    
    inicializarPagina();
    
    
    const formulario = document.getElementById('formCadastro');
    if (formulario) {
        formulario.addEventListener('submit', async function(e) {
            
            e.preventDefault();
            
            
            
            const senha = document.getElementById('Senha').value;
            const confirmarSenha = document.getElementById('ConfirmarSenha').value;
            
            if (!senha || senha.length < 6) {
                alert("A senha deve ter pelo menos 6 caracteres.");
                document.getElementById('Senha').focus();
                return false;
            }
            
            if (senha !== confirmarSenha) {
                alert("As senhas não conferem.");
                document.getElementById('ConfirmarSenha').focus();
                return false;
            }
            
            
            const cpfComMascara = document.getElementById('cpf').value;
            
            const cpfSemMascara = cpfComMascara.replace(/\D/g, '');
            
            if (cpfSemMascara.length !== 11) {
                alert("O CPF deve ter 11 dígitos.");
                document.getElementById('cpf').focus();
                return false;
            }
            
            
            const email = document.getElementById('email').value;
            if (!verificarEmail(email)) {
                alert("E-mail inválido.");
                document.getElementById('email').focus();
                return false;
            }
            
            
            try {
                const emailExiste = await verificarEmailDuplicado(email);
                if (emailExiste) {
                    alert("Este e-mail já está cadastrado. Por favor, use outro e-mail.");
                    document.getElementById('email').focus();
                    return false;
                }
            } catch (error) {
                console.error("Erro ao verificar email:", error);
                
            }
            
            
            
            
            const cpfOriginal = document.getElementById('cpf').value;
            const telefoneOriginal = document.getElementById('telefone').value;
            const cepOriginal = document.getElementById('cep').value;
            
            
            removerMascarasFormulario();
            
            
            
            
            const botaoEnviar = document.getElementById('btnEnviar');
            if (botaoEnviar) {
                botaoEnviar.disabled = true;
                botaoEnviar.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Processando...';
            }
            
            
            
            setTimeout(function() {
                formulario.submit();
            }, 100);
        });
    } else {
        console.error("Formulário de cadastro não encontrado");
    }
});

function aplicarMascaras() {
    
    const elementoCpf = document.getElementById("cpf");
    if (elementoCpf) {
        VMasker(elementoCpf).maskPattern("999.999.999-99");
    }
    
    
    const elementoTelefone = document.getElementById("telefone");
    if (elementoTelefone) {
        
        function mascaraTelefone(telefone) {
            const valor = telefone.value.replace(/\D/g, '');
            const mascara = valor.length > 10 ? "(99) 99999-9999" : "(99) 9999-9999";
            VMasker(telefone).maskPattern(mascara);
        }
        
        mascaraTelefone(elementoTelefone);
        
        elementoTelefone.addEventListener("input", function() {
            mascaraTelefone(this);
        });
        
    }
    
    const elementoCep = document.getElementById("cep");
    if (elementoCep) {
        VMasker(elementoCep).maskPattern("99999-999");
    }
}

function inicializarPagina() {
    
    aplicarMascarasInput();
    
    configurarVerificacoesCampos();
    
    const cepInput = document.getElementById('cep');
    if (cepInput) {
        cepInput.addEventListener('blur', function() {
            
            const cepSemMascara = this.value.replace(/\D/g, '');
            
            if (cepSemMascara.length === 0) {
                return;
            }
            
            if (cepSemMascara.length !== 8) {
                const feedbackCEP = document.getElementById('cep-feedback');
                if (feedbackCEP) {
                    feedbackCEP.textContent = 'CEP deve ter 8 dígitos';
                    feedbackCEP.className = 'feedback-validacao invalido';
                    feedbackCEP.style.display = 'block';
                }
                return;
            }
            
            
            buscarCEP(cepSemMascara);
        });
    }
    
    
    const inputCpf = document.getElementById('cpf');
    if (inputCpf) {
        inputCpf.addEventListener('focus', function() {
            
            const feedback = document.getElementById('cpf-feedback');
            if (feedback) {
                feedback.textContent = '';
                feedback.className = 'feedback-validacao';
            }
            
            
            const mensagemErro = document.querySelector("span[data-valmsg-for='CPF']");
            if (mensagemErro) {
                mensagemErro.textContent = '';
                mensagemErro.className = 'field-validation-valid';
                mensagemErro.style.display = 'none';
            }
        });
    }
    
    
    const inputCep = document.getElementById('cep');
    if (inputCep) {
        inputCep.addEventListener('focus', function() {
            
            const feedback = document.getElementById('cep-feedback');
            if (feedback) {
                feedback.textContent = '';
                feedback.className = 'feedback-validacao';
                feedback.style.display = 'none';
            }
        });
    }
    
    
    ajustarLayout();
    window.addEventListener('resize', ajustarLayout);
}


function alternarVisibilidadeSenha(campoId) {
    const campo = document.getElementById(campoId);
    if (!campo) {
        console.error(`Campo ${campoId} não encontrado`);
        return;
    }
    
    const tipo = campo.type;
    campo.type = tipo === 'password' ? 'text' : 'password';
    
    
    const botao = document.querySelector(`button[onclick="alternarVisibilidadeSenha('${campoId}')"]`);
    if (botao) {
        const icone = botao.querySelector('i');
        if (icone) {
            icone.className = tipo === 'password' ? 'fas fa-eye-slash' : 'fas fa-eye';
        }
    }
}


function aplicarMascarasInput() {
    
    
    const inputCpf = document.getElementById('cpf');
    if (inputCpf) {
        
        inputCpf.addEventListener('blur', function() {
            formatarCPF(this, this.value);
        });
        
        
        inputCpf.addEventListener('input', function() {
            
            let valorAtual = this.value.replace(/\D/g, '');
            
            if (valorAtual.length > 11) {
                valorAtual = valorAtual.substring(0, 11);
            }
            
            if (valorAtual.length <= 11) {
                if (valorAtual.length <= 3) {
                    this.value = valorAtual;
                } else if (valorAtual.length <= 6) {
                    this.value = valorAtual.substring(0, 3) + '.' + valorAtual.substring(3);
                } else if (valorAtual.length <= 9) {
                    this.value = valorAtual.substring(0, 3) + '.' + valorAtual.substring(3, 6) + '.' + valorAtual.substring(6);
                } else {
                    this.value = valorAtual.substring(0, 3) + '.' + valorAtual.substring(3, 6) + '.' + valorAtual.substring(6, 9) + '-' + valorAtual.substring(9);
                }
            }
            
            verificarCPF(this.value);
        });
    }
    
    const inputTelefone = document.getElementById('telefone');
    if (inputTelefone) {
        inputTelefone.addEventListener('blur', function() {
            formatarTelefone(this, this.value);
        });

        inputTelefone.addEventListener('input', function() {
            formatarTelefone(this, this.value);
        });
    }
    
    
    const inputCep = document.getElementById('cep');
    if (inputCep) {
        inputCep.addEventListener('input', function() {
            formatarCEP(this, this.value);
        });
    }
}

function formatarCPF(elemento, valor) {
    
    const cpfLimpo = valor.replace(/\D/g, '');
    
    if (cpfLimpo.length < 3) {
        elemento.value = cpfLimpo;
        return;
    }
    
    let cpfFormatado = cpfLimpo.substring(0, 3);
    
    if (cpfLimpo.length > 3) {
        cpfFormatado += '.' + cpfLimpo.substring(3, 6);
    }
    if (cpfLimpo.length > 6) {
        cpfFormatado += '.' + cpfLimpo.substring(6, 9);
    }
    if (cpfLimpo.length > 9) {
        cpfFormatado += '-' + cpfLimpo.substring(9, 11);
    }
    
    elemento.value = cpfFormatado;
}

function formatarTelefone(elemento, valor) {
    let telefoneLimpo = valor.replace(/\D/g, '');
    
    if (telefoneLimpo.length > 11) {
        telefoneLimpo = telefoneLimpo.substring(0, 11);
    }
    
    let telefoneFormatado = '';
    
    if (telefoneLimpo.length <= 2) {
        telefoneFormatado = telefoneLimpo;
    } else if (telefoneLimpo.length <= 6) {
        telefoneFormatado = '(' + telefoneLimpo.substring(0, 2) + ') ' + telefoneLimpo.substring(2);
    } else if (telefoneLimpo.length <= 10) {
        telefoneFormatado = '(' + telefoneLimpo.substring(0, 2) + ') ' + telefoneLimpo.substring(2, 6) + '-' + telefoneLimpo.substring(6);
    } else {
        telefoneFormatado = '(' + telefoneLimpo.substring(0, 2) + ') ' + telefoneLimpo.substring(2, 7) + '-' + telefoneLimpo.substring(7);
    }
    
    elemento.value = telefoneFormatado;
}

function formatarCEP(elemento, valor) {
    let cepLimpo = valor.replace(/\D/g, '');
    
    if (cepLimpo.length > 8) {
        cepLimpo = cepLimpo.substring(0, 8);
    }
    
    let cepFormatado = '';
    
    if (cepLimpo.length <= 5) {
        cepFormatado = cepLimpo;
    } else {
        cepFormatado = cepLimpo.substring(0, 5) + '-' + cepLimpo.substring(5);
    }
    
    elemento.value = cepFormatado;
    
    if (cepLimpo.length === 8) {
        const feedback = document.getElementById('cep-feedback');
        if (feedback) {
            feedback.textContent = 'Validando CEP...';
            feedback.className = 'feedback-validacao';
            feedback.style.display = 'block';
        }
    }
}

function buscarCEP(cep) {
    
    
    const feedback = document.getElementById('cep-feedback');
    if (feedback) {
        feedback.textContent = 'Buscando CEP...';
        feedback.className = 'feedback-validacao processando';
        feedback.style.display = 'block';
    }
    
    
    if (cep.length !== 8) {
        console.error("CEP inválido:", cep);
        if (feedback) {
            feedback.textContent = 'CEP deve ter 8 dígitos';
            feedback.className = 'feedback-validacao invalido';
        }
        return;
    }
    
    
    fetch(`https://viacep.com.br/ws/${cep}/json/`)
        .then(response => {
            if (!response.ok) {
                throw new Error("Erro na requisição do CEP");
            }
            return response.json();
        })
        .then(data => {
            
            
            if (data.erro) {
                console.error("CEP não encontrado");
                if (feedback) {
                    feedback.textContent = 'CEP não encontrado';
                    feedback.className = 'feedback-validacao invalido';
                }
                return;
            }
            
            
            if (data.logradouro) document.getElementById('logradouro').value = data.logradouro;
            if (data.bairro) document.getElementById('bairro').value = data.bairro;
            if (data.localidade) document.getElementById('cidade').value = data.localidade;
            if (data.uf) document.getElementById('estado').value = data.uf;
            
            
            document.getElementById('numero').focus();
            
            
            if (feedback) {
                feedback.textContent = 'CEP encontrado';
                feedback.className = 'feedback-validacao valido';
                
                
                setTimeout(() => {
                    feedback.style.display = 'none';
                }, 3000);
            }
        })
        .catch(error => {
            console.error("Erro ao buscar CEP:", error);
            
            
            if (feedback) {
                feedback.textContent = 'Erro ao buscar CEP';
                feedback.className = 'feedback-validacao invalido';
            }
        });
}


function verificarEmail(email) {
    
    if (!email) return false;
    
    
    const regexEmail = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const valido = regexEmail.test(email);
    
    
    const feedback = document.getElementById('email-feedback');
    if (feedback) {
        if (!valido) {
            feedback.textContent = 'E-mail inválido';
            feedback.className = 'feedback-validacao invalido';
        }
        
    }
    
    return valido;
}


const inputEmail = document.getElementById('email');
if (inputEmail) {
    let timeoutId;
    inputEmail.addEventListener('blur', function() {
        clearTimeout(timeoutId);
        timeoutId = setTimeout(async () => {
            if (!campoVazio(this.value)) {
                
                const formatoValido = verificarFormatoEmail(this.value);
                if (formatoValido) {
                    
                    await verificarEmailDuplicado(this.value);
                }
            }
        }, 300);
    });
}


function verificarFormatoEmail(email) {
    
    if (!email) return false;
    
    
    const regexEmail = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const valido = regexEmail.test(email);
    
    
    const feedback = document.getElementById('email-feedback');
    if (feedback) {
        if (!valido) {
            feedback.textContent = 'E-mail inválido';
            feedback.className = 'feedback-validacao invalido';
        }
    }
    
    return valido;
}


async function verificarEmailDuplicado(email) {
    
    
    const feedback = document.getElementById('email-feedback');
    if (feedback) {
        feedback.textContent = 'Verificando e-mail...';
        feedback.className = 'feedback-validacao processando';
    }
    
    try {
        
        const response = await fetch(`/usuario/verificar-email?email=${encodeURIComponent(email)}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        
        if (!response.ok) {
            throw new Error("Erro ao verificar email");
        }
        
        const resultado = await response.json();
        
        
        if (feedback) {
            if (resultado.emailExiste) {
                feedback.textContent = 'Este e-mail já está cadastrado';
                feedback.className = 'feedback-validacao invalido';
            } else {
                feedback.textContent = '';
                feedback.className = 'feedback-validacao';
            }
        }
        
        return resultado.emailExiste;
    } catch (error) {
        console.error("Erro ao verificar se email existe:", error);
        
        
        if (feedback) {
            feedback.textContent = 'Erro ao verificar e-mail';
            feedback.className = 'feedback-validacao invalido';
        }
        
        return false;
    }
}


function campoVazio(valor) {
    return valor === null || valor === undefined || valor.trim() === '';
}

function configurarVerificacoesCampos() {
    
    const inputCPF = document.getElementById('cpf');
    if (inputCPF) {
        let timeoutId;
        inputCPF.addEventListener('blur', function() {
            clearTimeout(timeoutId);
            timeoutId = setTimeout(async () => {
                if (!campoVazio(this.value)) {
                    
                    const formatoValido = verificarFormatoCPF(this.value);
                    if (formatoValido) {
                        
                        await verificarCPFDuplicado(this.value);
                    }
                }
            }, 300);
        });
    }
    
    
    const inputEmail = document.getElementById('email');
    if (inputEmail) {
        inputEmail.addEventListener('blur', function() {
            if (!campoVazio(this.value)) {
                verificarEmail(this.value);
            }
        });
    }
    
    
    const inputSenha = document.getElementById('Senha');
    if (inputSenha) {
        inputSenha.addEventListener('input', function() {
            const senhaValue = this.value;
            
            
            const requirements = {
                length: senhaValue.length >= 8,
                upper: /[A-Z]/.test(senhaValue),
                lower: /[a-z]/.test(senhaValue),
                number: /[0-9]/.test(senhaValue),
                special: /[!@#$%^&*(),.?":{}|<>]/.test(senhaValue)
            };
            
            let passedCount = 0;
            Object.entries(requirements).forEach(([key, passed]) => {
                const indicator = document.querySelector(`[data-requirement="${key}"] i`);
                if (indicator) {
                    indicator.className = passed ? 'fas fa-check' : 'fas fa-times';
                    if (passed) passedCount++;
                }
            });
            
            const strengthPercent = senhaValue.length === 0 ? 0 : (passedCount / 5) * 100;
            
            const strengthBar = document.querySelector('.strength-level');
            if (strengthBar) {
                strengthBar.style.width = `${strengthPercent}%`;
                
                if (strengthPercent === 0) {
                    strengthBar.style.background = 'var(--gray-light)';
                } else if (strengthPercent <= 20) {
                    strengthBar.style.background = 'var(--danger)';
                } else if (strengthPercent <= 40) {
                    strengthBar.style.background = 'var(--warning)';
                } else if (strengthPercent <= 60) {
                    strengthBar.style.background = 'var(--info)';
                } else if (strengthPercent <= 80) {
                    strengthBar.style.background = 'var(--secondary)';
                } else {
                    strengthBar.style.background = 'var(--success)';
                }
            }
            
            const strengthText = document.querySelector('.strength-text strong');
            if (strengthText) {
                if (senhaValue.length === 0) {
                    strengthText.textContent = 'Digite uma senha';
                } else if (strengthPercent <= 20) {
                    strengthText.textContent = 'Muito fraca';
                } else if (strengthPercent <= 40) {
                    strengthText.textContent = 'Fraca';
                } else if (strengthPercent <= 60) {
                    strengthText.textContent = 'Média';
                } else if (strengthPercent <= 80) {
                    strengthText.textContent = 'Forte';
                } else {
                    strengthText.textContent = 'Muito forte';
                }
            }

            const feedback = document.getElementById('senha-feedback');
            if (feedback) {
                if (senhaValue.length === 0) {
                    feedback.textContent = '';
                    feedback.className = 'feedback-validacao';
                } else if (passedCount < 5) {
                    feedback.textContent = 'A senha não atende a todos os requisitos';
                    feedback.className = 'feedback-validacao invalido';
                } else {
                    feedback.textContent = 'Senha válida';
                    feedback.className = 'feedback-validacao valido';
                }
            }
        });
    }
    
    const inputConfirmarSenha = document.getElementById('ConfirmarSenha');
    if (inputConfirmarSenha) {
        inputConfirmarSenha.addEventListener('input', function() {
            const feedback = document.getElementById('confirmarSenha-feedback');
            if (feedback && inputSenha) {
                if (this.value === inputSenha.value) {
                    feedback.textContent = 'Senhas coincidem';
                    feedback.className = 'feedback-validacao valido';
                } else {
                    feedback.textContent = 'Senhas não coincidem';
                    feedback.className = 'feedback-validacao invalido';
                }
            }
        });
    }
}

function ajustarLayout() {
    const larguraTela = window.innerWidth;
    const passoTextos = document.querySelectorAll('.passo-texto');
    
    if (larguraTela < 768) {
        passoTextos.forEach(texto => {
            texto.style.display = 'none';
        });
    } else {
        passoTextos.forEach(texto => {
            texto.style.display = 'block';
        });
    }
}

function removerMascarasFormulario() {
    
    const cpfInput = document.getElementById('cpf');
    if (cpfInput) {
        const cpfFormatado = cpfInput.value;
        
        const cpfLimpo = cpfInput.value.replace(/\D/g, '');
        
        if (cpfLimpo.length === 11) {
            cpfInput.value = cpfLimpo;
        } else {
            console.warn(`CPF "${cpfFormatado}" / "${cpfLimpo}" não possui 11 dígitos`);
        }
    } else {
        console.warn("Campo CPF não encontrado para remover máscara");
    }
    
    
    const telefoneInput = document.getElementById('telefone');
    if (telefoneInput) {
        
        const telefoneFormatado = telefoneInput.value;
        
        
        const telefoneLimpo = telefoneInput.value.replace(/\D/g, '');
        
        if (telefoneLimpo.length >= 10) {
            telefoneInput.value = telefoneLimpo;
        } else {
            console.warn(`Telefone "${telefoneFormatado}" / "${telefoneLimpo}" tem menos de 10 dígitos`);
        }
    } else {
        console.warn("Campo telefone não encontrado para remover máscara");
    }
    
    const cepInput = document.getElementById('cep');
    if (cepInput) {
        
        const cepFormatado = cepInput.value;
        
        
        const cepLimpo = cepInput.value.replace(/\D/g, '');
        
        if (cepLimpo.length === 8) {
            cepInput.value = cepLimpo;
        } else {
            console.warn(`CEP "${cepFormatado}" / "${cepLimpo}" não possui 8 dígitos`);
        }
    } else {
        console.warn("Campo CEP não encontrado para remover máscara");
    }
}

function validarEtapa(numeroEtapa) {
    
    const etapaAtual = document.querySelector(`.etapa[data-etapa="${numeroEtapa}"]`);
    if (!etapaAtual) {
        console.error(`Container da etapa ${numeroEtapa} não encontrado`);
        return false;
    }
    
    const camposObrigatorios = etapaAtual.querySelectorAll('[required]:not([id="complemento"])');
    let todosValidos = true;
    
    camposObrigatorios.forEach(campo => {
        campo.classList.remove('campo-invalido');
        campo.classList.remove('campo-valido');
        
        
        const idFeedback = `${campo.id}-feedback`;
        const feedbackElement = document.getElementById(idFeedback);
        
        
        if (campo.value.trim() === '') {
            todosValidos = false;
            campo.classList.add('campo-invalido');
            
            
            if (feedbackElement) {
                feedbackElement.textContent = 'Este campo é obrigatório';
                feedbackElement.className = 'feedback-validacao invalido';
                feedbackElement.style.display = 'block';
            }
            
        } else {
            
            switch (campo.id) {
                case 'email':
                    if (!verificarEmail(campo.value)) {
                        todosValidos = false;
                        campo.classList.add('campo-invalido');
                        
                        if (feedbackElement) {
                            feedbackElement.textContent = 'Email inválido';
                            feedbackElement.className = 'feedback-validacao invalido';
                            feedbackElement.style.display = 'block';
                        }
                        
                    } else {
                        campo.classList.add('campo-valido');
                    }
                    break;
                    
                case 'cpf':
                    
                    const cpfSemMascara = campo.value.replace(/\D/g, '');
                    
                    if (cpfSemMascara.length !== 11) {
                        todosValidos = false;
                        campo.classList.add('campo-invalido');
                        
                        if (feedbackElement) {
                            feedbackElement.textContent = 'CPF deve ter 11 dígitos';
                            feedbackElement.className = 'feedback-validacao invalido';
                            feedbackElement.style.display = 'block';
                        }
                        
                    } else {
                        campo.classList.add('campo-valido');
                    }
                    break;
                    
                case 'Senha':
                    const senhaValue = campo.value;
                    const requirements = {
                        length: senhaValue.length >= 8,
                        upper: /[A-Z]/.test(senhaValue),
                        lower: /[a-z]/.test(senhaValue),
                        number: /[0-9]/.test(senhaValue),
                        special: /[!@#$%^&*(),.?":{}|<>]/.test(senhaValue)
                    };
                    
                    const passedCount = Object.values(requirements).filter(Boolean).length;
                    
                    if (passedCount < 5) {
                        todosValidos = false;
                        campo.classList.add('campo-invalido');
                        
                        if (feedbackElement) {
                            feedbackElement.textContent = 'A senha deve atender a todos os requisitos';
                            feedbackElement.className = 'feedback-validacao invalido';
                            feedbackElement.style.display = 'block';
                        }
                        
                    } else {
                        campo.classList.add('campo-valido');
                    }
                    break;
                    
                case 'ConfirmarSenha':
                    const senha = document.getElementById('Senha');
                    if (senha && campo.value !== senha.value) {
                        todosValidos = false;
                        campo.classList.add('campo-invalido');
                        
                        if (feedbackElement) {
                            feedbackElement.textContent = 'As senhas não conferem';
                            feedbackElement.className = 'feedback-validacao invalido';
                            feedbackElement.style.display = 'block';
                        }
                        
                    } else {
                        campo.classList.add('campo-valido');
                    }
                    break;
                    
                case 'cep':
                    const cepSemMascara = campo.value.replace(/\D/g, '');
                    
                    if (cepSemMascara.length !== 8) {
                        todosValidos = false;
                        campo.classList.add('campo-invalido');
                        
                        if (feedbackElement) {
                            feedbackElement.textContent = 'CEP deve ter 8 dígitos';
                            feedbackElement.className = 'feedback-validacao invalido';
                            feedbackElement.style.display = 'block';
                        }
                        
                    } else {
                        campo.classList.add('campo-valido');
                    }
                    break;
                    
                default:
                    campo.classList.add('campo-valido');
                    break;
            }
        }
    });
    
    if (!todosValidos) {
        const primeiroInvalido = etapaAtual.querySelector('.campo-invalido');
        if (primeiroInvalido) {
            primeiroInvalido.focus();
        }
        
        return false;
    }
    
    return true;
}

function proximaEtapa() {
    
    const etapaAtual = document.querySelector('.etapa.ativo');
    if (!etapaAtual) {
        console.error("Etapa atual não encontrada");
        return;
    }
    
    const numeroEtapaAtual = parseInt(etapaAtual.getAttribute('data-etapa'));
    const proximaEtapaNumero = numeroEtapaAtual + 1;
    
    
    
    const etapaValida = validarEtapa(numeroEtapaAtual);
    if (!etapaValida) {
        return;
    }
    
    
    const proximaEtapa = document.querySelector(`.etapa[data-etapa="${proximaEtapaNumero}"]`);
    if (!proximaEtapa) {
        console.error("Próxima etapa não encontrada");
        return;
    }
    
    
    const indicadorProgresso = document.querySelector('.passos-progresso');
    if (indicadorProgresso) {
        indicadorProgresso.setAttribute('data-etapa', proximaEtapaNumero);
        
        
        const proximoPasso = document.querySelector(`.passo[data-passo="${proximaEtapaNumero}"]`);
        if (proximoPasso) {
            proximoPasso.classList.add('ativo');
        }
    }
    
    
    etapaAtual.classList.remove('ativo');
    proximaEtapa.classList.add('ativo');
    
    
    const botoesNavegacao = document.querySelector('.botoes-navegacao');
    if (botoesNavegacao) {
        botoesNavegacao.setAttribute('data-etapa-atual', proximaEtapaNumero);
        
        
        const botaoVoltar = document.getElementById('botaoVoltar');
        if (botaoVoltar) {
            botaoVoltar.style.visibility = proximaEtapaNumero > 1 ? 'visible' : 'hidden';
        }
        
        
        const botaoProximo = document.getElementById('botaoProximo');
        const botaoEnviar = document.getElementById('botaoEnviar');
        
        if (proximaEtapaNumero === 3) {
            if (botaoProximo) botaoProximo.classList.add('oculto');
            if (botaoEnviar) botaoEnviar.classList.remove('oculto');
        } else {
            if (botaoProximo) botaoProximo.classList.remove('oculto');
            if (botaoEnviar) botaoEnviar.classList.add('oculto');
        }
    }
    
    
    const contedorLogin = document.querySelector('.contedor-login');
    if (contedorLogin) {
        contedorLogin.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    }
}

function anteriorEtapa() {
    
    
    const etapaAtual = document.querySelector('.etapa.ativo');
    if (!etapaAtual) {
        console.error("Etapa atual não encontrada");
        return;
    }
    
    const numeroEtapaAtual = parseInt(etapaAtual.getAttribute('data-etapa'));
    
    if (numeroEtapaAtual <= 1) {
        return;
    }
    
    const etapaAnteriorNumero = numeroEtapaAtual - 1;
    
    
    const etapaAnterior = document.querySelector(`.etapa[data-etapa="${etapaAnteriorNumero}"]`);
    if (!etapaAnterior) {
        console.error("Etapa anterior não encontrada");
        return;
    }
    
    
    const indicadorProgresso = document.querySelector('.passos-progresso');
    if (indicadorProgresso) {
        indicadorProgresso.setAttribute('data-etapa', etapaAnteriorNumero);
        
        
        const passoAtual = document.querySelector(`.passo[data-passo="${numeroEtapaAtual}"]`);
        if (passoAtual) {
            passoAtual.classList.remove('ativo');
        }
    }
    
    
    etapaAtual.classList.remove('ativo');
    etapaAnterior.classList.add('ativo');
    
    
    const botoesNavegacao = document.querySelector('.botoes-navegacao');
    if (botoesNavegacao) {
        botoesNavegacao.setAttribute('data-etapa-atual', etapaAnteriorNumero);
        
        
        const botaoVoltar = document.getElementById('botaoVoltar');
        if (botaoVoltar) {
            botaoVoltar.style.visibility = etapaAnteriorNumero > 1 ? 'visible' : 'hidden';
        }
        
        
        const botaoProximo = document.getElementById('botaoProximo');
        const botaoEnviar = document.getElementById('botaoEnviar');
        
        if (botaoProximo) botaoProximo.classList.remove('oculto');
        if (botaoEnviar) botaoEnviar.classList.add('oculto');
    }
    
    
    const contedorLogin = document.querySelector('.contedor-login');
    if (contedorLogin) {
        contedorLogin.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    }
}


function enviarFormulario() {
    
    
    const etapaAtual = document.querySelector('.etapa.ativo');
    if (!etapaAtual) {
        console.error("Etapa atual não encontrada");
        return;
    }
    
    const numeroEtapaAtual = parseInt(etapaAtual.getAttribute('data-etapa'));
    const etapaValida = validarEtapa(numeroEtapaAtual);
    if (!etapaValida) {
        return;
    }
    
    
    const checkboxTermos = document.getElementById('aceitarTermos');
    if (checkboxTermos && !checkboxTermos.checked) {
        alert("Você precisa aceitar os Termos de Uso e Política de Privacidade para continuar.");
        checkboxTermos.focus();
        return;
    }
    
    
    
    removerMascarasFormulario();
    
    
    const botaoEnviar = document.getElementById('botaoEnviar');
    if (botaoEnviar) {
        botaoEnviar.disabled = true;
        botaoEnviar.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Processando...';
    }
    
    
    const formulario = document.getElementById('formCadastro');
    if (formulario) {
        formulario.submit();
    } else {
        console.error("Formulário não encontrado");
    }
}

function verificarCPF(cpf) {
    
    
}


function verificarFormatoCPF(cpf) {
    
    if (!cpf) return false;
    
    
    const cpfLimpo = cpf.replace(/\D/g, '');
    
    const valido = cpfLimpo.length === 11;
    
    const feedback = document.getElementById('cpf-feedback');
    if (feedback) {
        if (!valido) {
            feedback.textContent = 'CPF deve ter 11 dígitos';
            feedback.className = 'feedback-validacao invalido';
        }
    }
    
    return valido;
}


async function verificarCPFDuplicado(cpf) {
    
    
    const feedback = document.getElementById('cpf-feedback');
    if (feedback) {
        feedback.textContent = 'Verificando CPF...';
        feedback.className = 'feedback-validacao processando';
    }
    
    try {
        
        const response = await fetch(`/usuario/verificar-cpf?cpf=${encodeURIComponent(cpf)}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        
        if (!response.ok) {
            throw new Error("Erro ao verificar CPF");
        }
        
        const resultado = await response.json();
        
        
        if (feedback) {
            if (resultado.cpfExiste) {
                feedback.textContent = 'Este CPF já está cadastrado';
                feedback.className = 'feedback-validacao invalido';
            } else {
                feedback.textContent = '';
                feedback.className = 'feedback-validacao';
            }
        }
        
        return resultado.cpfExiste;
    } catch (error) {
        console.error("Erro ao verificar se CPF existe:", error);
        
        
        if (feedback) {
            feedback.textContent = 'Erro ao verificar CPF';
            feedback.className = 'feedback-validacao invalido';
        }
        
        return false;
    }
}


const inputTelefone = document.getElementById('telefone');
if (inputTelefone) {
    let timeoutId;
    inputTelefone.addEventListener('blur', function() {
        clearTimeout(timeoutId);
        timeoutId = setTimeout(() => {
            if (!campoVazio(this.value)) {
                verificarFormatoTelefone(this.value);
            }
        }, 300);
    });
}


function verificarFormatoTelefone(telefone) {
    
    if (!telefone) return false;
    
    
    const telefoneLimpo = telefone.replace(/\D/g, '');
    
    const valido = telefoneLimpo.length >= 10;
    
    const feedback = document.getElementById('telefone-feedback');
    if (feedback) {
        if (!valido) {
            feedback.textContent = 'Telefone inválido';
            feedback.className = 'feedback-validacao invalido';
        } else {
            feedback.textContent = '';
            feedback.className = 'feedback-validacao';
        }
    }
    
    return valido;
}


function configurarNavbar() {
    
    window.addEventListener('scroll', function() {
        const navbar = document.querySelector('.navbar');
        if (window.scrollY > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    });
} 