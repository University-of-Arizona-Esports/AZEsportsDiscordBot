using System.Collections.Generic;
using Discord.WebSocket;

namespace AZEsportsDiscordBot.Framework
{
    /// <summary>
    /// Handles the loading and unloading of <see cref="Cog"/> components.
    /// </summary>
    public class CogManager
    {
        public readonly DiscordSocketClient Discord;

        private Dictionary<string, LoadedAssembly> _assemblies;
        
        public CogManager(DiscordSocketClient discord)
        {
            Discord = discord;
            _assemblies = new Dictionary<string, LoadedAssembly>();
        }
    }
}