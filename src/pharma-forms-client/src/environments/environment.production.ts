export const environment = {
  production: true,
  apiUrl: 'https://api.example.com/api',
  appTitle: 'Pharmaceutical Forms System',
  defaultLanguage: 'ar-SA',
  tokenRefreshInterval: 540000, // 9 minutes in milliseconds
  auth: {
    clientId: 'pharma-forms-client',
    authority: 'https://auth.example.com/',
    redirectUri: 'https://forms.example.com/auth-callback',
    postLogoutRedirectUri: 'https://forms.example.com/',
    scope: 'openid profile email api://pharma-forms/forms.read api://pharma-forms/forms.write'
  }
};
