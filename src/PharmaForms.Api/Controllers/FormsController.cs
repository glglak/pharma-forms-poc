using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PharmaForms.Core.Entities;
using PharmaForms.Core.Interfaces;

namespace PharmaForms.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FormsController : ControllerBase
    {
        private readonly IFormRepository _formRepository;
        private readonly IFormDependencyService _formDependencyService;
        private readonly ILogger<FormsController> _logger;

        public FormsController(
            IFormRepository formRepository,
            IFormDependencyService formDependencyService,
            ILogger<FormsController> logger)
        {
            _formRepository = formRepository ?? throw new ArgumentNullException(nameof(formRepository));
            _formDependencyService = formDependencyService ?? throw new ArgumentNullException(nameof(formDependencyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all forms with pagination.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<FormDefinition>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetForms([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (forms, totalCount) = await _formRepository.GetFormsAsync(page, pageSize);
                
                var response = new PaginatedResponse<FormDefinition>
                {
                    Items = forms.ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting forms");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving forms");
            }
        }

        /// <summary>
        /// Get a form by ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FormDefinition), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetForm(string id)
        {
            try
            {
                var form = await _formRepository.GetFormByIdAsync(id);
                
                if (form == null)
                {
                    return NotFound();
                }
                
                return Ok(form);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting form {FormId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the form");
            }
        }

        /// <summary>
        /// Create a new form.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(FormDefinition), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateForm([FromBody] FormDefinition form)
        {
            try
            {
                if (form == null)
                {
                    return BadRequest("Form data is required");
                }
                
                // Assign ID if not provided
                if (string.IsNullOrEmpty(form.Id))
                {
                    form.Id = Guid.NewGuid().ToString();
                }
                
                // Set creation metadata
                form.CreatedAt = DateTime.UtcNow;
                form.CreatedBy = User.Identity.Name;
                
                var createdForm = await _formRepository.CreateFormAsync(form);
                
                return CreatedAtAction(nameof(GetForm), new { id = createdForm.Id }, createdForm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating form");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the form");
            }
        }

        /// <summary>
        /// Update an existing form.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(FormDefinition), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateForm(string id, [FromBody] FormDefinition form)
        {
            try
            {
                if (form == null)
                {
                    return BadRequest("Form data is required");
                }
                
                if (id != form.Id)
                {
                    return BadRequest("Form ID mismatch");
                }
                
                var existingForm = await _formRepository.GetFormByIdAsync(id);
                
                if (existingForm == null)
                {
                    return NotFound();
                }
                
                // Set update metadata
                form.UpdatedAt = DateTime.UtcNow;
                form.UpdatedBy = User.Identity.Name;
                form.CreatedAt = existingForm.CreatedAt;
                form.CreatedBy = existingForm.CreatedBy;
                
                var updatedForm = await _formRepository.UpdateFormAsync(form);
                
                return Ok(updatedForm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating form {FormId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the form");
            }
        }

        /// <summary>
        /// Delete a form.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteForm(string id)
        {
            try
            {
                var result = await _formRepository.DeleteFormAsync(id);
                
                if (!result)
                {
                    return NotFound();
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting form {FormId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the form");
            }
        }

        /// <summary>
        /// Search for forms.
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(PaginatedResponse<FormDefinition>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchForms([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (forms, totalCount) = await _formRepository.SearchFormsAsync(query, page, pageSize);
                
                var response = new PaginatedResponse<FormDefinition>
                {
                    Items = forms.ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching forms with query {Query}", query);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching forms");
            }
        }

        /// <summary>
        /// Get form submissions.
        /// </summary>
        [HttpGet("{formId}/submissions")]
        [ProducesResponseType(typeof(PaginatedResponse<FormSubmission>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFormSubmissions(string formId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Check if form exists
                var form = await _formRepository.GetFormByIdAsync(formId);
                
                if (form == null)
                {
                    return NotFound("Form not found");
                }
                
                var (submissions, totalCount) = await _formRepository.GetFormSubmissionsAsync(formId, page, pageSize);
                
                var response = new PaginatedResponse<FormSubmission>
                {
                    Items = submissions.ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting submissions for form {FormId}", formId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving form submissions");
            }
        }

        /// <summary>
        /// Get a form submission.
        /// </summary>
        [HttpGet("{formId}/submissions/{id}")]
        [ProducesResponseType(typeof(FormSubmission), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFormSubmission(string formId, string id)
        {
            try
            {
                var submission = await _formRepository.GetSubmissionByIdAsync(id);
                
                if (submission == null || submission.FormId != formId)
                {
                    return NotFound();
                }
                
                return Ok(submission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting submission {SubmissionId} for form {FormId}", id, formId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the form submission");
            }
        }

        /// <summary>
        /// Submit form data.
        /// </summary>
        [HttpPost("{formId}/submissions")]
        [ProducesResponseType(typeof(FormSubmission), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubmitForm(string formId, [FromBody] JsonDocument formData)
        {
            try
            {
                // Check if form exists
                var form = await _formRepository.GetFormByIdAsync(formId);
                
                if (form == null)
                {
                    return NotFound("Form not found");
                }
                
                // Process dependencies
                var processedData = await _formDependencyService.ProcessDependenciesAsync(formId, formData);
                
                // Validate with dependencies
                var (isValid, errors) = await _formDependencyService.ValidateWithDependenciesAsync(formId, processedData);
                
                if (!isValid)
                {
                    return BadRequest(new { Errors = errors });
                }
                
                // Create submission
                var submission = new FormSubmission
                {
                    Id = Guid.NewGuid().ToString(),
                    FormId = formId,
                    Data = processedData,
                    Status = "submitted",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.Identity.Name
                };
                
                var createdSubmission = await _formRepository.CreateSubmissionAsync(submission);
                
                return CreatedAtAction(
                    nameof(GetFormSubmission),
                    new { formId = formId, id = createdSubmission.Id },
                    createdSubmission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting form {FormId}", formId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while submitting the form");
            }
        }

        /// <summary>
        /// Update a form submission status.
        /// </summary>
        [HttpPatch("{formId}/submissions/{id}/status")]
        [ProducesResponseType(typeof(FormSubmission), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSubmissionStatus(
            string formId,
            string id,
            [FromBody] UpdateStatusRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.Status))
                {
                    return BadRequest("Status is required");
                }
                
                var submission = await _formRepository.GetSubmissionByIdAsync(id);
                
                if (submission == null || submission.FormId != formId)
                {
                    return NotFound();
                }
                
                // Update status and metadata
                submission.Status = request.Status.ToLower();
                submission.UpdatedAt = DateTime.UtcNow;
                submission.UpdatedBy = User.Identity.Name;
                
                // Update approval/rejection metadata
                if (request.Status.ToLower() == "approved")
                {
                    submission.ApprovedAt = DateTime.UtcNow;
                    submission.ApprovedBy = User.Identity.Name;
                }
                else if (request.Status.ToLower() == "rejected")
                {
                    submission.RejectedAt = DateTime.UtcNow;
                    submission.RejectedBy = User.Identity.Name;
                    submission.Comments = request.Comments;
                }
                
                var updatedSubmission = await _formRepository.UpdateSubmissionAsync(submission);
                
                return Ok(updatedSubmission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating submission status {SubmissionId} for form {FormId}", id, formId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the submission status");
            }
        }

        /// <summary>
        /// Get form dependencies.
        /// </summary>
        [HttpGet("{formId}/dependencies")]
        [ProducesResponseType(typeof(IEnumerable<FormDependency>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFormDependencies(string formId)
        {
            try
            {
                // Check if form exists
                var form = await _formRepository.GetFormByIdAsync(formId);
                
                if (form == null)
                {
                    return NotFound("Form not found");
                }
                
                var dependencies = await _formDependencyService.GetDependenciesForFormAsync(formId);
                
                return Ok(dependencies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dependencies for form {FormId}", formId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving form dependencies");
            }
        }

        /// <summary>
        /// Create a form dependency.
        /// </summary>
        [HttpPost("{formId}/dependencies")]
        [ProducesResponseType(typeof(FormDependency), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateFormDependency(string formId, [FromBody] FormDependency dependency)
        {
            try
            {
                if (dependency == null)
                {
                    return BadRequest("Dependency data is required");
                }
                
                // Check if form exists
                var form = await _formRepository.GetFormByIdAsync(formId);
                
                if (form == null)
                {
                    return NotFound("Form not found");
                }
                
                // Set form ID if not provided
                if (string.IsNullOrEmpty(dependency.SourceFormId))
                {
                    dependency.SourceFormId = formId;
                }
                else if (dependency.SourceFormId != formId && dependency.TargetFormId != formId)
                {
                    return BadRequest("Dependency must be related to the specified form");
                }
                
                // Assign ID if not provided
                if (string.IsNullOrEmpty(dependency.Id))
                {
                    dependency.Id = Guid.NewGuid().ToString();
                }
                
                // Set creation metadata
                dependency.CreatedBy = User.Identity.Name;
                
                var createdDependency = await _formDependencyService.CreateDependencyAsync(dependency);
                
                return CreatedAtAction(
                    nameof(GetFormDependencies),
                    new { formId = formId },
                    createdDependency);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dependency for form {FormId}", formId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the form dependency");
            }
        }
    }

    /// <summary>
    /// Paginated response model.
    /// </summary>
    public class PaginatedResponse<T>
    {
        /// <summary>
        /// The items on the current page.
        /// </summary>
        public List<T> Items { get; set; }
        
        /// <summary>
        /// The total number of items.
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// The current page number.
        /// </summary>
        public int Page { get; set; }
        
        /// <summary>
        /// The number of items per page.
        /// </summary>
        public int PageSize { get; set; }
        
        /// <summary>
        /// The total number of pages.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        
        /// <summary>
        /// Whether there is a previous page.
        /// </summary>
        public bool HasPreviousPage => Page > 1;
        
        /// <summary>
        /// Whether there is a next page.
        /// </summary>
        public bool HasNextPage => Page < TotalPages;
    }

    /// <summary>
    /// Request model for updating submission status.
    /// </summary>
    public class UpdateStatusRequest
    {
        /// <summary>
        /// The new status.
        /// </summary>
        public string Status { get; set; }
        
        /// <summary>
        /// Optional comments.
        /// </summary>
        public string Comments { get; set; }
    }
}
