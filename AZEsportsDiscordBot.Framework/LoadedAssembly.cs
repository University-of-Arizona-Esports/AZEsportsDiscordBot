using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Discord;

namespace AZEsportsDiscordBot.Framework
{
    /// <summary>
    /// Handles the loading and unloading of cogs from a single assembly.
    /// </summary>
    internal class LoadedAssembly : AssemblyLoadContext
    {
        /// <summary>
        /// Full path to the assembly file to load.
        /// </summary>
        public readonly string Path;

        /// <summary>Handles looking up dependencies in the loaded assembly.</summary>
        private AssemblyDependencyResolver _resolver;

        /// <summary>Cogs currently loaded from the assembly</summary>
        private List<Cog> _loadedCogs;

        /// <summary>
        /// Create a loader for an assembly file.
        /// </summary>
        /// <param name="path">Full path to the assembly file to load.</param>
        public LoadedAssembly(string path) : base(true)
        {
            // base(true) allows this context to be unloaded
            Path = path;
            _resolver = new AssemblyDependencyResolver(path);
        }

        // This method is called when the context needs to lookup
        // an assembly, such as a dependency in the assembly.
        protected override Assembly Load(AssemblyName assemblyName)
        {
            var path = _resolver.ResolveAssemblyToPath(assemblyName);
            return path != null ? LoadFromAssemblyPath(path) : null;
        }

        /// <summary>Value indicating whether this loader has loaded the assembly.</summary>
        public bool IsLoaded => _loadedCogs != null;

        /// <summary>Number of cogs that have been loaded from the assembly, or else -1.</summary>
        public int CogCount => IsLoaded ? _loadedCogs.Count : -1;

        /// <summary>
        /// Attempt to load a cog from an object type.
        /// If the type is not correct to construct a cog, it is ignored.
        /// If an exception is thrown while constructing or executing the <see cref="Cog.OnCogLoad"/>
        /// method, then a log message will be sent to the <see cref="CogManager"/>.
        /// </summary>
        /// <param name="type">Type to load.</param>
        /// <param name="manager">Manager to set for the cog and to handle logging.</param>
        private void TryLoadCog(Type type, CogManager manager)
        {
            // Type must be a class that inherits from Cog and not abstract.
            if (!type.IsSubclassOf(typeof(Cog)) || !type.IsClass || type.IsAbstract) return;
            // Must have a no-parameter constructor. 
            var constructor = type.GetConstructor(new Type[0]);
            if (constructor == null) return;
            try
            {
                // Construct the cog and load it
                var cog = (Cog) constructor.Invoke(null);
                cog.CogManager = manager;
                cog.OnCogLoad();
                _loadedCogs.Add(cog);
            }
            catch (Exception e)
            {
                var log = new LogMessage(LogSeverity.Warning, nameof(CogManager), $"Exception occured while loading Cog ({type.Name}).", e);
                manager.HandleLog(log);
            }
        }

        /// <summary>
        /// Load cogs from the assembly.
        /// </summary>
        /// <param name="manager">Manager to set in cogs and to handle logging.</param>
        public void LoadCogs(CogManager manager)
        {
            if (IsLoaded) return;
            _loadedCogs = new List<Cog>();
            // Load assembly file and load any valid Cog type.
            var assembly = LoadFromAssemblyPath(Path);
            foreach (var type in assembly.GetTypes())
            {
                TryLoadCog(type, manager);
            }
        }

        private void UnsafeAction(CogManager manager, string source, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                var log = new LogMessage(LogSeverity.Warning,
                    nameof(CogManager),
                    $"Exception occured while unloading Cog ({source})",
                    e);
                manager.HandleLog(log);
            }
        }

        /// <summary>
        /// Unload cogs in the assembly.
        /// </summary>
        /// <param name="manager">Manager to handle logging.</param>
        public void UnloadCogs(CogManager manager)
        {
            if (!IsLoaded) return;
            foreach (var cog in _loadedCogs)
            {
                UnsafeAction(manager, cog.GetType().Name, () => cog.OnCogPreUnload());
                Task.Run(async () =>
                {
                    await cog.AllTasksComplete();
                    UnsafeAction(manager, cog.GetType().Name, () => cog.OnCogUnload());
                    cog.CogManager = null;
                });
            }
            // Remove references to assembly types.
            _loadedCogs.Clear();
            _loadedCogs = null;
        }
    }
}