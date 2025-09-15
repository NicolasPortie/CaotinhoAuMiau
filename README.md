# CaotinhoAuMiau

Sistema web para gerenciamento de adoções de cães e gatos desenvolvido em ASP.NET Core MVC.

## Sobre o Projeto

O CaotinhoAuMiau conecta pessoas interessadas em adotar pets com animais disponíveis para adoção. O sistema gerencia todo o processo desde o cadastro inicial até a finalização da adoção, incluindo contratos digitais e ferramentas administrativas.

## Funcionalidades

### Área Pública
- Página inicial com pets em destaque
- Busca e filtros por espécie, idade, porte e sexo
- Sistema de autenticação com BCrypt
- Páginas institucionais

### Área do Usuário
- Cadastro com validação de dados
- Perfil com upload de foto
- Formulário de adoção detalhado
- Acompanhamento de solicitações
- Notificações integradas
- Assinatura digital de contratos
- Histórico de adoções

### Área Administrativa
- Dashboard com estatísticas
- Gerenciamento de pets (CRUD completo)
- Análise de formulários de adoção
- Aprovação/rejeição de solicitações
- Gestão de usuários e colaboradores
- Sistema de logs e auditoria
- Configurações de email
- Relatórios

### Sistema de Contratos
- Geração automática de PDFs
- Assinatura digital
- Versionamento de documentos
- Visualização web

### Automações
- Emails automáticos por etapa
- Notificações em tempo real
- Background services
- Sistema de quarentena

## Tecnologias

### Backend
- .NET 9.0
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- BCrypt.Net

### Frontend
- HTML5/CSS3
- JavaScript
- Bootstrap
- jQuery

### Bibliotecas
- iTextSharp (geração de PDFs)
- Redis (cache opcional)
- Newtonsoft.Json
- Razor Pages

## Requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (LocalDB ou instância completa)
- [Redis](https://redis.io/download) (opcional)
- Visual Studio ou VS Code

## Instalação

### 1. Clone o repositório
```bash
git clone https://github.com/usuario/CaotinhoAuMiau.git
cd CaotinhoAuMiau
```

### 2. Configure a string de conexão
Edite o arquivo `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local);Database=CaotinhoAuMiau;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true",
    "Redis": "localhost:6379"
  }
}
```

### 3. Restaure as dependências
```bash
dotnet restore
```

### 4. Execute as migrações do banco
```bash
dotnet ef database update
```

### 5. Execute o projeto
```bash
dotnet run
```

Ou para desenvolvimento com hot reload:
```bash
dotnet watch run
```

### 6. Acesse a aplicação
- URL: `https://localhost:5001` ou `http://localhost:5000`
- Admin padrão: `admin@caotinhoaumiau.com.br` / senha: `admin`

## Como Testar

### Requisitos para Teste
1. Windows (configurado para SQL Server Windows)
2. SQL Server LocalDB ou completo
3. .NET 9.0 SDK
4. Redis (opcional)

### Configuração Rápida para Teste

#### Opção 1: Visual Studio Code
```bash
# 1. Clone e navegue até o projeto
git clone https://github.com/NicolasPortie/CaotinhoAuMiau.git
cd CaotinhoAuMiau

# 2. Instale as dependências
dotnet restore

# 3. Instale o Entity Framework CLI (se não tiver)
dotnet tool install --global dotnet-ef

# 4. Verifique se o SQL Server LocalDB está rodando
sqllocaldb info

# 5. Se não estiver, inicie o LocalDB
sqllocaldb start mssqllocaldb

# 6. Execute as migrações para criar o banco
dotnet ef database update

# 7. Execute o projeto
dotnet watch run
```

#### Opção 2: Visual Studio
1. Abra o arquivo `CaotinhoAuMiau.sln`
2. Configure a string de conexão se necessário
3. No Package Manager Console: `Update-Database`
4. Pressione F5 para executar

### Configuração do Banco de Dados

O projeto está configurado para usar **SQL Server com Windows Authentication**:

```json
"DefaultConnection": "Server=(local);Database=CaotinhoAuMiau;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

#### Se não tiver SQL Server instalado:
1. **Instale SQL Server LocalDB** (mais leve):
   - Download: [SQL Server Express LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)

2. **Ou use SQL Server completo**:
   - Download: [SQL Server Developer Edition](https://www.microsoft.com/sql-server/sql-server-downloads)

#### Comandos úteis para troubleshooting:
```bash
# Verificar instâncias do LocalDB
sqllocaldb info

# Criar nova instância se necessário
sqllocaldb create "v11.0" -s

# Ver status das migrações
dotnet ef migrations list

# Recriar banco do zero (CUIDADO: apaga dados)
dotnet ef database drop
dotnet ef database update
```

### Testando as Funcionalidades

#### 1. **Área Administrativa**
- URL: `/admin`
- **Login**: `admin@caotinhoaumiau.com.br`
- **Senha**: `admin`
- **Teste**: Dashboard, cadastro de pets, aprovação de formulários

#### 2. **Cadastro de Usuário**
- URL: `/autenticacao/cadastro`
- **Teste**: Crie uma conta de usuário normal
- **Validações**: CPF, email único, senha segura

#### 3. **Sistema de Adoção**
- **Cadastre pets** como admin
- **Faça login como usuário** e explore os pets
- **Preencha formulário** de adoção
- **Volte como admin** e aprove o formulário
- **Teste assinatura** digital do contrato

#### 4. **Upload de Imagens**
- **Formatos aceitos**: JPG, PNG
- **Tamanho máximo**: 100MB (configurado no projeto)
- **Pasta**: `wwwroot/imagens/pets/`

### Configurações Opcionais

#### Redis (Cache)
Se quiser testar com Redis:
```bash
# Instalar Redis no Windows (via Chocolatey)
choco install redis-64

# Ou usar Docker
docker run -d -p 6379:6379 redis:alpine

# A aplicação funciona sem Redis (fallback para MemoryCache)
```

#### Email (Desenvolvimento)
Para testar emails, configure um provedor no `appsettings.json`:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "Username": "seu-email@gmail.com",
    "Password": "sua-senha-app"
  }
}
```

### Problemas Comuns

#### Erro de Conexão com Banco
```bash
# Verifique se o SQL Server está rodando
services.msc # Procure por "SQL Server"

# Ou reinicie o LocalDB
sqllocaldb stop mssqllocaldb
sqllocaldb start mssqllocaldb
```

#### Erro de Migração
```bash
# Limpe e recrie as migrações
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### Porta em Uso
```bash
# Verifique processos na porta 5001
netstat -ano | findstr :5001

# Mate o processo se necessário
taskkill /PID [número_do_processo] /F
```

### Estrutura de Teste Sugerida

1. **Cadastre 3-5 pets** com fotos diferentes
2. **Crie 2-3 usuários** para testar diferentes cenários
3. **Teste o fluxo completo** de adoção
4. **Verifique relatórios** no dashboard admin
5. **Teste cancelamentos** e reativações

## Guia de Uso

### Primeiro Acesso

#### Configuração do Administrador
Na primeira execução, o sistema cria automaticamente:
- Email admin: `admin@caotinhoaumiau.com.br`
- Senha: `admin`
- Acesso total ao sistema

#### Estrutura de Pastas
O sistema cria automaticamente as pastas:
```
wwwroot/
├── imagens/
│   ├── pets/          # Fotos dos pets
│   └── perfil/        # Fotos de perfil dos usuários
└── contratos/         # PDFs dos contratos gerados
```

### Área do Usuário

#### Cadastro (`/autenticacao/cadastro`)
Dados obrigatórios:
- CPF (validação automática, deve ser único)
- Nome completo
- Email (deve ser único)
- Senha (mínimo 6 caracteres)
- Telefone
- Data de nascimento
- Endereço completo

Validações:
- CPF válido e único
- Email válido e único
- Confirmação de senha
- Campos obrigatórios

#### Login (`/autenticacao/login`)
- Email e senha cadastrados
- Sessão de 2 horas (renovável)
- Redirecionamento automático

#### Perfil (`/usuario/perfil`)
Funcionalidades:
- Editar dados pessoais
- Upload de foto (JPG/PNG até 100MB)
- Alterar senha
- Ver histórico de adoções

#### **4. Explorar Pets** (`/usuario/pets/explorar`)
**Filtros disponíveis:**
- **Espécie**: Cão ou Gato
- **Idade**: Filhote, Jovem, Adulto, Idoso
- **Porte**: Pequeno, Médio, Grande
- **Sexo**: Macho ou Fêmea
- **Pesquisa**: Por nome ou raça

**Informações dos pets:**
- Foto principal
- Nome, idade (anos e meses)
- Espécie, raça, porte, sexo
- Descrição detalhada
- Status atual (Disponível, Em Processo, Adotado)

#### **5. Formulário de Adoção** (`/usuario/formulario-adocao/{id}`)
**Seções do formulário:**
- **Dados financeiros**: Renda mensal
- **Moradia**: Número de moradores, descrição da casa/apartamento
- **Experiência**: Se já teve pets antes
- **Motivação**: Por que quer adotar
- **Condições financeiras**: Como vai custear o pet
- **Viagens**: Planejamento para ausências
- **Espaço**: Se tem espaço adequado
- **Tempo**: Disponibilidade para cuidar

**Validações:**
- Todos os campos são obrigatórios
- Renda deve ser maior que 0
- Número de moradores deve ser positivo

#### **6. Acompanhar Adoções** (`/usuario/adocoes`)
**Status possíveis:**
- **Pendente**: Aguardando análise da equipe
- **Em Análise**: Sendo avaliado pelo admin
- **Aprovado**: Formulário aceito, aguardando contrato
- **Aguardando Assinatura**: Contrato disponível
- **Contrato Assinado**: Aguardando retirada
- **Finalizado**: Adoção concluída
- **Cancelado**: Por vários motivos possíveis

**Ações disponíveis:**
- **Ver detalhes**: Informações completas
- **Cancelar**: Apenas formulários pendentes
- **Reativar**: Formulários cancelados
- **Assinar contrato**: Quando aprovado

### 🛡️ **Área Administrativa - Guia Completo**

#### **1. Dashboard** (`/admin`)
**Métricas em tempo real:**
- Total de pets cadastrados
- Pets disponíveis vs adotados
- Formulários pendentes
- Adoções finalizadas
- Usuários ativos

**Gráficos e estatísticas:**
- Adoções por mês
- Tipos de pets mais adotados
- Taxa de aprovação
- Tempo médio de processo

#### **2. Gerenciamento de Pets** (`/admin/pets`)
**Cadastrar novo pet:**
- **Dados básicos**: Nome, espécie, raça, sexo
- **Idade**: Anos e meses separadamente
- **Características**: Porte, descrição
- **Foto**: Upload obrigatório
- **Status inicial**: Sempre "Disponível"

**Ações disponíveis:**
- **Editar**: Todos os dados exceto ID
- **Alterar status**: Disponível → Em Processo → Adotado
- **Ver histórico**: Todos os formulários deste pet
- **Excluir**: Apenas se não tiver formulários

#### **3. Gerenciamento de Formulários** (`/admin/formularios`)
**Análise de formulários:**
- **Dados do usuário**: Nome, CPF, email, telefone
- **Dados do pet**: Nome, foto, características
- **Formulário completo**: Todas as respostas
- **Data de envio**: Ordem cronológica

**Ações possíveis:**
- **Aprovar**: Cria adoção automática
- **Rejeitar**: Com motivo obrigatório
- **Adicionar observações**: Notas internas

#### **4. Gerenciamento de Adoções** (`/admin/adocoes`)
**Controle completo do processo:**
- **Aprovar**: Gera contrato automaticamente
- **Rejeitar**: Com motivo obrigatório
- **Finalizar**: Marca pet como adotado
- **Cancelar**: Em qualquer etapa
- **Ver contrato**: PDF gerado automaticamente

**Status de adoção:**
- **Aprovado**: Aguardando assinatura
- **Aguardando Assinatura**: Contrato disponível
- **Contrato Assinado**: Aguardando retirada
- **Aguardando Buscar**: Pet pronto para retirada
- **Finalizado**: Processo completo
- **Cancelado**: Por admin ou sistema

#### **5. Gerenciamento de Usuários** (`/admin/usuarios`)
**Controle de usuários:**
- **Ver perfil completo**: Todos os dados
- **Histórico de adoções**: Sucessos e cancelamentos
- **Ativar/Desativar**: Bloquear acesso
- **Adicionar observações**: Notas administrativas
- **Sistema de quarentena**: Usuários temporariamente bloqueados

#### **6. Gerenciamento de Colaboradores** (`/admin/colaboradores`)
**Tipos de colaborador:**
- **Administrador**: Acesso total
- **Colaborador**: Acesso limitado
- **Voluntário**: Apenas visualização

**Funcionalidades:**
- **Cadastrar**: Novos membros da equipe
- **Editar permissões**: Alterar tipo de acesso
- **Ativar/Desativar**: Controle de acesso

#### **7. Logs e Auditoria** (`/admin/logs`)
**Registro completo:**
- **Todas as ações**: Criar, editar, excluir
- **Usuário responsável**: Quem fez a ação
- **Data/hora**: Timestamp preciso
- **Detalhes**: Dados específicos da ação
- **Categoria**: Tipo de operação

**Filtros disponíveis:**
- Por data (hoje, 7 dias, 30 dias)
- Por usuário
- Por tipo de ação
- Por categoria

#### **8. Configurações de Email** (`/admin/email`)
**Configurar envios automáticos:**
- **Servidor SMTP**: Configurações do provedor
- **Templates**: Modelos de email personalizáveis
- **Logs de envio**: Histórico de emails
- **Teste de configuração**: Validar funcionamento

### 📄 **Sistema de Contratos - Como Funciona**

#### **Geração Automática**
1. **Aprovação**: Admin aprova formulário
2. **Dados coletados**: Usuário, pet, formulário
3. **PDF criado**: Com biblioteca iTextSharp
4. **Armazenamento**: Pasta `wwwroot/contratos/`
5. **Notificação**: Email automático para usuário

#### **Assinatura Digital**
- **Acesso**: Link único no email
- **Visualização**: PDF no navegador
- **Assinatura**: Nome completo + CPF
- **Validação**: Data/hora + IP do usuário
- **Confirmação**: Email de confirmação

#### **Versionamento**
- Cada contrato tem ID único
- Histórico de assinaturas mantido
- Backup automático dos PDFs

### Configurações

#### Email (appsettings.json)
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "Username": "email@dominio.com",
    "Password": "senha-do-app",
    "FromName": "Cãotinho AuMiau",
    "FromEmail": "noreply@caotinhoaumiau.com"
  }
}
```

#### Cache Redis
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

### Relatórios

#### Dashboard Administrativo
- Pets: total, disponíveis, em processo, adotados
- Usuários: ativos, novos cadastros, em quarentena
- Formulários: pendentes, aprovados, rejeitados
- Adoções: finalizadas, canceladas, em andamento
- Performance: tempo médio de processo

#### Relatórios Disponíveis
- Adoções por mês
- Distribuição por espécie
- Filhotes vs adultos
- Taxa de aprovação
- Rankings de usuários

### Notificações

#### Emails Automáticos
- Cadastro (boas-vindas)
- Formulário enviado (confirmação)
- Formulário aprovado (link do contrato)
- Contrato assinado (instruções)
- Adoção finalizada
- Cancelamentos

#### Notificações na Plataforma
- Status de formulários para usuários
- Novos formulários para admins
- Alertas do sistema

### Segurança

#### Autenticação
- Cookies seguros (HttpOnly, SameSite)
- Sessões com timeout de 2 horas
- Renovação automática
- Logout por inatividade

#### Validações
- CPF com algoritmo oficial
- Email: formato e unicidade
- Uploads: tipo e tamanho
- Campos obrigatórios
- Proteção CSRF

#### Logs de Auditoria
- Registro automático de ações
- Dados sensíveis protegidos
- Rastreabilidade completa
- Retenção configurável

## Estrutura do Projeto

```
CaotinhoAuMiau/
├── Controllers/          # Controladores MVC
│   ├── Admin/           # Área administrativa
│   ├── Autenticacao/    # Sistema de login
│   ├── Home/            # Páginas públicas
│   └── Usuario/         # Área do usuário
├── Models/              # Modelos de dados
│   ├── Enums/          # Enumerações
│   └── ViewModels/     # ViewModels
├── Services/            # Serviços de negócio
├── Views/               # Views Razor
│   ├── Admin/          # Views administrativas
│   ├── Home/           # Views públicas
│   ├── Shared/         # Layouts compartilhados
│   └── Usuario/        # Views do usuário
├── wwwroot/            # Arquivos estáticos
│   ├── css/           # Estilos CSS
│   ├── js/            # Scripts JavaScript
│   └── imagens/       # Imagens e uploads
└── Data/               # Contexto do banco de dados
```

## Funcionalidades Avançadas

### Sistema de Logs
- Registro automático de ações críticas
- Categorização por tipo de operação
- Níveis de severidade (Info, Warning, Error)
- Interface administrativa para consulta

### Performance
- Cache Redis para dados frequentes
- Lazy loading para otimização
- Compressão de assets estáticos
- Connection pooling

### Monitoramento
- Dashboard com métricas em tempo real
- Estatísticas de adoções
- Acompanhamento de performance
- Relatórios personalizados

## Contribuição

Para contribuir:

1. Fork o projeto
2. Crie uma branch (`git checkout -b feature/NovaFuncionalidade`)
3. Commit suas mudanças (`git commit -m 'Adicionar nova funcionalidade'`)
4. Push para a branch (`git push origin feature/NovaFuncionalidade`)
5. Abra um Pull Request

## Contato

**Nicolas Portie** - Desenvolvedor

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/nicolasportie/)

## Licença

Este projeto está licenciado sob a [MIT License](LICENSE).

---

Desenvolvido para facilitar a adoção de pets e promover o bem-estar animal.