namespace DynamicValidation.Domain.Entities;


/// <summary>
/// Validasyon sonucu
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; }
    public string ModelName { get; }
    public IReadOnlyCollection<ValidationError> Errors { get; }

    public ValidationResult(string modelName, bool isValid, IEnumerable<ValidationError> errors = null)
    {
        ModelName = modelName;
        IsValid = isValid;
        Errors = errors?.ToList() ?? new List<ValidationError>();
    }
}