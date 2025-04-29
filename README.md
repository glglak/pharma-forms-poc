# Pharmaceutical Forms System POC

A proof of concept for a pharmaceutical data collection system with multi-form support for Arabic forms.

## Technology Stack

- Backend: .NET Core 8 API
- Frontend: Angular 20+
- Database: SQL Server (with potential Elasticsearch integration for search)
- Authentication: JWT with Active Directory integration

## Features

- Support for 250+ pharmaceutical data forms
- Arabic (RTL) interface
- Form interdependencies and cross-validation
- Dynamic form generation
- Excel import/export

## Project Structure

- `/src/PharmaForms.Api` - .NET Core 8 API backend
- `/src/PharmaForms.Core` - Core business logic and domain models
- `/src/PharmaForms.Infrastructure` - Data access and external services
- `/src/pharma-forms-client` - Angular 20+ frontend application

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- SQL Server or SQL Server Express
- Visual Studio 2022 or VS Code

### Setup Instructions

Details on setting up development environment will be added soon.

## Form Generation Approach

This project uses a form-by-form generation approach, where each pharmaceutical form is generated individually with its specific fields and validations, while sharing common components and services for:

- Form state management
- Cross-form dependencies
- Validation logic
- Data submission

## License

MIT
