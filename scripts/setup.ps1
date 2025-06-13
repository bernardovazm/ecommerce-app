Write-Host "Configurando projeto Ecommerce..." -ForegroundColor Green

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "Docker não encontrado" -ForegroundColor Red
    exit 1
}
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host ".NET SDK não encontrado" -ForegroundColor Red
    exit 1
}
if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "Node.js não encontrado" -ForegroundColor Red
    exit 1
}

Write-Host "Configurando backend..." -ForegroundColor Cyan
Set-Location api
dotnet restore
dotnet build
Set-Location ..

Write-Host "Configurando frontend..." -ForegroundColor Cyan
Set-Location web
Remove-Item -Recurse -Force node_modules, package-lock.json -ErrorAction SilentlyContinue
npm install
Set-Location ..

Write-Host "Iniciando serviços..." -ForegroundColor Cyan
Copy-Item .env.example .env -ErrorAction SilentlyContinue
docker compose up db rabbitmq -d

Write-Host "Setup concluído." -ForegroundColor Green
Write-Host "Próximos passos:" -ForegroundColor Yellow
Write-Host " - Frontend: cd web && npm run dev"
Write-Host " - Backend: cd api && dotnet run --project Ecommerce.Api"
Write-Host " - Containers: docker compose up --build"
