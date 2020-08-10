using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AZEsportsDiscordBot.Framework;
using AZEsportsDiscordBot.Intrinsic.Commands;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace AZEsportsDiscordBot
{
    /// <summary>
    /// Runs everything related to the AZEsports discord bot, including the web portal socket.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Run the async startup and wait on it (forever)
            new Program().AsyncMain(args).GetAwaiter().GetResult();
        }

        /// <summary>Logger that sends messages to their correct place.</summary>
        private IAzBotLogger _logger;

        /// <summary>Core components required for everything to function.</summary>
        private List<ILoadable> _internalComponents;

        /// <summary>Collection of services that are used by core components and cogs.</summary>
        private IServiceCollection _services;

        /// <summary>
        /// Run the program startup but in an async context so that tasks can be awaited.
        /// </summary>
        /// <param name="args">Arguments passed to <see cref="Main"/></param>
        /// <returns>Indefinite task that holds the program while everything runs.</returns>
        private async Task AsyncMain(string[] args)
        {
            _internalComponents = new List<ILoadable>();
            _services = new ServiceCollection();
            
            // Start up the core components
            _logger = await LoadComponent<LogManager, IAzBotLogger>();
            // TODO await LoadComponent<DatabaseManager>();
            await LoadComponent<DiscordLoader, DiscordSocketClient>();
            await LoadComponent<CommandServiceLoader, CommandService>();
            await LoadComponent<CogManagerLoader, CogManager>();
            await LoadComponent<CommandHandler>();
            // TODO await LoadComponent<WebSocket>();
            await Log("Core components loaded.");

            // TODO load some cogs?

            // Delay indefinitely so the program keeps running
            await Task.Delay(-1);
        }

        /// <summary>
        /// Shutdown all core components and exit the program.
        /// </summary>
        /// <param name="code">Exit code</param>
        /// <returns>Task that waits on unloading components.</returns>
        private async Task FullShutdown(int code = 0)
        {
            await UnloadComponents();
            Environment.Exit(code);
        }

        /// <summary>
        /// Unloads core components in the reverse order that they were loaded in.
        /// </summary>
        /// <returns>Task waiting on unloading components.</returns>
        private async Task UnloadComponents()
        {
            for (var i = _internalComponents.Count - 1; i >= 0; i--)
            {
                await _internalComponents[i].Unload();
            }
            _internalComponents.Clear();
            _services.Clear();
        }

        /// <summary>
        /// Loads a core component.
        /// This method is called by one of the LoadComponent methods.
        /// The component is added to the list of core components and registered
        /// with the service collection.
        /// </summary>
        /// <param name="converter">Func which converts the loader into the loaded value.</param>
        /// <typeparam name="TLoader">Type which loads the component.</typeparam>
        /// <typeparam name="TLoad">Type that is loaded by the <see cref="TLoader"/>.</typeparam>
        /// <returns>Task that returns the loaded component.</returns>
        /// <remarks>If the loader throws an exception while loading a core component,
        /// it is fatal and the program will initiate a shutdown.</remarks>
        private async Task<TLoad> LoadComponentInternal<TLoader, TLoad>(Func<TLoader, TLoad> converter)
            where TLoader : ILoadable, new()
            where TLoad : class
        {
            await Log($"Loading core component: {typeof(TLoad).Name}");
            try
            {
                // Construct loader and load component
                var component = new TLoader();
                await component.Load(_services);
                _internalComponents.Add(component);
                var value = converter(component);
                _services.AddSingleton(typeof(TLoad), value);
                return value;
            }
            catch (Exception e)
            {
                // Core component failure causes shutdown
                var msg = new LogMessage(
                    LogSeverity.Error,
                    nameof(LoadComponent),
                    $"Error while loading core component ({typeof(TLoader).Name} -> {typeof(TLoad).Name}).",
                    e);
                await Log(msg.ToString(), Destination.All);
                await FullShutdown();
            }
            return null;
        }

        /// <summary>
        /// Load a core component. The component is added to the list of
        /// core components and registered with the service collection.
        /// </summary>
        /// <typeparam name="TLoader">Type which loads the component.</typeparam>
        /// <typeparam name="TLoad">Type that is loaded by the <see cref="TLoader"/></typeparam>
        /// <returns>Task that returns the loaded component.</returns>
        private Task<TLoad> LoadComponent<TLoader, TLoad>()
            where TLoader : ILoadable<TLoad>, new()
            where TLoad : class
        {
            return LoadComponentInternal<TLoader, TLoad>(loader => loader.Value);
        }

        /// <summary>
        /// Load a core component. The component is added to the list of
        /// core components and registered with the service collection.
        /// </summary>
        /// <typeparam name="TLoad">Type to load.</typeparam>
        /// <returns>Task that returns the loaded component.</returns>
        private Task<TLoad> LoadComponent<TLoad>()
            where TLoad : class, ILoadable, new()
        {
            return LoadComponentInternal<TLoad, TLoad>(loader => loader);
        }

        /// <summary>
        /// Log a message using the global logger or fallback to the console.
        /// </summary>
        /// <param name="content">Message content.</param>
        /// <param name="destination">Where to send the message.</param>
        /// <returns>Task from the logger.</returns>
        private async Task Log(string content, Destination destination = Destination.Console)
        {
            if (_logger == null)
            {
                Console.WriteLine(content);
                return;
            }
            await _logger.Log(content, destination);
        }
    }
}