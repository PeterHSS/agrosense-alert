# 🌱 AgroSense Alert API

Microsserviço responsável pelo gerenciamento de alertas da plataforma **AgroSense** — um sistema de monitoramento agrícola inteligente baseado em sensores de campo.

---

## 📋 Sobre o Projeto

O **AgroSense Alert API** é um serviço da arquitetura de microsserviços do AgroSense, responsável por:

- Receber e processar eventos de sensores agrícolas via mensageria (RabbitMQ)
- Criar, gerenciar e consultar alertas com base em regras configuráveis
- Notificar outros serviços quando condições críticas são detectadas (temperatura, umidade, solo, etc.)
- Persistir o histórico de alertas no banco de dados

---

## 🏗️ Arquitetura

O serviço faz parte de um ecossistema de microsserviços implantado em **Kubernetes**, composto por:

| Serviço | Responsabilidade |
|---|---|
| `agrosense-api-gateway` | Roteamento e entrada de requisições externas |
| `agrosense-api-identity` | Autenticação e autorização |
| `agrosense-api-alert` | **Este serviço** — gestão de alertas |
| `agrosense-api-property` | Gestão de propriedades rurais |
| `agrosense-api-sensor` | Ingestão de dados dos sensores |

---

## 🛠️ Tecnologias

- **.NET / C#** — Framework principal
- **PostgreSQL** — Banco de dados relacional (`postgres-alerts`)
- **RabbitMQ** — Mensageria assíncrona entre microsserviços
- **Docker** — Containerização
- **Kubernetes** — Orquestração de containers
- **Prometheus** — Coleta de métricas
- **Grafana + Loki** — Observabilidade e centralização de logs

---

## 🚀 Como Executar

### Pré-requisitos

- [.NET SDK](https://dotnet.microsoft.com/download) 8+
- [Docker](https://www.docker.com/)
- [kubectl](https://kubernetes.io/docs/tasks/tools/) (para deploy no cluster)

### Localmente

```bash
# Clone o repositório
git clone https://github.com/PeterHSS/agrosense-alert.git
cd agrosense-alert

# Restaure as dependências
dotnet restore

# Execute
dotnet run --project Api/
```

### Com Docker

```bash
# Build da imagem
docker build -t agrosense-alert .

# Execute o container
docker run -p 8080:80 agrosense-alert
```

---

## ☸️ Deploy no Kubernetes

O serviço é implantado no namespace `agrosense` como um `ClusterIP`:

```bash
# Verifique o serviço no cluster
kubectl get services -n agrosense

# Verifique os pods em execução
kubectl get pods -n agrosense -l app=agrosense-api-alert

# Logs do serviço
kubectl logs -n agrosense -l app=agrosense-api-alert --follow
```

O serviço está acessível internamente no cluster via:
```
http://agrosense-api-alert.agrosense.svc.cluster.local:80
```

---

## 📊 Observabilidade

### Prometheus

Métricas expostas no endpoint `/metrics`, coletadas pelo Prometheus em:
```
http://prometheus:9090
```

### Grafana + Loki

Dashboards e logs centralizados acessíveis via Grafana. Datasources configurados:

| Fonte | URL interna |
|---|---|
| Prometheus | `http://prometheus:9090` |
| Loki | `http://loki:3100` |

---

## 🔁 CI/CD

O repositório utiliza **GitHub Actions** (`.github/workflows/`) para:

- Build e testes automáticos a cada push
- Build e push da imagem Docker para o registry
- Deploy automatizado no cluster Kubernetes

---

## 📁 Estrutura do Projeto

```
agrosense-alert/
├── .github/
│   └── workflows/              # Pipelines de CI/CD
├── Api/
│   ├── Common/
│   │   └── Middlewares/        # Middlewares globais da aplicação
│   ├── Domain/
│   │   ├── Abstractions/
│   │   │   ├── Rules/          # Regras de negócio abstratas
│   │   │   └── UseCases/       # Interfaces dos casos de uso
│   │   ├── Entities/           # Entidades do domínio
│   │   └── Events/             # Eventos de domínio
│   ├── Features/
│   │   └── Alert/
│   │       ├── GetActive/      # Caso de uso: buscar alertas ativos
│   │       └── ProcessReading/ # Caso de uso: processar leitura de sensor
│   ├── Infrastructure/
│   │   ├── Messaging/          # Integração com RabbitMQ
│   │   ├── Persistence/
│   │   │   ├── Configurations/ # Configurações do EF Core
│   │   │   ├── Contexts/       # DbContext
│   │   │   └── Migrations/     # Migrações do banco de dados
│   │   └── Settings/           # Configurações de infraestrutura
│   └── Properties/             # Configurações do projeto .NET
├── .dockerignore
├── .gitignore
├── AgroSense.Alert.slnx        # Solution file
└── README.md
```

---

## 📄 Licença

Este projeto está licenciado sob a licença **MIT**. Consulte o arquivo [LICENSE](./LICENSE) para mais detalhes.
