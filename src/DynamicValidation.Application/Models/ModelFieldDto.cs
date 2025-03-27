using System.Collections.Generic;

namespace DynamicValidation.Application.Models
{
    public class ModelFieldDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string DataType { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public int? MaxLength { get; set; }
        public string DefaultValue { get; set; }
        public int DisplayOrder { get; set; }
        public int? NestedModelId { get; set; }
        public List<ValidationRuleDto> ValidationRules { get; set; } = new List<ValidationRuleDto>();
    }
}