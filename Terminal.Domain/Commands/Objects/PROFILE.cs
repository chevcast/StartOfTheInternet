using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Domain.Enums;
using Terminal.Domain.Objects;
using Terminal.Domain.Commands.Interfaces;
using Terminal.Domain.Entities;
using Terminal.Domain.Settings;
using System.IO;
using Mono.Options;
using Terminal.Domain.Repositories.Interfaces;
using Terminal.Domain.ExtensionMethods;
using Terminal.Domain.Utilities;

namespace Terminal.Domain.Commands.Objects
{
    public class PROFILE : ICommand
    {
        public PROFILE()
        {
            
        }

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
                this.CommandResult.WriteLine(DisplayTemplates.InvalidArguments);
            }
            else
            {
                try
                {
                    var parsedArgs = options.Parse(args).ToArray();

                    if (parsedArgs.Length == args.Length)
                    {
                        this.CommandResult.WriteLine(DisplayTemplates.InvalidArguments);
                    }
                    else
                    {
                        if (showHelp)
                        {
                            HelpUtility.WriteHelpInformation(
                                this.CommandResult,
                                this.Name,
                                this.Parameters,
                                this.Description,
                                options
                            );
                        }
                        else if (showInvites)
                        {
                            this.CommandResult.ClearScreen = true;
                            var inviteCodes = this.CommandResult.CurrentUser.InviteCodes;
                            foreach (var inviteCode in inviteCodes)
                                this.CommandResult.WriteLine(DisplayMode.DontType, inviteCode.Code);
                            this.CommandResult.WriteLine();
                            this.CommandResult.WriteLine("You have {0} invite codes available", inviteCodes.Count);
                            this.CommandResult.WriteLine();
                        }
                        else if (showCredits)
                        {
                            this.CommandResult.WriteLine("Total credits available: {0}", this.CommandResult.CurrentUser.Credits);
                            this.CommandResult.WriteLine();
                        }
                    }
                }
                catch (OptionException ex)
                {
                    this.CommandResult.WriteLine(ex.Message);
                }
            }
        }
    }
}
