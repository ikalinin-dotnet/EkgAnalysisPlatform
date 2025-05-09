#!/bin/bash

# Build and run EKG Analysis Platform

# Create directories if they don't exist
echo -e "\e[32mChecking Dockerfile locations...\e[0m"
mkdir -p Gateway/EkgAnalysisPlatform.ApiGateway
mkdir -p Services/PatientService/EkgAnalysisPlatform.PatientService.API
mkdir -p Services/EkgSignalService/EkgAnalysisPlatform.EkgSignalService.API
mkdir -p Services/AnalysisService/EkgAnalysisPlatform.AnalysisService.API
mkdir -p Services/BatchProcessingService/EkgAnalysisPlatform.BatchProcessingService.API

# Move Dockerfiles to correct locations
echo -e "\e[32mCopying Dockerfiles to their correct locations...\e[0m"
cp -f Gateway/EkgAnalysisPlatform.ApiGateway/Dockerfile Gateway/EkgAnalysisPlatform.ApiGateway/
cp -f Services/PatientService/EkgAnalysisPlatform.PatientService.API/Dockerfile Services/PatientService/EkgAnalysisPlatform.PatientService.API/
cp -f Services/EkgSignalService/EkgAnalysisPlatform.EkgSignalService.API/Dockerfile Services/EkgSignalService/EkgAnalysisPlatform.EkgSignalService.API/
cp -f Services/AnalysisService/EkgAnalysisPlatform.AnalysisService.API/Dockerfile Services/AnalysisService/EkgAnalysisPlatform.AnalysisService.API/
cp -f Services/BatchProcessingService/EkgAnalysisPlatform.BatchProcessingService.API/Dockerfile Services/BatchProcessingService/EkgAnalysisPlatform.BatchProcessingService.API/

# Add the missing IIntegrationEventHandler.cs file
echo -e "\e[32mAdding missing IIntegrationEventHandler.cs file...\e[0m"
INTEGRATION_EVENT_HANDLER_PATH="BuildingBlocks/EventBus/IIntegrationEventHandler.cs"
cat > "$INTEGRATION_EVENT_HANDLER_PATH" << 'EOF'
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
EOF

# Build and run with Docker Compose
echo -e "\e[32mBuilding and running with Docker Compose...\e[0m"
docker-compose build
docker-compose up -d

echo -e "\e[32mAll services are running!\e[0m"
echo -e "\e[33mAccess the API Gateway at http://localhost:5279\e[0m"

echo -e "\e[36mService endpoints:\e[0m"
echo -e "\e[36m- Patient Service: http://localhost:5055\e[0m"
echo -e "\e[36m- EKG Signal Service: http://localhost:5125\e[0m"
echo -e "\e[36m- Analysis Service: http://localhost:5246\e[0m"
echo -e "\e[36m- Batch Processing Service: http://localhost:5181\e[0m"
echo -e "\e[36m- RabbitMQ Management UI: http://localhost:15672 (guest/guest)\e[0m"