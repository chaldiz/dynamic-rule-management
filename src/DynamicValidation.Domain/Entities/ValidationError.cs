
namespace DynamicValidation.Domain.Entities;

/// <summary>
/// Validasyon hatası
/// </summary>
public class ValidationError
{
    public string FieldName { get; }
    public string ErrorMessage { get; }
    public string RuleName { get; }

    public ValidationError(string ruleName, string errorMessage, string fieldName = null)
    {
        RuleName = ruleName;
        ErrorMessage = errorMessage;
        FieldName = fieldName;
    }
}