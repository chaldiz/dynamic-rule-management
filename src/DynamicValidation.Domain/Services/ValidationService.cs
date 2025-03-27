using System.Text.Json;
using System.Text.RegularExpressions;
using DynamicValidation.Domain.Entities;

namespace DynamicValidation.Domain.Services;

/// <summary>
/// Validasyon servisi implementasyonu
/// </summary>
public class ValidationService : IValidationService
{
    public ValidationResult Validate(DynamicModel model, JsonElement data)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        var errors = new List<ValidationError>();

        // Model seviyesi validasyonları uygula
        foreach (var rule in model.ValidationRules)
        {
            var isValid = ApplyModelValidationRule(rule, data);
            if (!isValid)
            {
                errors.Add(new ValidationError(rule.RuleName, rule.ErrorMessage));
            }
        }

        // Alan seviyesi validasyonları uygula
        foreach (var field in model.Fields)
        {
            // Alanın verisi var mı kontrol et
            if (!data.TryGetProperty(field.Name, out JsonElement fieldValue))
            {
                // Alan gelmemiş, required ise hata ver
                if (field.IsRequired)
                {
                    errors.Add(new ValidationError("Required", $"'{field.Name}' alanı gereklidir", field.Name));
                }
                continue;
            }

            // Veri tipi kontrolü
            bool typeIsValid = ValidateDataType(field.DataType, fieldValue);
            if (!typeIsValid)
            {
                errors.Add(new ValidationError("InvalidType", $"'{field.Name}' alanı '{field.DataType}' tipinde olmalıdır", field.Name));
                continue;
            }

            // Alan validasyonlarını uygula
            foreach (var rule in field.ValidationRules)
            {
                var isValid = ApplyFieldValidationRule(rule, field, fieldValue);
                if (!isValid)
                {
                    errors.Add(new ValidationError(rule.RuleName, rule.ErrorMessage, field.Name));
                }
            }
        }

        return new ValidationResult(model.Name, errors.Count == 0, errors);
    }

    private bool ValidateDataType(string dataType, JsonElement value)
    {
        switch (dataType)
        {
            case "String":
                return value.ValueKind == JsonValueKind.String;
            case "Int":
                return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out _);
            case "Decimal":
                return value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out _);
            case "DateTime":
                return (value.ValueKind == JsonValueKind.String && DateTime.TryParse(value.GetString(), out _));
            case "Boolean":
                return value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False;
            case "Object":
                return value.ValueKind == JsonValueKind.Object;
            case "Array":
                return value.ValueKind == JsonValueKind.Array;
            default:
                return true; // Bilinmeyen tipler için doğru kabul et
        }
    }

    private bool ApplyModelValidationRule(ValidationRule rule, JsonElement data)
    {
        switch (rule.RuleName)
        {
            case "RequiredFields":
                if (rule.Parameters.TryGetValue("fields", out object fieldsObj))
                {
                    var fields = fieldsObj as string[];
                    foreach (var field in fields)
                    {
                        if (!data.TryGetProperty(field, out _))
                            return false;
                    }
                }
                return true;

            case "ConditionalRequired":
                if (rule.Parameters.TryGetValue("ifField", out object ifFieldObj) &&
                    rule.Parameters.TryGetValue("ifValue", out object ifValueObj) &&
                    rule.Parameters.TryGetValue("thenFields", out object thenFieldsObj))
                {
                    string ifField = ifFieldObj.ToString();
                    string ifValue = ifValueObj.ToString();
                    var thenFields = thenFieldsObj as string[];

                    if (data.TryGetProperty(ifField, out JsonElement fieldValue) &&
                        fieldValue.ToString() == ifValue)
                    {
                        foreach (var field in thenFields)
                        {
                            if (!data.TryGetProperty(field, out _))
                                return false;
                        }
                    }
                }
                return true;

            // Diğer model seviyesi validasyon kuralları eklenebilir

            default:
                return true;
        }
    }

    private bool ApplyFieldValidationRule(ValidationRule rule, ModelField field, JsonElement value)
    {
        switch (rule.RuleName)
        {
            case "Required":
                return value.ValueKind != JsonValueKind.Null && value.ValueKind != JsonValueKind.Undefined;

            case "MinLength":
                if (rule.Parameters.TryGetValue("min", out object minObj) &&
                    int.TryParse(minObj.ToString(), out int min) &&
                    value.ValueKind == JsonValueKind.String)
                {
                    string str = value.GetString();
                    return str != null && str.Length >= min;
                }
                return true;

            case "MaxLength":
                if (rule.Parameters.TryGetValue("max", out object maxObj) &&
                    int.TryParse(maxObj.ToString(), out int max) &&
                    value.ValueKind == JsonValueKind.String)
                {
                    string str = value.GetString();
                    return str != null && str.Length <= max;
                }
                return true;

            case "Pattern":
                if (rule.Parameters.TryGetValue("regex", out object regexObj) &&
                    value.ValueKind == JsonValueKind.String)
                {
                    string pattern = regexObj.ToString();
                    string str = value.GetString();
                    return str != null && Regex.IsMatch(str, pattern);
                }
                return true;

            case "Range":
                if (rule.Parameters.TryGetValue("min", out object minValObj) &&
                    rule.Parameters.TryGetValue("max", out object maxValObj) &&
                    value.ValueKind == JsonValueKind.Number)
                {
                    // JsonElement'i güvenli bir şekilde double'a dönüştür
                    double minValue;
                    double maxValue;

                    if (minValObj is JsonElement minElement)
                    {
                        minValue = minElement.GetDouble();
                    }
                    else
                    {
                        minValue = Convert.ToDouble(minValObj);
                    }

                    if (maxValObj is JsonElement maxElement)
                    {
                        maxValue = maxElement.GetDouble();
                    }
                    else
                    {
                        maxValue = Convert.ToDouble(maxValObj);
                    }

                    double val = value.GetDouble();
                    return val >= minValue && val <= maxValue;
                }
                return true;
            case "Email":
                if (value.ValueKind == JsonValueKind.String)
                {
                    string email = value.GetString();
                    return email != null && Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                }
                return true;

            case "OneOf":
                if (rule.Parameters.TryGetValue("values", out object valuesObj))
                {
                    var allowedValues = JsonSerializer.Deserialize<string[]>(valuesObj.ToString());
                    string valueStr = value.ToString();
                    return allowedValues.Contains(valueStr);



                    // var allowedValues = valuesObj as string[];
                    // string valueStr = value.ToString();
                    // return allowedValues.Contains(valueStr);
                }
                return true;

            // Diğer alan seviyesi validasyon kuralları eklenebilir

            default:
                return true;
        }
    }
}