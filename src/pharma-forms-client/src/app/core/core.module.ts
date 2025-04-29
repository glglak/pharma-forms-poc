import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

// Guards
import { AuthGuard } from './guards/auth.guard';
import { AdminGuard } from './guards/admin.guard';

// Services
import { AuthService } from './services/auth.service';
import { FormService } from './services/form.service';
import { FormDependencyService } from './services/form-dependency.service';
import { StorageService } from './services/storage.service';
import { NotificationService } from './services/notification.service';

// Interceptors are provided in the app module

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    HttpClientModule,
    RouterModule
  ],
  providers: [
    AuthGuard,
    AdminGuard,
    AuthService,
    FormService,
    FormDependencyService,
    StorageService,
    NotificationService
  ]
})
export class CoreModule {
  constructor(@Optional() @SkipSelf() parentModule: CoreModule) {
    if (parentModule) {
      throw new Error('CoreModule is already loaded. Import it in the AppModule only');
    }
  }
}
