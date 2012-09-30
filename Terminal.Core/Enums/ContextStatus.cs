namespace Terminal.Core.Enums
{
    /// <summary>
    /// Represents the available options for a command context object.
    /// </summary>
    public enum ContextStatus
    {
        /// <summary>
        /// No context is currently active.
        /// </summary>
        Disabled,

        /// <summary>
        /// A context is active but existing commands take priority.
        /// </summary>
        Passive,

        /// <summary>
        /// A context is active and all data returned fromt he client (except for the cancel command)
        /// will be passed to the active command method.
        /// </summary>
        Forced,
    }
}
