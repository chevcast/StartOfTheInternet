using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Core.Enums;
using Terminal.Core.Objects;
using Terminal.Core.Commands.Interfaces;
using Terminal.Core.Data.Entities;
using Terminal.Core.Settings;
using System.IO;
using Mono.Options;
using Terminal.Core.Utilities;

namespace Terminal.Core.Commands.Objects
{
    public class INITIALIZE : ICommand
    {
        public CommandResult CommandResult { get; set; }

        public IEnumerable<ICommand> AvailableCommands { get; set; }

        public string[] Roles
        {
            get { return RoleTemplates.Everyone; }
        }

        public string Name
        {
            get { return "INITIALIZE"; }
        }

        public string Parameters
        {
            get { return "[Option(s)]"; }
        }

        public string Description
        {
            get { return "Displays startup screen."; }
        }

        public bool ShowHelp
        {
            get { return false; }
        }

        public void Invoke(string[] args)
        {
            var options = new OptionSet();
            options.Add(
                "?|help",
                "Show help information.",
                x => HelpUtility.WriteHelpInformation(this, options)
            );

            if (args == null)
            {
                if (CommandResult.CurrentUser == null)
                {
                    CommandResult.DeactivateContext();
                    //var lines = AppSettings.Logo.Split('\n');
                    //foreach (var line in lines)
                    //{
                    //    CommandResult.WriteLine(DisplayMode.DontWrap, line);
                    //}
                    //CommandResult.WriteLine();
                    CommandResult.WriteLine(DisplayMode.Inverted, "OMEGA LOCKDOWN ACTIVE");
                    CommandResult.WriteLine();
                    CommandResult.WriteLine("As per article 21 subsection D of the Omega Directive,");
                    CommandResult.WriteLine("only authorized agents are allowed beyond this point.");
                }
                else
                    CommandResult.WriteLine("You are currently logged in as {0}.", CommandResult.CurrentUser.Username);
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
