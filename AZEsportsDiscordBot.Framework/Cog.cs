using Discord.WebSocket;

namespace AZEsportsDiscordBot.Framework
{
    /// <summary>
    /// A Cog is a modular component that is used to ease the loading of various components
    /// in the discord bot.
    /// </summary>
    public abstract class Cog
    {
        /// <summary>
        /// Access the <see cref="CogManager"/> that this cog is loaded into, if any.
        /// </summary>
        public CogManager CogManager { get; internal set; }

        /// <summary>
        /// Convenient access to the discord client instance.
        /// </summary>
        protected DiscordSocketClient Discord => CogManager.Discord;

        /// <summary>
        /// Called when this Cog is loaded by a <see cref="CogManager"/>.
        /// This is where any setup for the cog should occur.
        /// </summary>
        public virtual void OnCogLoad() { }

        /// <summary>
        /// Called when this Cog is unloaded. By the end of this method
        /// the Cog should have completely cleaned up any resources it needs
        /// to, and unregistered events and listeners attached to discord.
        /// </summary>
        public virtual void OnCogUnload() { }
    }
}