# Ecommerce App

Aplicação full stack de loja virtual com .NET, React, PostgreSQL e Docker.

## Tecnologias

**Backend**: ASP.NET Core, Entity Framework, MediatR/CQRS, XUnit  
**Frontend**: React, Vite, Tailwind CSS, Vitest  
**DevOps**: Docker Compose, GitHub Actions, SonarQube

## Executar o Projeto

```bash
# Configurar
./scripts/setup.sh     # Linux/Mac
scripts\setup.ps1      # Windows

# Iniciar containers
docker compose up --build

# Ao finalizar, acesse
# Frontend: http://localhost:5173
# API: http://localhost:7000/swagger
```

### Testes e Setup

```bash
./scripts/setup.sh     # Configuração inicial
./scripts/build.sh     # Build completo
./scripts/test.sh      # Executar testes
./scripts/quality.sh   # Análise de qualidade
```

### Desenvolvimento

```bash
# Banco de dados
docker compose up db -d

# Frontend
cd web && npm run dev

# Backend
cd api && dotnet run --project Ecommerce.Api

# Teste
dotnet test --filter "Category=Unit"
npm test -- --coverage
```

Projeto demonstrativo com foco em Clean Architecture, DDD e padrões modernos.
