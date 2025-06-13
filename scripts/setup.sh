#!/bin/bash
set -e

echo "Configurando projeto Ecommerce..."

command -v docker >/dev/null 2>&1 || { echo "Docker não encontrado"; exit 1; }
command -v dotnet >/dev/null 2>&1 || { echo ".NET SDK não encontrado"; exit 1; }
command -v node >/dev/null 2>&1 || { echo "Node.js não encontrado"; exit 1; }

echo "Configurando backend..."
cd api
dotnet restore
dotnet build
cd ..

echo "Configurando frontend..."
cd web
rm -rf node_modules package-lock.json 2>/dev/null || true
npm install
cd ..

echo "Iniciando serviços..."
cp .env.example .env 2>/dev/null || true
docker compose up db rabbitmq -d

echo "Setup concluído."
echo "Próximos passos:"
echo " - Frontend: cd web && npm run dev"
echo " - Backend: cd api && dotnet run --project Ecommerce.Api"
echo " - Containers: docker compose up --build"
