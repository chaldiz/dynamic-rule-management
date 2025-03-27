using System.Collections.Generic;

namespace DynamicValidation.Application.Models
{
    public class ValidationErrorDto
    {
        public string FieldName { get; set; }
        public string ErrorMessage { get; set; }
        public string RuleName { get; set; }
    }
}