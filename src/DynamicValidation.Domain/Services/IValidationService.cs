using System.Text.Json;
using DynamicValidation.Domain.Entities;

namespace DynamicValidation.Domain.Services
{
    /// <summary>
    /// Validasyon servis arayüzü
    /// </summary>
    public interface IValidationService
    {
        ValidationResult Validate(DynamicModel model, JsonElement data);
    }
}