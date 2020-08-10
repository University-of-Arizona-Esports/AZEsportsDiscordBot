using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace AZEsportsDiscordBot.Framework
{
    /// <summary>
    /// A Cog is a modular component that is used to ease the loading of various components
    /// in the discord bot.
    /// </summary>
    public abstract class Cog
    {
        // Dictionary is used to emulate a HashSet as C# has no ConcurrentHashSet
        private ConcurrentDictionary<Task, byte> _tasks = new ConcurrentDictionary<Task, byte>();

        // True if this cog is no longer accepting more work
        private bool _unloading;

        /// <summary>Bool indicating if this cog is accepting work</summary>
        internal bool Unloading
        {
            get => Volatile.Read(ref _unloading);
            set => Volatile.Write(ref _unloading, value);
        }

        /// <summary>
        /// Returns a Task that completes when this cog is finished
        /// performing work and is ready to be unloaded.
        /// </summary>
        /// <returns>Task of all remaining work for this cog.</returns>
        internal Task AllTasksComplete()
        {
            return Task.WhenAll(_tasks.Keys);
        }

        /// <summary>
        /// Wraps an action so that the cog will not be unloaded while
        /// the task is executing.
        /// If the cog has been unloaded, this method returns immediately
        /// without executing the action.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        protected void TaskWrapper(Action action)
        {
            if (Unloading) return;
            var task = new Task(action);
            _tasks.TryAdd(task, 0);
            task.RunSynchronously();
            _tasks.TryRemove(task, out _);
        }

        /// <summary>
        /// Wrap a task so that the cog will not be unloaded while
        /// the task is executing.
        /// If the cog has been unloaded, this method returns a completed
        /// task immediately without executing the function.
        /// </summary>
        /// <param name="func">Function to execute.</param>
        /// <param name="startTask">If the task's <see cref="Task.Start()"/> method should be called.</param>
        /// <returns>Task from the function.</returns>
        protected Task TaskWrapper(Func<Task> func, bool startTask = false)
        {
            if (Unloading) return Task.CompletedTask;
            var task = func();
            _tasks.TryAdd(task, 0);
            if (startTask) task.Start();
            Task.Run(async () =>
            {
                await task;
                _tasks.TryRemove(task, out _);
            });
            return task;
        }

        /// <summary>
        /// Access the <see cref="CogManager"/> that this cog is loaded into, if any.
        /// </summary>
        public CogManager CogManager { get; internal set; }

        /// <summary>
        /// Convenient access to the discord client instance.
        /// </summary>
        protected DiscordSocketClient Discord => CogManager.Discord;

        /// <summary>
        /// Convenient access to the manager's service provider.
        /// </summary>
        protected IServiceProvider Services => CogManager.Services;

        /// <summary>
        /// Called when this Cog is loaded by a <see cref="CogManager"/>.
        /// This is where any setup for the cog should occur.
        /// </summary>
        public virtual void OnCogLoad() { }
        
        /// <summary>
        /// Called when an unload for this cog has been requested.
        /// Basic unloading should be done here so that the cog does not
        /// receive any more work to do.
        /// </summary>
        public virtual void OnCogPreUnload() { }

        /// <summary>
        /// Called when this Cog should be fully unloaded. By the end of this method
        /// the Cog should have completely cleaned up any resources it needs
        /// to, and unregistered events and listeners attached to discord.
        /// </summary>
        public virtual void OnCogUnload() { }
    }
}