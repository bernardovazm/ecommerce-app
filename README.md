# Ecommerce App

Aplicação full stack de loja virtual, desenvolvida com .NET no backend e React (Vite, Tailwind) no frontend, utilizando Clean Architecture e Docker para build.

## Tecnologias

Backend (.NET + PostgreSQL)

- ASP.NET Core, Entity Framework Core
- MediatR/CQRS, AutoMapper
- Docker, Swagger
- RabbitMQ

Frontend (React)

- React + Vite
- Tailwind CSS, React Router, Axios
- Context API (gerenciamento de estado)

DevOps

- Docker Compose
- Integração via Swagger
- PostgreSQL como banco de dados

## Executar

Instale o Docker, clone o repositório e execute os comandos abaixo:

```bash
# crie o arquivo env e configure variáveis de ambiente
cp .env.example .env
# inicie os containers na raiz do projeto
docker compose up --build
# frontend http://localhost:5173
# swagger http://localhost:7000/swagger
```

---

Comandos para visualizar em desenvolvimento:

```bash
# db
docker compose up db -d
# web
cd web && pnpm dev
# api
cd api && dotnet run --project Ecommerce.Api
# containers
docker compose ps
```

## Estrutura

Backend

- Domain: Entidades e regras de negócio
- Application: Casos de uso (CQRS)
- Infrastructure: Acesso a dados e serviços
- Api: Controllers e configuração
- Tests: Testes unitários

Frontend

- components: Componentes reutilizáveis
- pages: Páginas da aplicação
- context: Context API (estado global)
- services: Integração com API

Banco de dados

- Usuários e Autenticação: Users, Customers
- Catálogo de Produtos: Categories, Products
- Sistema de Pedidos: Orders, OrderItems
- Pagamentos e Envio: Payments, Shipments

Projeto demonstrativo com foco em arquitetura limpa, sistema de mensageria, CQRS para separação de comandos e consultas, DDD, JWT, design responsivo e padrões modernos.
