# EKG Analysis Platform - Microservices Architecture

This project implements a microservices-based platform for EKG signal analysis using .NET 8.0 and Docker.

## Architecture Overview

The platform consists of the following microservices:

1. **API Gateway**: Entry point for all client requests, handles routing and authentication
2. **Patient Service**: Manages patient data
3. **EKG Signal Service**: Processes and stores EKG signal data
4. **Analysis Service**: Performs analysis on EKG signals, providing diagnostic insights
5. **Batch Processing Service**: Handles batch processing jobs for large datasets

## Technology Stack

- **.NET 8.0**: Core application framework
- **Entity Framework Core**: ORM for data access
- **RabbitMQ**: Message broker for inter-service communication
- **Docker & Docker Compose**: Containerization and orchestration
- **SQLite**: Lightweight database for each service

## Getting Started

### Prerequisites

- Docker & Docker Compose
- .NET 8.0 SDK (for development)

### Running the Application

#### Using Scripts

For Windows users:
```powershell
.\build-and-run.ps1
```

For Linux/macOS users:
```bash
chmod +x build-and-run.sh
./build-and-run.sh
```

#### Manual Setup

1. Clone the repository
2. Place the Dockerfiles in their respective directories:
   ```
   Gateway/EkgAnalysisPlatform.ApiGateway/Dockerfile
   Services/PatientService/EkgAnalysisPlatform.PatientService.API/Dockerfile
   Services/EkgSignalService/EkgAnalysisPlatform.EkgSignalService.API/Dockerfile
   Services/AnalysisService/EkgAnalysisPlatform.AnalysisService.API/Dockerfile
   Services/BatchProcessingService/EkgAnalysisPlatform.BatchProcessingService.API/Dockerfile
   ```
3. Run the docker-compose command:
   ```
   docker-compose build
   docker-compose up -d
   ```

## Service Endpoints

- **API Gateway**: http://localhost:5279
- **Patient Service**: http://localhost:5055
- **EKG Signal Service**: http://localhost:5125
- **Analysis Service**: http://localhost:5246
- **Batch Processing Service**: http://localhost:5181
- **RabbitMQ Management UI**: http://localhost:15672 (username: guest, password: guest)

## Project Structure

```
EkgAnalysisPlatform-Microservices/
├── BuildingBlocks/
│   ├── Common/
│   └── EventBus/
├── Gateway/
│   └── EkgAnalysisPlatform.ApiGateway/
├── Services/
│   ├── PatientService/
│   │   ├── EkgAnalysisPlatform.PatientService.API/
│   │   ├── EkgAnalysisPlatform.PatientService.Domain/
│   │   └── EkgAnalysisPlatform.PatientService.Infrastructure/
│   ├── EkgSignalService/
│   │   ├── EkgAnalysisPlatform.EkgSignalService.API/
│   │   ├── EkgAnalysisPlatform.EkgSignalService.Domain/
│   │   └── EkgAnalysisPlatform.EkgSignalService.Infrastructure/
│   ├── AnalysisService/
│   │   ├── EkgAnalysisPlatform.AnalysisService.API/
│   │   ├── EkgAnalysisPlatform.AnalysisService.Domain/
│   │   └── EkgAnalysisPlatform.AnalysisService.Infrastructure/
│   └── BatchProcessingService/
│       ├── EkgAnalysisPlatform.BatchProcessingService.API/
│       ├── EkgAnalysisPlatform.BatchProcessingService.Domain/
│       └── EkgAnalysisPlatform.BatchProcessingService.Infrastructure/
└── docker-compose.yml
```

## Development

Each service follows a clean architecture pattern with three projects:
- **API**: Controllers, DTOs, and API configuration
- **Domain**: Domain models, interfaces, and business logic
- **Infrastructure**: Data access, external service integration

## Communication

Services communicate using:
1. **Synchronous REST API calls** via the API Gateway
2. **Asynchronous messaging** using RabbitMQ for event-driven communication

The `IEventBus` interface in the EventBus building block provides a consistent API for publishing and subscribing to integration events.

## Persistence

Each service maintains its own SQLite database:
- **PatientService**: `patient.db`
- **EkgSignalService**: `ekgsignal.db`
- **AnalysisService**: `analysis.db`
- **BatchProcessingService**: `batch.db`

For production deployments, consider replacing SQLite with more robust database solutions like SQL Server, PostgreSQL, or MongoDB.