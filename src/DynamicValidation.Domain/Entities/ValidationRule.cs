namespace DynamicValidation.Domain.Entities;

/// <summary>
/// Validasyon kuralı
/// </summary>
public class ValidationRule
{
    public int Id { get; private set; }
    public string RuleName { get; private set; }
    public string ErrorMessage { get; private set; }
    public bool IsModelLevel { get; private set; }
    private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
    public IReadOnlyDictionary<string, object> Parameters => _parameters;

    // Constructor for new rule
    public ValidationRule(string ruleName, string errorMessage, bool isModelLevel, Dictionary<string, object> parameters = null)
    {
        if (string.IsNullOrWhiteSpace(ruleName))
            throw new ArgumentException("Rule name cannot be empty", nameof(ruleName));

        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty", nameof(errorMessage));

        RuleName = ruleName;
        ErrorMessage = errorMessage;
        IsModelLevel = isModelLevel;

        if (parameters != null)
        {
            foreach (var param in parameters)
                _parameters[param.Key] = param.Value;
        }
    }

    // Constructor for loading from db
    private ValidationRule(int id, string ruleName, string errorMessage, bool isModelLevel, Dictionary<string, object> parameters)
    {
        Id = id;
        RuleName = ruleName;
        ErrorMessage = errorMessage;
        IsModelLevel = isModelLevel;

        if (parameters != null)
        {
            foreach (var param in parameters)
                _parameters[param.Key] = param.Value;
        }
    }

    public void Update(string ruleName, string errorMessage, Dictionary<string, object> parameters)
    {
        if (string.IsNullOrWhiteSpace(ruleName))
            throw new ArgumentException("Rule name cannot be empty", nameof(ruleName));

        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty", nameof(errorMessage));

        RuleName = ruleName;
        ErrorMessage = errorMessage;

        _parameters.Clear();
        if (parameters != null)
        {
            foreach (var param in parameters)
                _parameters[param.Key] = param.Value;
        }
    }

    // Factory method for easy creation from db
    public static ValidationRule Create(int id, string ruleName, string errorMessage, bool isModelLevel, Dictionary<string, object> parameters)
    {
        return new ValidationRule(id, ruleName, errorMessage, isModelLevel, parameters);
    }
}