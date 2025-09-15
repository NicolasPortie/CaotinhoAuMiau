// Dashboard CaotinhoAuMiau - GrÃ¡ficos Completos
// ConfiguraÃ§Ãµes globais Chart.js
Chart.defaults.font.family = "'Inter', sans-serif";
Chart.defaults.plugins.tooltip.backgroundColor = 'rgba(17, 24, 39, 0.9)';
Chart.defaults.plugins.tooltip.padding = 12;
Chart.defaults.plugins.tooltip.cornerRadius = 8;
// VariÃ¡veis globais para os grÃ¡ficos
let graficoProcesso, graficoStatus, graficoConversao, graficoTendencia;
// Paleta de cores
const CORES = {
    primary: '#2563eb',
    secondary: '#7c3aed',
    success: '#16a34a',
    warning: '#eab308',
    danger: '#dc2626',
    info: '#0891b2',
    orange: '#f97316',
    purple: '#9333ea'
};
// === GRÃFICO 1: PROCESSO DE ADOÃ‡ÃƒO (FUNIL) ===
async function criarGraficoProcesso(dados) {
    const ctx = document.getElementById('graficoProcessoAdocao');
    if (!ctx) return;
    // Destruir grÃ¡fico existente
    if (graficoProcesso) {
        graficoProcesso.destroy();
    }
    graficoProcesso = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: [
                'FormulÃ¡rios Pendentes',
                'Aguardando Contrato',
                'Aguardando Retirada',
                'Finalizados'
            ],
            datasets: [{
                label: 'Quantidade',
                data: [
                    dados.formulariosPendentes || 0,
                    dados.adocoes?.aguardandoContrato || 0,
                    dados.adocoes?.aguardandoRetirada || 0,
                    dados.adocoes?.finalizadas || 0
                ],
                backgroundColor: [
                    CORES.warning,    // Pendentes - Caotinho precisa analisar
                    CORES.info,       // Aguardando Contrato - UsuÃ¡rio precisa assinar
                    CORES.danger,     // Aguardando Retirada - UsuÃ¡rio precisa buscar
                    CORES.success     // Finalizados - Processo completo
                ],
                borderRadius: 8,
                borderSkipped: false
            }]
        },
        options: {
            indexAxis: 'y',
            responsive: true,
            maintainAspectRatio: false,
            layout: {
                padding: {
                    left: 20,
                    right: 20,
                    top: 10,
                    bottom: 10
                }
            },
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        title: (context) => context[0].label,
                        label: (context) => `${context.formattedValue} casos`
                    }
                }
            },
            scales: {
                x: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0,0,0,0.05)'
                    },
                    ticks: {
                        font: {
                            size: 14
                        },
                        stepSize: 1,
                        precision: 0,
                        callback: function(value) {
                            return Number.isInteger(value) ? value : '';
                        }
                    }
                },
                y: {
                    grid: {
                        display: false
                    },
                    ticks: {
                        font: {
                            size: 14,
                            weight: 'bold'
                        },
                        color: '#374151',
                        maxRotation: 0,
                        padding: 10
                    }
                }
            },
            animation: {
                duration: 1000,
                easing: 'easeInOutQuart'
            }
        }
    });
}
// === GRÃFICO 2: STATUS DOS PETS (DOUGHNUT) ===
async function criarGraficoStatusPets(dados) {
    const ctx = document.getElementById('graficoStatusPets');
    if (!ctx) return;
    if (graficoStatus) {
        graficoStatus.destroy();
    }
    graficoStatus = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['DisponÃ­vel', 'Adotado', 'Em Processo', 'Rascunho'],
            datasets: [{
                data: [
                    dados.pets?.disponiveis || 0,
                    dados.pets?.adotados || 0,
                    dados.pets?.emProcesso || 0,
                    dados.pets?.rascunho || 0
                ],
                backgroundColor: [
                    CORES.success,
                    CORES.primary,
                    CORES.warning,
                    CORES.secondary
                ],
                borderWidth: 3,
                borderColor: '#fff',
                hoverBorderWidth: 5
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        padding: 20,
                        usePointStyle: true,
                        font: {
                            size: 12
                        }
                    }
                },
                tooltip: {
                    callbacks: {
                        label: (context) => {
                            const label = context.label || '';
                            const value = context.formattedValue;
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = ((context.raw / total) * 100).toFixed(1);
                            return `${label}: ${value} pets (${percentage}%)`;
                        }
                    }
                }
            },
            cutout: '60%',
            animation: {
                animateRotate: true,
                duration: 1200
            }
        }
    });
}
// === GRÃFICO 3: CONVERSÃƒO POR ESPÃ‰CIE (BARRAS AGRUPADAS) ===
async function criarGraficoConversaoEspecies(dados) {
    const ctx = document.getElementById('graficoConversaoEspecies');
    if (!ctx) return;
    if (graficoConversao) {
        graficoConversao.destroy();
    }
    const cachorrosTotal = dados.especies?.cachorrosTotal || 0;
    const cachorrosAdotados = dados.especies?.cachorrosAdotados || 0;
    const gatosTotal = dados.especies?.gatosTotal || 0;
    const gatosAdotados = dados.especies?.gatosAdotados || 0;
    // Calcular taxas de conversÃ£o
    const taxaCachorros = cachorrosTotal > 0 ? ((cachorrosAdotados / cachorrosTotal) * 100).toFixed(1) : 0;
    const taxaGatos = gatosTotal > 0 ? ((gatosAdotados / gatosTotal) * 100).toFixed(1) : 0;
    graficoConversao = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: [`CÃ£es - Adotados (${taxaCachorros}%)`, `Gatos - Adotados (${taxaGatos}%)`],
            datasets: [{
                label: 'Pets Adotados',
                data: [cachorrosAdotados, gatosAdotados],
                backgroundColor: [CORES.orange, CORES.purple],
                borderRadius: 6,
                borderWidth: 2,
                borderColor: [CORES.orange, CORES.purple],
                barThickness: 60,
                maxBarThickness: 80
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        title: (context) => {
                            const index = context[0].dataIndex;
                            const especie = index === 0 ? 'CÃ£es' : 'Gatos';
                            const total = index === 0 ? cachorrosTotal : gatosTotal;
                            const adotados = index === 0 ? cachorrosAdotados : gatosAdotados;
                            const taxa = total > 0 ? ((adotados / total) * 100).toFixed(1) : '0';
                            return `${especie} - Taxa de ConversÃ£o: ${taxa}%`;
                        },
                        label: (context) => {
                            const index = context.dataIndex;
                            const total = index === 0 ? cachorrosTotal : gatosTotal;
                            return `Adotados: ${context.formattedValue} de ${total} total`;
                        },
                        afterBody: (context) => {
                            const index = context[0].dataIndex;
                            const total = index === 0 ? cachorrosTotal : gatosTotal;
                            const adotados = index === 0 ? cachorrosAdotados : gatosAdotados;
                            const disponveis = total - adotados;
                            return `Ainda disponÃ­veis: ${disponveis}`;
                        }
                    }
                }
            },
            scales: {
                x: {
                    grid: {
                        display: false
                    },
                    ticks: {
                        font: {
                            size: 12,
                            weight: 'bold'
                        },
                        maxRotation: 0
                    }
                },
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0,0,0,0.05)'
                    },
                    ticks: {
                        stepSize: 1,
                        precision: 0,
                        callback: function(value) {
                            return Number.isInteger(value) ? value : '';
                        }
                    },
                    title: {
                        display: true,
                        text: 'NÃºmero de Pets Adotados',
                        font: {
                            size: 12
                        }
                    }
                }
            },
            animation: {
                duration: 1000,
                easing: 'easeOutBounce'
            }
        }
    });
}
// === GRÃFICO 4: TENDÃŠNCIA DE FORMULÃRIOS (LINHA) ===
async function criarGraficoTendencia(dados, periodo = 12) {
    const ctx = document.getElementById('graficoTendenciaFormularios');
    if (!ctx) return;
    if (graficoTendencia) {
        graficoTendencia.destroy();
    }
    const dadosFiltrados = dados.tendencia?.slice(-periodo) || [];
    graficoTendencia = new Chart(ctx, {
        type: 'line',
        data: {
            labels: dadosFiltrados.map(d => d.mes) || [],
            datasets: [{
                label: 'FormulÃ¡rios Enviados',
                data: dadosFiltrados.map(d => d.quantidade) || [],
                borderColor: CORES.primary,
                backgroundColor: CORES.primary + '20',
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                pointBackgroundColor: CORES.primary,
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                pointRadius: 6,
                pointHoverRadius: 8
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    mode: 'index',
                    intersect: false,
                    callbacks: {
                        title: (context) => `${context[0].label}`,
                        label: (context) => `${context.formattedValue} formulÃ¡rios`
                    }
                }
            },
            scales: {
                x: {
                    grid: {
                        display: false
                    }
                },
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0,0,0,0.05)'
                    }
                }
            },
            interaction: {
                mode: 'nearest',
                axis: 'x',
                intersect: false
            },
            animation: {
                duration: 1500,
                easing: 'easeInOutCubic'
            }
        }
    });
}
// === CARREGAR DADOS E CRIAR GRÃFICOS ===
async function carregarDadosGraficos() {
    try {
        const response = await fetch('/admin/dashboard/dados-graficos');
        if (!response.ok) {
            throw new Error(`Erro HTTP: ${response.status}`);
        }
        const dados = await response.json();
        if (!dados.sucesso) {
            throw new Error(dados.mensagem || 'Erro ao carregar dados');
        }
        // Criar todos os grÃ¡ficos
        await Promise.all([
            criarGraficoProcesso(dados),
            criarGraficoStatusPets(dados),
            criarGraficoConversaoEspecies(dados),
            criarGraficoTendencia(dados)
        ]);
        // Configurar filtros de perÃ­odo
        configurarFiltrosPeriodo(dados);
        // Carregar atividades recentes
        await carregarAtividadesRecentes();
    } catch (error) {
        mostrarErroGraficos(error.message);
    }
}
// === CONFIGURAR FILTROS DE PERÃODO ===
function configurarFiltrosPeriodo(dados) {
    const filtros = document.querySelectorAll('.filtro-periodo');
    filtros.forEach(filtro => {
        filtro.addEventListener('click', async function() {
            // Remover classe ativo de todos
            filtros.forEach(f => f.classList.remove('ativo'));
            // Adicionar ao clicado
            this.classList.add('ativo');
            // Recriar grÃ¡fico de tendÃªncia com novo perÃ­odo
            const periodo = parseInt(this.dataset.periodo);
            await criarGraficoTendencia(dados, periodo);
        });
    });
}
// === EXIBIR ERROS ===
function mostrarErroGraficos(mensagem) {
    const canvasIds = ['graficoProcessoAdocao', 'graficoStatusPets', 'graficoConversaoEspecies', 'graficoTendenciaFormularios'];
    canvasIds.forEach(id => {
        const canvas = document.getElementById(id);
        if (canvas) {
            const container = canvas.parentElement;
            container.innerHTML = `
                <div class="grafico-carregando">
                    <i class="fa-solid fa-exclamation-triangle" style="color: #dc2626; animation: none;"></i>
                    <span>Erro ao carregar grÃ¡fico</span>
                    <small style="color: #666; margin-top: 0.5rem;">${mensagem}</small>
                </div>
            `;
        }
    });
}
// === INICIALIZAÃ‡ÃƒO ===
function inicializarDashboard() {
    // Verificar se Chart.js estÃ¡ carregado
    if (typeof Chart === 'undefined') {
        return;
    }
    // Aguardar um pouco para garantir que DOM estÃ¡ pronto
    setTimeout(() => {
        carregarDadosGraficos();
    }, 100);
}
// === EVENT LISTENERS ===
document.addEventListener('DOMContentLoaded', function() {
    inicializarDashboard();
});
// Recarregar grÃ¡ficos em caso de redimensionamento
window.addEventListener('resize', function() {
    // Debounce para evitar muitas chamadas
    clearTimeout(this.resizeTimeout);
    this.resizeTimeout = setTimeout(() => {
        if (graficoProcesso) graficoProcesso.resize();
        if (graficoStatus) graficoStatus.resize();
        if (graficoConversao) graficoConversao.resize();
        if (graficoTendencia) graficoTendencia.resize();
    }, 250);
});
// === CARREGAR ATIVIDADES RECENTES ===
async function carregarAtividadesRecentes() {
    try {
        const response = await fetch('/admin/dashboard/AtividadesRecentes');
        if (!response.ok) {
            throw new Error(`Erro HTTP: ${response.status}`);
        }
        const dados = await response.json();
        if (!dados.sucesso) {
            throw new Error(dados.mensagem || 'Erro ao carregar atividades');
        }
        exibirAtividadesRecentes(dados.atividades || []);
    } catch (error) {
        mostrarErroAtividades(error.message);
    }
}
function exibirAtividadesRecentes(atividades) {
    const container = document.querySelector('.lista-atividade');
    if (!container) return;
    if (atividades.length === 0) {
        container.innerHTML = `
            <div class="atividade-item sem-atividades">
                <div class="atividade-icone">
                    <i class="fa-solid fa-info-circle"></i>
                </div>
                <div class="atividade-conteudo">
                    <div class="atividade-titulo">Nenhuma atividade recente</div>
                    <div class="atividade-data">Sistema aguardando movimentaÃ§Ãµes</div>
                </div>
            </div>
        `;
        return;
    }
    const htmlAtividades = atividades.map(atividade => {
        const icone = obterIconeAtividade(atividade.tipo);
        const corStatus = obterCorStatus(atividade.status);
        const dataFormatada = formatarDataAtividade(atividade.dataOcorrencia);
        return `
            <div class="atividade-item">
                <div class="atividade-icone ${atividade.tipo}">
                    <i class="${icone}"></i>
                </div>
                <div class="atividade-conteudo">
                    <div class="atividade-titulo">${atividade.descricao}</div>
                    <div class="atividade-detalhes">
                        ${atividade.nomeUsuario ? `<span class="usuario">${atividade.nomeUsuario}</span>` : ''}
                        <span class="status ${corStatus}">${atividade.status}</span>
                        <span class="data">${dataFormatada}</span>
                    </div>
                </div>
            </div>
        `;
    }).join('');
    container.innerHTML = htmlAtividades;
}
function obterIconeAtividade(tipo) {
    const icones = {
        'formulario': 'fa-solid fa-file-text',
        'pet': 'fa-solid fa-heart',
        'usuario': 'fa-solid fa-user-plus',
        'adocao': 'fa-solid fa-handshake',
        'contrato': 'fa-solid fa-file-signature'
    };
    return icones[tipo] || 'fa-solid fa-circle';
}
function obterCorStatus(status) {
    const cores = {
        'Pendente': 'pendente',
        'Aprovado': 'aprovado',
        'Finalizado': 'finalizado',
        'Ativo': 'ativo',
        'DisponÃ­vel': 'disponivel',
        'Adotado': 'adotado'
    };
    return cores[status] || 'default';
}
function formatarDataAtividade(data) {
    const agora = new Date();
    const dataAtividade = new Date(data);
    const diferencaMs = agora - dataAtividade;
    const diferencaMinutos = Math.floor(diferencaMs / (1000 * 60));
    const diferencaHoras = Math.floor(diferencaMinutos / 60);
    const diferencaDias = Math.floor(diferencaHoras / 24);
    if (diferencaMinutos < 1) {
        return 'Agora mesmo';
    } else if (diferencaMinutos < 60) {
        return `${diferencaMinutos} min atrÃ¡s`;
    } else if (diferencaHoras < 24) {
        return `${diferencaHoras}h atrÃ¡s`;
    } else if (diferencaDias < 7) {
        return `${diferencaDias}d atrÃ¡s`;
    } else {
        return dataAtividade.toLocaleDateString('pt-BR');
    }
}
function mostrarErroAtividades(mensagem) {
    const container = document.querySelector('.lista-atividade');
    if (container) {
        container.innerHTML = `
            <div class="atividade-item erro">
                <div class="atividade-icone">
                    <i class="fa-solid fa-exclamation-triangle"></i>
                </div>
                <div class="atividade-conteudo">
                    <div class="atividade-titulo">Erro ao carregar atividades</div>
                    <div class="atividade-data">${mensagem}</div>
                </div>
            </div>
        `;
    }
}

