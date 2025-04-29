using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Jint;
using PharmaForms.Core.Entities;
using PharmaForms.Core.Interfaces;

namespace PharmaForms.Core.Services
{
    /// <summary>
    /// Service for managing form dependencies and executing dependency logic.
    /// </summary>
    public class FormDependencyService : IFormDependencyService
    {
        private readonly IFormRepository _formRepository;
        private readonly ILogger<FormDependencyService> _logger;

        public FormDependencyService(
            IFormRepository formRepository,
            ILogger<FormDependencyService> logger)
        {
            _formRepository = formRepository ?? throw new ArgumentNullException(nameof(formRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all dependencies for a form.
        /// </summary>
        public async Task<IEnumerable<FormDependency>> GetDependenciesForFormAsync(string formId)
        {
            try
            {
                return await _formRepository.GetDependenciesForFormAsync(formId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dependencies for form {FormId}", formId);
                throw;
            }
        }

        /// <summary>
        /// Create a new dependency.
        /// </summary>
        public async Task<FormDependency> CreateDependencyAsync(FormDependency dependency)
        {
            try
            {
                // Validate the dependency
                await ValidateDependencyAsync(dependency);

                // Set creation metadata
                dependency.CreatedAt = DateTime.UtcNow;

                // Save the dependency
                return await _formRepository.CreateDependencyAsync(dependency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dependency between {SourceForm}.{SourceField} and {TargetForm}.{TargetField}",
                    dependency.SourceFormId, dependency.SourceFieldId, dependency.TargetFormId, dependency.TargetFieldId);
                throw;
            }
        }

        /// <summary>
        /// Update an existing dependency.
        /// </summary>
        public async Task<FormDependency> UpdateDependencyAsync(FormDependency dependency)
        {
            try
            {
                // Validate the dependency
                await ValidateDependencyAsync(dependency);

                // Set update metadata
                dependency.UpdatedAt = DateTime.UtcNow;

                // Update the dependency
                return await _formRepository.UpdateDependencyAsync(dependency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dependency {DependencyId}", dependency.Id);
                throw;
            }
        }

        /// <summary>
        /// Delete a dependency.
        /// </summary>
        public async Task<bool> DeleteDependencyAsync(string id)
        {
            try
            {
                return await _formRepository.DeleteDependencyAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dependency {DependencyId}", id);
                throw;
            }
        }

        /// <summary>
        /// Process dependencies for a form submission.
        /// </summary>
        public async Task<JsonDocument> ProcessDependenciesAsync(string formId, JsonDocument formData)
        {
            try
            {
                // Get all dependencies related to this form
                var dependencies = await _formRepository.GetDependenciesForFormAsync(formId);
                
                // If no dependencies, return the original data
                if (!dependencies.Any())
                {
                    return formData;
                }

                // Create a dictionary to hold form data for all related forms
                var formDataCache = new Dictionary<string, JsonDocument>
                {
                    [formId] = formData
                };

                // Get dependencies in execution order
                var orderedDependencies = dependencies
                    .OrderBy(d => d.ExecutionOrder)
                    .ToList();

                // Process each dependency
                foreach (var dependency in orderedDependencies)
                {
                    await ProcessDependencyAsync(dependency, formDataCache);
                }

                // Return the updated form data
                return formDataCache[formId];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing dependencies for form {FormId}", formId);
                throw;
            }
        }

        /// <summary>
        /// Validate a form submission with its dependencies.
        /// </summary>
        public async Task<(bool IsValid, IEnumerable<string> Errors)> ValidateWithDependenciesAsync(string formId, JsonDocument formData)
        {
            try
            {
                // Process dependencies to get final form state
                var processedData = await ProcessDependenciesAsync(formId, formData);
                
                // Get validation dependencies
                var validationDependencies = (await _formRepository.GetDependenciesForFormAsync(formId))
                    .Where(d => d.DependencyType == "validation")
                    .ToList();
                
                // If no validation dependencies, consider it valid
                if (!validationDependencies.Any())
                {
                    return (true, Array.Empty<string>());
                }
                
                // Create a dictionary to hold form data for validation
                var formDataCache = new Dictionary<string, JsonDocument>
                {
                    [formId] = processedData
                };
                
                // Validate against each dependency
                var errors = new List<string>();
                foreach (var dependency in validationDependencies)
                {
                    var (isValid, errorMessage) = await ValidateDependencyRuleAsync(dependency, formDataCache);
                    if (!isValid)
                    {
                        errors.Add(errorMessage ?? $"Validation failed for {dependency.TargetFormId}.{dependency.TargetFieldId}");
                    }
                }
                
                return (errors.Count == 0, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating form {FormId} with dependencies", formId);
                throw;
            }
        }

        /// <summary>
        /// Validate a dependency before creating or updating it.
        /// </summary>
        private async Task ValidateDependencyAsync(FormDependency dependency)
        {
            // Validate that source and target forms exist
            var sourceForm = await _formRepository.GetFormByIdAsync(dependency.SourceFormId);
            if (sourceForm == null)
            {
                throw new ArgumentException($"Source form with ID {dependency.SourceFormId} not found", nameof(dependency.SourceFormId));
            }

            var targetForm = await _formRepository.GetFormByIdAsync(dependency.TargetFormId);
            if (targetForm == null)
            {
                throw new ArgumentException($"Target form with ID {dependency.TargetFormId} not found", nameof(dependency.TargetFormId));
            }

            // Validate that source and target fields exist in their respective forms
            if (!FormHasField(sourceForm, dependency.SourceFieldId))
            {
                throw new ArgumentException($"Source field {dependency.SourceFieldId} not found in form {dependency.SourceFormId}", nameof(dependency.SourceFieldId));
            }

            if (!FormHasField(targetForm, dependency.TargetFieldId))
            {
                throw new ArgumentException($"Target field {dependency.TargetFieldId} not found in form {dependency.TargetFormId}", nameof(dependency.TargetFieldId));
            }

            // Validate dependency type
            var validTypes = new[] { "value", "lookup", "visibility", "validation", "calculation" };
            if (!validTypes.Contains(dependency.DependencyType))
            {
                throw new ArgumentException($"Invalid dependency type: {dependency.DependencyType}. Must be one of: {string.Join(", ", validTypes)}", nameof(dependency.DependencyType));
            }

            // Validate that expressions are provided for certain dependency types
            if ((dependency.DependencyType == "calculation" || dependency.DependencyType == "visibility" || dependency.DependencyType == "validation") 
                && string.IsNullOrEmpty(dependency.Expression))
            {
                throw new ArgumentException($"Expression is required for dependency type {dependency.DependencyType}", nameof(dependency.Expression));
            }

            // Validate that lookup key is provided for lookup dependencies
            if (dependency.DependencyType == "lookup" && string.IsNullOrEmpty(dependency.LookupKey))
            {
                throw new ArgumentException("Lookup key is required for lookup dependencies", nameof(dependency.LookupKey));
            }

            // Check for circular dependencies
            if (await HasCircularDependencyAsync(dependency))
            {
                throw new InvalidOperationException("Creating this dependency would result in a circular dependency");
            }
        }

        /// <summary>
        /// Process a single dependency.
        /// </summary>
        private async Task ProcessDependencyAsync(FormDependency dependency, Dictionary<string, JsonDocument> formDataCache)
        {
            try
            {
                // Skip inactive dependencies
                if (!dependency.IsActive)
                {
                    return;
                }

                // Ensure we have source form data
                if (!formDataCache.TryGetValue(dependency.SourceFormId, out var sourceFormData))
                {
                    // Try to get the form data from the repository
                    var sourceSubmission = await _formRepository.GetSubmissionByIdAsync(dependency.SourceFormId);
                    if (sourceSubmission == null)
                    {
                        _logger.LogWarning("Source form data not found for form {FormId}", dependency.SourceFormId);
                        return;
                    }
                    sourceFormData = sourceSubmission.Data;
                    formDataCache[dependency.SourceFormId] = sourceFormData;
                }

                // Ensure we have target form data
                if (!formDataCache.TryGetValue(dependency.TargetFormId, out var targetFormData))
                {
                    // Try to get the form data from the repository
                    var targetSubmission = await _formRepository.GetSubmissionByIdAsync(dependency.TargetFormId);
                    if (targetSubmission == null)
                    {
                        _logger.LogWarning("Target form data not found for form {FormId}", dependency.TargetFormId);
                        return;
                    }
                    targetFormData = targetSubmission.Data;
                    formDataCache[dependency.TargetFormId] = targetFormData;
                }

                // Process the dependency based on its type
                switch (dependency.DependencyType)
                {
                    case "value":
                        ProcessValueDependency(dependency, sourceFormData, ref targetFormData);
                        break;
                    case "lookup":
                        await ProcessLookupDependencyAsync(dependency, sourceFormData, ref targetFormData);
                        break;
                    case "calculation":
                        ProcessCalculationDependency(dependency, formDataCache, ref targetFormData);
                        break;
                    case "visibility":
                        // Visibility is primarily for the frontend, but we can still evaluate it
                        ProcessVisibilityDependency(dependency, sourceFormData, ref targetFormData);
                        break;
                    case "validation":
                        // Validation is handled separately
                        break;
                    default:
                        _logger.LogWarning("Unknown dependency type: {DependencyType}", dependency.DependencyType);
                        break;
                }

                // Update the form data cache with the modified target form data
                formDataCache[dependency.TargetFormId] = targetFormData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing dependency {DependencyId}", dependency.Id);
                throw;
            }
        }

        /// <summary>
        /// Process a value dependency (copy value from source to target).
        /// </summary>
        private void ProcessValueDependency(FormDependency dependency, JsonDocument sourceFormData, ref JsonDocument targetFormData)
        {
            try
            {
                // Get the source field value
                if (!TryGetFieldValue(sourceFormData, dependency.SourceFieldId, out var sourceValue))
                {
                    _logger.LogWarning("Source field value not found for field {FieldId} in form {FormId}",
                        dependency.SourceFieldId, dependency.SourceFormId);
                    return;
                }

                // Set the target field value
                targetFormData = SetFieldValue(targetFormData, dependency.TargetFieldId, sourceValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing value dependency {DependencyId}", dependency.Id);
                throw;
            }
        }

        /// <summary>
        /// Process a lookup dependency (lookup value based on source value).
        /// </summary>
        private async Task ProcessLookupDependencyAsync(FormDependency dependency, JsonDocument sourceFormData, ref JsonDocument targetFormData)
        {
            try
            {
                // Get the source field value
                if (!TryGetFieldValue(sourceFormData, dependency.SourceFieldId, out var sourceValue))
                {
                    _logger.LogWarning("Source field value not found for field {FieldId} in form {FormId}",
                        dependency.SourceFieldId, dependency.SourceFormId);
                    return;
                }

                // TODO: Implement a lookup service
                // For now, we'll just use the source value directly
                var lookupValue = sourceValue;

                // Set the target field value
                targetFormData = SetFieldValue(targetFormData, dependency.TargetFieldId, lookupValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing lookup dependency {DependencyId}", dependency.Id);
                throw;
            }
        }

        /// <summary>
        /// Process a calculation dependency (calculate value based on expression).
        /// </summary>
        private void ProcessCalculationDependency(FormDependency dependency, Dictionary<string, JsonDocument> formDataCache, ref JsonDocument targetFormData)
        {
            try
            {
                // Skip if no expression
                if (string.IsNullOrEmpty(dependency.Expression))
                {
                    _logger.LogWarning("No expression provided for calculation dependency {DependencyId}", dependency.Id);
                    return;
                }

                // Create a JS engine for evaluating the expression
                var engine = new Engine();

                // Add all form data to the engine context
                foreach (var formEntry in formDataCache)
                {
                    var formObj = JsonSerializer.Deserialize<Dictionary<string, object>>(formEntry.Value.RootElement.GetRawText());
                    engine.SetValue(formEntry.Key, formObj);
                }

                // Evaluate the expression
                var result = engine.Evaluate(dependency.Expression);
                var value = result.ToObject();

                // Set the target field value
                targetFormData = SetFieldValue(targetFormData, dependency.TargetFieldId, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing calculation dependency {DependencyId}: {Expression}", 
                    dependency.Id, dependency.Expression);
                throw;
            }
        }

        /// <summary>
        /// Process a visibility dependency (show/hide target based on source value).
        /// </summary>
        private void ProcessVisibilityDependency(FormDependency dependency, JsonDocument sourceFormData, ref JsonDocument targetFormData)
        {
            try
            {
                // Skip if no expression
                if (string.IsNullOrEmpty(dependency.Expression))
                {
                    _logger.LogWarning("No expression provided for visibility dependency {DependencyId}", dependency.Id);
                    return;
                }

                // Get the source field value
                if (!TryGetFieldValue(sourceFormData, dependency.SourceFieldId, out var sourceValue))
                {
                    _logger.LogWarning("Source field value not found for field {FieldId} in form {FormId}",
                        dependency.SourceFieldId, dependency.SourceFormId);
                    return;
                }

                // Create a JS engine for evaluating the expression
                var engine = new Engine();
                engine.SetValue("value", sourceValue);

                // Evaluate the expression
                var result = engine.Evaluate(dependency.Expression);
                var isVisible = result.AsBoolean();

                // TODO: Apply visibility logic to the field
                // This is primarily a frontend concern, but we could potentially
                // add a special property to the target field to indicate visibility
                
                // For now, we'll just log the result
                _logger.LogDebug("Field {FieldId} in form {FormId} visibility set to {Visibility}",
                    dependency.TargetFieldId, dependency.TargetFormId, isVisible);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing visibility dependency {DependencyId}: {Expression}", 
                    dependency.Id, dependency.Expression);
                throw;
            }
        }

        /// <summary>
        /// Validate a form against a validation dependency rule.
        /// </summary>
        private async Task<(bool IsValid, string ErrorMessage)> ValidateDependencyRuleAsync(
            FormDependency dependency, Dictionary<string, JsonDocument> formDataCache)
        {
            try
            {
                // Skip if no expression
                if (string.IsNullOrEmpty(dependency.Expression))
                {
                    return (true, null);
                }

                // Ensure we have source form data
                if (!formDataCache.TryGetValue(dependency.SourceFormId, out var sourceFormData))
                {
                    // Try to get the form data from the repository
                    var sourceSubmission = await _formRepository.GetSubmissionByIdAsync(dependency.SourceFormId);
                    if (sourceSubmission == null)
                    {
                        _logger.LogWarning("Source form data not found for form {FormId}", dependency.SourceFormId);
                        return (false, $"Missing source form data: {dependency.SourceFormId}");
                    }
                    sourceFormData = sourceSubmission.Data;
                    formDataCache[dependency.SourceFormId] = sourceFormData;
                }

                // Ensure we have target form data
                if (!formDataCache.TryGetValue(dependency.TargetFormId, out var targetFormData))
                {
                    // Try to get the form data from the repository
                    var targetSubmission = await _formRepository.GetSubmissionByIdAsync(dependency.TargetFormId);
                    if (targetSubmission == null)
                    {
                        _logger.LogWarning("Target form data not found for form {FormId}", dependency.TargetFormId);
                        return (false, $"Missing target form data: {dependency.TargetFormId}");
                    }
                    targetFormData = targetSubmission.Data;
                    formDataCache[dependency.TargetFormId] = targetFormData;
                }

                // Create a JS engine for evaluating the expression
                var engine = new Engine();

                // Add all form data to the engine context
                foreach (var formEntry in formDataCache)
                {
                    var formObj = JsonSerializer.Deserialize<Dictionary<string, object>>(formEntry.Value.RootElement.GetRawText());
                    engine.SetValue(formEntry.Key, formObj);
                }

                // Evaluate the expression
                var result = engine.Evaluate(dependency.Expression);
                var isValid = result.AsBoolean();

                return (isValid, isValid ? null : dependency.Description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating dependency {DependencyId}: {Expression}", 
                    dependency.Id, dependency.Expression);
                return (false, $"Error validating: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if a dependency would create a circular reference.
        /// </summary>
        private async Task<bool> HasCircularDependencyAsync(FormDependency newDependency)
        {
            // Get all existing dependencies
            var allDependencies = (await _formRepository.GetDependenciesAsync(1, 1000)).Dependencies.ToList();
            
            // Add the new dependency
            allDependencies.Add(newDependency);
            
            // Build a directed graph of dependencies
            var graph = new Dictionary<string, HashSet<string>>();
            foreach (var dep in allDependencies)
            {
                var sourceKey = $"{dep.SourceFormId}.{dep.SourceFieldId}";
                var targetKey = $"{dep.TargetFormId}.{dep.TargetFieldId}";
                
                if (!graph.ContainsKey(sourceKey))
                {
                    graph[sourceKey] = new HashSet<string>();
                }
                
                graph[sourceKey].Add(targetKey);
            }
            
            // Check for cycles in the graph
            var visited = new HashSet<string>();
            var recursionStack = new HashSet<string>();
            
            foreach (var node in graph.Keys)
            {
                if (HasCycleDFS(graph, node, visited, recursionStack))
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Depth-first search to check for cycles in a graph.
        /// </summary>
        private bool HasCycleDFS(Dictionary<string, HashSet<string>> graph, string node, 
            HashSet<string> visited, HashSet<string> recursionStack)
        {
            // If node is not visited yet, mark it as visited and add to recursion stack
            if (!visited.Contains(node))
            {
                visited.Add(node);
                recursionStack.Add(node);
                
                // Visit all adjacent nodes
                if (graph.TryGetValue(node, out var neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        // If neighbor is in recursion stack, there's a cycle
                        if (recursionStack.Contains(neighbor))
                        {
                            return true;
                        }
                        
                        // If neighbor is not visited and has a cycle, return true
                        if (!visited.Contains(neighbor) && HasCycleDFS(graph, neighbor, visited, recursionStack))
                        {
                            return true;
                        }
                    }
                }
            }
            
            // Remove from recursion stack
            recursionStack.Remove(node);
            return false;
        }

        /// <summary>
        /// Check if a form has a field with the given ID.
        /// </summary>
        private bool FormHasField(FormDefinition form, string fieldId)
        {
            return form.Sections.SelectMany(s => s.Fields).Any(f => f.Id == fieldId);
        }

        /// <summary>
        /// Try to get a field value from a JSON document.
        /// </summary>
        private bool TryGetFieldValue(JsonDocument doc, string fieldId, out object value)
        {
            try
            {
                if (doc.RootElement.TryGetProperty(fieldId, out var property))
                {
                    value = GetValueFromJsonElement(property);
                    return true;
                }
                
                value = null;
                return false;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Get a value from a JSON element.
        /// </summary>
        private object GetValueFromJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt32(out var intValue))
                    {
                        return intValue;
                    }
                    if (element.TryGetInt64(out var longValue))
                    {
                        return longValue;
                    }
                    if (element.TryGetDouble(out var doubleValue))
                    {
                        return doubleValue;
                    }
                    return 0;
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    return element.GetRawText();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Set a field value in a JSON document.
        /// </summary>
        private JsonDocument SetFieldValue(JsonDocument doc, string fieldId, object value)
        {
            try
            {
                // Deserialize the document to a dictionary
                var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(doc.RootElement.GetRawText());
                
                // Set the field value
                dictionary[fieldId] = value;
                
                // Serialize back to JSON
                var json = JsonSerializer.Serialize(dictionary);
                return JsonDocument.Parse(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting field value for field {FieldId}", fieldId);
                return doc;
            }
        }
    }
}
