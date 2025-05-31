#!/bin/bash
echo -e "\e[32mEKG Analysis Platform - Build and Run Script\e[0m"
echo -e "\e[32m=============================================\e[0m"

# Create the missing IIntegrationEventHandler.cs file
echo -e "\e[33mCreating missing interface file...\e[0m"
INTEGRATION_EVENT_HANDLER_PATH="BuildingBlocks/EventBus/IIntegrationEventHandler.cs"
if [ ! -f "$INTEGRATION_EVENT_HANDLER_PATH" ]; then
    mkdir -p "$(dirname "$INTEGRATION_EVENT_HANDLER_PATH")"
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
    echo -e "\e[32mCreated IIntegrationEventHandler.cs\e[0m"
fi

# Check if Docker is running
echo -e "\e[33mChecking Docker...\e[0m"
if ! docker version >/dev/null 2>&1; then
    echo -e "\e[31mError: Docker is not running. Please start Docker.\e[0m"
    exit 1
fi
echo -e "\e[32mDocker is running\e[0m"

# Stop any existing containers
echo -e "\e[33mStopping existing containers...\e[0m"
docker-compose down 2>/dev/null

# Build the solution first to catch any compilation errors
echo -e "\e[33mBuilding .NET solution...\e[0m"
if ! dotnet build EkgAnalysisPlatform.sln --configuration Release; then
    echo -e "\e[31mBuild failed. Please fix compilation errors.\e[0m"
    exit 1
fi
echo -e "\e[32mBuild successful\e[0m"

# Build and run with Docker Compose
echo -e "\e[33mBuilding Docker images...\e[0m"
if ! docker-compose build; then
    echo -e "\e[31mDocker build failed.\e[0m"
    exit 1
fi

echo -e "\e[33mStarting services...\e[0m"
if ! docker-compose up -d; then
    echo -e "\e[31mFailed to start services.\e[0m"
    exit 1
fi

# Wait for services to start
echo -e "\e[33mWaiting for services to start...\e[0m"
sleep 30

# Check service health
echo -e "\e[33mChecking service health...\e[0m"
max_retries=10
retry_count=0

while [ $retry_count -lt $max_retries ]; do
    if curl -f -s http://localhost:5279/health >/dev/null 2>&1; then
        echo -e "\e[32mAll services are running!\e[0m"
        break
    else
        retry_count=$((retry_count + 1))
        if [ $retry_count -lt $max_retries ]; then
            echo -e "\e[33mServices not ready yet, waiting... ($retry_count/$max_retries)\e[0m"
            sleep 10
        else
            echo -e "\e[33mServices may not be fully ready. Check individual service logs.\e[0m"
            break
        fi
    fi
done

echo ""
echo -e "\e[32m=== EKG Analysis Platform is Running ===\e[0m"
echo ""
echo -e "\e[36mService Endpoints:\e[0m"
echo -e "\e[37m- API Gateway (Main Entry): http://localhost:5279\e[0m"
echo -e "\e[37m- API Gateway Swagger: http://localhost:5279/swagger\e[0m"
echo -e "\e[90m- Patient Service: http://localhost:5055\e[0m"
echo -e "\e[90m- EKG Signal Service: http://localhost:5125\e[0m"
echo -e "\e[90m- Analysis Service: http://localhost:5246\e[0m"
echo -e "\e[90m- Batch Processing Service: http://localhost:5181\e[0m"
echo -e "\e[33m- RabbitMQ Management: http://localhost:15672 (guest/guest)\e[0m"
echo ""
echo -e "\e[32m=== Quick Test Commands ===\e[0m"
echo -e "\e[36m# Check overall health:\e[0m"
echo -e "\e[37mcurl http://localhost:5279/health\e[0m"
echo ""
echo -e "\e[36m# Create a test patient:\e[0m"
echo -e '\e[37mcurl -X POST http://localhost:5279/api/patients -H "Content-Type: application/json" -d "{\"patientCode\":\"TEST001\",\"age\":30,\"gender\":\"Male\",\"contactInfo\":\"test@example.com\"}"\e[0m'
echo ""
echo -e "\e[33mTo stop all services: docker-compose down\e[0m"
echo -e "\e[33mTo view logs: docker-compose logs -f [service-name]\e[0m"