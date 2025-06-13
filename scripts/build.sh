#!/bin/bash
set -e

echo "Executando build..."

cd api
dotnet build --configuration Release
cd ..

cd web
npm run build
cd ..

echo "Build concluído."
echo "Artefatos:"
echo " - Backend: api/bin/Release/"
echo " - Frontend: web/dist/"
