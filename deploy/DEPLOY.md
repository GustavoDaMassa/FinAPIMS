# Deploy — MSFinanceApi

## Mapa de máquinas

| EC2 | IP Privado | IP Público | Serviço |
|-----|-----------|------------|---------|
| EC2-1 | 172.31.51.75 | 54.158.191.104 | Gateway |
| EC2-2 | 172.31.55.147 | — | Identity + PostgreSQL |
| EC2-3 | 172.31.52.210 | — | Finance + PostgreSQL + Kafka |
| EC2-4 | 172.31.57.127 | — | Webhook |

---

## Security Groups (configurar no console AWS)

### EC2-1 — Gateway
| Tipo | Porta | Origem |
|------|-------|--------|
| SSH | 22 | Seu IP |
| HTTP | 8080 | 0.0.0.0/0 |

### EC2-2 — Identity
| Tipo | Porta | Origem |
|------|-------|--------|
| SSH | 22 | Seu IP |
| TCP | 8080 | 172.31.51.75/32 (Gateway) |

### EC2-3 — Finance
| Tipo | Porta | Origem |
|------|-------|--------|
| SSH | 22 | Seu IP |
| TCP | 8080 | 172.31.51.75/32 (Gateway) |
| TCP | 9092 | 172.31.57.127/32 (Webhook) |

### EC2-4 — Webhook
| Tipo | Porta | Origem |
|------|-------|--------|
| SSH | 22 | Seu IP |
| TCP | 8080 | 172.31.51.75/32 (Gateway) |

---

## Passo a passo

### 1. Instalar Docker em todas as EC2 (executar em cada uma)

```bash
chmod +x setup-docker.sh
./setup-docker.sh
# Após o script: logout e login novamente
```

### 2. EC2-3 — Finance + Kafka (subir primeiro — Webhook depende do Kafka)

```bash
scp deploy/ec2-3-finance/docker-compose.yml ec2-user@172.31.52.210:~/
ssh ec2-user@172.31.52.210

cp docker-compose.yml docker-compose.yml
cat > .env << 'EOF'
DATABASE_PASSWORD=sua-senha
EOF

docker-compose up -d
docker-compose logs -f finance  # acompanhar até "Now listening on"
```

### 3. EC2-2 — Identity

```bash
scp deploy/ec2-2-identity/docker-compose.yml ec2-user@172.31.55.147:~/
ssh ec2-user@172.31.55.147

cat > .env << 'EOF'
DATABASE_PASSWORD=sua-senha
JWT_SECRET=sua-chave-32-chars
MASTER_KEY=sua-master-key
EOF

docker-compose up -d
docker-compose logs -f identity
```

### 4. EC2-4 — Webhook

```bash
scp deploy/ec2-4-webhook/docker-compose.yml ec2-user@172.31.57.127:~/
ssh ec2-user@172.31.57.127

cat > .env << 'EOF'
PLUGGY_CLIENT_ID=seu-id
PLUGGY_CLIENT_SECRET=seu-secret
EOF

docker-compose up -d
```

### 5. EC2-1 — Gateway (subir por último)

```bash
scp deploy/ec2-1-gateway/docker-compose.yml ec2-user@172.31.51.75:~/
ssh ec2-user@172.31.51.75

cat > .env << 'EOF'
JWT_SECRET=sua-chave-32-chars   # deve ser IDÊNTICA à do Identity
EOF

docker-compose up -d
docker-compose logs -f gateway
```

---

## Verificar se o sistema está de pé

```bash
# Do seu computador, via Gateway público:
curl http://54.158.191.104:8080/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name":"Teste","email":"teste@teste.com","password":"123456"}'
```

Resposta esperada: `{"token":"...","id":"...","email":"...","name":"...",...}`

---

## Logs em tempo real (para o demo)

Abrir 4 terminais, um por EC2:

```bash
# Terminal 1 — Gateway
ssh ec2-user@172.31.51.75 "docker logs -f finance-gateway"

# Terminal 2 — Identity
ssh ec2-user@172.31.55.147 "docker logs -f finance-identity"

# Terminal 3 — Finance
ssh ec2-user@172.31.52.210 "docker logs -f finance-core"

# Terminal 4 — Webhook
ssh ec2-user@172.31.57.127 "docker logs -f finance-webhook"
```

---

## FinanceSite — configurar o endpoint do Gateway

Alterar a URL da API no FinanceSite para apontar para o Gateway público:

```
http://54.158.191.104:8080
```

---

## JWT_SECRET — atenção

O `JWT_SECRET` deve ser **exatamente o mesmo** no Gateway (EC2-1) e no Identity (EC2-2).
O Gateway usa para validar o token. O Identity usa para assinar.
