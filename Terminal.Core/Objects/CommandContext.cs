using System;
using System.Collections.Generic;
using Terminal.Core.Enums;

namespace Terminal.Core.Objects
{
    /// <summary>
    /// The command context describes the current state of the terminal.
    /// It helps the terminal core make decisions about how to execute commands.
    /// </summary>
    [Serializable]
    public class CommandContext
    {
        /// <summary>
        /// The currently contexted command.
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// The currently contexted arguments.
        /// </summary>
        public string[] Args { get; set; }

        /// <summary>
        /// The console text to display for the current context.
        /// </summary>
        public string Text { get; set; }

        public bool Prompt { get; set; }

        /// <summary>
        /// Custom string array for storing multi-word values from prompts.
        /// </summary>
        public string[] PromptData { get; set; }

        /// <summary>
        /// The currently displayed page.
        /// </summary>
        public int CurrentPage { get; set; }

        public List<string> CurrentLinkTags { get; set; }

        public List<string> CurrentSearchTerms { get; set; }

        public string CurrentSortOrder { get; set; }

        /// <summary>
        /// The current status of the context.
        /// 
        /// Options:
        /// 
        /// Disabled - The context is disabled.
        /// Passive - The context contains data but normal command execution should be attempted first.
        /// Forced - The context has data and must be used for command execution.
        /// </summary>
        public ContextStatus Status { get; set; }

        /// <summary>
        /// Stores the previous command context when the Backup method is called.
        /// </summary>
        public CommandContext PreviousContext { get; set; }
    }
}
