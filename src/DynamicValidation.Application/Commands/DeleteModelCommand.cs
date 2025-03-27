using System.Text.Json;
using DynamicValidation.Application.Models;
using DynamicValidation.Domain.Entities;
using DynamicValidation.Domain.Repositories;
using DynamicValidation.Domain.Services;
using MediatR;

namespace DynamicValidation.Application.Commands
{
    // 3. Delete Model Command
    public class DeleteModelCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }

    public class DeleteModelCommandHandler : IRequestHandler<DeleteModelCommand, bool>
    {
        private readonly IDynamicModelRepository _repository;

        public DeleteModelCommandHandler(IDynamicModelRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeleteModelCommand request, CancellationToken cancellationToken)
        {
            await _repository.DeleteAsync(request.Id);
            return true;
        }
    }
}