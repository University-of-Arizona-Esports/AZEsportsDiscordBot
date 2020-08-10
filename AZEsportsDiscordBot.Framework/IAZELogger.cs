using System;
using System.Threading.Tasks;
using Discord;

namespace AZEsportsDiscordBot.Framework
{
    /// <summary>
    /// Logger for the discord bot.
    /// Logs can be sent to various log destinations.
    /// </summary>
    public interface IAzBotLogger
    {
        /// <summary>
        /// Log a message to the specified log destinations.
        /// </summary>
        /// <param name="content">Content to log.</param>
        /// <param name="destination">Where to log the message.</param>
        /// <returns>Task that completes after logging is done.</returns>
        Task Log(string content, Destination destination = Destination.Console);

        /// <inheritdoc cref="Log(string,AZEsportsDiscordBot.Framework.Destination)"/>
        async Task Log(LogMessage message, Destination destination = Destination.Console)
        {
            await Log(message.ToString(), destination);
        }
    }

    /// <summary>
    /// Specifies where a log message is sent.
    /// </summary>
    [Flags]
    public enum Destination
    {
        All     = 0b_11,
        Console = 0b_01,
        Discord = 0b_10
    }
}