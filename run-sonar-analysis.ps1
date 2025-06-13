Write-Host "Iniciando SonarQube"

try {
    Invoke-WebRequest -Uri "http://localhost:9000/api/system/status" -UseBasicParsing -TimeoutSec 5 | Out-Null
} catch {
    Write-Host "SonarQube indisponível"
    exit 1
}

Set-Location api

$sonarScannerInstalled = dotnet tool list -g | Select-String "dotnet-sonarscanner"
if (-not $sonarScannerInstalled) {
    dotnet tool install --global dotnet-sonarscanner
}

dotnet clean

dotnet sonarscanner begin `
    /k:"ecommerce-api" `
    /d:sonar.host.url="http://localhost:9000" `
    /d:sonar.login="admin" `
    /d:sonar.password="admin" `
    /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" `
    /d:sonar.cs.vstest.reportsPaths="**/TestResults/*.trx"

dotnet build --no-restore

if (Test-Path "Ecommerce.Tests") {
    dotnet test --no-build `
        --collect:"XPlat Code Coverage" `
        --results-directory ./TestResults/ `
        --logger "trx;LogFileName=test_results.trx" `
        -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
}

dotnet sonarscanner end /d:sonar.login="admin" /d:sonar.password="admin"
Set-Location ..

Set-Location web

if (-not (Test-Path "node_modules")) {
    npm install
}

npm run lint
npm run test:coverage
npx sonar-scanner

Set-Location ..

Write-Host "Análise concluída"
