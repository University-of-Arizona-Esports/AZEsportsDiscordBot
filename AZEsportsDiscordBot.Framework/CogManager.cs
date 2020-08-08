using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace AZEsportsDiscordBot.Framework
{
    /// <summary>
    /// Handles the loading and unloading of <see cref="Cog"/> components.
    /// </summary>
    public class CogManager
    {
        /// <summary>
        /// The discord client that this manager works with.
        /// </summary>
        public readonly DiscordSocketClient Discord;

        /// <summary>
        /// Service provider given to this manager, used by some cogs.
        /// </summary>
        public readonly IServiceProvider Services;

        /// <summary>
        /// Global logger for the cogs.
        /// </summary>
        public readonly IAzBotLogger Logger;

        /// <summary>
        /// Folder relative to current working directory to look in for cogs, or
        /// null to use the current working directory. Default value: "cogs" folder.
        /// </summary>
        public string RelativeFolder = "cogs";

        /// <summary>Currently loaded assemblies.</summary>
        private ConcurrentDictionary<string, LoadedAssembly> _assemblies;

        /// <summary>Unloaded assemblies that have not been garbage collected.</summary>
        /// <seealso cref="CleanupUnloaded"/>
        private ConcurrentDictionary<string, WeakReference<LoadedAssembly>> _unloaded;

        /// <summary>
        /// Create a CogManager for a discord client.
        /// </summary>
        /// <param name="discord">Client that the cogs will have access to.</param>
        /// <param name="services">Service provider that cogs can use.</param>
        public CogManager(DiscordSocketClient discord, IServiceProvider services, IAzBotLogger logger)
        {
            Discord = discord;
            Services = services;
            Logger = logger;
            _assemblies = new ConcurrentDictionary<string, LoadedAssembly>();
            _unloaded = new ConcurrentDictionary<string, WeakReference<LoadedAssembly>>();
        }

        /// <summary>
        /// Get the names of the currently loaded assemblies.
        /// </summary>
        public IEnumerable<string> LoadedAssemblies => _assemblies.Keys;

        /// <summary>
        /// Get the names of the unloaded assemblies that have yet to be garbage collected.
        /// </summary>
        /// <remarks>
        /// Accessing this property will first cause the manager to check if
        /// any assemblies have been collected and remove them from this result.
        /// </remarks>
        public IEnumerable<string> UnloadingAssemblies
        {
            get
            {
                CleanupUnloaded();
                return _unloaded.Keys;
            }
        }

        /// <summary>
        /// Log event that fires when an exception occurs while loading or unloading an assembly.
        /// The event has the same signature as the log event from the discord client, however
        /// the task returned to this event is not awaited.
        /// </summary>
        public event Func<LogMessage, Task> Log;

        /// <summary>
        /// Fire the log event.
        /// </summary>
        /// <param name="message">Log message to send</param>
        internal void HandleLog(LogMessage message)
        {
            Log?.Invoke(message);
        }

        /// <summary>
        /// Check and see which unloaded modules have been garbage collected and remove
        /// them from the unloaded list.
        /// </summary>
        private void CleanupUnloaded()
        {
            // If the weak reference fails to get the target, then it has been garbage collected
            var collected = _unloaded.Keys.Where(key => !_unloaded[key].TryGetTarget(out _)).ToList();
            foreach (var key in collected)
            {
                _unloaded.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Get the path to the folder to search in when loading an assembly.
        /// Uses the value of <see cref="RelativeFolder"/> to get the folder path.
        /// If the value of the relative folder is null, then the
        /// current working directory is used.
        /// </summary>
        public string CogFolder
        {
            get
            {
                if (string.IsNullOrEmpty(RelativeFolder)) return Environment.CurrentDirectory;
                return Path.Combine(Environment.CurrentDirectory, RelativeFolder);
            }
        }

        /// <summary>
        /// Load an assembly and all the cogs that it contains.
        /// Cogs that cannot be instantiated are ignored, and cogs that throw an exception
        /// are logged with the <see cref="Log"/> event.
        /// </summary>
        /// <param name="name">Name of the assembly to load.</param>
        /// <returns><see cref="CogLoadResult"/> with info on how the cog was loaded.</returns>
        /// <seealso cref="CogFolder"/>
        public CogLoadResult LoadAssembly(string name)
        {
            // Fail fast if the assembly is already loaded or a previous version is still unloading
            if (_assemblies.ContainsKey(name)) return new CogLoadResult(CogLoadState.AlreadyLoaded);
            CleanupUnloaded();
            if (_unloaded.ContainsKey(name)) return new CogLoadResult(CogLoadState.PreviousStillUnloading);

            // Get the path of the assembly file in the cog folder
            var assemblyPath = Path.Combine(CogFolder, name + ".dll");
            if (!File.Exists(assemblyPath)) return new CogLoadResult(CogLoadState.FileNotFound);
            try
            {
                // Load the cogs and save the loader
                var loader = new LoadedAssembly(assemblyPath);
                loader.LoadCogs(this);
                _assemblies[name] = loader;
                return new CogLoadResult(loader.CogCount);
            }
            catch (Exception e)
            {
                return new CogLoadResult(CogLoadState.Exception, e);
            }
        }

        /// <summary>
        /// Unload an assembly and all contained cogs.
        /// </summary>
        /// <param name="name">Name of the assembly to unload.</param>
        /// <returns><see cref="CogUnloadState"/> with info on whether the cog was unloaded.</returns>
        public CogUnloadState UnloadAssembly(string name)
        {
            if (!_assemblies.TryRemove(name, out var loader)) return CogUnloadState.NotLoaded;
            loader.UnloadCogs(this);
            // Unload the assembly context itself
            loader.Unload();
            // Store the loader in a weak reference so that it is eligible for garbage
            // collection after this method, but we can still track when it is collected.
            _unloaded[name] = new WeakReference<LoadedAssembly>(loader, true);
            return CogUnloadState.Normal;
        }

        /// <summary>
        /// Execute the garbage collector in an attempt to fully remove an unloaded cog.
        /// </summary>
        public static void RunGc()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            // First call to GC executes the finalizer for objects
            // Second call should actually collect the objects
            GC.Collect();
        }
    }
}