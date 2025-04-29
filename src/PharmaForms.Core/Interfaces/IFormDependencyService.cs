using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using PharmaForms.Core.Entities;

namespace PharmaForms.Core.Interfaces
{
    /// <summary>
    /// Service interface for managing form dependencies and executing dependency logic.
    /// </summary>
    public interface IFormDependencyService
    {
        /// <summary>
        /// Get all dependencies for a form.
        /// </summary>
        /// <param name="formId">The form ID</param>
        /// <returns>The list of dependencies</returns>
        Task<IEnumerable<FormDependency>> GetDependenciesForFormAsync(string formId);
        
        /// <summary>
        /// Create a new dependency.
        /// </summary>
        /// <param name="dependency">The dependency to create</param>
        /// <returns>The created dependency</returns>
        Task<FormDependency> CreateDependencyAsync(FormDependency dependency);
        
        /// <summary>
        /// Update an existing dependency.
        /// </summary>
        /// <param name="dependency">The dependency to update</param>
        /// <returns>The updated dependency</returns>
        Task<FormDependency> UpdateDependencyAsync(FormDependency dependency);
        
        /// <summary>
        /// Delete a dependency.
        /// </summary>
        /// <param name="id">The ID of the dependency to delete</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteDependencyAsync(string id);
        
        /// <summary>
        /// Process dependencies for a form submission.
        /// This applies all dependencies to the form data.
        /// </summary>
        /// <param name="formId">The form ID</param>
        /// <param name="formData">The form data</param>
        /// <returns>The processed form data</returns>
        Task<JsonDocument> ProcessDependenciesAsync(string formId, JsonDocument formData);
        
        /// <summary>
        /// Validate a form submission with its dependencies.
        /// </summary>
        /// <param name="formId">The form ID</param>
        /// <param name="formData">The form data</param>
        /// <returns>Whether the form is valid and any error messages</returns>
        Task<(bool IsValid, IEnumerable<string> Errors)> ValidateWithDependenciesAsync(string formId, JsonDocument formData);
    }
}
