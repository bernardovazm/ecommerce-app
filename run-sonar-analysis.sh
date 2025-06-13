#!/bin/bash

echo "Iniciando SonarQube"

if ! curl -s http://localhost:9000/api/system/status > /dev/null; then
    echo "SonarQube não está disponível. Execute 'docker compose up sonarqube -d'"
    exit 1
fi

cd api

if ! dotnet tool list -g | grep -q dotnet-sonarscanner; then
    dotnet tool install --global dotnet-sonarscanner
fi

dotnet clean

dotnet sonarscanner begin \
    /k:"ecommerce-api" \
    /d:sonar.host.url="http://localhost:9000" \
    /d:sonar.login="admin" \
    /d:sonar.password="admin" \
    /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" \
    /d:sonar.cs.vstest.reportsPaths="**/TestResults/*.trx"

dotnet build --no-restore

if [ -d "Ecommerce.Tests" ]; then
    dotnet test --no-build \
        --collect:"XPlat Code Coverage" \
        --results-directory ./TestResults/ \
        --logger "trx;LogFileName=test_results.trx" \
        -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
fi

dotnet sonarscanner end /d:sonar.login="admin" /d:sonar.password="admin"
cd ..

cd web

if [ ! -d "node_modules" ]; then
    npm install
fi

npm run lint
npm run test:coverage
npx sonar-scanner

cd ..

echo "Análise concluída"
