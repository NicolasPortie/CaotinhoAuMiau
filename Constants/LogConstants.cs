namespace CaotinhoAuMiau.Constants
{
    public static class LogConstants
    {
        public static class TiposAcao
        {
            // Login, logout e coisas de autenticação
            public const string LOGIN_SUCESSO = "Login_Sucesso";
            public const string LOGIN_FALHA = "Login_Falha";
            public const string LOGOUT = "Logout";

            // Tudo relacionado ao cadastro e gestão dos pets
            public const string CADASTRAR_PET = "Cadastrar_Pet";
            public const string EDITAR_PET = "Editar_Pet";
            public const string EXCLUIR_PET = "Excluir_Pet";
            public const string ALTERAR_STATUS_PET = "Alterar_Status_Pet";
            public const string UPLOAD_IMAGEM_PET = "Upload_Imagem_Pet";
            public const string REMOVER_IMAGEM_PET = "Remover_Imagem_Pet";

            // Processo de adoção dos animais
            public const string SUBMETER_FORMULARIO = "Submeter_Formulario";
            public const string EDITAR_FORMULARIO = "Editar_Formulario";
            public const string APROVAR_ADOCAO = "Aprovar_Adocao";
            public const string REJEITAR_ADOCAO = "Rejeitar_Adocao";
            public const string CANCELAR_ADOCAO = "Cancelar_Adocao";
            public const string AGENDAR_RETIRADA = "Agendar_Retirada";
            public const string CONFIRMAR_RETIRADA = "Confirmar_Retirada";

            // Geração e assinatura de contratos
            public const string GERAR_CONTRATO = "Gerar_Contrato";
            public const string ASSINAR_CONTRATO = "Assinar_Contrato";
            public const string DOWNLOAD_CONTRATO = "Download_Contrato";

            // Gerenciamento dos usuários do sistema
            public const string CADASTRAR_USUARIO = "Cadastrar_Usuario";
            public const string EDITAR_PERFIL = "Editar_Perfil";
            public const string ALTERAR_SENHA = "Alterar_Senha";
            public const string EXCLUIR_USUARIO = "Excluir_Usuario";
            public const string DESATIVAR_USUARIO = "Desativar_Usuario";
            public const string REATIVAR_USUARIO = "Reativar_Usuario";

            // Administração da equipe e colaboradores
            public const string CADASTRAR_COLABORADOR = "Cadastrar_Colaborador";
            public const string EDITAR_COLABORADOR = "Editar_Colaborador";
            public const string ALTERAR_CARGO_COLABORADOR = "Alterar_Cargo_Colaborador";
            public const string DESATIVAR_COLABORADOR = "Desativar_Colaborador";
            public const string REATIVAR_COLABORADOR = "Reativar_Colaborador";

            // Configuração e envio de emails automáticos
            public const string CONFIGURAR_EMAIL = "Configurar_Email";
            public const string ENVIAR_EMAIL = "Enviar_Email";
            public const string TESTE_EMAIL = "Teste_Email";

            // Erros e logs gerais do sistema
            public const string REQUISICAO_HTTP = "Requisicao_HTTP";
            public const string RESPOSTA_HTTP_ERRO = "Resposta_HTTP_Erro";
            public const string EXCECAO = "Excecao";
            public const string ACESSO_NEGADO = "Acesso_Negado";

            // Quando o usuário navega e visualiza coisas
            public const string VISUALIZAR_DETALHES = "Visualizar_Detalhes";
            public const string ACESSAR_RELATORIO = "Acessar_Relatorio";
            public const string EXPORTAR_DADOS = "Exportar_Dados";
            public const string FILTRAR_DADOS = "Filtrar_Dados";
            public const string PESQUISAR = "Pesquisar";
            public const string ORDENAR_LISTA = "Ordenar_Lista";
            public const string PAGINAR = "Paginar";

            // Ações dos admins no painel de controle
            public const string ACESSAR_DASHBOARD = "Acessar_Dashboard";
            public const string VISUALIZAR_ESTATISTICAS = "Visualizar_Estatisticas";
            public const string GERENCIAR_PETS = "Gerenciar_Pets";
            public const string GERENCIAR_ADOCOES = "Gerenciar_Adocoes";
            public const string GERENCIAR_FORMULARIOS = "Gerenciar_Formularios";
            public const string GERENCIAR_COLABORADORES = "Gerenciar_Colaboradores";
            public const string GERENCIAR_USUARIOS = "Gerenciar_Usuarios";
            public const string AVALIAR_FORMULARIO = "Avaliar_Formulario";

            // O que os usuários comuns fazem no site
            public const string EXPLORAR_PETS = "Explorar_Pets";
            public const string VISUALIZAR_PET = "Visualizar_Pet";
            public const string SOLICITAR_ADOCAO = "Solicitar_Adocao";
            public const string ACOMPANHAR_ADOCAO = "Acompanhar_Adocao";
            public const string CANCELAR_SOLICITACAO = "Cancelar_Solicitacao";

            // Sistema de notificações e alertas
            public const string ENVIAR_NOTIFICACAO = "Enviar_Notificacao";
            public const string MARCAR_NOTIFICACAO_LIDA = "Marcar_Notificacao_Lida";
            public const string LIMPAR_NOTIFICACOES = "Limpar_Notificacoes";

            // Configurações de conta e recuperação de senha
            public const string ALTERAR_CONFIGURACAO = "Alterar_Configuracao";
            public const string RECUPERAR_SENHA = "Recuperar_Senha";
            public const string VERIFICAR_EMAIL = "Verificar_Email";

            // Operações em massa e backups
            public const string OPERACAO_EM_LOTE = "Operacao_Em_Lote";
            public const string IMPORTAR_DADOS = "Importar_Dados";
            public const string BACKUP_DADOS = "Backup_Dados";
        }

        public static class Categorias
        {
            public const string AUTENTICACAO = "Autenticação";
            public const string PET = "Pet";
            public const string ADOCAO = "Adoção";
            public const string FORMULARIO = "Formulário";
            public const string CONTRATO = "Contrato";
            public const string USUARIO = "Usuário";
            public const string COLABORADOR = "Colaborador";
            public const string EMAIL = "Email";
            public const string SISTEMA = "Sistema";
            public const string SEGURANCA = "Segurança";
            public const string RELATORIO = "Relatório";
            public const string DASHBOARD = "Dashboard";
            public const string CONFIGURACAO = "Configuração";
        }

        public static class NiveisSeveridade
        {
            public const string INFO = "Info";
            public const string WARNING = "Warning";
            public const string ERROR = "Error";
            public const string CRITICAL = "Critical";
        }

        public static class EntidadesAfetadas
        {
            public const string PET = "Pet";
            public const string USUARIO = "Usuario";
            public const string COLABORADOR = "Colaborador";
            public const string FORMULARIO_ADOCAO = "FormularioAdocao";
            public const string ADOCAO = "Adocao";
            public const string CONTRATO_ADOCAO = "ContratoAdocao";
            public const string CONFIGURACAO_EMAIL = "ConfiguracaoEmail";
            public const string NOTIFICACAO = "Notificacao";
        }
    }
}