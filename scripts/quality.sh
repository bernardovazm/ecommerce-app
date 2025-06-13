#!/bin/bash
set -e

echo "Iniciando análise com SonarQube..."

if ! curl -s http://localhost:9000/api/system/status >/dev/null; then
    echo "SonarQube não disponível. Iniciando container..."
    docker compose up sonarqube -d
    echo "Aguardando SonarQube..."
    until curl -s http://localhost:9000/api/system/status | grep -q "UP"; do
        sleep 10
    done
fi

cd api
dotnet tool install --global dotnet-sonarscanner >/dev/null 2>&1 || true
dotnet sonarscanner begin \
    /k:"ecommerce-api" \
    /d:sonar.host.url="http://localhost:9000" \
    /d:sonar.login="admin" \
    /d:sonar.password="admin"
dotnet build
dotnet sonarscanner end /d:sonar.login="admin" /d:sonar.password="admin"
cd ..

cd web
npm run lint
npx sonar-scanner >/dev/null 2>&1 || echo "SonarScanner JS não configurado"
cd ..

echo "Análise concluída. Acesse: http://localhost:9000"
