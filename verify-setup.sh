#!/bin/bash

if [ ! -f "api/Ecommerce.sln" ]; then
    echo "Arquivo Ecommerce.sln não encontrado"
    exit 1
fi

if [ ! -f "web/package.json" ]; then
    echo "Arquivo package.json não encontrado"
    exit 1
fi

cd api
if ! dotnet restore --verbosity quiet; then
    echo "Falha ao restaurar dependências .NET"
    exit 1
fi

if ! dotnet build --no-restore --verbosity quiet; then
    echo "Falha ao compilar projeto .NET"
    exit 1
fi
cd ..

cd web
if ! npm install --silent; then
    echo "Falha ao instalar dependências npm"
    exit 1
fi

if ! npm run build --silent; then
    echo "Falha ao compilar projeto React"
    exit 1
fi
cd ..

echo "Projeto configurado corretamente"