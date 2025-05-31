Write-Host "EKG Analysis Platform - Build and Run Script" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

# Create the missing IIntegrationEventHandler.cs file
Write-Host "Creating missing interface file..." -ForegroundColor Yellow
$integrationEventHandlerPath = "BuildingBlocks/EventBus/IIntegrationEventHandler.cs"
if (-not (Test-Path $integrationEventHandlerPath)) {
    $directory = Split-Path -Path $integrationEventHandlerPath -Parent
    if (-not (Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    
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
    Write-Host "Created IIntegrationEventHandler.cs" -ForegroundColor Green
}

# Check if Docker is running
Write-Host "Checking Docker..." -ForegroundColor Yellow
try {
    docker version | Out-Null
    Write-Host "Docker is running" -ForegroundColor Green
} catch {
    Write-Host "Error: Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}

# Stop any existing containers
Write-Host "Stopping existing containers..." -ForegroundColor Yellow
docker-compose down 2>$null

# Build the solution first to catch any compilation errors
Write-Host "Building .NET solution..." -ForegroundColor Yellow
try {
    dotnet build EkgAnalysisPlatform.sln --configuration Release
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed. Please fix compilation errors." -ForegroundColor Red
        exit 1
    }
    Write-Host "Build successful" -ForegroundColor Green
} catch {
    Write-Host "Error building solution: $_" -ForegroundColor Red
    exit 1
}

# Build and run with Docker Compose
Write-Host "Building Docker images..." -ForegroundColor Yellow
docker-compose build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker build failed." -ForegroundColor Red
    exit 1
}

Write-Host "Starting services..." -ForegroundColor Yellow
docker-compose up -d
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to start services." -ForegroundColor Red
    exit 1
}

# Wait for services to start
Write-Host "Waiting for services to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Check service health
Write-Host "Checking service health..." -ForegroundColor Yellow
$maxRetries = 10
$retryCount = 0

do {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5279/health" -TimeoutSec 5 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Host "All services are running!" -ForegroundColor Green
            break
        }
    } catch {
        $retryCount++
        if ($retryCount -lt $maxRetries) {
            Write-Host "Services not ready yet, waiting... ($retryCount/$maxRetries)" -ForegroundColor Yellow
            Start-Sleep -Seconds 10
        } else {
            Write-Host "Services may not be fully ready. Check individual service logs." -ForegroundColor Yellow
            break
        }
    }
} while ($retryCount -lt $maxRetries)

Write-Host ""
Write-Host "=== EKG Analysis Platform is Running ===" -ForegroundColor Green
Write-Host ""
Write-Host "Service Endpoints:" -ForegroundColor Cyan
Write-Host "- API Gateway (Main Entry): http://localhost:5279" -ForegroundColor White
Write-Host "- API Gateway Swagger: http://localhost:5279/swagger" -ForegroundColor White
Write-Host "- Patient Service: http://localhost:5055" -ForegroundColor Gray
Write-Host "- EKG Signal Service: http://localhost:5125" -ForegroundColor Gray
Write-Host "- Analysis Service: http://localhost:5246" -ForegroundColor Gray
Write-Host "- Batch Processing Service: http://localhost:5181" -ForegroundColor Gray
Write-Host "- RabbitMQ Management: http://localhost:15672 (guest/guest)" -ForegroundColor Yellow
Write-Host ""
Write-Host "=== Quick Test Commands ===" -ForegroundColor Green
Write-Host "# Check overall health:" -ForegroundColor Cyan
Write-Host "curl http://localhost:5279/health" -ForegroundColor White
Write-Host ""
Write-Host "# Create a test patient:" -ForegroundColor Cyan
Write-Host 'curl -X POST http://localhost:5279/api/patients -H "Content-Type: application/json" -d "{\"patientCode\":\"TEST001\",\"age\":30,\"gender\":\"Male\",\"contactInfo\":\"test@example.com\"}"' -ForegroundColor White
Write-Host ""
Write-Host "To stop all services: docker-compose down" -ForegroundColor Yellow
Write-Host "To view logs: docker-compose logs -f [service-name]" -ForegroundColor Yellow