# Build and run EKG Analysis Platform

# Make sure Dockerfiles are in the correct locations
Write-Host "Checking Dockerfile locations..." -ForegroundColor Green

# Create directories if they don't exist
$directories = @(
    "Gateway/EkgAnalysisPlatform.ApiGateway",
    "Services/PatientService/EkgAnalysisPlatform.PatientService.API",
    "Services/EkgSignalService/EkgAnalysisPlatform.EkgSignalService.API",
    "Services/AnalysisService/EkgAnalysisPlatform.AnalysisService.API",
    "Services/BatchProcessingService/EkgAnalysisPlatform.BatchProcessingService.API"
)

foreach ($dir in $directories) {
    if (-not (Test-Path $dir)) {
        Write-Host "Creating directory: $dir" -ForegroundColor Yellow
        New-Item -Path $dir -ItemType Directory -Force | Out-Null
    }
}

# Copy Dockerfile templates to their correct locations if needed
Write-Host "Ensuring Dockerfiles are in the correct locations..." -ForegroundColor Green

# Add the missing IIntegrationEventHandler.cs file
Write-Host "Adding missing IIntegrationEventHandler.cs file..." -ForegroundColor Green
$integrationEventHandlerPath = "BuildingBlocks/EventBus/IIntegrationEventHandler.cs"
if (-not (Test-Path $integrationEventHandlerPath)) {
    Set-Content -Path $integrationEventHandlerPath -Value @"
namespace EkgAnalysisPlatform.BuildingBlocks.EventBus
{
    public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
        where TIntegrationEvent : IntegrationEvent
    {
        Task Handle(TIntegrationEvent @event);
    }

    public interface IIntegrationEventHandler
    {
    }
}
"@
}

# Build and run with Docker Compose
Write-Host "Building and running with Docker Compose..." -ForegroundColor Green
docker-compose -f docker-compose.yml build
docker-compose -f docker-compose.yml up -d

Write-Host "All services are running!" -ForegroundColor Green
Write-Host "Access the API Gateway at http://localhost:5279" -ForegroundColor Yellow

Write-Host "Service endpoints:" -ForegroundColor Cyan
Write-Host "- Patient Service: http://localhost:5055" -ForegroundColor Cyan
Write-Host "- EKG Signal Service: http://localhost:5125" -ForegroundColor Cyan
Write-Host "- Analysis Service: http://localhost:5246" -ForegroundColor Cyan
Write-Host "- Batch Processing Service: http://localhost:5181" -ForegroundColor Cyan
Write-Host "- RabbitMQ Management UI: http://localhost:15672 (guest/guest)" -ForegroundColor Cyan