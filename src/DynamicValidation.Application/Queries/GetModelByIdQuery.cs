using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicValidation.Application.Models;
using DynamicValidation.Domain.Entities;
using DynamicValidation.Domain.Repositories;
using MediatR;

namespace DynamicValidation.Application.Queries
{
    // 1. Get Model By Id Query
    public class GetModelByIdQuery : IRequest<DynamicModelDto>
    {
        public int Id { get; set; }
    }

    public class GetModelByIdQueryHandler : IRequestHandler<GetModelByIdQuery, DynamicModelDto>
    {
        private readonly IDynamicModelRepository _repository;

        public GetModelByIdQueryHandler(IDynamicModelRepository repository)
        {
            _repository = repository;
        }

        public async Task<DynamicModelDto> Handle(GetModelByIdQuery request, CancellationToken cancellationToken)
        {
            var model = await _repository.GetByIdAsync(request.Id);
            if (model == null)
                return null;

            // Map domain model to DTO
            return MapModelToDto(model);
        }

        private DynamicModelDto MapModelToDto(DynamicModel model)
        {
            var dto = new DynamicModelDto
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description
            };
            
            // Map validation rules
            foreach (var rule in model.ValidationRules)
            {
                dto.ModelValidationRules.Add(new ValidationRuleDto
                {
                    Id = rule.Id,
                    RuleName = rule.RuleName,
                    ErrorMessage = rule.ErrorMessage,
                    Parameters = rule.Parameters.ToDictionary(p => p.Key, p => p.Value)
                });
            }
            
            // Map fields
            foreach (var field in model.Fields)
            {
                var fieldDto = new ModelFieldDto
                {
                    Id = field.Id,
                    Name = field.Name,
                    DataType = field.DataType,
                    Description = field.Description,
                    IsRequired = field.IsRequired,
                    MaxLength = field.MaxLength,
                    DefaultValue = field.DefaultValue,
                    DisplayOrder = field.DisplayOrder,
                    NestedModelId = field.NestedModelId
                };
                
                // Map field validation rules
                foreach (var rule in field.ValidationRules)
                {
                    fieldDto.ValidationRules.Add(new ValidationRuleDto
                    {
                        Id = rule.Id,
                        RuleName = rule.RuleName,
                        ErrorMessage = rule.ErrorMessage,
                        Parameters = rule.Parameters.ToDictionary(p => p.Key, p => p.Value)
                    });
                }
                
                dto.Fields.Add(fieldDto);
            }
            
            return dto;
        }
    }
}