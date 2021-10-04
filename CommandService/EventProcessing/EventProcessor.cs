using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Text.Json;

namespace CommandService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMapper _mapper;

        public EventProcessor(IServiceScopeFactory serviceScopeFactory, IMapper mapper)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _mapper = mapper;
        }

        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);
        }

        private EventType DetermineEvent(string notificationMessage)
        {
            Log.Information("--> Determining Event");

            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);

            if (eventType.Event == "Platform_Published")
            {
                Log.Information("Platform Published Event Detected");
                return EventType.PlatformPublished;
            }

            Log.Information("Could not determine the event type");
            return EventType.Undetermined;
        }

        private void AddPlatform(string platformPublishedMessage)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var respository = scope.ServiceProvider.GetRequiredService<ICommandRepository>();
                var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishDto>(platformPublishedMessage);

                try
                {
                    var platform = _mapper.Map<Platform>(platformPublishedDto);

                    if (!respository.ExternalPlatformExist(platform.ExternalId))
                    {
                        respository.CreatePlatform(platform);
                        respository.SaveChanges();
                        return;
                    }

                    Log.Information("--> Platform already exists...");
                }
                catch (Exception e)
                {
                    Log.Error($"--> Could not add Platform to DB {e.Message}");
                }
            }
        }
    }

    enum EventType
    {
        PlatformPublished,
        Undetermined
    }
}
