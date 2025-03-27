using System.Text.Json;
using DynamicValidation.Application.Models;
using DynamicValidation.Domain.Repositories;
using DynamicValidation.Domain.Services;
using MediatR;

namespace DynamicValidation.Application.Commands
{
    // 4. Validate Data Command
    public class ValidateDataCommand : IRequest<ValidationResultDto>
    {
        public string ModelName { get; set; }
        public JsonElement Data { get; set; }
    }

    public class ValidateDataCommandHandler : IRequestHandler<ValidateDataCommand, ValidationResultDto>
    {
        private readonly IDynamicModelRepository _repository;
        private readonly IValidationService _validationService;

        public ValidateDataCommandHandler(IDynamicModelRepository repository, IValidationService validationService)
        {
            _repository = repository;
            _validationService = validationService;
        }

        public async Task<ValidationResultDto> Handle(ValidateDataCommand request, CancellationToken cancellationToken)
        {
            // Get model by name
            var model = await _repository.GetByNameAsync(request.ModelName);
            if (model == null)
                throw new Exception($"Model with name '{request.ModelName}' not found");
            
            // Validate data against model
            var validationResult = _validationService.Validate(model, request.Data);
            
            // Map domain validation result to DTO
            return new ValidationResultDto
            {
                IsValid = validationResult.IsValid,
                ModelName = validationResult.ModelName,
                Errors = validationResult.Errors.Select(e => new ValidationErrorDto
                {
                    FieldName = e.FieldName,
                    ErrorMessage = e.ErrorMessage,
                    RuleName = e.RuleName
                }).ToList()
            };
        }
    }
}