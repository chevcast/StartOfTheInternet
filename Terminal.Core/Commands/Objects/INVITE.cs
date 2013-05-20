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
    public class INVITE : ICommand
    {
        private IDataBucket _dataBucket;

        public INVITE(IDataBucket dataBucket)
        {
            _dataBucket = dataBucket;
        }

        public CommandResult CommandResult { get; set; }

        public IEnumerable<ICommand> AvailableCommands { get; set; }

        public string[] Roles
        {
            get { return RoleTemplates.Administrators; }
        }

        public string Name
        {
            get { return "INVITE"; }
        }

        public string Parameters
        {
            get { return "[Option(s)]"; }
        }

        public string Description
        {
            get { return "Generate invites for new users."; }
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
                    CommandResult.WriteLine("You must supply a username.");
                }
                else if (args.Length == 1)
                {
                    var username = args[0];
                    if (!_dataBucket.UserRepository.CheckUserExists(username))
                    {
                        var random = new Random();
                        string code = random.Next(1 << 16).ToString("X4")
                            + random.Next(1 << 16).ToString("X4")
                            + random.Next(1 << 16).ToString("X4")
                            + random.Next(1 << 16).ToString("X4");
                        var inviteCode = new InviteCode
                        {
                            Code = code,
                            Username = username
                        };
                        _dataBucket.InviteCodeRepository.AddInviteCode(inviteCode);
                        _dataBucket.SaveChanges();
                        CommandResult.WriteLine("Invite Code: {0}", code);
                    }
                    else
                        CommandResult.WriteLine("That username already exists.");
                }
            }
        }
    }
}
