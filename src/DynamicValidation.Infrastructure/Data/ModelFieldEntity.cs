using System.Text.Json;
using DynamicValidation.Domain.Entities;
using DynamicValidation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DynamicValidation.Infrastructure.Data
{
    public class ModelFieldEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DataType { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public int? MaxLength { get; set; }
        public string DefaultValue { get; set; }
        public int DisplayOrder { get; set; }
        
        public int ModelId { get; set; }
        public virtual DynamicModelEntity Model { get; set; }
        
        public int? NestedModelId { get; set; }
        public virtual DynamicModelEntity NestedModel { get; set; }
        
        public virtual ICollection<ValidationRuleEntity> ValidationRules { get; set; }
    }
}