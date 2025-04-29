using System;

namespace PharmaForms.Core.Entities
{
    /// <summary>
    /// Represents a dependency between fields across different forms.
    /// This is used to enforce relationships and validations between forms.
    /// </summary>
    public class FormDependency
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// The ID of the source form in the dependency.
        /// </summary>
        public string SourceFormId { get; set; }
        
        /// <summary>
        /// The ID of the source field in the dependency.
        /// </summary>
        public string SourceFieldId { get; set; }
        
        /// <summary>
        /// The ID of the target form in the dependency.
        /// </summary>
        public string TargetFormId { get; set; }
        
        /// <summary>
        /// The ID of the target field in the dependency.
        /// </summary>
        public string TargetFieldId { get; set; }
        
        /// <summary>
        /// The type of dependency.
        /// </summary>
        public string DependencyType { get; set; } // value, lookup, visibility, validation, calculation
        
        /// <summary>
        /// JavaScript expression to evaluate for the dependency.
        /// For calculation dependencies, this contains the formula.
        /// For visibility dependencies, this contains the condition.
        /// For validation dependencies, this contains the validation rule.
        /// </summary>
        public string Expression { get; set; }
        
        /// <summary>
        /// For lookup dependencies, this specifies the lookup key to use.
        /// </summary>
        public string LookupKey { get; set; }
        
        /// <summary>
        /// Human-readable description of the dependency.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// The order in which this dependency should be evaluated relative to other dependencies.
        /// </summary>
        public int ExecutionOrder { get; set; }
        
        /// <summary>
        /// When the dependency was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Who created the dependency.
        /// </summary>
        public string CreatedBy { get; set; }
        
        /// <summary>
        /// When the dependency was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// Who last updated the dependency.
        /// </summary>
        public string UpdatedBy { get; set; }
        
        /// <summary>
        /// Whether the dependency is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
