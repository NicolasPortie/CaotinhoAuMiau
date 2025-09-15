
document.addEventListener('DOMContentLoaded', function() {
    
    
    configurarNavbar();
    
    
    
    if (typeof VMasker === 'undefined') {
        
        
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
            
            if (!senha || senha.trim() === '') {
                console.warn("A senha é obrigatória.");
                document.getElementById('Senha').focus();
                return false;
            }
            
            if (senha !== confirmarSenha) {
                console.warn("As senhas não conferem.");
                document.getElementById('ConfirmarSenha').focus();
                return false;
            }
            
            
            const cpfComMascara = document.getElementById('cpf').value;
            const cpfSemMascara = cpfComMascara.replace(/\D/g, '');
            
            if (cpfSemMascara.length === 0) {
                console.warn("CPF é obrigatório.");
                document.getElementById('cpf').focus();
                return false;
            }
            
            const email = document.getElementById('email').value;
            if (!email || email.trim() === '') {
                console.warn("E-mail é obrigatório.");
                document.getElementById('email').focus();
                return false;
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
            
            if (cepSemMascara.length === 0) {
                const feedbackCEP = document.getElementById('cep-feedback');
                if (feedbackCEP) {
                    feedbackCEP.textContent = 'CEP é obrigatório';
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

async function buscarCEP(cep) {

    const feedback = document.getElementById('cep-feedback');
    if (feedback) {
        feedback.textContent = 'Buscando CEP...';
        feedback.className = 'feedback-validacao processando';
        feedback.style.display = 'block';
    }

    if (cep.length === 0) {
        if (feedback) {
            feedback.textContent = 'CEP é obrigatório';
            feedback.className = 'feedback-validacao invalido';
        }
        return;
    }

    try {
        const response = await fetch(`https://viacep.com.br/ws/${cep}/json/`);
        if (!response.ok) {
            throw new Error('Erro na requisição do CEP');
        }

        const data = await response.json();

        if (data.erro) {
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
    } catch (error) {
        if (feedback) {
            feedback.textContent = 'Erro ao buscar CEP';
            feedback.className = 'feedback-validacao invalido';
        }
    }
}




// Validação de email removida - será feita no controller




// Função removida - verificação será feita no controller


function campoVazio(valor) {
    return valor === null || valor === undefined || valor.trim() === '';
}

function configurarVerificacoesCampos() {
    
    // Validação de CPF removida - será feita no controller
    
    // Validação de email removida - será feita no controller
    
    
    const inputSenha = document.getElementById('Senha');
    if (inputSenha) {
        inputSenha.addEventListener('input', function() {
            const senhaValue = this.value;
            
            
            const feedback = document.getElementById('senha-feedback');
            if (feedback) {
                if (senhaValue.length === 0) {
                    feedback.textContent = '';
                    feedback.className = 'feedback-validacao';
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
        }
    } else {
    }
    
    
    const telefoneInput = document.getElementById('telefone');
    if (telefoneInput) {
        
        const telefoneFormatado = telefoneInput.value;
        
        
        const telefoneLimpo = telefoneInput.value.replace(/\D/g, '');
        
        if (telefoneLimpo.length >= 10) {
            telefoneInput.value = telefoneLimpo;
        } else {
        }
    } else {
    }
    
    const cepInput = document.getElementById('cep');
    if (cepInput) {
        
        const cepFormatado = cepInput.value;
        
        
        const cepLimpo = cepInput.value.replace(/\D/g, '');
        
        if (cepLimpo.length === 8) {
            cepInput.value = cepLimpo;
        } else {
        }
    } else {
    }
}

function validarEtapa(numeroEtapa) {
    const etapaAtual = document.querySelector(`.etapa[data-etapa="${numeroEtapa}"]`);
    if (!etapaAtual) {
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
                            const emailVazio = !campo.value.trim() || !campo.value.includes('@');
                    if (emailVazio) {
                        todosValidos = false;
                        campo.classList.add('campo-invalido');
                        
                        if (feedbackElement) {
                            feedbackElement.textContent = 'E-mail é obrigatório';
                            feedbackElement.className = 'feedback-validacao invalido';
                            feedbackElement.style.display = 'block';
                        }
                        
                    } else {
                        campo.classList.add('campo-valido');
                    }
                    break;
                    
                case 'cpf':
                    if (campo.value.trim() === '') {
                        todosValidos = false;
                        campo.classList.add('campo-invalido');
                        
                        if (feedbackElement) {
                            feedbackElement.textContent = 'CPF é obrigatório';
                            feedbackElement.className = 'feedback-validacao invalido';
                            feedbackElement.style.display = 'block';
                        }
                        
                    } else {
                        campo.classList.add('campo-valido');
                    }
                    break;
                    
                case 'Senha':
                    const senhaValue = campo.value;
                    
                    if (senhaValue.length === 0) {
                        todosValidos = false;
                        campo.classList.add('campo-invalido');
                        
                        if (feedbackElement) {
                            feedbackElement.textContent = 'Senha é obrigatória';
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
                    
                    if (cepSemMascara.length === 0) {
                        todosValidos = false;
                        campo.classList.add('campo-invalido');
                        
                        if (feedbackElement) {
                            feedbackElement.textContent = 'CEP é obrigatório';
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
        return;
    }
    
    const numeroEtapaAtual = parseInt(etapaAtual.getAttribute('data-etapa'));
    
    if (numeroEtapaAtual <= 1) {
        return;
    }
    
    const etapaAnteriorNumero = numeroEtapaAtual - 1;
    
    
    const etapaAnterior = document.querySelector(`.etapa[data-etapa="${etapaAnteriorNumero}"]`);
    if (!etapaAnterior) {
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
        return;
    }

    const numeroEtapaAtual = parseInt(etapaAtual.getAttribute('data-etapa'));

    const etapaValida = validarEtapa(numeroEtapaAtual);
    if (!etapaValida) {
        return;
    }
    
    const checkboxTermos = document.getElementById('aceitarTermos');
    if (checkboxTermos && !checkboxTermos.checked) {
        console.warn("Você precisa aceitar os Termos de Uso e Política de Privacidade para continuar.");
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
    }
}

function verificarCPF(cpf) {
    // Função removida - validação de CPF desabilitada
}

// Função de teste para submeter diretamente
function testeEnviarFormulario() {
    const formulario = document.getElementById('formCadastro');
    if (formulario) {
        formulario.submit();
    }
}




// Função removida - verificação será feita no controller


const inputTelefone = document.getElementById('telefone');
if (inputTelefone) {
    inputTelefone.addEventListener('blur', function() {
        // Validação removida - será feita no controller
    });
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