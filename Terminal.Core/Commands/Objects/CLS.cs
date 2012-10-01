using Mono.Options;
using System.Collections.Generic;
using Terminal.Core.Commands.Interfaces;
using Terminal.Core.Objects;
using Terminal.Core.Settings;
using Terminal.Core.Utilities;

namespace Terminal.Core.Commands.Objects
{
    public class CLS : ICommand
    {
        public CommandResult CommandResult { get; set; }

        public IEnumerable<ICommand> AvailableCommands { get; set; }

        public string[] Roles
        {
            get { return RoleTemplates.Everyone; }
        }

        public string Name
        {
            get { return "CLS"; }
        }

        public string Parameters
        {
            get { return "[Option(s)]"; }
        }

        public string Description
        {
            get { return "Clears the screen."; }
        }

        public bool ShowHelp
        {
            get { return true; }
        }

        public void Invoke(string[] args)
        {
            var options = new OptionSet();
            options.Add(
                "?|help",
                "Show help information.",
                x =>
                {
                    HelpUtility.WriteHelpInformation(
                        CommandResult,
                        Name,
                        Parameters,
                        Description,
                        options
                    );
                }
            );

            if (args == null)
            {
                CommandResult.ClearScreen = true;
                CommandResult.CommandContext.Deactivate();
            }
            else
                try
                {
                    options.Parse(args);
                }
                catch (OptionException ex)
                {
                    CommandResult.WriteLine(ex.Message);
                }
        }
    }
}