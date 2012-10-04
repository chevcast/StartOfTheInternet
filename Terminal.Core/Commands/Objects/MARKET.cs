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
using Terminal.Core.Data;

namespace Terminal.Core.Commands.Objects
{
    public class MARKET : ICommand
    {
        private IDataBucket _dataBucket;

        public MARKET(IDataBucket dataBucket)
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
                CommandResult.WriteLine(DisplayMode.Inverted | DisplayMode.Bold, "Welcome to the marketplace!");
                CommandResult.WriteLine();
                CommandResult.WriteLine(DisplayMode.Bold, "The following items are available for purchase:");
                CommandResult.WriteLine();
                CommandResult.WriteLine("1) Invite Code (1000 Credits)");
            }
            else
            {
                try
                {
                    var parsedArgs = options.Parse(args).ToArray();

                    if (parsedArgs.Length == args.Length) // If no args matched mono options.
                    {
                        CommandResult.WriteLine(DisplayTemplates.InvalidArguments);
                    }
                    else
                    {
                        if (showHelp)
                        {
                            HelpUtility.WriteHelpInformation(this, options);
                        }
                        else if (buy != null)
                        {
                            if (buy.IsShort())
                            {
                                switch (buy.ToShort())
                                {
                                    case 1:
                                        if (CommandResult.CurrentUser.Credits >= 1000)
                                        {
                                            var random = new Random();
                                            string code = null;
                                            while (true)
                                            {
                                                code = random.Next(1 << 16).ToString("X4")
                                                    + random.Next(1 << 16).ToString("X4")
                                                    + random.Next(1 << 16).ToString("X4")
                                                    + random.Next(1 << 16).ToString("X4");
                                                if (_dataBucket.InviteCodeRepository.GetInviteCode(code) == null)
                                                    break;
                                            }
                                            var inviteCode = new InviteCode
                                            {
                                                Code = code,
                                                Username = CommandResult.CurrentUser.Username
                                            };
                                            _dataBucket.InviteCodeRepository.AddInviteCode(inviteCode);
                                            CommandResult.CurrentUser.Credits -= 1000;
                                            _dataBucket.UserRepository.UpdateUser(CommandResult.CurrentUser);
                                            _dataBucket.SaveChanges();
                                            CommandResult.WriteLine("Invite purchased. 1000 credits subtracted from your account.");
                                        }
                                        else
                                            CommandResult.WriteLine("You do not have enough credits.");
                                        break;
                                    default:
                                        CommandResult.WriteLine("There is no item number {0} on the marketplace.", buy);
                                        break;
                                }
                            }
                            else
                                CommandResult.WriteLine("You must enter a valid item number.");
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
