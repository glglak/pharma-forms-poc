using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmaForms.Core.Entities;
using PharmaForms.Core.Interfaces;
using PharmaForms.Infrastructure.Data;

namespace PharmaForms.Infrastructure.Repositories
{
    /// <summary>
    /// SQL implementation of the form repository interface.
    /// </summary>
    public class SqlFormRepository : IFormRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<SqlFormRepository> _logger;

        public SqlFormRepository(
            ApplicationDbContext dbContext,
            ILogger<SqlFormRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Form Definitions

        public async Task<(IEnumerable<FormDefinition> Forms, int TotalCount)> GetFormsAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                // Ensure page and pageSize are valid
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 100);

                // Get total count
                var totalCount = await _dbContext.FormDefinitions.CountAsync();

                // Get paginated results
                var forms = await _dbContext.FormDefinitions
                    .OrderByDescending(f => f.UpdatedAt ?? f.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (forms, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting forms with page {Page} and pageSize {PageSize}", page, pageSize);
                throw;
            }
        }

        public async Task<FormDefinition> GetFormByIdAsync(string id)
        {
            try
            {
                return await _dbContext.FormDefinitions
                    .FirstOrDefaultAsync(f => f.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting form with ID {FormId}", id);
                throw;
            }
        }

        public async Task<FormDefinition> CreateFormAsync(FormDefinition form)
        {
            try
            {
                _dbContext.FormDefinitions.Add(form);
                await _dbContext.SaveChangesAsync();
                return form;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating form {FormName}", form.Name);
                throw;
            }
        }

        public async Task<FormDefinition> UpdateFormAsync(FormDefinition form)
        {
            try
            {
                form.UpdatedAt = DateTime.UtcNow;
                _dbContext.FormDefinitions.Update(form);
                await _dbContext.SaveChangesAsync();
                return form;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating form with ID {FormId}", form.Id);
                throw;
            }
        }

        public async Task<bool> DeleteFormAsync(string id)
        {
            try
            {
                var form = await _dbContext.FormDefinitions.FindAsync(id);
                if (form == null)
                {
                    return false;
                }

                _dbContext.FormDefinitions.Remove(form);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting form with ID {FormId}", id);
                throw;
            }
        }

        public async Task<(IEnumerable<FormDefinition> Forms, int TotalCount)> SearchFormsAsync(string searchTerm, int page = 1, int pageSize = 10)
        {
            try
            {
                // Ensure page and pageSize are valid
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 100);

                // Normalize search term
                searchTerm = searchTerm?.Trim().ToLower() ?? string.Empty;

                // Create the query
                var query = _dbContext.FormDefinitions.Where(f =>
                    f.Name.ToLower().Contains(searchTerm) ||
                    f.Description.ToLower().Contains(searchTerm));

                // Get total count
                var totalCount = await query.CountAsync();

                // Get paginated results
                var forms = await query
                    .OrderByDescending(f => f.UpdatedAt ?? f.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (forms, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching forms with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        #endregion

        #region Form Submissions

        public async Task<(IEnumerable<FormSubmission> Submissions, int TotalCount)> GetFormSubmissionsAsync(string formId, int page = 1, int pageSize = 10)
        {
            try
            {
                // Ensure page and pageSize are valid
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 100);

                // Get total count
                var totalCount = await _dbContext.FormSubmissions
                    .Where(s => s.FormId == formId)
                    .CountAsync();

                // Get paginated results
                var submissions = await _dbContext.FormSubmissions
                    .Where(s => s.FormId == formId)
                    .OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (submissions, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting submissions for form {FormId}", formId);
                throw;
            }
        }

        public async Task<FormSubmission> GetSubmissionByIdAsync(string id)
        {
            try
            {
                return await _dbContext.FormSubmissions
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting submission with ID {SubmissionId}", id);
                throw;
            }
        }

        public async Task<FormSubmission> CreateSubmissionAsync(FormSubmission submission)
        {
            try
            {
                _dbContext.FormSubmissions.Add(submission);
                await _dbContext.SaveChangesAsync();
                return submission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating submission for form {FormId}", submission.FormId);
                throw;
            }
        }

        public async Task<FormSubmission> UpdateSubmissionAsync(FormSubmission submission)
        {
            try
            {
                submission.UpdatedAt = DateTime.UtcNow;
                _dbContext.FormSubmissions.Update(submission);
                await _dbContext.SaveChangesAsync();
                return submission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating submission with ID {SubmissionId}", submission.Id);
                throw;
            }
        }

        public async Task<bool> DeleteSubmissionAsync(string id)
        {
            try
            {
                var submission = await _dbContext.FormSubmissions.FindAsync(id);
                if (submission == null)
                {
                    return false;
                }

                _dbContext.FormSubmissions.Remove(submission);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting submission with ID {SubmissionId}", id);
                throw;
            }
        }

        #endregion

        #region Form Dependencies

        public async Task<(IEnumerable<FormDependency> Dependencies, int TotalCount)> GetDependenciesAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                // Ensure page and pageSize are valid
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 100);

                // Get total count
                var totalCount = await _dbContext.FormDependencies.CountAsync();

                // Get paginated results
                var dependencies = await _dbContext.FormDependencies
                    .OrderBy(d => d.ExecutionOrder)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (dependencies, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dependencies with page {Page} and pageSize {PageSize}", page, pageSize);
                throw;
            }
        }

        public async Task<FormDependency> GetDependencyByIdAsync(string id)
        {
            try
            {
                return await _dbContext.FormDependencies
                    .FirstOrDefaultAsync(d => d.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dependency with ID {DependencyId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<FormDependency>> GetDependenciesForFormAsync(string formId)
        {
            try
            {
                return await _dbContext.FormDependencies
                    .Where(d => d.SourceFormId == formId || d.TargetFormId == formId)
                    .OrderBy(d => d.ExecutionOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dependencies for form {FormId}", formId);
                throw;
            }
        }

        public async Task<FormDependency> CreateDependencyAsync(FormDependency dependency)
        {
            try
            {
                _dbContext.FormDependencies.Add(dependency);
                await _dbContext.SaveChangesAsync();
                return dependency;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dependency between {SourceForm}.{SourceField} and {TargetForm}.{TargetField}",
                    dependency.SourceFormId, dependency.SourceFieldId, dependency.TargetFormId, dependency.TargetFieldId);
                throw;
            }
        }

        public async Task<FormDependency> UpdateDependencyAsync(FormDependency dependency)
        {
            try
            {
                dependency.UpdatedAt = DateTime.UtcNow;
                _dbContext.FormDependencies.Update(dependency);
                await _dbContext.SaveChangesAsync();
                return dependency;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dependency with ID {DependencyId}", dependency.Id);
                throw;
            }
        }

        public async Task<bool> DeleteDependencyAsync(string id)
        {
            try
            {
                var dependency = await _dbContext.FormDependencies.FindAsync(id);
                if (dependency == null)
                {
                    return false;
                }

                _dbContext.FormDependencies.Remove(dependency);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dependency with ID {DependencyId}", id);
                throw;
            }
        }

        #endregion
    }
}
