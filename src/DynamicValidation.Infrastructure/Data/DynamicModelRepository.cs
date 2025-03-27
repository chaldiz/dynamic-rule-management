using System.Text.Json;
using DynamicValidation.Domain.Entities;
using DynamicValidation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DynamicValidation.Infrastructure.Data
{
    /// <summary>
    /// Repository implementation
    /// </summary>
    public class DynamicModelRepository : IDynamicModelRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public DynamicModelRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DynamicModel> GetByIdAsync(int id)
        {
            var modelEntity = await _dbContext.DynamicModels
                .Include(m => m.Fields)
                    .ThenInclude(f => f.ValidationRules)
                .Include(m => m.ValidationRules)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

            if (modelEntity == null)
                return null;

            return MapEntityToDomainModel(modelEntity);
        }

        public async Task<DynamicModel> GetByNameAsync(string name)
        {
            var modelEntity = await _dbContext.DynamicModels
                .Include(m => m.Fields)
                    .ThenInclude(f => f.ValidationRules)
                .Include(m => m.ValidationRules)
                .FirstOrDefaultAsync(m => m.Name == name && m.IsActive);

            if (modelEntity == null)
                return null;

            return MapEntityToDomainModel(modelEntity);
        }

        public async Task<List<DynamicModel>> GetAllAsync()
        {
            var modelEntities = await _dbContext.DynamicModels
                .Where(m => m.IsActive)
                .Include(m => m.Fields)
                    .ThenInclude(f => f.ValidationRules)
                .Include(m => m.ValidationRules)
                .ToListAsync();

            return modelEntities.Select(MapEntityToDomainModel).ToList();
        }

        public async Task<DynamicModel> AddAsync(DynamicModel model)
        {
            var modelEntity = new DynamicModelEntity
            {
                Name = model.Name,
                Description = model.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Fields = new List<ModelFieldEntity>(),
                ValidationRules = new List<ValidationRuleEntity>()
            };

            // Add model validation rules
            foreach (var rule in model.ValidationRules)
            {
                modelEntity.ValidationRules.Add(new ValidationRuleEntity
                {
                    RuleName = rule.RuleName,
                    ErrorMessage = rule.ErrorMessage,
                    Parameters = rule.Parameters.Count > 0 ? JsonSerializer.Serialize(rule.Parameters) : null,
                    IsModelLevel = true
                });
            }

            // Add fields and their validation rules
            foreach (var field in model.Fields)
            {
                var fieldEntity = new ModelFieldEntity
                {
                    Name = field.Name,
                    DataType = field.DataType,
                    Description = field.Description,
                    IsRequired = field.IsRequired,
                    MaxLength = field.MaxLength,
                    DefaultValue = field.DefaultValue,
                    DisplayOrder = field.DisplayOrder,
                    NestedModelId = field.NestedModelId,
                    ValidationRules = new List<ValidationRuleEntity>()
                };

                // Add field validation rules
                foreach (var rule in field.ValidationRules)
                {
                    fieldEntity.ValidationRules.Add(new ValidationRuleEntity
                    {
                        RuleName = rule.RuleName,
                        ErrorMessage = rule.ErrorMessage,
                        Parameters = rule.Parameters.Count > 0 ? JsonSerializer.Serialize(rule.Parameters) : null,
                        IsModelLevel = false
                    });
                }

                modelEntity.Fields.Add(fieldEntity);
            }

            _dbContext.DynamicModels.Add(modelEntity);
            await _dbContext.SaveChangesAsync();

            // Reload the entity with all related data
            return await GetByIdAsync(modelEntity.Id);
        }

        public async Task UpdateAsync(DynamicModel model)
        {
            var modelEntity = await _dbContext.DynamicModels
                .Include(m => m.Fields)
                    .ThenInclude(f => f.ValidationRules)
                .Include(m => m.ValidationRules)
                .FirstOrDefaultAsync(m => m.Id == model.Id);

            if (modelEntity == null)
                throw new Exception($"Model with ID {model.Id} not found");

            // Update basic properties
            modelEntity.Name = model.Name;
            modelEntity.Description = model.Description;
            modelEntity.UpdatedAt = DateTime.UtcNow;

            // Remove existing validation rules and fields
            _dbContext.ValidationRules.RemoveRange(modelEntity.ValidationRules);
            _dbContext.ValidationRules.RemoveRange(modelEntity.Fields.SelectMany(f => f.ValidationRules));
            _dbContext.ModelFields.RemoveRange(modelEntity.Fields);

            // Add updated model validation rules
            modelEntity.ValidationRules = new List<ValidationRuleEntity>();
            foreach (var rule in model.ValidationRules)
            {
                modelEntity.ValidationRules.Add(new ValidationRuleEntity
                {
                    RuleName = rule.RuleName,
                    ErrorMessage = rule.ErrorMessage,
                    Parameters = rule.Parameters.Count > 0 ? JsonSerializer.Serialize(rule.Parameters) : null,
                    IsModelLevel = true,
                    ModelId = modelEntity.Id
                });
            }

            // Add updated fields and their validation rules
            modelEntity.Fields = new List<ModelFieldEntity>();
            foreach (var field in model.Fields)
            {
                var fieldEntity = new ModelFieldEntity
                {
                    Name = field.Name,
                    DataType = field.DataType,
                    Description = field.Description,
                    IsRequired = field.IsRequired,
                    MaxLength = field.MaxLength,
                    DefaultValue = field.DefaultValue,
                    DisplayOrder = field.DisplayOrder,
                    NestedModelId = field.NestedModelId,
                    ModelId = modelEntity.Id,
                    ValidationRules = new List<ValidationRuleEntity>()
                };

                // Add field validation rules
                foreach (var rule in field.ValidationRules)
                {
                    fieldEntity.ValidationRules.Add(new ValidationRuleEntity
                    {
                        RuleName = rule.RuleName,
                        ErrorMessage = rule.ErrorMessage,
                        Parameters = rule.Parameters.Count > 0 ? JsonSerializer.Serialize(rule.Parameters) : null,
                        IsModelLevel = false
                    });
                }

                modelEntity.Fields.Add(fieldEntity);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var modelEntity = await _dbContext.DynamicModels.FindAsync(id);
            if (modelEntity == null)
                throw new Exception($"Model with ID {id} not found");

            // Soft delete
            modelEntity.IsActive = false;
            modelEntity.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        private DynamicModel MapEntityToDomainModel(DynamicModelEntity entity)
        {
            // Create fields
            var fields = new List<ModelField>();
            foreach (var fieldEntity in entity.Fields)
            {
                // Create field validation rules
                var fieldRules = new List<ValidationRule>();
                foreach (var ruleEntity in fieldEntity.ValidationRules)
                {
                    var parameters = string.IsNullOrEmpty(ruleEntity.Parameters)
                        ? new Dictionary<string, object>()
                        : JsonSerializer.Deserialize<Dictionary<string, object>>(ruleEntity.Parameters);

                    fieldRules.Add(ValidationRule.Create(
                        ruleEntity.Id,
                        ruleEntity.RuleName,
                        ruleEntity.ErrorMessage,
                        false,
                        parameters
                    ));
                }

                // Create field
                fields.Add(ModelField.Create(
                    fieldEntity.Id,
                    fieldEntity.Name,
                    fieldEntity.DataType,
                    fieldEntity.Description,
                    fieldEntity.IsRequired,
                    fieldEntity.MaxLength,
                    fieldEntity.DefaultValue,
                    fieldEntity.DisplayOrder,
                    fieldEntity.NestedModelId,
                    fieldRules
                ));
            }

            // Create model validation rules
            var modelRules = new List<ValidationRule>();
            foreach (var ruleEntity in entity.ValidationRules)
            {
                var parameters = string.IsNullOrEmpty(ruleEntity.Parameters)
                    ? new Dictionary<string, object>()
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(ruleEntity.Parameters);

                modelRules.Add(ValidationRule.Create(
                    ruleEntity.Id,
                    ruleEntity.RuleName,
                    ruleEntity.ErrorMessage,
                    true,
                    parameters
                ));
            }

            // Create model
            return DynamicModel.Create(
                entity.Id,
                entity.Name,
                entity.Description,
                entity.IsActive,
                entity.CreatedAt,
                entity.UpdatedAt,
                fields,
                modelRules
            );
        }
    }
}