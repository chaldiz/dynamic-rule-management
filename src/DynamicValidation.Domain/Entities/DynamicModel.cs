using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicValidation.Domain.Entities
{
    /// <summary>
    /// Dinamik model tanımı
    /// </summary>
    public class DynamicModel
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private readonly List<ModelField> _fields = new List<ModelField>();
        public IReadOnlyCollection<ModelField> Fields => _fields.AsReadOnly();

        private readonly List<ValidationRule> _validationRules = new List<ValidationRule>();
        public IReadOnlyCollection<ValidationRule> ValidationRules => _validationRules.AsReadOnly();

        // Constructor for new model
        public DynamicModel(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Model name cannot be empty", nameof(name));

            Name = name;
            Description = description;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // Constructor for loading from db
        private DynamicModel(int id, string name, string description, bool isActive, DateTime createdAt, DateTime updatedAt)
        {
            Id = id;
            Name = name;
            Description = description;
            IsActive = isActive;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public void Update(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Model name cannot be empty", nameof(name));

            Name = name;
            Description = description;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddField(ModelField field)
        {
            if (_fields.Any(f => f.Name == field.Name))
                throw new InvalidOperationException($"Field with name '{field.Name}' already exists");

            _fields.Add(field);
        }

        public void RemoveField(int fieldId)
        {
            var field = _fields.FirstOrDefault(f => f.Id == fieldId);
            if (field == null)
                throw new InvalidOperationException($"Field with id {fieldId} does not exist");

            _fields.Remove(field);
        }

        public void AddValidationRule(ValidationRule rule)
        {
            _validationRules.Add(rule);
        }

        public void RemoveValidationRule(int ruleId)
        {
            var rule = _validationRules.FirstOrDefault(r => r.Id == ruleId);
            if (rule == null)
                throw new InvalidOperationException($"Validation rule with id {ruleId} does not exist");

            _validationRules.Remove(rule);
        }

        // Factory method for easy creation from db
        public static DynamicModel Create(int id, string name, string description, bool isActive,
            DateTime createdAt, DateTime updatedAt, IEnumerable<ModelField> fields, IEnumerable<ValidationRule> rules)
        {
            var model = new DynamicModel(id, name, description, isActive, createdAt, updatedAt);

            foreach (var field in fields)
                model._fields.Add(field);

            foreach (var rule in rules)
                model._validationRules.Add(rule);

            return model;
        }
    }
}