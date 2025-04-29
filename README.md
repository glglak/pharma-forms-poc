# Pharmaceutical Forms System POC

A proof of concept for a pharmaceutical data collection system with multi-form support for Arabic forms.

## Technology Stack

- **Backend**: .NET Core 8 API
- **Frontend**: Angular 20+
- **Database**: SQL Server (with potential Elasticsearch integration for search)
- **Authentication**: JWT with Active Directory integration

## Features

- Support for 250+ pharmaceutical data forms with Arabic (RTL) interface
- Form interdependencies and cross-form validation
- Dynamic form generation
- Excel import/export
- User management with role-based access control
- Reporting and analytics
- Audit logging

## Project Structure

- `/src/PharmaForms.Api` - .NET Core 8 API backend
- `/src/PharmaForms.Core` - Core business logic and domain models
- `/src/PharmaForms.Infrastructure` - Data access and external services
- `/src/pharma-forms-client` - Angular 20+ frontend application

## Key Components

### Backend

- **Form Definition System**: Defines the structure of forms with sections and fields
- **Form Submission System**: Captures and manages submitted form data
- **Form Dependency Engine**: Handles cross-form relationships and validations
- **Data Storage**: SQL Server for relational data with potential Elasticsearch for search

### Frontend

- **Dynamic Form Renderer**: Creates forms based on definitions from the backend
- **Form State Manager**: Manages form data and cross-form dependencies
- **RTL Support**: Full Arabic language and right-to-left layout support
- **Validation Engine**: Client-side validation with server validation integration

## Form Generation Approach

This project uses a form-by-form generation approach, where each pharmaceutical form is generated individually with its specific fields and validations, while sharing common components and services for:

- Form state management
- Cross-form dependencies
- Validation logic
- Data submission

By combining individually tailored forms with shared infrastructure, we achieve a balance between:
1. Customization for specific form requirements
2. Consistency across the application
3. Development efficiency
4. Maintainability

### Dependencies Between Forms

One of the key challenges addressed in this POC is handling dependencies between different forms. For example:

- A product registration form might depend on values from a compound analysis form
- A batch release form might require data from a manufacturing record form
- A pricing form might need information from both registration and manufacturing forms

To handle these dependencies, we've implemented a `FormDependencyService` that:
- Tracks dependencies between forms
- Propagates value changes across forms
- Validates cross-form constraints
- Handles lookups based on values in other forms

## Implementation Progress

We have successfully implemented the core structure of the POC:

- **Backend**:
  - Core domain models for forms, submissions, and dependencies
  - Repository pattern for data access
  - Dependency management service
  - RESTful API endpoints for form operations
  - SQL Server integration

- **Frontend**:
  - Angular project structure
  - Form dependency service for cross-form communication
  - Dynamic form component for form rendering
  - RTL support for Arabic forms
  - Product registration form as an example implementation

## Next Steps

- Complete the form conversion from Excel to form definitions
- Implement Excel import/export functionality
- Add unit and integration tests
- Set up continuous integration
- Add dashboard and reporting features
- Implement user management and authentication

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- SQL Server or SQL Server Express
- Visual Studio 2022 or VS Code

### Backend Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/glglak/pharma-forms-poc.git
   cd pharma-forms-poc
   ```

2. Restore packages and build the solution:
   ```bash
   dotnet restore
   dotnet build
   ```

3. Update the database connection string in `src/PharmaForms.Api/appsettings.json`

4. Run migrations to create the database:
   ```bash
   cd src/PharmaForms.Api
   dotnet ef database update
   ```

5. Run the API:
   ```bash
   dotnet run
   ```

### Frontend Setup

1. Navigate to the Angular project:
   ```bash
   cd src/pharma-forms-client
   ```

2. Install npm packages:
   ```bash
   npm install
   ```

3. Update the API URL in `src/environments/environment.ts` if needed

4. Run the development server:
   ```bash
   npm start
   ```

5. Open your browser to `http://localhost:4200`

## License

MIT
