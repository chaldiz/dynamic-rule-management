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
    // 2. Get Model By Name Query
    public class GetModelByNameQuery : IRequest<DynamicModelDto>
    {
        public string Name { get; set; }
    }

    public class GetModelByNameQueryHandler : IRequestHandler<GetModelByNameQuery, DynamicModelDto>
    {
        private readonly IDynamicModelRepository _repository;
        private readonly IMediator _mediator;

        public GetModelByNameQueryHandler(IDynamicModelRepository repository, IMediator mediator)
        {
            _repository = repository;
            _mediator = mediator;
        }

        public async Task<DynamicModelDto> Handle(GetModelByNameQuery request, CancellationToken cancellationToken)
        {
            var model = await _repository.GetByNameAsync(request.Name);
            if (model == null)
                return null;

            // Reuse the GetModelById query handler to map the model to DTO
            return await _mediator.Send(new GetModelByIdQuery { Id = model.Id }, cancellationToken);
        }
    }
}