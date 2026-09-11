# FCG.PaymentsAPI

Microsserviço responsável pelo processamento de pagamentos da plataforma **Fiap Cloud Games (FCG)**.

## Responsabilidades

- **Consome** `OrderPlacedEvent` publicado pelo `fcg-catalog-api`
- **Simula** processamento de pagamento via gateway externo (latência de 200ms)
- **Publica** `PaymentProcessedEvent` com status `Approved` ou `Rejected`
- Taxa de aprovação padrão: **90%**

## Fluxo de Eventos

```
CatalogAPI → [OrderPlacedEvent] → PaymentsAPI → [PaymentProcessedEvent] → CatalogAPI + NotificationsAPI
```

## Estrutura

```
fcg-payments-api/
├── src/
│   └── FCG.PaymentsAPI/
│       ├── Consumers/
│       │   └── OrderPlacedConsumer.cs   # Consome pedido, publica resultado
│       ├── Program.cs
│       └── appsettings.json
├── k8s/
│   ├── namespace.yaml
│   ├── configmap.yaml
│   ├── secret.yaml
│   ├── deployment.yaml
│   └── service.yaml
├── Dockerfile
└── docker-compose.yml
```

## Variáveis de Ambiente

| Variável                  | Padrão      | Descrição                         |
|---------------------------|-------------|-----------------------------------|
| `RabbitMQ__Host`          | `localhost` | Host do RabbitMQ                  |
| `RabbitMQ__VirtualHost`   | `/`         | VirtualHost do RabbitMQ           |
| `RabbitMQ__Username`      | `guest`     | Usuário do RabbitMQ               |
| `RabbitMQ__Password`      | `guest`     | Senha do RabbitMQ *(via Secret)*  |

## Executar localmente

```bash
# Subir RabbitMQ + PaymentsAPI
docker compose up -d

# Health check
curl http://localhost:8083/health
```

## Exemplo de log

```
[PAGAMENTO] 📥 Pedido recebido | OrderId: abc123 | UserId: def456 | Jogo: Elden Ring | Valor: R$ 149,99
[PAGAMENTO] ✅ Pagamento APROVADO | OrderId: abc123 | TransactionId: FA3C9B21... | Valor: R$ 149,99
[PAGAMENTO] 📤 PaymentProcessedEvent publicado | OrderId: abc123 | Status: Approved
```

## Deploy no Kubernetes

```bash
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/secret.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
```

## Dependências

- **FCG.Contracts** 1.0.0 (NuGet) — contratos compartilhados de eventos
- **MassTransit.RabbitMQ** 8.2.5
- **Serilog.AspNetCore** 8.0.2

## Grupo 17 — Pos-Tech FIAP
- Letícia Lopes Ribeiro Vasconcelos
- Lucas Monte Ferreri Castilho
- Marcelo Henrique Cornelis Rei
- Rafael Ribeiro Arantes
- Vinícius Calixto Real
