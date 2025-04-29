import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

// Form components
import { ProductRegistrationComponent } from './product-registration/product-registration.component';
import { FormListComponent } from './form-list/form-list.component';
import { FormSubmissionsComponent } from './form-submissions/form-submissions.component';

const routes: Routes = [
  {
    path: '',
    component: FormListComponent
  },
  {
    path: 'submissions',
    component: FormSubmissionsComponent
  },
  {
    path: 'product-registration',
    component: ProductRegistrationComponent
  },
  {
    path: 'product-registration/:id',
    component: ProductRegistrationComponent
  }
  // Additional form routes will be added here
];

@NgModule({
  declarations: [
    ProductRegistrationComponent,
    FormListComponent,
    FormSubmissionsComponent
  ],
  imports: [
    CommonModule,
    SharedModule,
    RouterModule.forChild(routes)
  ]
})
export class FormsModule { }
