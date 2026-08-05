# GitHub Copilot Instructions

## Project Overview

This project is a DevOps Monitoring Platform that monitors websites and APIs.
The goal is to build a production-like application demonstrating modern .NET development practices.

## Technology Stack

Backend:
- ASP.NET Core Web API
- C#
- Entity Framework Core
- SQL Server
- xUnit for testing

Frontend:
- Angular
- TypeScript

DevOps:
- Docker
- Azure
- CI/CD

## Architecture

Follow clean architecture principles.

Organize code into:

- Controllers
- Services
- Models
- DTOs
- Data
- BackgroundServices

Keep business logic out of controllers.
Controllers should only handle HTTP requests and responses.

## Coding Guidelines

- Use async/await for database and network operations.
- Use dependency injection.
- Use meaningful names.
- Keep methods small and focused.
- Prefer readable code over clever solutions.
- Add error handling where appropriate.

## Database

Use Entity Framework Core.

Do not put database logic directly in controllers.
Use services for business logic.

## API Guidelines

- Use RESTful API conventions.
- Return proper HTTP status codes.
- Use DTOs instead of exposing database entities directly.

## Testing

Write unit tests for important business logic.

## Development Goal

Prioritize production-quality code suitable for a portfolio project and junior developer job applications.