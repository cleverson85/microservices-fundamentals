using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlatformService.AsyncDataService;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataService.Http;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformController : ControllerBase
    {
        private readonly IPlatformRepository _repository;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _commandClient;
        private readonly ILogger _logger;
        private readonly IMessageBusClient _messageBusClient;

        public PlatformController(IPlatformRepository repository, IMapper mapper,
                                  ICommandDataClient commandClient, ILogger<PlatformController> logger,
                                  IMessageBusClient messageBusClient)
        {
            _repository = repository;
            _mapper = mapper;
            _commandClient = commandClient;
            _logger = logger;
            _messageBusClient = messageBusClient;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            var result = _repository.GetAllPlatforms();
            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(result));
        }

        [HttpGet("{id}", Name = "GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int id)
        {
            var result = _repository.GetPlatformById(id);

            if (result == null)
            {
                return NotFound("Result not found");
            }

            return Ok(_mapper.Map<PlatformReadDto>(result));
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platformCreateDto)
        {
            var model = _mapper.Map<Platform>(platformCreateDto);
            _repository.CreatePlatform(model);
            _repository.SaveChanges();

            var platformReadDto = _mapper.Map<PlatformReadDto>(model);

            try
            {
                _logger.LogInformation($"--> Sending sync message");
                await _commandClient.SendPlatformToCommand(platformReadDto);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"--> Could not send synchronously: {ex.Message}");
            }

            try
            {
                _logger.LogInformation($"--> Sending async message");

                var platformPublishedDto = _mapper.Map<PlatformPublishedDto>(platformReadDto);
                platformPublishedDto.Event = "Platform_Published";

                _messageBusClient.PublishNewPlatform(platformPublishedDto);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"--> Could not send asynchronously: {ex.Message}");
            }




            return CreatedAtRoute(nameof(GetPlatformById), new { platformReadDto.Id }, platformReadDto);
        }
    }
}