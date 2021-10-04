using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Generic;

namespace CommandService.Controllers
{
    [Route("api/c/[controller]")]
    [ApiController]
    public class PlatformController : ControllerBase
    {
        private readonly ILogger<PlatformController> _logger;
        private readonly ICommandRepository _commandRepository;
        private readonly IMapper _mapper;

        public PlatformController(ILogger<PlatformController> logger,
                                  ICommandRepository commandRepository,
                                  IMapper mapper)
        {
            _logger = logger;
            _commandRepository = commandRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            var result = _commandRepository.GetAllPlatforms();
            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(result));
        }

        [HttpPost]
        public ActionResult TesteInboundConnection()
        {
            _logger.LogInformation("TesteInboundConnection");

            return Ok();
        }
    }
}