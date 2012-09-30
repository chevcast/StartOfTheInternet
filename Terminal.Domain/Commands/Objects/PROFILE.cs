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
using Terminal.Core.ExtensionMethods;
using Terminal.Core.Utilities;

namespace Terminal.Core.Commands.Objects
{
    public class PROFILE : ICommand
    {
        public CommandResult CommandResult { get; set; }

        public IEnumerable<ICommand> AvailableCommands { get; set; }

        public string[] Roles
        {
            get { return RoleTemplates.AllLoggedIn; }
        }

        public string Name
        {
            get { return "PROFILE"; }
        }

        public string Parameters
        {
            get { return "<Options>"; }
        }

        public string Description
        {
            get { return "Allows you to access and modify your profile."; }
        }

        public bool ShowHelp
        {
            get { return true; }
        }

        public void Invoke(string[] args)
        {
            bool showHelp = false;
            bool showInvites = false;
            bool showCredits = false;

            var options = new OptionSet();
            options.Add(
                "?|help",
                "Show help information.",
                x => showHelp = x != null
            );
            options.Add(
                "invites",
                "See the invites you have available to give out.",
                x => showInvites = x != null
            );
            options.Add(
                "credits",
                "View your current credit balance.",
                x => showCredits = x != null
            );

            if (args == null)
            {
                CommandResult.WriteLine(DisplayTemplates.InvalidArguments);
            }
            else
            {
                try
                {
                    var parsedArgs = options.Parse(args).ToArray();

                    if (parsedArgs.Length == args.Length)
                    {
                        CommandResult.WriteLine(DisplayTemplates.InvalidArguments);
                    }
                    else
                    {
                        if (showHelp)
                        {
                            HelpUtility.WriteHelpInformation(
                                CommandResult,
                                Name,
                                Parameters,
                                Description,
                                options
                            );
                        }
                        else if (showInvites)
                        {
                            CommandResult.ClearScreen = true;
                            var inviteCodes = CommandResult.CurrentUser.InviteCodes;
                            foreach (var inviteCode in inviteCodes)
                                CommandResult.WriteLine(DisplayMode.DontType, inviteCode.Code);
                            CommandResult.WriteLine();
                            CommandResult.WriteLine("You have {0} invite codes available", inviteCodes.Count);
                            CommandResult.WriteLine();
                        }
                        else if (showCredits)
                        {
                            CommandResult.WriteLine("Total credits available: {0}", CommandResult.CurrentUser.Credits);
                            CommandResult.WriteLine();
                        }
                    }
                }
                catch (OptionException ex)
                {
                    CommandResult.WriteLine(ex.Message);
                }
            }
        }
    }
}
