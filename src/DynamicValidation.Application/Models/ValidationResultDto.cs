using System.Collections.Generic;

namespace DynamicValidation.Application.Models
{
    public class ValidationResultDto
    {
        public bool IsValid { get; set; }
        public string ModelName { get; set; }
        public List<ValidationErrorDto> Errors { get; set; } = new List<ValidationErrorDto>();
    }
}