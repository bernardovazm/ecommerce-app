# Ecommerce App

Aplicação full stack de loja virtual, desenvolvida com .NET no backend e React (Vite, Tailwind) no frontend, utilizando Clean Architecture e Docker para build.

## Tecnologias

Backend (.NET 8 + PostgreSQL)

- ASP.NET Core, Entity Framework Core
- MediatR/CQRS, AutoMapper
- Docker, Swagger

Frontend (React 18)

- React + Vite
- Tailwind CSS, React Router, Axios
- Context API (gerenciamento de estado)

DevOps

- Docker Compose
- Integração via Swagger
- PostgreSQL como banco de dados

## Executar

Instale o Docker; Docker Compose e execute o seguinte comando no terminal:

```bash
docker compose up --build
```

## Visualização

- Frontend: http://localhost:5173
- API: http://localhost:5220/api
- Swagger: http://localhost:5220/swagger

## Estrutura

### Backend .NET

- Domain: Entidades e regras de negócio
- Application: Casos de uso (CQRS)
- Infrastructure: Acesso a dados e serviços
- Api: Controllers e configuração
- Tests: Testes unitários

### Frontend React

- components: Componentes reutilizáveis
- pages: Páginas da aplicação
- context: Context API (estado global)
- services: Integração com API

### DB Postgresql

- Products(id, name, description, price, imageUrl, categoryId)
- Orders(id, customerId, total, status)
- OrderItems(orderId, productId, quantity, unitPrice)

Projeto demonstrativo com foco em arquitetura limpa, separar de responsabilidades e boas práticas de desenvolvimento.
