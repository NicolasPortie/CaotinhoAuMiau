# CaotinhoAuMiau

Sistema web para gerenciamento de adoções de cães e gatos, desenvolvido em **ASP.NET Core MVC** com foco em automação, segurança e facilidade de uso.

---

## Sobre o Projeto

O **CaotinhoAuMiau** é uma aplicação completa para gerenciamento de processos de adoção de animais.  
O sistema foi criado para conectar pessoas interessadas em adotar cães e gatos a instituições e protetores que disponibilizam os animais, digitalizando todas as etapas do processo.

A aplicação permite controlar desde o cadastro de pets e formulários de adoção até a geração e assinatura digital de contratos.  
Com áreas distintas para usuários e administradores, o sistema garante organização, rastreabilidade e segurança em cada adoção realizada.

---

## Funcionalidades

### Área Pública
- Exibição de pets disponíveis para adoção, com fotos e informações completas  
- Filtros por espécie, idade, porte e sexo  
- Sistema de autenticação de usuários com BCrypt  
- Páginas institucionais informativas  

### Área do Usuário
- Cadastro com validação de dados e upload de foto  
- Formulário de adoção detalhado (dados pessoais, renda, moradia e motivação)  
- Acompanhamento do status das solicitações  
- Assinatura digital de contratos  
- Histórico de adoções concluídas  
- Notificações automáticas de atualização de status  

### Área Administrativa
- Dashboard com métricas e estatísticas gerais  
- Gerenciamento completo de pets (CRUD)  
- Análise e aprovação de formulários de adoção  
- Controle e geração de contratos digitais  
- Administração de usuários e colaboradores  
- Logs de auditoria e histórico de ações  
- Configuração de parâmetros do sistema e envio de emails  
- Relatórios de adoções e desempenho  

### Sistema de Contratos
- Geração automática de documentos em PDF  
- Assinatura digital vinculada ao CPF e IP do usuário  
- Versionamento e histórico de contratos assinados  
- Armazenamento seguro em `wwwroot/contratos`  
- Envio automático de notificação após assinatura  

### Automações
- Envio de emails em cada etapa da adoção  
- Notificações em tempo real no painel administrativo  
- Serviços em segundo plano para rotinas automáticas  
- Mecanismo de quarentena para usuários bloqueados temporariamente  

---

## Tecnologias Utilizadas

### Backend
- .NET 9.0  
- ASP.NET Core MVC  
- Entity Framework Core  
- SQL Server  
- BCrypt.Net  

### Frontend
- Razor Pages  
- HTML5 / CSS3  
- JavaScript  
- Bootstrap  
- jQuery  

### Bibliotecas e Serviços
- iTextSharp (geração de PDFs)  
- Newtonsoft.Json  
- Redis (cache opcional)  

---

## Instalação e Execução

### 1. Clonar o repositório
```bash
git clone https://github.com/NicolasPortie/CaotinhoAuMiau.git
cd CaotinhoAuMiau
```

### 2. Configurar a conexão com o banco

Edite o arquivo `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local);Database=CaotinhoAuMiau;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true",
    "Redis": "localhost:6379"
  }
}
```

### 3. Restaurar dependências e atualizar o banco

```bash
dotnet restore
dotnet ef database update
```

### 4. Executar o projeto

```bash
dotnet watch run
```

Acesse no navegador:

* **URL:** `https://localhost:5001` ou `http://localhost:5000`
* **Administrador padrão:** `admin@caotinhoaumiau.com.br` / `admin`

---

## Segurança

* Criptografia de senhas com BCrypt
* Cookies protegidos (HttpOnly, SameSite)
* Proteção contra CSRF
* Sessões com tempo de expiração e renovação automática
* Logs detalhados de ações críticas
* Validação de dados de entrada e uploads de arquivos

---

## Relatórios e Estatísticas

* Adoções por período, espécie e status
* Evolução mensal de adoções concluídas
* Taxa de aprovação e rejeição de formulários
* Comparativo entre espécies e portes de animais
* Relatórios administrativos exportáveis em PDF

---

## Propósito do Projeto

O **CaotinhoAuMiau** foi criado com o intuito de apoiar iniciativas de adoção responsável, reduzindo burocracias e promovendo um controle eficiente de cadastros, formulários e contratos.
Com o sistema, o processo de adoção torna-se mais rápido, transparente e acessível, garantindo maior segurança para instituições e adotantes.

---

## Contato

**Desenvolvedor:** Nicolas Portie
[LinkedIn](https://www.linkedin.com/in/nicolasportie/)

---
Desenvolvido para promover a adoção responsável e o bem-estar animal através da tecnologia.
