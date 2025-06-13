if (-not (Test-Path "api/Ecommerce.sln")) {
    Write-Host "Arquivo Ecommerce.sln não encontrado" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path "web/package.json")) {
    Write-Host "Arquivo package.json não encontrado" -ForegroundColor Red
    exit 1
}

Set-Location api
try {
    dotnet restore --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }
    
    dotnet build --no-restore --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }
} catch {
    Write-Host "Falha no build .NET: $_" -ForegroundColor Red
    Set-Location ..
    exit 1
}
Set-Location ..

Set-Location web
try {
    npm install --silent
    if ($LASTEXITCODE -ne 0) { throw "npm install failed" }
    
    npm run build --silent
    if ($LASTEXITCODE -ne 0) { throw "npm build failed" }
} catch {
    Write-Host "Falha no build React: $_" -ForegroundColor Red
    Set-Location ..
    exit 1
}
Set-Location ..

Write-Host "Projeto configurado corretamente" -ForegroundColor Green
