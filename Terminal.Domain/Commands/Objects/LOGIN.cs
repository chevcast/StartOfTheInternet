using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Domain.Commands.Interfaces;
using Terminal.Domain.Objects;
using Terminal.Domain.Entities;
using Terminal.Domain.Settings;
using Terminal.Domain.ExtensionMethods;
using Mono.Options;
using System.IO;
using Terminal.Domain.Enums;
using Terminal.Domain.Repositories.Interfaces;
using Terminal.Domain.Utilities;

namespace Terminal.Domain.Commands.Objects
{
    public class LOGIN : ICommand
    {
        private IUserRepository _userRepository;

        public LOGIN(IUserRepository userRepository)
        {
            _userRepository = userRepository;
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
                        this.CommandResult,
                        this.Name,
                        this.Parameters,
                        this.Description,
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
                    this.CommandResult.WriteLine(ex.Message);
                }
            }

            if (!matchFound)
            {
                if (args.IsNullOrEmpty())
                {
                    this.CommandResult.WriteLine("Enter your username.");
                    this.CommandResult.CommandContext.Set(ContextStatus.Forced, this.Name, args, "Username");
                }
                else if (args.Length == 1)
                {
                    this.CommandResult.WriteLine("Enter your password.");
                    this.CommandResult.PasswordField = true;
                    this.CommandResult.CommandContext.Set(ContextStatus.Forced, this.Name, args, "Password");
                }
                else if (args.Length == 2)
                {
                    var user = this._userRepository.GetUser(args[0]);
                    if (user != null && args[1] == user.Password)
                    {
                        this.CommandResult.CurrentUser = user;
                        this.CommandResult.WriteLine("You are now logged in as {0}.", this.CommandResult.CurrentUser.Username);
                    }
                    else
                        this.CommandResult.WriteLine("Invalid username or password.");
                    this.CommandResult.CommandContext.Deactivate();
                }
            }
        }
    }
}
