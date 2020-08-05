using System;
using System.Threading.Tasks;
using AZEsportsDiscordBot.Framework;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace AZEsportsDiscordBot
{
    /// <summary>
    /// Loads the discord client.
    /// </summary>
    internal class DiscordLoader : ILoadable<DiscordSocketClient>
    {
        public DiscordSocketClient Value { get; private set; }

        /// <summary>Global logger</summary>
        private IAzBotLogger _logger;

        public async Task Load(IServiceCollection services)
        {
            Value = new DiscordSocketClient(GetConfig());
            var token = Environment.GetEnvironmentVariable("AZEsportsBotToken");
            await Value.LoginAsync(TokenType.Bot, token);
            await Value.StartAsync();

            _logger = services.BuildServiceProvider().GetRequiredService<IAzBotLogger>();
            Value.Log += OnLogMessage;
        }

        /// <summary>
        /// Default config to use for the discord client.
        /// </summary>
        /// <returns>Discord client config</returns>
        private DiscordSocketConfig GetConfig()
        {
            return new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 1000
            };
        }

        public async Task Unload()
        {
            if (Value == null) return;
            await Value.StopAsync();
            await Value.LogoutAsync();
            Value.Log -= OnLogMessage;
            Value = null;
            _logger = null;
        }

        /// <summary>
        /// Forward a log message from discord to the global logger.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <returns>Logger task.</returns>
        private async Task OnLogMessage(LogMessage message)
        {
            await _logger.Log(message);
        }
    }
}