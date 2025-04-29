using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PharmaForms.Core.Entities
{
    /// <summary>
    /// Represents a form submission with the data provided by users.
    /// </summary>
    public class FormSubmission
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string FormId { get; set; }
        
        [JsonIgnore]
        public FormDefinition Form { get; set; }
        
        public JsonDocument Data { get; set; }
        
        public string Status { get; set; } = "draft"; // draft, submitted, approved, rejected
        
        public string CreatedBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string UpdatedBy { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public string ApprovedBy { get; set; }
        
        public DateTime? ApprovedAt { get; set; }
        
        public string RejectedBy { get; set; }
        
        public DateTime? RejectedAt { get; set; }
        
        public string Comments { get; set; }
        
        /// <summary>
        /// Get the data as a strongly-typed object by deserializing the JSON document.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <returns>Deserialized object</returns>
        public T GetData<T>()
        {
            if (Data == null)
            {
                return default;
            }
            
            return JsonSerializer.Deserialize<T>(Data.RootElement.GetRawText());
        }
        
        /// <summary>
        /// Set the data by serializing an object to JSON.
        /// </summary>
        /// <typeparam name="T">The type of the data object</typeparam>
        /// <param name="data">The data object to serialize</param>
        public void SetData<T>(T data)
        {
            if (data == null)
            {
                Data = null;
                return;
            }
            
            var jsonString = JsonSerializer.Serialize(data);
            Data = JsonDocument.Parse(jsonString);
        }
    }
}
