using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace AZEsportsDiscordBot.Framework
{
    /// <summary>
    /// Represents a loadable component of the framework.
    /// </summary>
    public interface ILoadable
    { 
        /// <summary>
        /// Activate this component.
        /// </summary>
        /// <param name="services">Core services.</param>
        /// <returns>Task that completes after the component is loaded.</returns>
        Task Load(IServiceCollection services);

        /// <summary>
        /// Deactivate this component. Features of this component should
        /// no longer be used after unloading.
        /// </summary>
        /// <returns>Task that completes after the component is unloaded.</returns>
        Task Unload();
    }

    /// <summary>
    /// An <see cref="ILoadable"/> that specifies the type it loads.
    /// </summary>
    /// <typeparam name="T">Type that is loaded by this loader.</typeparam>
    public interface ILoadable<out T> : ILoadable
    {
        /// <summary>
        /// Get the loaded value of this component.
        /// </summary>
        T Value { get; }
    }
}