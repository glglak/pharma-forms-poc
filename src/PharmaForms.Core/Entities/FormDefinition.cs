using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PharmaForms.Core.Entities
{
    /// <summary>
    /// Represents a form definition in the system.
    /// Each form has a structure with sections and fields.
    /// </summary>
    public class FormDefinition
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public string Version { get; set; } = "1.0";
        
        public bool IsPublished { get; set; }
        
        public string Direction { get; set; } = "rtl"; // Default to RTL for Arabic
        
        public List<FormSection> Sections { get; set; } = new List<FormSection>();
        
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string CreatedBy { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public string UpdatedBy { get; set; }
    }

    /// <summary>
    /// Represents a section in a form containing fields.
    /// </summary>
    public class FormSection
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public bool IsCollapsible { get; set; }
        
        public bool IsCollapsed { get; set; }
        
        public int Order { get; set; }
        
        public List<FormField> Fields { get; set; } = new List<FormField>();
    }

    /// <summary>
    /// Represents a field in a form section.
    /// </summary>
    public class FormField
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string Label { get; set; }
        
        public string Type { get; set; } // text, number, date, select, checkbox, etc.
        
        public string Placeholder { get; set; }
        
        public string HelpText { get; set; }
        
        public bool IsRequired { get; set; }
        
        public bool IsReadOnly { get; set; }
        
        public bool IsHidden { get; set; }
        
        public string DefaultValue { get; set; }
        
        public int Order { get; set; }
        
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
        
        public List<ValidationRule> ValidationRules { get; set; } = new List<ValidationRule>();
        
        public List<FormFieldOption> Options { get; set; } = new List<FormFieldOption>(); // For select, radio, etc.
    }

    /// <summary>
    /// Represents a validation rule for a form field.
    /// </summary>
    public class ValidationRule
    {
        public string Type { get; set; } // required, regex, min, max, etc.
        
        public string Message { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object Value { get; set; } // The value to compare against
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string DependentFieldId { get; set; } // For conditional validation
    }

    /// <summary>
    /// Represents an option for select, radio, or checkbox fields.
    /// </summary>
    public class FormFieldOption
    {
        public string Value { get; set; }
        
        public string Label { get; set; }
        
        public bool IsDefault { get; set; }
    }
}
