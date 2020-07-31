using System;

namespace AZEsportsDiscordBot.Framework
{
    /// <summary>
    /// Result with info on the state of a loaded cog.
    /// </summary>
    public readonly struct CogLoadResult
    {
        /// <summary>
        /// True if the cog was loaded, false otherwise.
        /// Check the <see cref="State"/> field for a specific reason.
        /// </summary>
        public readonly bool Success;

        /// <summary>The number of cogs that were actually loaded from the assembly.
        /// or else -1.</summary>
        public readonly int CogsLoaded;

        /// <summary>The specific reason for this load state.</summary>
        public readonly CogLoadState State;

        /// <summary>The exception that was thrown when the load state is <see cref="CogLoadState.Exception"/></summary>
        public readonly Exception Exception;

        /// <summary>
        /// Create a success cog load result.
        /// </summary>
        /// <param name="cogsLoaded">The number of cogs that were loaded from the assembly.</param>
        public CogLoadResult(int cogsLoaded)
        {
            Success = true;
            CogsLoaded = cogsLoaded;
            State = CogLoadState.Normal;
            Exception = null;
        }

        /// <summary>
        /// Create a failure cog load result.
        /// </summary>
        /// <param name="state">The specific reason the cog could not be loaded.</param>
        /// <param name="exception">The exception that caused the failure, if any</param>
        public CogLoadResult(CogLoadState state, Exception exception = null)
        {
            Success = false;
            CogsLoaded = -1;
            State = state;
            Exception = exception;
        }
    }

    /// <summary>
    /// A reason that a cog was loaded.
    /// </summary>
    public enum CogLoadState
    {
        /// <summary>Cog was loaded normally</summary>
        Normal,

        /// <summary>Cog is already loaded.</summary>
        AlreadyLoaded,

        /// <summary>Previous version of the assembly has not been garbage collected yet.</summary>
        PreviousStillUnloading,

        /// <summary>The file for the specified assembly was not found.</summary>
        FileNotFound,

        /// <summary>An exception was thrown during cog loading.</summary>
        Exception,
    }

    /// <summary>
    /// Indicates the state of unloading a cog.
    /// </summary>
    public enum CogUnloadState
    {
        /// <summary>Cog was unloaded normally.</summary>
        Normal,

        /// <summary>Was was not unloaded because the specified assembly is not loaded.</summary>
        NotLoaded,
    }
}