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
        /// <summary>Get the assembly of the object.</summary>
        private Assembly CurrentAssembly => Assembly.GetAssembly(GetType());
        
        /// <summary>
        /// Loads command modules in the current assembly.
        /// This method should be called by implementing classes.
        /// </summary>
        public override void OnCogLoad()
        {
            // Load command modules into the framework's command service
            var commandService = Services.GetRequiredService<CommandService>();
            commandService.AddModulesAsync(CurrentAssembly, Services)
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Unload modules that were loaded by this cog.
        /// This method should be called by implementing classes.
        /// </summary>
        public override void OnCogPreUnload()
        {
            var commandService = Services.GetRequiredService<CommandService>();
            // Attempt to unload all types from the current assembly.
            // If ModuleInfo is used to unload module, then CommandService will
            // keep a reference to the type and prevent it from being unloaded.
            Task.WhenAll(CurrentAssembly.GetTypes().Select(type => commandService.RemoveModuleAsync(type)))
                .GetAwaiter()
                .GetResult();
        }
    }
}