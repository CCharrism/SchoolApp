#!/usr/bin/env powershell

# Start Backend API
Write-Host "Starting .NET API..." -ForegroundColor Green
Start-Job -Name "API" -ScriptBlock {
    Set-Location "c:\Users\HARRISMAIZAN\Desktop\SchoolApp\api"
    dotnet run
}

# Wait a moment for API to start
Start-Sleep -Seconds 3

# Start Frontend
Write-Host "Starting Angular Frontend..." -ForegroundColor Green
Start-Job -Name "Frontend" -ScriptBlock {
    Set-Location "c:\Users\HARRISMAIZAN\Desktop\SchoolApp\client"
    ng serve --open
}

Write-Host "Both services are starting..." -ForegroundColor Yellow
Write-Host "API will be available at: https://localhost:7039" -ForegroundColor Cyan
Write-Host "Frontend will be available at: http://localhost:4200" -ForegroundColor Cyan

# Monitor both jobs
do {
    Start-Sleep -Seconds 5
    $apiJob = Get-Job -Name "API" -ErrorAction SilentlyContinue
    $frontendJob = Get-Job -Name "Frontend" -ErrorAction SilentlyContinue
    
    if ($apiJob) {
        Write-Host "API Status: $($apiJob.State)" -ForegroundColor Green
    }
    if ($frontendJob) {
        Write-Host "Frontend Status: $($frontendJob.State)" -ForegroundColor Blue
    }
    
    # Check if either job failed
    if ($apiJob.State -eq "Failed" -or $frontendJob.State -eq "Failed") {
        Write-Host "One of the services failed. Check the output." -ForegroundColor Red
        break
    }
} while ($true)

# Clean up on exit
Remove-Job -Name "API","Frontend" -Force -ErrorAction SilentlyContinue
