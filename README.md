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

## Form Development Process

When creating a new form:

1. Define the form fields and validation rules
2. Create a dedicated component for the form
3. Register any dependencies with other forms
4. Add routing in the forms module
5. Implement localization for Arabic and English

## Working with Arabic (RTL) Support

The application fully supports right-to-left (RTL) layouts for Arabic with:

- RTL layout switching based on language
- Bidirectional text support
- Angular Material components configured for RTL
- Custom RTL-specific styles

## License

MIT
