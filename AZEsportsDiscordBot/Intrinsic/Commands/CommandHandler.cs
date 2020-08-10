using System;
using System.Threading.Tasks;
using AZEsportsDiscordBot.Framework;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace AZEsportsDiscordBot.Intrinsic.Commands
{
    /// <summary>
    /// Handles sending messages to the CommandService
    /// </summary>
    public class CommandHandler : ILoadable
    {
        private IServiceProvider _services;
        private DiscordSocketClient _discord;
        private CommandService _commands;

        public async Task Load(IServiceCollection services)
        {
            _services = services.BuildServiceProvider();
            _discord = _services.GetRequiredService<DiscordSocketClient>();
            _commands = _services.GetRequiredService<CommandService>();
            
            _discord.MessageReceived += OnMessageReceived;
            // Load any intrinsic commands in the bot
            await _commands.AddModulesAsync(GetType().Assembly, _services);
        }

        public Task Unload()
        {
            _discord.MessageReceived -= OnMessageReceived;
            return Task.CompletedTask;
        }

        private async Task OnMessageReceived(SocketMessage message)
        {
            // Reject system messages and bots
            if (!(message is SocketUserMessage msg)) return;
            if (msg.Author.IsBot) return;
            
            var context = new SocketCommandContext(_discord, msg);
            // TODO load prefix from database/config?
            var prefix = "!";

            var argPos = 0;
            if (msg.HasStringPrefix(prefix, ref argPos) ||
                msg.HasMentionPrefix(_discord.CurrentUser, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                // if (!result.IsSuccess) await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }
    }
}