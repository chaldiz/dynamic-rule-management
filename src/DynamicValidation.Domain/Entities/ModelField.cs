/// <summary>
/// Model alanı tanımı
/// </summary>
/// 

namespace DynamicValidation.Domain.Entities;
public class ModelField
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string DataType { get; private set; }
    public string Description { get; private set; }
    public bool IsRequired { get; private set; }
    public int? MaxLength { get; private set; }
    public string DefaultValue { get; private set; }
    public int DisplayOrder { get; private set; }
    public int? NestedModelId { get; private set; }

    private readonly List<ValidationRule> _validationRules = new List<ValidationRule>();
    public IReadOnlyCollection<ValidationRule> ValidationRules => _validationRules.AsReadOnly();

    // Constructor for new field
    public ModelField(string name, string dataType, string description, bool isRequired,
        int? maxLength, string defaultValue, int displayOrder, int? nestedModelId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Field name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(dataType))
            throw new ArgumentException("Data type cannot be empty", nameof(dataType));

        Name = name;
        DataType = dataType;
        Description = description;
        IsRequired = isRequired;
        MaxLength = maxLength;
        DefaultValue = defaultValue;
        DisplayOrder = displayOrder;
        NestedModelId = nestedModelId;
    }

    // Constructor for loading from db
    private ModelField(int id, string name, string dataType, string description, bool isRequired,
        int? maxLength, string defaultValue, int displayOrder, int? nestedModelId)
    {
        Id = id;
        Name = name;
        DataType = dataType;
        Description = description;
        IsRequired = isRequired;
        MaxLength = maxLength;
        DefaultValue = defaultValue;
        DisplayOrder = displayOrder;
        NestedModelId = nestedModelId;
    }

    public void Update(string name, string dataType, string description, bool isRequired,
        int? maxLength, string defaultValue, int displayOrder, int? nestedModelId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Field name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(dataType))
            throw new ArgumentException("Data type cannot be empty", nameof(dataType));

        Name = name;
        DataType = dataType;
        Description = description;
        IsRequired = isRequired;
        MaxLength = maxLength;
        DefaultValue = defaultValue;
        DisplayOrder = displayOrder;
        NestedModelId = nestedModelId;
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
    public static ModelField Create(int id, string name, string dataType, string description, bool isRequired,
        int? maxLength, string defaultValue, int displayOrder, int? nestedModelId, IEnumerable<ValidationRule> rules)
    {
        var field = new ModelField(id, name, dataType, description, isRequired, maxLength, defaultValue, displayOrder, nestedModelId);

        foreach (var rule in rules)
            field._validationRules.Add(rule);

        return field;
    }
}