Write-Host "Configurando ambiente"

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "Docker não está instalado"
    exit 1
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host ".NET SDK não está instalado"
    exit 1
}

if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "Node.js não está instalado"
    exit 1
}

Write-Host "Dependências OK"

dotnet tool install --global dotnet-sonarscanner --version 5.14.0

Set-Location web
npm install
Set-Location ..

docker compose up db rabbitmq sonarqube -d

do {
    Start-Sleep -Seconds 10
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:9000/api/system/status" -UseBasicParsing -TimeoutSec 5
        $status = ($response.Content | ConvertFrom-Json).status
        if ($status -eq "UP") {
            break
        }
    } catch {}
} while ($true)

Write-Host "SonarQube pronto"

$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("admin:admin"))
$headers = @{"Authorization"="Basic $auth"}

try {
    Invoke-RestMethod -Uri "http://localhost:9000/api/projects/create?project=ecommerce-fullstack&name=Ecommerce%20Full%20Stack%20Application" -Method POST -Headers $headers -ErrorAction SilentlyContinue
    Invoke-RestMethod -Uri "http://localhost:9000/api/projects/create?project=ecommerce-api&name=Ecommerce%20API" -Method POST -Headers $headers -ErrorAction SilentlyContinue
    Invoke-RestMethod -Uri "http://localhost:9000/api/projects/create?project=ecommerce-web&name=Ecommerce%20Web" -Method POST -Headers $headers -ErrorAction SilentlyContinue
} catch {}

Write-Host "Setup concluído"
