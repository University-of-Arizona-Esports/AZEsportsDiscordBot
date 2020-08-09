using System.Linq;
using System.Threading.Tasks;
using AZEsportsDiscordBot.Framework;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace AZEsportsDiscordBot.Intrinsic.Commands
{
    /// <summary>
    /// Loads the command service for the bot
    /// </summary>
    internal class CommandServiceLoader : ILoadable<CommandService>
    {
        public CommandService Value { get; private set; }

        // Global logger
        private IAzBotLogger _logger;

        public Task Load(IServiceCollection services)
        {
            Value = new CommandService(GetConfig());
            _logger = services.BuildServiceProvider().GetRequiredService<IAzBotLogger>();
            Value.Log += HandleLog;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get the default config for the command service.
        /// </summary>
        /// <returns>Default commands config.</returns>
        private CommandServiceConfig GetConfig()
        {
            return new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async
            };
        }

        public async Task Unload()
        {
            Value.Log -= HandleLog;
            // Modules are collected to list to prevent concurrent modification
            foreach (var module in Value.Modules.ToList())
            {
                await Value.RemoveModuleAsync(module);
            }
            Value = null;
        }

        /// <summary>
        /// Forward log messages from command service to global logger.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <returns>Logger task.</returns>
        private async Task HandleLog(LogMessage message)
        {
            await _logger.Log(message);
        }
    }
}