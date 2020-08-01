using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace AZEsportsDiscordBot.Framework
{
    /// <summary>
    /// Cog that registers types in the current assembly with the CommandService
    /// from discord.
    /// </summary>
    public abstract class CommandCog : Cog
    {
        // Modules that were loaded by the CommandService
        private List<ModuleInfo> _loadedModules;
        
        public override void OnCogLoad()
        {
            var commandService = Services.GetRequiredService<CommandService>();
            // Get the assembly of the type that extended this class to load modules from
            _loadedModules = commandService.AddModulesAsync(Assembly.GetAssembly(GetType()), Services)
                .GetAwaiter()
                .GetResult()
                .ToList();
        }

        public override void OnCogUnload()
        {
            var commandService = Services.GetRequiredService<CommandService>();
            // Wait for all modules to be removed
            Task.WhenAll(_loadedModules.Select(info => commandService.RemoveModuleAsync(info)))
                .GetAwaiter()
                .GetResult();
            _loadedModules.Clear();
            _loadedModules = null;
        }
    }
}