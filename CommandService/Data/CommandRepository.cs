using CommandService.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandService.Data
{
    public class CommandRepository : ICommandRepository
    {
        public readonly AppDbContext _context;
        private readonly ILogger<CommandRepository> _logger;

        public CommandRepository(AppDbContext context, ILogger<CommandRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void CreateCommand(int platformId, Command command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            command.PlatformId = platformId;
            _context.Command.Add(command);
        }

        public void CreatePlatform(Platform platform)
        {
            if (platform == null)
            {
                throw new ArgumentNullException(nameof(platform));
            }

            _context.Add(platform);
        }

        public bool ExternalPlatformExist(int externalPlatformId)
        {
            return _context.Platform.Any(c => c.ExternalId == externalPlatformId);
        }

        public IEnumerable<Platform> GetAllPlatforms()
        {
            _logger.LogInformation("--> Getting all Platforms...");

            return _context.Platform.ToList();
        }

        public Command GetCommand(int platformId, int commandId)
        {
            _logger.LogInformation($"--> Getting Command Platform Id {platformId}, Command Id {commandId}");

            return _context.Command.Where(c => c.PlatformId == platformId && c.Id == commandId).FirstOrDefault();
        }

        public IEnumerable<Command> GetCommandsForPlatform(int platformId)
        {
            _logger.LogInformation($"--> Getting Command for Platform Id {platformId}");

            return _context.Command.Where(c => c.PlatformId == platformId).OrderBy(c => c.Platform.Name);
        }

        public bool PlatformExist(int platformId)
        {
            return _context.Platform.Any(c => c.Id == platformId);
        }

        public bool SaveChanges()
        {
            return (_context.SaveChanges() >= 0);
        }
    }
}
