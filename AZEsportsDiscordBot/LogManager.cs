using System;
using System.Linq;
using System.Threading.Tasks;
using AZEsportsDiscordBot.Framework;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace AZEsportsDiscordBot
{
    /// <summary>
    /// Global source of logging.
    /// Log messages are sent here and directed to the various log locations.
    /// </summary>
    internal class LogManager : ILoadable<IAzBotLogger>, IAzBotLogger
    {
        public IAzBotLogger Value { get; private set; }

        /// <summary>Core services</summary>
        private IServiceCollection _services;
        /// <summary>Client instance used for discord channel logging</summary>
        private DiscordSocketClient _discord;

        public Task Load(IServiceCollection services)
        {
            Value = this;
            _services = services;
            return Task.CompletedTask;
        }

        public Task Unload()
        {
            Value = null;
            _services = null;
            _discord = null;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Return the discord client, or else search for it in the service collection,
        /// or else return null.
        /// </summary>
        /// <returns>Discord client if it exists, or else null.</returns>
        private DiscordSocketClient BindDiscord()
        {
            if (_discord != null) return _discord;
            _discord = _services?.Where(descriptor => descriptor.ServiceType == typeof(DiscordSocketClient))
                .Select(descriptor => descriptor.ImplementationInstance)
                .SingleOrDefault() as DiscordSocketClient;
            return _discord;
        }

        /// <inheritdoc cref="IAzBotLogger"/>
        public async Task Log(string content, Destination destination = Destination.Console)
        {
            if (destination.HasFlag(Destination.Console))
            {
                Console.WriteLine(content);
            }
            if (destination.HasFlag(Destination.Discord))
            {
                var discord = BindDiscord();
                // TODO handle case where content is larger than MaxMessageSize
                // TODO use embeds?
                // TODO get channel from config
                var serverId = 737843314136449145UL;
                var channelId = 738337647444885545UL;
                var channel = discord?.GetGuild(serverId)?.GetTextChannel(channelId);
                var task = channel?.SendMessageAsync(content);
                if (task != null) await task;
            }
        }
    }
}