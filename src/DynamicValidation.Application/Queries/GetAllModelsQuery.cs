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
    // 3. Get All Models Query
    public class GetAllModelsQuery : IRequest<List<DynamicModelDto>>
    {
    }

    public class GetAllModelsQueryHandler : IRequestHandler<GetAllModelsQuery, List<DynamicModelDto>>
    {
        private readonly IDynamicModelRepository _repository;
        private readonly IMediator _mediator;

        public GetAllModelsQueryHandler(IDynamicModelRepository repository, IMediator mediator)
        {
            _repository = repository;
            _mediator = mediator;
        }

        public async Task<List<DynamicModelDto>> Handle(GetAllModelsQuery request, CancellationToken cancellationToken)
        {
            var models = await _repository.GetAllAsync();
            var modelDtos = new List<DynamicModelDto>();
            
            foreach (var model in models)
            {
                // Reuse the GetModelById query handler to map each model to DTO
                var dto = await _mediator.Send(new GetModelByIdQuery { Id = model.Id }, cancellationToken);
                modelDtos.Add(dto);
            }
            
            return modelDtos;
        }
    }
}