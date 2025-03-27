using System.Text.Json;
using DynamicValidation.Domain.Entities;
using DynamicValidation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DynamicValidation.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework entities
    /// </summary>
    public class DynamicModelEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public virtual ICollection<ModelFieldEntity> Fields { get; set; }
        public virtual ICollection<ValidationRuleEntity> ValidationRules { get; set; }
    }
}