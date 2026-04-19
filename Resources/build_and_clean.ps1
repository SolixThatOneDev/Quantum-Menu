Write-Host "Building Quantum Menu..." -ForegroundColor Cyan
dotnet build QuantumMenu.csproj

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful. Cleaning up .txt files..." -ForegroundColor Green
    # Remove all .txt files in the current directory and subdirectories
    Get-ChildItem -Path . -Filter *.txt -Recurse | Remove-Item -Force
    Write-Host "Cleanup complete." -ForegroundColor Green
} else {
    Write-Host "Build failed." -ForegroundColor Red
}
