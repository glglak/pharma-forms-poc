import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { environment } from '@env/environment';

export interface FormDefinition {
  id: string;
  name: string;
  description?: string;
  version: string;
  isPublished: boolean;
  direction: 'rtl' | 'ltr';
  sections: FormSection[];
  metadata?: { [key: string]: any };
}

export interface FormSection {
  id: string;
  title: string;
  description?: string;
  isCollapsible: boolean;
  isCollapsed: boolean;
  fields: FormField[];
}

export interface FormField {
  id: string;
  label: string;
  type: string; // text, number, date, select, checkbox, etc.
  placeholder?: string;
  helpText?: string;
  isRequired: boolean;
  isReadOnly: boolean;
  isHidden: boolean;
  defaultValue?: string;
  attributes?: { [key: string]: any };
  validationRules?: ValidationRule[];
  options?: FormFieldOption[]; // For select, radio, etc.
}

export interface ValidationRule {
  type: string; // required, regex, min, max, etc.
  message: string;
  value?: any; // The value to compare against
  dependentFieldId?: string; // For conditional validation
}

export interface FormFieldOption {
  value: string;
  label: string;
  isDefault: boolean;
}

export interface FormSubmission {
  id?: string;
  formId: string;
  data: any;
  status: 'draft' | 'submitted' | 'approved' | 'rejected';
  createdBy?: string;
  createdAt?: Date;
  updatedBy?: string;
  updatedAt?: Date;
}

@Injectable({
  providedIn: 'root'
})
export class FormService {
  private apiUrl = `${environment.apiUrl}/forms`;

  constructor(private http: HttpClient) { }

  /**
   * Get all form definitions
   */
  getForms(page: number = 1, pageSize: number = 10): Observable<{ forms: FormDefinition[], total: number }> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<{ forms: FormDefinition[], total: number }>(this.apiUrl, { params })
      .pipe(
        catchError(error => {
          console.error('Error fetching forms', error);
          return of({ forms: [], total: 0 });
        })
      );
  }

  /**
   * Get a single form definition by ID
   */
  getForm(id: string): Observable<FormDefinition> {
    return this.http.get<FormDefinition>(`${this.apiUrl}/${id}`)
      .pipe(
        catchError(error => {
          console.error(`Error fetching form ${id}`, error);
          throw error;
        })
      );
  }

  /**
   * Create a new form definition
   */
  createForm(form: FormDefinition): Observable<FormDefinition> {
    return this.http.post<FormDefinition>(this.apiUrl, form)
      .pipe(
        catchError(error => {
          console.error('Error creating form', error);
          throw error;
        })
      );
  }

  /**
   * Update an existing form definition
   */
  updateForm(form: FormDefinition): Observable<FormDefinition> {
    return this.http.put<FormDefinition>(`${this.apiUrl}/${form.id}`, form)
      .pipe(
        catchError(error => {
          console.error(`Error updating form ${form.id}`, error);
          throw error;
        })
      );
  }

  /**
   * Delete a form definition
   */
  deleteForm(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
      .pipe(
        catchError(error => {
          console.error(`Error deleting form ${id}`, error);
          throw error;
        })
      );
  }

  /**
   * Submit form data
   */
  submitFormData(formId: string, data: any): Observable<FormSubmission> {
    return this.http.post<FormSubmission>(`${this.apiUrl}/${formId}/submissions`, data)
      .pipe(
        catchError(error => {
          console.error(`Error submitting form data for ${formId}`, error);
          throw error;
        })
      );
  }

  /**
   * Get submission by ID
   */
  getSubmission(formId: string, submissionId: string): Observable<FormSubmission> {
    return this.http.get<FormSubmission>(`${this.apiUrl}/${formId}/submissions/${submissionId}`)
      .pipe(
        catchError(error => {
          console.error(`Error fetching submission ${submissionId}`, error);
          throw error;
        })
      );
  }

  /**
   * Get all submissions for a form
   */
  getSubmissions(formId: string, page: number = 1, pageSize: number = 10): Observable<{ submissions: FormSubmission[], total: number }> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<{ submissions: FormSubmission[], total: number }>(`${this.apiUrl}/${formId}/submissions`, { params })
      .pipe(
        catchError(error => {
          console.error(`Error fetching submissions for form ${formId}`, error);
          return of({ submissions: [], total: 0 });
        })
      );
  }

  /**
   * Update submission status
   */
  updateSubmissionStatus(formId: string, submissionId: string, status: 'draft' | 'submitted' | 'approved' | 'rejected'): Observable<FormSubmission> {
    return this.http.patch<FormSubmission>(`${this.apiUrl}/${formId}/submissions/${submissionId}/status`, { status })
      .pipe(
        catchError(error => {
          console.error(`Error updating submission status ${submissionId}`, error);
          throw error;
        })
      );
  }

  /**
   * Import forms from Excel
   */
  importFormsFromExcel(file: File): Observable<{ imported: number, errors: any[] }> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<{ imported: number, errors: any[] }>(`${this.apiUrl}/import`, formData)
      .pipe(
        catchError(error => {
          console.error('Error importing forms from Excel', error);
          throw error;
        })
      );
  }

  /**
   * Export forms to Excel
   */
  exportFormsToExcel(formIds: string[]): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/export`, { formIds }, { responseType: 'blob' })
      .pipe(
        catchError(error => {
          console.error('Error exporting forms to Excel', error);
          throw error;
        })
      );
  }
}
