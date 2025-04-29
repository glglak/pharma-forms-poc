using System.Collections.Generic;
using System.Threading.Tasks;
using PharmaForms.Core.Entities;

namespace PharmaForms.Core.Interfaces
{
    /// <summary>
    /// Repository interface for form operations.
    /// </summary>
    public interface IFormRepository
    {
        /// <summary>
        /// Get all form definitions with pagination.
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>A tuple with the forms and total count</returns>
        Task<(IEnumerable<FormDefinition> Forms, int TotalCount)> GetFormsAsync(int page = 1, int pageSize = 10);
        
        /// <summary>
        /// Get a form definition by ID.
        /// </summary>
        /// <param name="id">The form ID</param>
        /// <returns>The form definition, or null if not found</returns>
        Task<FormDefinition> GetFormByIdAsync(string id);
        
        /// <summary>
        /// Create a new form definition.
        /// </summary>
        /// <param name="form">The form to create</param>
        /// <returns>The created form</returns>
        Task<FormDefinition> CreateFormAsync(FormDefinition form);
        
        /// <summary>
        /// Update an existing form definition.
        /// </summary>
        /// <param name="form">The form to update</param>
        /// <returns>The updated form</returns>
        Task<FormDefinition> UpdateFormAsync(FormDefinition form);
        
        /// <summary>
        /// Delete a form definition.
        /// </summary>
        /// <param name="id">The ID of the form to delete</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteFormAsync(string id);
        
        /// <summary>
        /// Search for forms by name or description.
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>A tuple with the matching forms and total count</returns>
        Task<(IEnumerable<FormDefinition> Forms, int TotalCount)> SearchFormsAsync(string searchTerm, int page = 1, int pageSize = 10);
        
        /// <summary>
        /// Get form submissions for a specific form.
        /// </summary>
        /// <param name="formId">The form ID</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>A tuple with the submissions and total count</returns>
        Task<(IEnumerable<FormSubmission> Submissions, int TotalCount)> GetFormSubmissionsAsync(string formId, int page = 1, int pageSize = 10);
        
        /// <summary>
        /// Get a specific form submission.
        /// </summary>
        /// <param name="id">The submission ID</param>
        /// <returns>The form submission, or null if not found</returns>
        Task<FormSubmission> GetSubmissionByIdAsync(string id);
        
        /// <summary>
        /// Create a new form submission.
        /// </summary>
        /// <param name="submission">The submission to create</param>
        /// <returns>The created submission</returns>
        Task<FormSubmission> CreateSubmissionAsync(FormSubmission submission);
        
        /// <summary>
        /// Update an existing form submission.
        /// </summary>
        /// <param name="submission">The submission to update</param>
        /// <returns>The updated submission</returns>
        Task<FormSubmission> UpdateSubmissionAsync(FormSubmission submission);
        
        /// <summary>
        /// Delete a form submission.
        /// </summary>
        /// <param name="id">The ID of the submission to delete</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteSubmissionAsync(string id);
        
        /// <summary>
        /// Get all form dependencies with pagination.
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>A tuple with the dependencies and total count</returns>
        Task<(IEnumerable<FormDependency> Dependencies, int TotalCount)> GetDependenciesAsync(int page = 1, int pageSize = 10);
        
        /// <summary>
        /// Get a form dependency by ID.
        /// </summary>
        /// <param name="id">The dependency ID</param>
        /// <returns>The form dependency, or null if not found</returns>
        Task<FormDependency> GetDependencyByIdAsync(string id);
        
        /// <summary>
        /// Get dependencies for a specific form.
        /// </summary>
        /// <param name="formId">The form ID</param>
        /// <returns>The list of dependencies</returns>
        Task<IEnumerable<FormDependency>> GetDependenciesForFormAsync(string formId);
        
        /// <summary>
        /// Create a new form dependency.
        /// </summary>
        /// <param name="dependency">The dependency to create</param>
        /// <returns>The created dependency</returns>
        Task<FormDependency> CreateDependencyAsync(FormDependency dependency);
        
        /// <summary>
        /// Update an existing form dependency.
        /// </summary>
        /// <param name="dependency">The dependency to update</param>
        /// <returns>The updated dependency</returns>
        Task<FormDependency> UpdateDependencyAsync(FormDependency dependency);
        
        /// <summary>
        /// Delete a form dependency.
        /// </summary>
        /// <param name="id">The ID of the dependency to delete</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteDependencyAsync(string id);
    }
}
