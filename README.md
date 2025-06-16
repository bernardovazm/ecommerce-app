# Ecommerce App

Aplicação full stack de loja virtual com .NET, React, PostgreSQL e Docker.

## Tecnologias

**Backend**: ASP.NET Core, Entity Framework, MediatR/CQRS, XUnit  
**Frontend**: React, Vite, Tailwind CSS, Vitest  
**DevOps**: Docker Compose, GitHub Actions, SonarQube

## Executar o Projeto

```bash
# Configurar
npm run setup
# Inicie os containers
npm run docker

# ou desenvolvimento local
npm run dev
```

**URLs:**

- Frontend: http://localhost:5173
- API: http://localhost:7000/swagger # Docker

### Automação e Setup

```bash
npm run setup
npm run build
npm run test
npm run dev
npm run docker
npm run clean
npm run quality
```

Projeto demonstrativo com foco em Clean Architecture, DDD e padrões modernos.
