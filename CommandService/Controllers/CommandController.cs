using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CommandService.Controllers
{
    [Route("api/c/platform/{platformId}/[controller]")]
    [ApiController]
    public class CommandController : ControllerBase
    {
        private readonly ILogger<CommandController> _logger;
        private readonly ICommandRepository _commandRepository;
        private readonly IMapper _mapper;

        public CommandController(ILogger<CommandController> logger,
                                 ICommandRepository commandRepository,
                                 IMapper mapper)
        {
            _logger = logger;
            _commandRepository = commandRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatforms(int platformId)
        {
            _logger.LogInformation($"--> Hit GetCommandsForPlatforms: {platformId}");

            if (!_commandRepository.PlatformExist(platformId))
            {
                return NotFound();
            }

            var result = _commandRepository.GetCommandsForPlatform(platformId);
            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(result));
        }

        [HttpGet("{commandId}", Name = "GetCommandForPlatform")]
        public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId)
        {
            _logger.LogInformation($"--> Hit GetCommandForPlatform: {platformId} / {commandId}");

            if (!_commandRepository.PlatformExist(platformId))
            {
                return NotFound();
            }

            var result = _commandRepository.GetCommand(platformId, commandId);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CommandReadDto>(result));
        }

        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommandForPlatform(int platformId, CommandCreateDto commandCreateDto)
        {
            _logger.LogInformation($"--> Hit CreateCommandForPlatform: {platformId}");

            if (!_commandRepository.PlatformExist(platformId))
            {
                return NotFound();
            }

            var command = _mapper.Map<Command>(commandCreateDto);
            _commandRepository.CreateCommand(platformId, command);
            _commandRepository.SaveChanges();

            var commandReadDto = _mapper.Map<CommandReadDto>(command);
            return CreatedAtRoute(nameof(GetCommandForPlatform), new { platformId = platformId, commandId = commandReadDto.Id }, commandReadDto);
        }
    }
}