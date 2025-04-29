export const environment = {
  production: false,
  apiUrl: 'https://localhost:7135/api',
  appTitle: 'Pharmaceutical Forms System - Development',
  defaultLanguage: 'ar-SA',
  tokenRefreshInterval: 540000, // 9 minutes in milliseconds
  auth: {
    clientId: 'pharma-forms-client',
    authority: 'https://example.com/',
    redirectUri: 'http://localhost:4200/auth-callback',
    postLogoutRedirectUri: 'http://localhost:4200/',
    scope: 'openid profile email api://pharma-forms/forms.read api://pharma-forms/forms.write'
  }
};
