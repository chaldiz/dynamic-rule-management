using System.Collections.Generic;

namespace DynamicValidation.Application.Models
{
    public class ValidationRuleDto
    {
        public int? Id { get; set; }
        public string RuleName { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}