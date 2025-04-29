import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { FormGroup, AbstractControl } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { map, catchError } from 'rxjs/operators';
import { environment } from '@env/environment';

export interface FormDependency {
  id: string;
  sourceFormId: string;
  sourceFieldId: string;
  targetFormId: string;
  targetFieldId: string;
  dependencyType: 'value' | 'lookup' | 'visibility' | 'validation' | 'calculation';
  expression?: string;
  lookupKey?: string;
  description?: string;
}

@Injectable({
  providedIn: 'root'
})
export class FormDependencyService {
  private formRegistry = new Map<string, FormGroup>();
  private formValuesRegistry = new Map<string, any>();
  private dependenciesSubject = new BehaviorSubject<FormDependency[]>([]);
  
  constructor(private http: HttpClient) {
    this.loadDependencies();
  }
  
  /**
   * Register a form with the dependency service
   */
  registerForm(formId: string, form: FormGroup): void {
    this.formRegistry.set(formId, form);
    
    // Store initial values
    this.formValuesRegistry.set(formId, form.value);
    
    // Subscribe to value changes to track dependencies
    form.valueChanges.subscribe(values => {
      this.formValuesRegistry.set(formId, values);
      this.processDependenciesForForm(formId);
    });
  }
  
  /**
   * Unregister a form when it's destroyed
   */
  unregisterForm(formId: string): void {
    this.formRegistry.delete(formId);
    this.formValuesRegistry.delete(formId);
  }
  
  /**
   * Get the current form values
   */
  getFormValues(formId: string): any {
    return this.formValuesRegistry.get(formId);
  }
  
  /**
   * Load dependencies from the backend
   */
  private loadDependencies(): void {
    this.http.get<FormDependency[]>(`${environment.apiUrl}/formdependencies`)
      .pipe(
        catchError(error => {
          console.error('Error loading form dependencies', error);
          return of([]);
        })
      )
      .subscribe(dependencies => {
        this.dependenciesSubject.next(dependencies);
      });
  }
  
  /**
   * Get dependencies for a specific form
   */
  getDependenciesForForm(formId: string): Observable<FormDependency[]> {
    return this.dependenciesSubject.pipe(
      map(dependencies => dependencies.filter(
        d => d.sourceFormId === formId || d.targetFormId === formId
      ))
    );
  }
  
  /**
   * Process dependencies when a form's values change
   */
  private processDependenciesForForm(formId: string): void {
    const dependencies = this.dependenciesSubject.value;
    
    // Find dependencies where this form is the source
    const relevantDependencies = dependencies.filter(
      dep => dep.sourceFormId === formId
    );
    
    // Process each dependency
    relevantDependencies.forEach(dependency => {
      this.processDependency(dependency);
    });
  }
  
  /**
   * Process a single dependency
   */
  private processDependency(dependency: FormDependency): void {
    // Get the source form value
    const sourceValues = this.formValuesRegistry.get(dependency.sourceFormId);
    if (!sourceValues) return;
    
    // Get the source field value
    const sourceValue = sourceValues[dependency.sourceFieldId];
    
    // Get the target form
    const targetForm = this.formRegistry.get(dependency.targetFormId);
    if (!targetForm) return;
    
    // Get the target field
    const targetField = targetForm.get(dependency.targetFieldId);
    if (!targetField) return;
    
    // Process based on dependency type
    switch (dependency.dependencyType) {
      case 'value':
        this.processValueDependency(sourceValue, targetField);
        break;
      case 'lookup':
        this.processLookupDependency(dependency, sourceValue, targetField);
        break;
      case 'visibility':
        this.processVisibilityDependency(dependency, sourceValue, targetField);
        break;
      case 'validation':
        this.processValidationDependency(targetField);
        break;
      case 'calculation':
        this.processCalculationDependency(dependency, targetField);
        break;
    }
  }
  
  /**
   * Process a simple value dependency (copy value)
   */
  private processValueDependency(sourceValue: any, targetField: AbstractControl): void {
    targetField.setValue(sourceValue, { emitEvent: false });
  }
  
  /**
   * Process a lookup dependency
   */
  private processLookupDependency(dependency: FormDependency, sourceValue: any, targetField: AbstractControl): void {
    if (!sourceValue || !dependency.lookupKey) return;
    
    this.http.get(`${environment.apiUrl}/lookup/${dependency.lookupKey}/${sourceValue}`)
      .pipe(
        catchError(error => {
          console.error('Error in lookup dependency', error);
          return of(null);
        })
      )
      .subscribe(lookupValue => {
        if (lookupValue !== null) {
          targetField.setValue(lookupValue, { emitEvent: false });
        }
      });
  }
  
  /**
   * Process a visibility dependency
   */
  private processVisibilityDependency(dependency: FormDependency, sourceValue: any, targetField: AbstractControl): void {
    if (!dependency.expression) return;
    
    try {
      // Safe evaluation of expression
      const result = this.evaluateExpression(dependency.expression, { value: sourceValue });
      
      if (result) {
        targetField.enable({ emitEvent: false });
      } else {
        targetField.disable({ emitEvent: false });
      }
    } catch (error) {
      console.error('Error evaluating visibility expression', error);
    }
  }
  
  /**
   * Process a validation dependency
   */
  private processValidationDependency(targetField: AbstractControl): void {
    targetField.updateValueAndValidity({ emitEvent: false });
  }
  
  /**
   * Process a calculation dependency
   */
  private processCalculationDependency(dependency: FormDependency, targetField: AbstractControl): void {
    if (!dependency.expression) return;
    
    try {
      // Get all form values to use in the calculation
      const context: any = {};
      
      // Add each form's values to the context
      this.formValuesRegistry.forEach((values, formId) => {
        context[formId] = values;
      });
      
      // Evaluate the expression
      const result = this.evaluateExpression(dependency.expression, context);
      
      // Set the result
      targetField.setValue(result, { emitEvent: false });
    } catch (error) {
      console.error('Error evaluating calculation expression', error);
    }
  }
  
  /**
   * Safely evaluate a JavaScript expression
   */
  private evaluateExpression(expression: string, context: any): any {
    // Create a function from the expression with the context as parameters
    const keys = Object.keys(context);
    const values = Object.values(context);
    
    // Use Function constructor for a sandbox (safer than eval)
    const func = new Function(...keys, `return ${expression};`);
    
    // Execute the function with the context values
    return func(...values);
  }
  
  /**
   * Manually trigger dependency processing for a form
   */
  refreshDependencies(formId: string): void {
    this.processDependenciesForForm(formId);
  }
  
  /**
   * Add a new dependency programmatically
   */
  addDependency(dependency: FormDependency): void {
    const dependencies = this.dependenciesSubject.value;
    dependencies.push(dependency);
    this.dependenciesSubject.next(dependencies);
    
    // Process the new dependency
    this.processDependency(dependency);
  }
}
