# Pharma Forms Client

This is the Angular 20+ frontend application for the Pharmaceutical Forms System.

## Features

- Dynamic form rendering
- Arabic (RTL) support
- Form state management
- Cross-form dependencies
- Excel import/export interface
- Authentication and authorization

## Project Structure

- `/src/app/core` - Core services, guards, and interceptors
- `/src/app/shared` - Shared components, directives, and pipes
- `/src/app/features` - Feature modules (forms, admin, reports)
- `/src/app/forms` - Individual form components
- `/src/app/layout` - Layout components
- `/src/assets/i18n` - Internationalization files

## RTL Support

The application is configured for right-to-left (RTL) text direction for Arabic language support using Angular Material's built-in RTL capabilities.

## Form State Management

The application uses a combination of Angular's Reactive Forms and a custom form state service to manage form data and cross-form dependencies.

## Building and Running

```bash
# Install dependencies
npm install

# Development server
npm start

# Build
npm run build

# Tests
npm test
```

## Form Generation Approach

Each form is generated as an individual component with its specific fields and validations, while leveraging shared services for:

- Form state management
- Cross-form dependencies
- Validation logic
- Data submission
