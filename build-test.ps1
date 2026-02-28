#!/usr/bin/env pwsh
cd "C:\Users\rjr\RiderProjects\StockPilot\StockPilot.Bmad"
Write-Host "Building..."
dotnet build
$buildStatus = $LASTEXITCODE
Write-Host "Build status: $buildStatus"

if ($buildStatus -eq 0) {
    Write-Host "Testing..."
    dotnet test
    Write-Host "Test complete"
}
else {
    Write-Host "Build failed"
}

