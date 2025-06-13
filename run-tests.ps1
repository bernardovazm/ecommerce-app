# teste e cobertura
Set-Location api

dotnet clean
dotnet restore

dotnet test `
    --configuration Release `
    --no-restore `
    --collect:"XPlat Code Coverage" `
    --results-directory ./TestResults/ `
    --logger "trx;LogFileName=test_results.trx" `
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

if ($LASTEXITCODE -eq 0) {
    Write-Host "Testes finalizados com sucesso"

    if (Test-Path "./TestResults") {
        if (-not (dotnet tool list -g | Select-String "reportgenerator")) {
            dotnet tool install -g dotnet-reportgenerator-globaltool
        }

        reportgenerator `
            -reports:"./TestResults/**/coverage.cobertura.xml" `
            -targetdir:"./TestResults/CoverageReport" `
            -reporttypes:"Html"

        $reportPath = Resolve-Path "./TestResults/CoverageReport/index.html"
        if (Test-Path $reportPath) {
            Start-Process $reportPath
        }
    }
} else {
    Write-Host "Falha nos testes"
    exit 1
}

Set-Location ..
