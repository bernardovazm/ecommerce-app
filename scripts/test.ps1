Write-Host "Executando testes..." -ForegroundColor Green

Set-Location api
dotnet test --configuration Release --verbosity normal
Set-Location ..

Set-Location web
npm test
npm run lint
Set-Location ..

Write-Host "Testes concluídos." -ForegroundColor Green
