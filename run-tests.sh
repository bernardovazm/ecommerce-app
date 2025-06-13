#!/bin/bash
# teste e cobertura

cd api

dotnet clean
dotnet restore

dotnet test \
    --configuration Release \
    --no-restore \
    --collect:"XPlat Code Coverage" \
    --results-directory ./TestResults/ \
    --logger "trx;LogFileName=test_results.trx" \
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

if [ $? -eq 0 ]; then
    echo "Testes finalizados com sucesso"

    if [ -d "./TestResults" ]; then
        if ! dotnet tool list -g | grep -q reportgenerator; then
            dotnet tool install -g dotnet-reportgenerator-globaltool
        fi

        reportgenerator \
            -reports:"./TestResults/**/coverage.cobertura.xml" \
            -targetdir:"./TestResults/CoverageReport" \
            -reporttypes:"Html"
    fi
else
    echo "Falha nos testes"
    exit 1
fi

cd ..
