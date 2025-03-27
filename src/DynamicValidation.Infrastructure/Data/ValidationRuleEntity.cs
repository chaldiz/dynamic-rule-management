using System.Text.Json;
using DynamicValidation.Domain.Entities;
using DynamicValidation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DynamicValidation.Infrastructure.Data
{
    public class ValidationRuleEntity
    {
        public int Id { get; set; }
        public string RuleName { get; set; }
        public string ErrorMessage { get; set; }
        public string Parameters { get; set; }
        public bool IsModelLevel { get; set; }
        
        public int? ModelId { get; set; }
        public virtual DynamicModelEntity Model { get; set; }
        
        public int? FieldId { get; set; }
        public virtual ModelFieldEntity Field { get; set; }
    }
}