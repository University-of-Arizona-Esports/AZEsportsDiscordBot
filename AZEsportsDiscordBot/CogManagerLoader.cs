using System.Linq;
using System.Threading.Tasks;
using AZEsportsDiscordBot.Framework;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace AZEsportsDiscordBot
{
    /// <summary>
    /// Create the cog manager
    /// </summary>
    internal class CogManagerLoader : ILoadable<CogManager>
    {
        public CogManager Value { get; private set; }

        /// <summary>Global logger</summary>
        private IAzBotLogger _logger;

        public Task Load(IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            _logger = provider.GetRequiredService<IAzBotLogger>();
            Value = new CogManager(provider.GetRequiredService<DiscordSocketClient>(), provider, _logger);

            // Forward log messages
            Value.Log += OnLogReceived;
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Forward logs to the global logger.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <returns>Logger task.</returns>
        private async Task OnLogReceived(LogMessage message)
        {
            await _logger.Log(message);
        }

        public Task Unload()
        {
            foreach (var assembly in Value.LoadedAssemblies.ToList())
            {
                Value.UnloadAssembly(assembly);
            }
            Value.Log -= OnLogReceived;
            Value = null;
            _logger = null;
            return Task.CompletedTask;
        }
    }
}