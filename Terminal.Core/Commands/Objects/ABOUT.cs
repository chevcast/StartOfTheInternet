using Mono.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Core.Commands.Interfaces;
using Terminal.Core.Enums;
using Terminal.Core.Settings;
using Terminal.Core.Utilities;

namespace Terminal.Core.Commands.Objects
{
    public class ABOUT : ICommand
    {
        public Core.Objects.CommandResult CommandResult { get; set; }

        public IEnumerable<ICommand> AvailableCommands { get; set; }

        public string[] Roles
        {
            get { return RoleTemplates.Everyone; }
        }

        public string Name
        {
            get { return "ABOUT"; }
        }

        public string Parameters
        {
            get { return "[Option(s)]"; }
        }

        public string Description
        {
            get { return "Display information about the SOTI terminal project."; }
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
                x => HelpUtility.WriteHelpInformation(this, options)
            );

            if (args != null)
            {
                try
                {
                    options.Parse(args);
                }
                catch (OptionException ex)
                {
                    CommandResult.WriteLine(ex.ToString());
                }
            }
            else
            {
                CommandResult.WriteLine("The SOTI project is an open-source web terminal project which is maintained on Github.");
                CommandResult.WriteLine(DisplayMode.Parse | DisplayMode.DontType, "[url]https://github.com/Chevex/StartOfTheInternet[/url]");
                CommandResult.WriteLine();
                CommandResult.WriteLine("For more information about the project, how to get involved, or how to compile your own version of SOTI on your own computer visit the SOTI wiki.");
                CommandResult.WriteLine(DisplayMode.Parse | DisplayMode.DontType, "[url]https://github.com/Chevex/StartOfTheInternet/wiki[/url]");
            }
        }
    }
}
