using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MediatR;
using System.Text.Json;
using DynamicValidation.Application.Commands;
using DynamicValidation.Application.Queries;
using DynamicValidation.Application.Models;
using Microsoft.AspNetCore.Cors;
using System.Text;

namespace DynamicValidation.API.Controllers
{
    [EnableCors("DynamicValidationPolicy")]
    [ApiController]
    [Route("api/[controller]")]
    public class DynamicModelController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DynamicModelController> _logger;

        public DynamicModelController(IMediator mediator, ILogger<DynamicModelController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateModel([FromBody] DynamicModelDto model)
        {
            try
            {
                var command = new CreateModelCommand { Model = model };
                var result = await _mediator.Send(command);
                return Ok(new { Id = result, Name = model.Name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating model");
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateModel(int id, [FromBody] DynamicModelDto model)
        {
            try
            {
                var command = new UpdateModelCommand { Id = id, Model = model };
                var result = await _mediator.Send(command);
                return Ok(new { Success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating model");
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModel(int id)
        {
            try
            {
                var command = new DeleteModelCommand { Id = id };
                var result = await _mediator.Send(command);
                return Ok(new { Success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting model");
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetModel(int id)
        {
            try
            {
                var query = new GetModelByIdQuery { Id = id };
                var result = await _mediator.Send(query);

                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting model");
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("byname/{name}")]
        public async Task<IActionResult> GetModelByName(string name)
        {
            try
            {
                var query = new GetModelByNameQuery { Name = name };
                var result = await _mediator.Send(query);

                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting model by name");
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllModels()
        {
            try
            {
                var query = new GetAllModelsQuery();
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all models");
                return BadRequest(new { Error = ex.Message });
            }
        }

        // Önce bir DTO (Data Transfer Object) tanımlayın
        public class ValidateRequestDto
        {
            public string ModelName { get; set; }
            public object Data { get; set; }
        }

        // Sonra controller metodunu şu şekilde değiştirin
        [HttpPost("validate1")]
        public async Task<IActionResult> ValidateData([FromBody] ValidateRequestDto request)
        {
            try
            {
                _logger.LogInformation($"Validating data for model {request.ModelName}");

                // Data null kontrolü
                if (request.Data == null)
                {
                    return BadRequest(new { Error = "Data cannot be null" });
                }

                // Convert data to JsonElement
                var jsonString = JsonSerializer.Serialize(request.Data);
                _logger.LogInformation($"Received data: {jsonString}");

                var jsonDocument = JsonDocument.Parse(jsonString);
                var data = jsonDocument.RootElement;

                var command = new ValidateDataCommand { ModelName = request.ModelName, Data = data };
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating data for model: {request.ModelName ?? "unknown"}");
                return BadRequest(new { Error = ex.Message });
            }
        }


        [HttpPost("validate")]
        public async Task<IActionResult> AutoDetectAndValidate([FromBody] JsonElement data)
        {
            try
            {
                // First, detect the model type
                var detectCommand = new DetectModelTypeCommand { Data = data };
                var modelName = await _mediator.Send(detectCommand);

                if (string.IsNullOrEmpty(modelName))
                    return BadRequest(new { Error = "Could not determine model type" });

                // Then validate against the detected model
                var validateCommand = new ValidateDataCommand { ModelName = modelName, Data = data };
                var result = await _mediator.Send(validateCommand);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-detecting and validating data");
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}