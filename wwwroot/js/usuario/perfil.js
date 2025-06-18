

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
    inicializarHoverSecao();
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
    document.querySelectorAll('.tabs__button').forEach(btn => {
      btn.addEventListener('click', () => {
        const tabId = btn.dataset.tab;
        document.querySelectorAll('.tabs__button').forEach(b => b.classList.remove('tabs__button--active'));
        document.querySelectorAll('.tabs__panel').forEach(p => p.classList.remove('tabs__panel--active'));
        btn.classList.add('tabs__button--active');
        document.getElementById(tabId)?.classList.add('tabs__panel--active');
      });
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
    
    const buscarCep = () => {
      const cep = cepInput.value.replace(/\D/g, '');
      if (cep.length !== 8) {
        mostrarNotificacao('Digite um CEP válido com 8 dígitos', 'error');
        return;
      }
      
      if (btnBuscarCep) {
        btnBuscarCep.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Buscando...';
        btnBuscarCep.disabled = true;
      }
      
      fetch(`https://viacep.com.br/ws/${cep}/json/`)
        .then(res => res.json())
        .then(data => {
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
            
            mostrarNotificacao('Endereço encontrado com sucesso!', 'success');
          } else {
            mostrarNotificacao('CEP não encontrado', 'error');
          }
        })
        .catch(err => {
          console.error('Error fetching address:', err);
          mostrarNotificacao('Erro ao buscar o CEP', 'error');
        })
        .finally(() => {
          if (btnBuscarCep) {
            btnBuscarCep.innerHTML = '<i class="fas fa-search"></i> Buscar CEP';
            btnBuscarCep.disabled = false;
          }
        });
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
            length: value.length >= 8,
            upper: /[A-Z]/.test(value),
            lower: /[a-z]/.test(value),
            number: /[0-9]/.test(value),
            special: /[!@#$%^&*(),.?":{}|<>]/.test(value)
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
                strengthText.textContent = 'Média';
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
        : '<i class="fas fa-times" style="color:var(--danger)"></i> Senhas não coincidem';
    }
    
    confirmInput.addEventListener('input', verificarCorrespondencia);
    passwordInput.addEventListener('input', verificarCorrespondencia);
  }
  
  
  function inicializarFormularioPerfil() {
    const form = document.getElementById('formEditarPerfil');
    if (!form) return;
    
    form.addEventListener('submit', e => {
      e.preventDefault();
      
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
        mostrarNotificacao('Preencha todos os campos obrigatórios', 'error');
        return;
      }
      
      form.submit();
    });
  }
  
  
  function inicializarFormularioSenha() {
    const form = document.getElementById('formAlterarSenha');
    if (!form) return;
    
    form.addEventListener('submit', e => {
      e.preventDefault();
      
      const currentPassword = document.getElementById('senhaAtual').value;
      const newPassword = document.getElementById('novaSenha').value;
      const confirmPassword = document.getElementById('confirmarSenha').value;
      
      if (!currentPassword) {
        mostrarNotificacao('Digite sua senha atual', 'error');
        return;
      }
      
      if (!newPassword) {
        mostrarNotificacao('Digite a nova senha', 'error');
        return;
      }
      
      const meetsRequirements = [
        newPassword.length >= 8,
        /[A-Z]/.test(newPassword),
        /[a-z]/.test(newPassword),
        /[0-9]/.test(newPassword),
        /[!@#$%^&*(),.?":{}|<>]/.test(newPassword)
      ].filter(Boolean).length >= 3;
      
      if (!meetsRequirements) {
        mostrarNotificacao('A senha não atende aos requisitos mínimos de segurança', 'error');
        return;
      }
      
      if (newPassword !== confirmPassword) {
        mostrarNotificacao('As senhas não coincidem', 'error');
        return;
      }
      
      form.submit();
    });
  }
  
  
  function inicializarHoverSecao() {
    const items = document.querySelectorAll('.perfil__item');
    
    items.forEach(item => {
      item.addEventListener('mouseenter', () => {
        const icon = item.querySelector('.perfil__item-icon i');
        if (icon) {
          icon.classList.add('fa-bounce');
        }
      });
      
      item.addEventListener('mouseleave', () => {
        const icon = item.querySelector('.perfil__item-icon i');
        if (icon) {
          icon.classList.remove('fa-bounce');
        }
      });
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
    
    cards.forEach(card => {
      card.addEventListener('mouseenter', () => {
        const icon = card.querySelector('.contact-card__icon i');
        if (icon) {
          icon.classList.add('fa-bounce');
        }
      });
      
      card.addEventListener('mouseleave', () => {
        const icon = card.querySelector('.contact-card__icon i');
        if (icon) {
          icon.classList.remove('fa-bounce');
      }
      });
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
  
  
  function removerFotoPerfil() {
    const removeBtn = document.querySelector('.perfil__avatar-remove');
    if (!removeBtn) return;
    
    const originalHTML = removeBtn.innerHTML;
    removeBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
    removeBtn.style.pointerEvents = 'none';
    
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    
    fetch('/usuario/perfil/remover-foto', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: `__RequestVerificationToken=${token}`
    })
    .then(response => {
        if (response.redirected) {
            window.location.href = response.url;
        } else {
            return response.json();
        }
    })
    .then(data => {
        if (data && !data.success) {
            mostrarNotificacao(data.message || 'Erro ao remover foto de perfil', 'error');
            removeBtn.innerHTML = originalHTML;
            removeBtn.style.pointerEvents = 'auto';
        } else {
            const container = document.querySelector('.perfil__avatar');
            const img = container.querySelector('img');
            
            if (img) {
                const nome = document.querySelector('.perfil__name').textContent.trim();
                const inicial = nome.charAt(0).toUpperCase();
                
                const initialAvatar = document.createElement('div');
                initialAvatar.className = 'perfil__avatar-initial';
                initialAvatar.innerHTML = `<span>${inicial}</span>`;
                
                img.replaceWith(initialAvatar);
            }
            
            container.querySelector('.perfil__avatar-upload')?.remove();
            container.querySelector('.perfil__avatar-remove')?.remove();
            
            mostrarNotificacao('Foto de perfil removida com sucesso!', 'success');
            
            fecharModal('modalConfirmarRemoverFoto');
        }
    })
    .catch(error => {
        console.error('Erro ao remover foto de perfil:', error);
        mostrarNotificacao('Erro de conexão ao remover foto de perfil', 'error');
        removeBtn.innerHTML = originalHTML;
        removeBtn.style.pointerEvents = 'auto';
    });
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
  
    function uploadFotoPerfil(event) {
    const file = event.target.files[0];
    if (!file) return;
    
    if (file.size > 5 * 1024 * 1024) {
        mostrarNotificacao('A imagem deve ter até 5MB', 'error');
        event.target.value = '';
        return;
    }
    
    if (!file.type.startsWith('image/')) {
        mostrarNotificacao('Por favor, selecione um arquivo de imagem válido', 'error');
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
    
    fetch('/usuario/perfil/atualizarFoto', {
        method: 'POST',
        body: formData,
        headers: {'X-Requested-With': 'XMLHttpRequest'}
    })
    .then(res => res.json())
    .then(data => {
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
                    const uploadBtn = document.createElement('div');
                    uploadBtn.className = 'perfil__avatar-upload';
                    uploadBtn.setAttribute('onclick', "document.getElementById('uploadFoto').click()");
                    uploadBtn.setAttribute('title', 'Alterar foto');
                    uploadBtn.innerHTML = '<i class="fas fa-camera"></i>';
                    container.appendChild(uploadBtn);
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
    })
    .catch(err => {
        console.error('Error updating profile photo:', err);
        mostrarNotificacao('Erro de conexão ao atualizar a foto de perfil', 'error');
    })
    .finally(() => {
        const uploadBtn = document.querySelector('.perfil__avatar-upload');
        if (uploadBtn) {
            uploadBtn.querySelector('i').className = 'fas fa-camera';
        }
    });
  }
  