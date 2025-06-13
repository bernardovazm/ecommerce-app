#!/bin/bash
set -e

echo "Executando testes..."

cd api
dotnet test --configuration Release --verbosity normal
cd ..

cd web
npm test
npm run lint
cd ..

echo "Testes concluídos."
