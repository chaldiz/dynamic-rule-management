using System.Text.Json;
using DynamicValidation.Application.Models;
using DynamicValidation.Domain.Entities;
using DynamicValidation.Domain.Repositories;
using DynamicValidation.Domain.Services;
using MediatR;

namespace DynamicValidation.Application.Commands
{
    public class UpdateModelCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public DynamicModelDto Model { get; set; }
    }

    public class UpdateModelCommandHandler : IRequestHandler<UpdateModelCommand, bool>
    {
        private readonly IDynamicModelRepository _repository;

        public UpdateModelCommandHandler(IDynamicModelRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(UpdateModelCommand request, CancellationToken cancellationToken)
        {
            // Get existing model
            var model = await _repository.GetByIdAsync(request.Id);
            if (model == null)
                throw new Exception($"Model with ID {request.Id} not found");
            
            // Update basic properties
            model.Update(request.Model.Name, request.Model.Description);
            
            // Clear existing validation rules and fields to recreate them
            foreach (var rule in model.ValidationRules.ToList())
            {
                model.RemoveValidationRule(rule.Id);
            }
            
            foreach (var field in model.Fields.ToList())
            {
                model.RemoveField(field.Id);
            }
            
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
            
            // Save changes
            await _repository.UpdateAsync(model);
            return true;
        }
    }
}