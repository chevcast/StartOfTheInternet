using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Domain.Objects;
using Terminal.Domain.Enums;
using Terminal.Domain.Data.Entities;

namespace Terminal.Domain.Commands.Interfaces
{
    /// <summary>
    /// A command that is available for execution by the terminal core.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// The command result created by the terminal core.
        /// The command can modify and add to this result and return it to the terminal core.
        /// </summary>
        CommandResult CommandResult { get; set; }

        /// <summary>
        /// A list of commands available to the user.
        /// The terminal core should set this to the list of commands that are available to the user.
        /// </summary>
        IEnumerable<ICommand> AvailableCommands { get; set; }

        /// <summary>
        /// The roles that the command is allowed to run under.
        /// Note: You can use role templates to simplify what this property returns.
        /// Example: RoleTemplates.Visitors
        /// </summary>
        string[] Roles { get; }

        /// <summary>
        /// The name of the command.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Parameters that can be supplied in the order they should be supplied.
        /// </summary>
        string Parameters { get; }

        /// <summary>
        /// A brief description of the command. If ShowHelp is true then this is displayed in the help menu.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// True if the command should be shown in the help menu.
        /// Setting this to false can be useful for non-user commands such as INITIALIZE or for hidden commands.
        /// </summary>
        bool ShowHelp { get; }

        /// <summary>
        /// Executes the command's function and returns a command result.
        /// </summary>
        /// <param name="args">The arguments passed in from the terminal core.</param>
        /// <returns>A command result containing usable information by the UI.</returns>
        void Invoke(string[] args);
    }
}
