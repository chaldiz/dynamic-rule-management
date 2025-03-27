using System.Text.Json;
using DynamicValidation.Application.Models;
using DynamicValidation.Domain.Entities;
using DynamicValidation.Domain.Repositories;
using DynamicValidation.Domain.Services;
using MediatR;

namespace DynamicValidation.Application.Commands
{
    // 5. Auto-detect Model Type Command
    public class DetectModelTypeCommand : IRequest<string>
    {
        public JsonElement Data { get; set; }
    }

    public class DetectModelTypeCommandHandler : IRequestHandler<DetectModelTypeCommand, string>
    {
        private readonly IDynamicModelRepository _repository;

        public DetectModelTypeCommandHandler(IDynamicModelRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(DetectModelTypeCommand request, CancellationToken cancellationToken)
        {
            // Get all active models
            var allModels = await _repository.GetAllAsync();
            if (allModels.Count == 0)
                return null;

            var modelScores = new Dictionary<string, int>();

            // Calculate match score for each model
            foreach (var model in allModels)
            {
                int score = 0;
                int totalFields = model.Fields.Count;
                
                // Check if there's a modelType field that explicitly states the model type
                if (request.Data.TryGetProperty("modelType", out JsonElement modelTypeElement) && 
                    modelTypeElement.ValueKind == JsonValueKind.String)
                {
                    string modelType = modelTypeElement.GetString();
                    if (modelType == model.Name)
                    {
                        // Model type is explicitly specified, return it directly
                        return model.Name;
                    }
                }

                // Check all fields of the model
                foreach (var field in model.Fields)
                {
                    if (request.Data.TryGetProperty(field.Name, out JsonElement _))
                    {
                        // Add score for each matching field
                        score++;
                    }
                }

                // Calculate match percentage for this model
                if (totalFields > 0)
                {
                    double matchPercentage = (double)score / totalFields;
                    
                    // Add model to scores if match percentage is above threshold (70%)
                    if (matchPercentage >= 0.7)
                    {
                        modelScores[model.Name] = score;
                    }
                }
            }

            // Find the best matching model
            if (modelScores.Count > 0)
            {
                // Return the model with highest score
                return modelScores.OrderByDescending(s => s.Value).First().Key;
            }

            // No model matched sufficiently
            return null;
        }
    }
}