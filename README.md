EKG Analysis Platform
A medical signal processing application for analyzing and visualizing electrocardiogram (EKG) data. Built with .NET 8, this solution demonstrates a modern architecture for handling medical telemetry data.
Overview
This platform enables medical professionals to:

Upload and visualize EKG signal data
Perform automated analysis for heart rate calculation and arrhythmia detection
Process signals in batch using background processing
Store and retrieve patient cardiac data

Architecture
The solution follows clean architecture principles with these components:

EkgAnalysisPlatform.Core: Domain models and business logic
EkgAnalysisPlatform.Infrastructure: Data access and external services
EkgAnalysisPlatform.Api: REST API endpoints
EkgAnalysisPlatform.Web: Blazor WebAssembly UI
EkgAnalysisPlatform.Functions: Azure Functions for batch processing
EkgAnalysisPlatform.Tests: Unit tests

Technologies Used

ASP.NET Core 8
Entity Framework Core
Blazor WebAssembly
Azure Functions
SQL Server
xUnit for testing

Features

Signal Visualization: Interactive charts for EKG data
Automated Analysis: Heart rate calculation and arrhythmia detection
Batch Processing: Background analysis using Azure Functions
RESTful API: Clean interface for data access

Getting Started
Prerequisites

.NET 8 SDK
SQL Server (LocalDB or higher)

Installation

Clone the repository
git clone https://github.com/ikalinin-dotnet/EkgAnalysisPlatform.git
cd EkgAnalysisPlatform

Restore dependencies
dotnet restore

Build the solution
dotnet build

Run the API
cd EkgAnalysisPlatform.Api
dotnet run

Run the Web app (in a separate terminal)
cd EkgAnalysisPlatform.Web
dotnet run


Project Status
This is a portfolio project demonstrating .NET development skills for medical software applications. It showcases architecturally sound practices for handling specialized signal processing requirements.