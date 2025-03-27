using System.Collections.Generic;

namespace DynamicValidation.Application.Models
{
    // DTOs
    public class DynamicModelDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ModelFieldDto> Fields { get; set; } = new List<ModelFieldDto>();
        public List<ValidationRuleDto> ModelValidationRules { get; set; } = new List<ValidationRuleDto>();
    }
}