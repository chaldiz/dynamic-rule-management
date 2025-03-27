using System.Text.Json;
using DynamicValidation.Application.Models;
using DynamicValidation.Domain.Entities;
using DynamicValidation.Domain.Repositories;
using DynamicValidation.Domain.Services;
using MediatR;

namespace DynamicValidation.Application.Commands
{
    // 1. Create Model Command
    public class CreateModelCommand : IRequest<int>
    {
        public DynamicModelDto Model { get; set; }
    }

    public class CreateModelCommandHandler : IRequestHandler<CreateModelCommand, int>
    {
        private readonly IDynamicModelRepository _repository;

        public CreateModelCommandHandler(IDynamicModelRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(CreateModelCommand request, CancellationToken cancellationToken)
        {
            // Create domain model from DTO
            var model = new DynamicModel(request.Model.Name, request.Model.Description);
            
            // Add model level validation rules
            foreach (var ruleDto in request.Model.ModelValidationRules)
            {
                var rule = new ValidationRule(
                    ruleDto.RuleName,
                    ruleDto.ErrorMessage,
                    true,
                    ruleDto.Parameters
                );
                model.AddValidationRule(rule);
            }
            
            // Add fields with validation rules
            foreach (var fieldDto in request.Model.Fields)
            {
                var field = new ModelField(
                    fieldDto.Name,
                    fieldDto.DataType,
                    fieldDto.Description,
                    fieldDto.IsRequired,
                    fieldDto.MaxLength,
                    fieldDto.DefaultValue,
                    fieldDto.DisplayOrder,
                    fieldDto.NestedModelId
                );
                
                // Add field level validation rules
                foreach (var ruleDto in fieldDto.ValidationRules)
                {
                    var rule = new ValidationRule(
                        ruleDto.RuleName,
                        ruleDto.ErrorMessage,
                        false,
                        ruleDto.Parameters
                    );
                    field.AddValidationRule(rule);
                }
                
                model.AddField(field);
            }
            
            // Save model to repository
            var result = await _repository.AddAsync(model);
            return result.Id;
        }
    }
}