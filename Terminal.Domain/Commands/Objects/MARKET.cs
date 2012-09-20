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
    public class MARKET : ICommand
    {
        private IInviteCodeRepository _inviteCodeRepository;
        private IUserRepository _userRepository;

        public MARKET(IInviteCodeRepository inviteCodeRepository, IUserRepository userRepository)
        {
            _inviteCodeRepository = inviteCodeRepository;
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
            get { return "MARKET"; }
        }

        public string Parameters
        {
            get { return "[Option(s)]"; }
        }

        public string Description
        {
            get { return "Allows you to view and buy items on the market."; }
        }

        public bool ShowHelp
        {
            get { return true; }
        }

        public void Invoke(string[] args)
        {
            bool showHelp = false;
            string buy = null;

            var options = new OptionSet();
            options.Add(
                "?|help",
                "Show help information.",
                x => showHelp = x != null
            );
            options.Add(
                "buy=",
                "Enter the {number} of the item you wish to purchase.",
                x => buy = x
            );

            if (args == null)
            {
                this.CommandResult.WriteLine(DisplayMode.Inverted | DisplayMode.Bold, "Welcome to the marketplace!");
                this.CommandResult.WriteLine();
                this.CommandResult.WriteLine(DisplayMode.Bold, "The following items are available for purchase:");
                this.CommandResult.WriteLine();
                this.CommandResult.WriteLine("1) Invite Code (1000 Credits)");
            }
            else
            {
                try
                {
                    var parsedArgs = options.Parse(args).ToArray();

                    if (parsedArgs.Length == args.Length) // If no args matched mono options.
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
                        else if (buy != null)
                        {
                            if (buy.IsShort())
                            {
                                switch (buy.ToShort())
                                {
                                    case 1:
                                        if (this.CommandResult.CurrentUser.Credits >= 1000)
                                        {
                                            var random = new Random();
                                            string code = null;
                                            while (true)
                                            {
                                                code = random.Next(1 << 16).ToString("X4")
                                                    + random.Next(1 << 16).ToString("X4")
                                                    + random.Next(1 << 16).ToString("X4")
                                                    + random.Next(1 << 16).ToString("X4");
                                                if (_inviteCodeRepository.GetInviteCode(code) == null)
                                                    break;
                                            }
                                            var inviteCode = new InviteCode
                                            {
                                                Code = code,
                                                Username = this.CommandResult.CurrentUser.Username
                                            };
                                            _inviteCodeRepository.AddInviteCode(inviteCode);
                                            _inviteCodeRepository.SaveChanges();
                                            this.CommandResult.CurrentUser.Credits -= 1000;
                                            _userRepository.UpdateUser(this.CommandResult.CurrentUser);
                                            this.CommandResult.WriteLine("Invite purchased. 1000 credits subtracted from your account.");
                                        }
                                        else
                                            this.CommandResult.WriteLine("You do not have enough credits.");
                                        break;
                                    default:
                                        this.CommandResult.WriteLine("There is no item number {0} on the marketplace.", buy);
                                        break;
                                }
                            }
                            else
                                this.CommandResult.WriteLine("You must enter a valid item number.");
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
