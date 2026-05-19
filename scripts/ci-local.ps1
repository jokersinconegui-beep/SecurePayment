# scripts/ci-local.ps1
Write-Host "🚀 Running CI locally..." -ForegroundColor Cyan

Write-Host "`n1. Cleaning..." -ForegroundColor Yellow
dotnet clean

Write-Host "`n2. Restoring packages..." -ForegroundColor Yellow
dotnet restore

Write-Host "`n3. Building solution..." -ForegroundColor Yellow
dotnet build --configuration Release

Write-Host "`n4. Running Domain tests..." -ForegroundColor Yellow
dotnet test tests/Domain.UnitTests/Domain.UnitTests.csproj --configuration Release --verbosity normal

Write-Host "`n5. Running Application tests..." -ForegroundColor Yellow
dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --configuration Release --verbosity normal

Write-Host "`n6. Running Integration tests..." -ForegroundColor Yellow
dotnet test tests/IntegrationTests/IntegrationTests.csproj --configuration Release --verbosity normal

Write-Host "`n✅ CI completed successfully!" -ForegroundColor Green