# PharmaForms.Infrastructure

This project contains infrastructure-related implementations like data access, external services integration, and cross-cutting concerns.

## Features

- SQL Server data access
- (Future) Elasticsearch integration
- External service clients
- Caching implementation
- File storage

## Project Structure

- `/Data` - Database contexts and migrations
- `/Repositories` - Data access implementations
- `/Services` - External service clients and implementations
- `/Persistence` - ORM configurations and mappings
- `/Caching` - Cache implementations
- `/DependencyInjection` - Infrastructure service registration

## Configuration

This project provides extension methods for service registration in the API project.
