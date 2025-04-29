import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { FormService, FormSubmission } from '@core/services/form.service';
import { FormDependencyService } from '@core/services/form-dependency.service';
import { NotificationService } from '@core/services/notification.service';
import { StorageService } from '@core/services/storage.service';

@Component({
  selector: 'app-product-registration',
  templateUrl: './product-registration.component.html',
  styleUrls: ['./product-registration.component.scss']
})
export class ProductRegistrationComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  isLoading = false;
  isSubmitting = false;
  formId = 'product-registration';
  submissionId?: string;
  isEditMode = false;
  
  // Lookup data
  activeIngredients: any[] = [];
  manufacturerOptions: any[] = [];
  dosageForms: any[] = [];
  
  // Utils
  private destroy$ = new Subject<void>();
  
  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private formService: FormService,
    private dependencyService: FormDependencyService,
    private notificationService: NotificationService,
    private storageService: StorageService,
    private translate: TranslateService
  ) { }

  ngOnInit(): void {
    // Check if we're in edit mode
    this.route.params.pipe(takeUntil(this.destroy$)).subscribe(params => {
      this.submissionId = params['id'];
      this.isEditMode = !!this.submissionId;
      
      this.initForm();
      this.loadLookupData();
      
      if (this.isEditMode && this.submissionId) {
        this.loadSubmission(this.submissionId);
      }
    });
  }
  
  ngOnDestroy(): void {
    // Unsubscribe from all observables
    this.destroy$.next();
    this.destroy$.complete();
    
    // Unregister form from dependency service
    this.dependencyService.unregisterForm(this.formId);
  }
  
  /**
   * Initialize the form with validation rules
   */
  private initForm(): void {
    this.form = this.fb.group({
      // Basic Information
      productName: ['', [Validators.required, Validators.maxLength(200)]],
      activeIngredientId: ['', Validators.required],
      strengthValue: ['', [Validators.required, Validators.min(0)]],
      strengthUnit: ['', Validators.required],
      dosageForm: ['', Validators.required],
      packSize: ['', [Validators.required, Validators.min(1)]],
      
      // Manufacturer Information
      manufacturerId: ['', Validators.required],
      manufacturingDate: [null, Validators.required],
      expiryDate: [null, Validators.required],
      batchNumber: ['', [Validators.required, Validators.pattern(/^[A-Z0-9-]{5,20}$/)]],
      
      // Regulatory Information
      registrationNumber: ['', Validators.pattern(/^[A-Z0-9-]{10,15}$/)],
      registrationDate: [null],
      approvalStatus: ['pending', Validators.required],
      
      // Special fields for dependencies
      relatedCompoundId: [''],
      parentRegistrationId: ['']
    });
    
    // Register form with dependency service
    this.dependencyService.registerForm(this.formId, this.form);
    
    // Set up form field dependencies
    this.setupFieldDependencies();
  }
  
  /**
   * Load related lookup data
   */
  private loadLookupData(): void {
    this.isLoading = true;
    
    // Load active ingredients (simplified for the demo, would come from API)
    this.activeIngredients = [
      { id: 'ing1', name: 'Paracetamol' },
      { id: 'ing2', name: 'Ibuprofen' },
      { id: 'ing3', name: 'Amoxicillin' },
      { id: 'ing4', name: 'Omeprazole' }
    ];
    
    // Load manufacturers (simplified for the demo, would come from API)
    this.manufacturerOptions = [
      { id: 'man1', name: 'Pharma International' },
      { id: 'man2', name: 'Medical Solutions' },
      { id: 'man3', name: 'Healthline Pharmaceuticals' }
    ];
    
    // Load dosage forms (simplified for the demo, would come from API)
    this.dosageForms = [
      { value: 'tablet', label: this.translate.instant('FORMS.DOSAGE_FORMS.TABLET') },
      { value: 'capsule', label: this.translate.instant('FORMS.DOSAGE_FORMS.CAPSULE') },
      { value: 'syrup', label: this.translate.instant('FORMS.DOSAGE_FORMS.SYRUP') },
      { value: 'injection', label: this.translate.instant('FORMS.DOSAGE_FORMS.INJECTION') },
      { value: 'cream', label: this.translate.instant('FORMS.DOSAGE_FORMS.CREAM') }
    ];
    
    this.isLoading = false;
  }
  
  /**
   * Load an existing submission for editing
   */
  private loadSubmission(submissionId: string): void {
    this.isLoading = true;
    
    this.formService.getSubmission(this.formId, submissionId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (submission) => {
          // Patch the form with the submission data
          this.form.patchValue(submission.data);
          this.isLoading = false;
        },
        error: (error) => {
          this.notificationService.showError(
            this.translate.instant('ERRORS.LOAD_SUBMISSION_FAILED')
          );
          this.isLoading = false;
        }
      });
  }
  
  /**
   * Set up field dependencies
   */
  private setupFieldDependencies(): void {
    // When manufacturing date changes, calculate expiry date (2 years later)
    this.form.get('manufacturingDate')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(date => {
        if (date) {
          const expiryDate = new Date(date);
          expiryDate.setFullYear(expiryDate.getFullYear() + 2);
          this.form.get('expiryDate')?.setValue(expiryDate);
        }
      });
      
    // Listen for changes from other forms (through the dependency service)
    this.dependencyService.getDependenciesForForm(this.formId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(dependencies => {
        // Process each dependency if needed
        console.log('Form dependencies:', dependencies);
      });
  }
  
  /**
   * Save the form as a draft
   */
  saveDraft(): void {
    // Basic validation - we still allow saving with invalid fields for drafts
    if (this.form.get('productName')?.value === '') {
      this.notificationService.showWarning(
        this.translate.instant('FORMS.PRODUCT_NAME_REQUIRED')
      );
      return;
    }
    
    this.submitForm('draft');
  }
  
  /**
   * Submit the form
   */
  submitForm(status: 'draft' | 'submitted' = 'submitted'): void {
    // Full validation for submitted forms
    if (status === 'submitted' && this.form.invalid) {
      this.markFormGroupTouched(this.form);
      this.notificationService.showWarning(
        this.translate.instant('FORMS.VALIDATION_ERRORS')
      );
      return;
    }
    
    this.isSubmitting = true;
    
    const formData = this.form.value;
    const submission: Partial<FormSubmission> = {
      formId: this.formId,
      data: formData,
      status: status
    };
    
    // If we're in edit mode, update the existing submission
    if (this.isEditMode && this.submissionId) {
      this.formService.updateSubmissionStatus(this.formId, this.submissionId, status)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (result) => {
            this.handleSubmitSuccess(status);
          },
          error: (error) => {
            this.handleSubmitError();
          }
        });
    } else {
      // Otherwise create a new submission
      this.formService.submitFormData(this.formId, submission)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (result) => {
            this.handleSubmitSuccess(status);
          },
          error: (error) => {
            this.handleSubmitError();
          }
        });
    }
  }
  
  /**
   * Handle successful submission
   */
  private handleSubmitSuccess(status: 'draft' | 'submitted'): void {
    this.isSubmitting = false;
    
    if (status === 'draft') {
      this.notificationService.showSuccess(
        this.translate.instant('FORMS.DRAFT_SAVED')
      );
    } else {
      this.notificationService.showSuccess(
        this.translate.instant('FORMS.SUBMISSION_SUCCESSFUL')
      );
      // Navigate to the confirmation page or form list
      this.router.navigate(['/forms/submissions']);
    }
  }
  
  /**
   * Handle submission error
   */
  private handleSubmitError(): void {
    this.isSubmitting = false;
    this.notificationService.showError(
      this.translate.instant('ERRORS.SUBMISSION_FAILED')
    );
  }
  
  /**
   * Reset the form to initial state
   */
  resetForm(): void {
    this.form.reset();
    this.initForm();
  }
  
  /**
   * Cancel and go back
   */
  cancel(): void {
    this.router.navigate(['/forms/submissions']);
  }
  
  /**
   * Mark all form controls as touched to trigger validation display
   */
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      
      if ((control as any).controls) {
        this.markFormGroupTouched(control as FormGroup);
      }
    });
  }
}
