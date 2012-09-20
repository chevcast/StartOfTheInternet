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
    public class LOGOUT : ICommand
    {
        private IUserRepository _userRepository;

        public LOGOUT(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public CommandResult CommandResult { get; set; }

        public IEnumerable<ICommand> AvailableCommands { get; set; }

        public string[] Roles
        {
            get { return RoleTemplates.AllLoggedIn; }
        }

        public string Name
        {
            get { return "LOGOUT"; }
        }

        public string Parameters
        {
            get { return "[Option(s)]"; }
        }

        public string Description
        {
            get { return "Logout of your account."; }
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
                    this.CommandResult.CurrentUser.LastLogin = DateTime.UtcNow.AddMinutes(-10);
                    _userRepository.UpdateUser(this.CommandResult.CurrentUser);
                    this.CommandResult.WriteLine("You have been logged out.");
                    this.CommandResult.CommandContext.Deactivate();
                    this.CommandResult.CurrentUser = null;
                }
            }
        }
    }
}
