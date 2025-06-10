# Ecommerce App

Aplicação full stack de loja virtual, desenvolvida com .NET no backend e React (Vite, Tailwind) no frontend, utilizando Clean Architecture e Docker para build.

## Tecnologias

Backend (.NET + PostgreSQL)

- ASP.NET Core, Entity Framework Core
- MediatR/CQRS, AutoMapper
- Docker, Swagger

Frontend (React)

- React + Vite
- Tailwind CSS, React Router, Axios
- Context API (gerenciamento de estado)

DevOps

- Docker Compose
- Integração via Swagger
- PostgreSQL como banco de dados

## Executar

#### Instale o Docker e execute os comandos no terminal:

```bash
# clone o repositório
git clone <repository-url>
# configure variáveis de ambiente
cp .env.example .env
# na raiz do projeto, execute
docker compose up --build
# frontend: http://localhost:5173
# backend: http://localhost:5220/swagger
```

#### Ou em desenvolvimento:

```bash
# env vars
cp .env.example .env
# banco
docker compose up db -d
# backend
cd api
dotnet restore
dotnet run --project Ecommerce.Api
# frontend
cd web
pnpm install
pnpm dev
```

## Visualização

- Frontend: http://localhost:5173
- API: http://localhost:5220/api
- Swagger: http://localhost:5220/swagger

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

Projeto demonstrativo com foco em arquitetura limpa, CQRS para separação de comandos e consultas, DDD, JWT, design responsivo e padrões modernos.
