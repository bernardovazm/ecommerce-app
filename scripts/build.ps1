Write-Host "Executando build..." -ForegroundColor Green

Set-Location api
dotnet build --configuration Release
Set-Location ..

Set-Location web
npm run build
Set-Location ..

Write-Host "Build concluído."
Write-Host "Artefatos:"
Write-Host " - Backend: api/bin/Release/"
Write-Host " - Frontend: web/dist/"
