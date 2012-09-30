using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Domain.Commands.Interfaces;
using Terminal.Domain.Objects;
using Terminal.Domain.Data.Entities;
using Terminal.Domain.Settings;
using Terminal.Domain.ExtensionMethods;
using Mono.Options;
using System.IO;
using Terminal.Domain.Enums;
using Terminal.Domain.Utilities;
using Terminal.Domain.Data;

namespace Terminal.Domain.Commands.Objects
{
    public class LOGIN : ICommand
    {
        private IDataBucket _dataBucket;

        public LOGIN(IDataBucket dataBucket)
        {
            _dataBucket = dataBucket;
        }

        public CommandResult CommandResult { get; set; }

        public IEnumerable<ICommand> AvailableCommands { get; set; }

        public string[] Roles
        {
            get { return RoleTemplates.Visitor; }
        }

        public string Name
        {
            get { return "LOGIN"; }
        }

        public string Parameters
        {
            get { return "[Option(s)]"; }
        }

        public string Description
        {
            get { return "Login with your username and password."; }
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

            bool matchFound = false;

            if (args != null)
            {
                try
                {
                    var extra = options.Parse(args);
                    matchFound = args.Length != extra.Count;
                }
                catch (OptionException ex)
                {
                    CommandResult.WriteLine(ex.Message);
                }
            }

            if (!matchFound)
            {
                if (args.IsNullOrEmpty())
                {
                    CommandResult.WriteLine("Enter your username.");
                    CommandResult.CommandContext.Set(ContextStatus.Forced, Name, args, "Username");
                }
                else if (args.Length == 1)
                {
                    CommandResult.WriteLine("Enter your password.");
                    CommandResult.PasswordField = true;
                    CommandResult.CommandContext.Set(ContextStatus.Forced, Name, args, "Password");
                }
                else if (args.Length == 2)
                {
                    var user = _dataBucket.UserRepository.GetUser(args[0]);
                    if (user != null && args[1] == user.Password)
                    {
                        CommandResult.CurrentUser = user;
                        CommandResult.WriteLine("You are now logged in as {0}.", CommandResult.CurrentUser.Username);
                    }
                    else
                        CommandResult.WriteLine("Invalid username or password.");
                    CommandResult.CommandContext.Deactivate();
                }
            }
        }
    }
}
