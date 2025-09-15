document.addEventListener('DOMContentLoaded', () => {
    inicializarModais();
    inicializarAbas();
    inicializarScrollTopo();
    inicializarUploadAvatar();
    inicializarMascarasInput();
    inicializarBuscaCep();
    inicializarNotificacoes();
    inicializarAlternarSenha();
    inicializarForcaSenha();
    inicializarConferenciaSenha();
    inicializarFormularioPerfil();
    inicializarFormularioSenha();
    inicializarCartoesContato();
    formatarValoresExibicao();
    inicializarConfirmacaoRemoverFoto();
  });
  function abrirModal(id) {
    document.getElementById(id)?.classList.add('is-open');
    document.body.style.overflow = 'hidden';
  }
  function fecharModal(id) {
    document.getElementById(id)?.classList.remove('is-open');
    document.body.style.overflow = '';
  }
  function inicializarModais() {
    document.addEventListener('keydown', e => {
      if (e.key === 'Escape') {
        document.querySelectorAll('.modal.is-open').forEach(modal => {
          modal.classList.remove('is-open');
          document.body.style.overflow = '';
        });
      }
    });
  }
  function inicializarAbas() {
    const tabsContainer = document.querySelector('.tabs');
    if (!tabsContainer) return;
    tabsContainer.addEventListener('click', e => {
      const btn = e.target.closest('.tabs__button');
      if (!btn) return;
      const tabId = btn.dataset.tab;
      document.querySelectorAll('.tabs__button').forEach(b => b.classList.remove('tabs__button--active'));
      document.querySelectorAll('.tabs__panel').forEach(p => p.classList.remove('tabs__panel--active'));
      btn.classList.add('tabs__button--active');
      document.getElementById(tabId)?.classList.add('tabs__panel--active');
    });
  }
  function inicializarScrollTopo() {
    const btn = document.getElementById('scrollTop');
    if (!btn) return;
    window.addEventListener('scroll', () => {
      if (window.scrollY > 300) {
        btn.classList.add('ativo');
      } else {
        btn.classList.remove('ativo');
      }
    });
    btn.addEventListener('click', () => {
      window.scrollTo({
        top: 0,
        behavior: 'smooth'
      });
    });
  }
  function inicializarUploadAvatar() {
    const input = document.getElementById('uploadFoto');
    if (!input) return;
    input.addEventListener('change', uploadFotoPerfil);
  }
  function inicializarMascarasInput() {
    const cpfInput = document.getElementById('cpf');
    if (cpfInput) {
        IMask(cpfInput, {
            mask: '000.000.000-00',
            lazy: false
        });
    }
    const phoneInput = document.getElementById('telefone');
    if (phoneInput) {
        IMask(phoneInput, {
            mask: '(00) 00000-0000',
            lazy: false
        });
    }
    const cepInput = document.getElementById('cep');
    if (cepInput) {
        IMask(cepInput, {
            mask: '00000-000',
            lazy: false
        });
    }
  }
  function inicializarBuscaCep() {
    const cepInput = document.getElementById('cep');
    const btnBuscarCep = document.getElementById('btnBuscarCep');
    if (!cepInput) return;
    const buscarCep = async () => {
      const cep = cepInput.value.replace(/\D/g, '');
      if (cep.length === 0) {
        mostrarNotificacao('Digite um CEP', 'error');
        return;
      }
      if (btnBuscarCep) {
        btnBuscarCep.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Buscando...';
        btnBuscarCep.disabled = true;
      }
      try {
        const res = await fetch(`https://viacep.com.br/ws/${cep}/json/`);
        const data = await res.json();
        if (!data.erro) {
          const fieldMapping = {
            'logradouro': data.logradouro,
            'bairro': data.bairro,
            'cidade': data.localidade,
            'estado': data.uf
          };
          Object.keys(fieldMapping).forEach(field => {
            const el = document.getElementById(field);
            if (el && fieldMapping[field]) {
              el.value = fieldMapping[field];
            }
          });
          document.getElementById('numero')?.focus();
          mostrarNotificacao('EndereÃ§o encontrado com sucesso!', 'success');
        } else {
          mostrarNotificacao('CEP nÃ£o encontrado', 'error');
        }
      } catch (err) {
        mostrarNotificacao('Erro ao buscar o CEP', 'error');
      } finally {
        if (btnBuscarCep) {
          btnBuscarCep.innerHTML = '<i class="fas fa-search"></i> Buscar CEP';
          btnBuscarCep.disabled = false;
        }
      }
    };
    if (btnBuscarCep) {
      btnBuscarCep.addEventListener('click', buscarCep);
    }
    cepInput.addEventListener('blur', function() {
      const cep = this.value.replace(/\D/g, '');
      if (cep.length === 8) {
        buscarCep();
      }
    });
  }
  function mostrarNotificacao(message, type = 'info') {
    const notification = document.getElementById('notification');
    const text = document.getElementById('notification-text');
    if (!notification || !text) return;
    text.textContent = message;
    notification.className = 'notification';
    notification.classList.add(`notification--${type}`, 'visivel');
    const timeout = setTimeout(() => {
      notification.classList.remove('visivel');
    }, 5000);
    notification.querySelector('.notification__close')?.addEventListener('click', () => {
      clearTimeout(timeout);
      notification.classList.remove('visivel');
    });
  }
  function inicializarNotificacoes() {
    const flashMessage = document.getElementById('flashMessage');
    const flashType = document.getElementById('flashType');
    if (flashMessage && flashMessage.value) {
      mostrarNotificacao(flashMessage.value, flashType?.value || 'info');
    }
  }
  function inicializarAlternarSenha() {
    document.querySelectorAll('.toggle-password').forEach(btn => {
      btn.addEventListener('click', () => {
        const inputId = btn.dataset.toggle;
        const input = document.getElementById(inputId);
        if (!input) return;
        const type = input.type === 'password' ? 'text' : 'password';
        input.type = type;
        const icon = btn.querySelector('i');
        if (icon) {
          icon.className = type === 'text' ? 'fas fa-eye-slash' : 'fas fa-eye';
        }
      });
    });
  }
  function inicializarForcaSenha() {
    const pwdInput = document.getElementById('novaSenha');
    if (!pwdInput) return;
    pwdInput.addEventListener('input', () => {
        const value = pwdInput.value;
          const requirements = {
            length: value.length > 0,
            upper: false,
            lower: false,
            number: false,
            special: false
        };
        let passedCount = 0;
        Object.entries(requirements).forEach(([key, passed]) => {
            const indicator = document.querySelector(`[data-requirement="${key}"] i`);
            if (indicator) {
                indicator.className = passed ? 'fas fa-check' : 'fas fa-times';
                if (passed) passedCount++;
            }
        });
        const strengthPercent = value.length === 0 ? 0 : (passedCount / 5) * 100;
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
            if (value.length === 0) {
                strengthText.textContent = 'Digite uma senha';
            } else if (strengthPercent <= 20) {
                strengthText.textContent = 'Muito fraca';
            } else if (strengthPercent <= 40) {
                strengthText.textContent = 'Fraca';
            } else if (strengthPercent <= 60) {
                strengthText.textContent = 'MÃ©dia';
            } else if (strengthPercent <= 80) {
                strengthText.textContent = 'Forte';
            } else {
                strengthText.textContent = 'Muito forte';
            }
        }
    });
  }
  function inicializarConferenciaSenha() {
    const passwordInput = document.getElementById('novaSenha');
    const confirmInput = document.getElementById('confirmarSenha');
    const matchIndicator = document.querySelector('.password-match');
    if (!passwordInput || !confirmInput || !matchIndicator) return;
    function verificarCorrespondencia() {
      const password = passwordInput.value;
      const confirm = confirmInput.value;
      if (confirm === '') {
        matchIndicator.innerHTML = '';
        return;
      }
      const matches = password === confirm;
      matchIndicator.innerHTML = matches
        ? '<i class="fas fa-check" style="color:var(--success)"></i> Senhas coincidem'
        : '<i class="fas fa-times" style="color:var(--danger)"></i> Senhas nÃ£o coincidem';
    }
    confirmInput.addEventListener('input', verificarCorrespondencia);
    passwordInput.addEventListener('input', verificarCorrespondencia);
  }
  function inicializarFormularioPerfil() {
    const form = document.getElementById('formEditarPerfil');
    if (!form) return;
    form.addEventListener('submit', e => {
      e.preventDefault();
      // Remover mÃ¡scaras antes da validaÃ§Ã£o
      const cepInput = document.getElementById('cep');
      const telefoneInput = document.getElementById('telefone');
      const cpfInput = document.getElementById('cpf');
      if (cepInput) {
        cepInput.value = cepInput.value.replace(/\D/g, '');
      }
      if (telefoneInput) {
        telefoneInput.value = telefoneInput.value.replace(/\D/g, '');
      }
      if (cpfInput) {
        cpfInput.value = cpfInput.value.replace(/\D/g, '');
      }
      const requiredFields = form.querySelectorAll('[required]');
      let hasErrors = false;
      requiredFields.forEach(field => {
        if (!field.value.trim()) {
          field.classList.add('error');
          hasErrors = true;
        } else {
          field.classList.remove('error');
        }
      });
      if (hasErrors) {
        mostrarNotificacao('Preencha todos os campos obrigatÃ³rios', 'error');
        return;
      }
      form.submit();
    });
  }
  function inicializarFormularioSenha() {
    const form = document.getElementById('formAlterarSenha');
    const btnAlterar = document.getElementById('btnAlterarSenha');
    const senhaAtual = document.getElementById('senhaAtual');
    const novaSenha = document.getElementById('novaSenha');
    const confirmarSenha = document.getElementById('confirmarSenha');
    if (!form || !btnAlterar) return;
    // FunÃ§Ã£o para validar e habilitar/desabilitar o botÃ£o
    function validarFormulario() {
      const senhaAtualPreenchida = senhaAtual && senhaAtual.value.trim() !== '';
      const novaSenhaPreenchida = novaSenha && novaSenha.value.trim() !== '';
      const confirmarSenhaPreenchida = confirmarSenha && confirmarSenha.value.trim() !== '';
      const senhasIguais = novaSenha && confirmarSenha && novaSenha.value === confirmarSenha.value;
      const formularioValido = senhaAtualPreenchida && novaSenhaPreenchida && confirmarSenhaPreenchida && senhasIguais;
      btnAlterar.disabled = !formularioValido;
    }
    // Adicionar listeners para todos os campos
    [senhaAtual, novaSenha, confirmarSenha].forEach(input => {
      if (input) {
        input.addEventListener('input', validarFormulario);
        input.addEventListener('blur', validarFormulario);
      }
    });
    // ValidaÃ§Ã£o inicial
    validarFormulario();
    form.addEventListener('submit', async e => {
      e.preventDefault();
      const currentPassword = senhaAtual.value;
      const newPassword = novaSenha.value;
      const confirmPassword = confirmarSenha.value;
      if (!currentPassword) {
        mostrarNotificacao('Digite sua senha atual', 'error');
        return;
      }
      if (!newPassword) {
        mostrarNotificacao('Digite a nova senha', 'error');
        return;
      }
      if (newPassword !== confirmPassword) {
        mostrarNotificacao('As senhas nÃ£o coincidem', 'error');
        return;
      }
      // Desabilitar botÃ£o durante o envio
      btnAlterar.disabled = true;
      btnAlterar.innerHTML = '<i class="fas fa-spinner fa-spin"></i> <span>Alterando...</span>';
      try {
        const formData = new FormData(form);
        const response = await fetch('/usuario/perfil/alterar-senha', {
          method: 'POST',
          body: formData,
          headers: {
            'X-Requested-With': 'XMLHttpRequest'
          }
        });
        const result = await response.json();
        if (result.success) {
          // Fechar modal e mostrar sucesso
          document.getElementById('modalAlterarSenha').style.display = 'none';
          mostrarNotificacao(result.message || 'Senha alterada com sucesso!', 'success');
          // Limpar formulÃ¡rio
          form.reset();
        } else {
          // Mostrar erro no modal
          mostrarNotificacao(result.message || 'Erro ao alterar senha', 'error');
        }
      } catch (error) {
        mostrarNotificacao('Erro de conexÃ£o ao alterar senha', 'error');
      } finally {
        // Reabilitar botÃ£o
        btnAlterar.disabled = false;
        btnAlterar.innerHTML = '<i class="fas fa-check"></i> <span>Confirmar AlteraÃ§Ã£o</span>';
        validarFormulario(); // Revalidar para manter estado correto
      }
    });
  }
  function inicializarCartoesContato() {
    const cards = document.querySelectorAll('.contact-card');
    cards.forEach((card, index) => {
      setTimeout(() => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        setTimeout(() => {
          card.style.transition = 'all 0.5s ease';
          card.style.opacity = '1';
          card.style.transform = 'translateY(0)';
        }, 100);
      }, index * 150);
    });
  }
  function inicializarConfirmacaoRemoverFoto() {
    const btnRemover = document.getElementById('btnRemoverFoto');
    const modalConfirmar = document.getElementById('modalConfirmarRemoverFoto');
    const btnConfirmar = document.getElementById('btnConfirmarRemoverFoto');
    if (btnRemover && modalConfirmar) {
      btnRemover.addEventListener('click', e => {
        e.preventDefault();
        abrirModal('modalConfirmarRemoverFoto');
      });
    }
    if (btnConfirmar) {
      btnConfirmar.addEventListener('click', e => {
        e.preventDefault();
        removerFotoPerfil();
      });
    }
  }
  async function removerFotoPerfil() {
    const removeBtn = document.querySelector('.perfil__avatar-remove');
    if (!removeBtn) return;
    const originalHTML = removeBtn.innerHTML;
    removeBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
    removeBtn.style.pointerEvents = 'none';
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    try {
      const response = await fetch('/usuario/perfil/remover-foto', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
          'X-Requested-With': 'XMLHttpRequest'
        },
        body: `__RequestVerificationToken=${token}`
      });
      const data = response.redirected ? null : await response.json();
      if (response.redirected) {
        window.location.href = response.url;
        return;
      }
      if (data && !data.success) {
        mostrarNotificacao(data.message || 'Erro ao remover foto de perfil', 'error');
        removeBtn.innerHTML = originalHTML;
        removeBtn.style.pointerEvents = 'auto';
        return;
      }
      const container = document.querySelector('.perfil__avatar');
      const img = container.querySelector('img');
      if (img) {
        const nome = document.querySelector('.perfil__name').textContent.trim();
        const inicial = nome.charAt(0).toUpperCase();
        const initialAvatar = document.createElement('div');
        initialAvatar.className = 'perfil__avatar-initial';
        const spanElement = document.createElement('span');
        spanElement.textContent = inicial;
        initialAvatar.innerHTML = '';
        initialAvatar.appendChild(spanElement);
        img.replaceWith(initialAvatar);
      }
      container.querySelector('.perfil__avatar-upload')?.remove();
      container.querySelector('.perfil__avatar-remove')?.remove();
      mostrarNotificacao('Foto de perfil removida com sucesso!', 'success');
      fecharModal('modalConfirmarRemoverFoto');
    } catch (error) {
      mostrarNotificacao('Erro de conexÃ£o ao remover foto de perfil', 'error');
      removeBtn.innerHTML = originalHTML;
      removeBtn.style.pointerEvents = 'auto';
    }
  }
  function formatarValoresExibicao() {
    const cpfValue = document.querySelector('[data-campo="cpf"] .perfil__info-value');
    if (cpfValue) {
        const cpf = cpfValue.textContent.replace(/\D/g, '');
        if (cpf.length === 11) {
            cpfValue.textContent = cpf.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
        }
    }
    const phoneValue = document.querySelector('[data-campo="telefone"] .perfil__info-value');
    if (phoneValue) {
        const phone = phoneValue.textContent.replace(/\D/g, '');
        if (phone.length === 11) {
            phoneValue.textContent = phone.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
        }
    }
    const cepValue = document.querySelector('[data-campo="cep"] .perfil__info-value');
    if (cepValue) {
        const cep = cepValue.textContent.replace(/\D/g, '');
        if (cep.length === 8) {
            cepValue.textContent = cep.replace(/(\d{5})(\d{3})/, '$1-$2');
        }
    }
  }
    async function uploadFotoPerfil(event) {
    const file = event.target.files[0];
    if (!file) return;
    if (file.size > 5 * 1024 * 1024) {
        mostrarNotificacao('A imagem deve ter atÃ© 5MB', 'error');
        event.target.value = '';
        return;
    }
    if (!file.type.startsWith('image/')) {
        mostrarNotificacao('Por favor, selecione um arquivo de imagem vÃ¡lido', 'error');
        event.target.value = '';
        return;
    }
    const formData = new FormData();
    formData.append('FotoPerfil', file);
    formData.append('__RequestVerificationToken', document.querySelector('input[name="__RequestVerificationToken"]').value);
    const uploadBtn = document.querySelector('.perfil__avatar-upload');
    if (uploadBtn) {
        uploadBtn.querySelector('i').className = 'fas fa-spinner fa-spin';
    }
    try {
        const res = await fetch('/usuario/perfil/atualizarFoto', {
            method: 'POST',
            body: formData,
            headers: {'X-Requested-With': 'XMLHttpRequest'}
        });
        const data = await res.json();
        if (data.success) {
            const reader = new FileReader();
            reader.onload = e => {
                let container = document.querySelector('.perfil__avatar');
                let initialAvatar = container.querySelector('.perfil__avatar-initial');
                let img = container.querySelector('img');
                if (initialAvatar && !img) {
                    initialAvatar.remove();
                    img = document.createElement('img');
                    img.alt = 'Foto de perfil';
                    container.prepend(img);
                }
                if (img) {
                    img.src = e.target.result;
                }
                if (!container.querySelector('.perfil__avatar-upload')) {
                    const novoUploadBtn = document.createElement('div');
                    novoUploadBtn.className = 'perfil__avatar-upload';
                    novoUploadBtn.setAttribute('onclick', "document.getElementById('uploadFoto').click()");
                    novoUploadBtn.setAttribute('title', 'Alterar foto');
                    novoUploadBtn.innerHTML = '<i class="fas fa-camera"></i>';
                    container.appendChild(novoUploadBtn);
                }
                if (!container.querySelector('.perfil__avatar-remove')) {
                    const removeBtn = document.createElement('div');
                    removeBtn.className = 'perfil__avatar-remove';
                    removeBtn.setAttribute('onclick', "abrirModal('modalConfirmarRemoverFoto')");
                    removeBtn.setAttribute('title', 'Remover foto');
                    removeBtn.innerHTML = '<i class="fas fa-trash"></i>';
                    container.appendChild(removeBtn);
                }
            };
            reader.readAsDataURL(file);
            mostrarNotificacao('Foto de perfil atualizada com sucesso!', 'success');
        } else {
            mostrarNotificacao(data.message || 'Erro ao atualizar a foto de perfil', 'error');
        }
    } catch (err) {
        mostrarNotificacao('Erro de conexÃ£o ao atualizar a foto de perfil', 'error');
    } finally {
        const finalUploadBtn = document.querySelector('.perfil__avatar-upload');
        if (finalUploadBtn) {
            finalUploadBtn.querySelector('i').className = 'fas fa-camera';
        }
    }
  }
  
