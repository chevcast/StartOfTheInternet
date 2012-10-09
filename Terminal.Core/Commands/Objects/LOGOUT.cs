using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Core.Commands.Interfaces;
using Terminal.Core.Objects;
using Terminal.Core.Data.Entities;
using Terminal.Core.Settings;
using Terminal.Core.ExtensionMethods;
using Mono.Options;
using System.IO;
using Terminal.Core.Enums;
using Terminal.Core.Utilities;
using Terminal.Core.Data;

namespace Terminal.Core.Commands.Objects
{
    public class LOGOUT : ICommand
    {
        private IDataBucket _dataBucket;

        public LOGOUT(IDataBucket dataBucket)
        {
            _dataBucket = dataBucket;
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
                x => HelpUtility.WriteHelpInformation(this, options)
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
                    CommandResult.CurrentUser.LastLogin = DateTime.UtcNow.AddMinutes(-10);
                    _dataBucket.UserRepository.UpdateUser(CommandResult.CurrentUser);
                    _dataBucket.SaveChanges();
                    CommandResult.WriteLine("You have been logged out.");
                    CommandResult.CommandContext.Restore();
                    CommandResult.CurrentUser = null;
                }
            }
        }
    }
}
