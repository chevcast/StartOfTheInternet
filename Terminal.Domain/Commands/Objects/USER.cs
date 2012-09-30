using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Domain.Enums;
using Terminal.Domain.Objects;
using Terminal.Domain.Commands.Interfaces;
using Terminal.Domain.Data.Entities;
using Terminal.Domain.Settings;
using System.IO;
using Mono.Options;
using Terminal.Domain.ExtensionMethods;
using Terminal.Domain.Utilities;
using System.ServiceProcess;
using System.Threading;
using System.Diagnostics;
using Terminal.Domain.Data;

namespace Terminal.Domain.Commands.Objects
{
    public class USER : ICommand
    {
        private IDataBucket _dataBucket;

        public USER(IDataBucket dataBucket)
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
            get { return "USER"; }
        }

        public string Parameters
        {
            get { return "<Username> [Option(s)]"; }
        }

        public string Description
        {
            get { return "Display the specified user's profile."; }
        }

        public bool ShowHelp
        {
            get { return true; }
        }

        public void Invoke(string[] args)
        {
            bool showHelp = false;
            bool? ignore = null;
            bool warn = false;
            bool? ban = null;
            bool history = false;
            string addRole = null;
            string removeRole = null;
            string giveCredits = null;
            string giveInvites = null;

            var options = new OptionSet();
            options.Add(
                "?|help",
                "Show help information.",
                x => showHelp = x != null
            );
            options.Add(
                "i|ignore",
                "Ignore the specified user.",
                x => ignore = x != null
            );
            if (CommandResult.CurrentUser.IsModerator || CommandResult.CurrentUser.IsAdministrator)
            {
                options.Add(
                    "h|history",
                    "Show warning & ban history for the user.",
                    x => history = x != null
                );
                options.Add(
                    "w|warn",
                    "Warn the user about an offense.",
                    x => warn = x != null
                );
                options.Add(
                    "b|ban",
                    "Ban the user for a specified amount of time.",
                    x => ban = x != null
                );
            }
            if (CommandResult.CurrentUser.IsAdministrator)
            {
                options.Add(
                    "addRole=",
                    "Add the specified role to the user.",
                    x => addRole = x
                );
                options.Add(
                    "removeRole=",
                    "Remove the specified role from the user.",
                    x => removeRole = x
                );
                options.Add(
                    "gc|giveCredits=",
                    "Give the user some {amount} of credits.",
                    x => giveCredits = x
                );
                options.Add(
                    "gi|giveInvites=",
                    "Give the user some {amount} of invites.",
                    x => giveInvites = x
                );
            }

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
                        if (parsedArgs.Length == 1)
                        {
                            var user = _dataBucket.UserRepository.GetUser(parsedArgs[0]);
                            if (user != null)
                            {
                                // display user profile.
                            }
                            else
                                CommandResult.WriteLine("There is no user with the username '{0}'.", parsedArgs[0]);
                        }
                        else
                            CommandResult.WriteLine("You must specify a username.");
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
                        else if (warn)
                        {
                            if (parsedArgs.Length == 1)
                            {
                                var user = _dataBucket.UserRepository.GetUser(parsedArgs[0]);
                                if (user != null)
                                {
                                    if ((!user.IsModerator && !user.IsAdministrator) || CommandResult.CurrentUser.IsAdministrator)
                                    {
                                        if (CommandResult.CommandContext.PromptData == null)
                                        {
                                            CommandResult.WriteLine("Type the details of your warning to '{0}'.", user.Username);
                                            CommandResult.CommandContext.SetPrompt(Name, args, string.Format("{0} WARNING", user.Username));
                                        }
                                        else if (CommandResult.CommandContext.PromptData.Length == 1)
                                        {
                                            user.UserActivityLog.Add(new UserActivityLogItem
                                            {
                                                Type = "Warning",
                                                Date = DateTime.UtcNow,
                                                Information = string.Format(
                                                    "Warning given by '{0}'\n\nDetails: {1}",
                                                    CommandResult.CurrentUser.Username,
                                                    CommandResult.CommandContext.PromptData[0]
                                                )
                                            });
                                            user.ReceivedMessages.Add(new Message
                                            {
                                                Sender = CommandResult.CurrentUser.Username,
                                                SentDate = DateTime.UtcNow,
                                                Subject = string.Format(
                                                    "{0} has given you a warning.",
                                                    CommandResult.CurrentUser.Username
                                                ),
                                                Body = string.Format(
                                                    "You have been given a warning by '{0}'.\n\n{1}",
                                                    CommandResult.CurrentUser.Username,
                                                    CommandResult.CommandContext.PromptData[0]
                                                )
                                            });
                                            _dataBucket.UserRepository.UpdateUser(user);
                                            _dataBucket.SaveChanges();
                                            CommandResult.CommandContext.Restore();
                                            CommandResult.WriteLine("Warning successfully issued to '{0}'.", user.Username);
                                        }
                                    }
                                    else
                                        CommandResult.WriteLine("You are not authorized to give warnings to user '{0}'.", user.Username);
                                }
                                else
                                    CommandResult.WriteLine("There is no user with the username '{0}'.", parsedArgs[0]);
                            }
                            else
                                CommandResult.WriteLine("You must specify a username.");
                        }
                        else if (ban != null)
                        {
                            if (parsedArgs.Length == 1)
                            {
                                var user = _dataBucket.UserRepository.GetUser(parsedArgs[0]);
                                if (user != null)
                                {
                                    if ((!user.IsModerator && !user.IsAdministrator) || CommandResult.CurrentUser.IsAdministrator)
                                    {
                                        if ((bool)ban)
                                        {
                                            if (user.BanInfo == null)
                                            {
                                                if (CommandResult.CommandContext.PromptData == null)
                                                {
                                                    CommandResult.WriteLine("How severe should the ban be?");
                                                    CommandResult.WriteLine();
                                                    CommandResult.WriteLine("1 = One Hour");
                                                    CommandResult.WriteLine("2 = Three Hours");
                                                    CommandResult.WriteLine("3 = One Day");
                                                    CommandResult.WriteLine("4 = One Week");
                                                    CommandResult.WriteLine("5 = One Month");
                                                    CommandResult.WriteLine("6 = One Year");
                                                    CommandResult.WriteLine("7 = 42 Years");
                                                    CommandResult.CommandContext.SetPrompt(Name, args, string.Format("{0} BAN SEVERITY", user.Username));
                                                }
                                                else if (CommandResult.CommandContext.PromptData.Length == 1)
                                                {
                                                    string promptData = CommandResult.CommandContext.PromptData[0];
                                                    if (promptData.IsShort())
                                                    {
                                                        var banType = promptData.ToShort();
                                                        if (banType >= 1 && banType <= 7)
                                                        {
                                                            CommandResult.WriteLine("Type a reason for this ban.");
                                                            CommandResult.CommandContext.SetPrompt(Name, args, string.Format("{0} BAN REASON", user.Username));
                                                        }
                                                        else
                                                            CommandResult.WriteLine("'{0}' is not a valid ban type.", banType);
                                                    }
                                                    else
                                                        CommandResult.WriteLine("'{0}' is not a valid ban type.", promptData);
                                                }
                                                else if (CommandResult.CommandContext.PromptData.Length == 2)
                                                {
                                                    string promptData = CommandResult.CommandContext.PromptData[0];
                                                    if (promptData.IsShort())
                                                    {
                                                        var banType = promptData.ToShort();
                                                        if (banType >= 1 && banType <= 7)
                                                        {
                                                            DateTime expirationDate = DateTime.UtcNow;
                                                            switch (banType)
                                                            {
                                                                case 1:
                                                                    expirationDate = DateTime.UtcNow.AddHours(1);
                                                                    break;
                                                                case 2:
                                                                    expirationDate = DateTime.UtcNow.AddHours(3);
                                                                    break;
                                                                case 3:
                                                                    expirationDate = DateTime.UtcNow.AddDays(1);
                                                                    break;
                                                                case 4:
                                                                    expirationDate = DateTime.UtcNow.AddDays(7);
                                                                    break;
                                                                case 5:
                                                                    expirationDate = DateTime.UtcNow.AddMonths(1);
                                                                    break;
                                                                case 6:
                                                                    expirationDate = DateTime.UtcNow.AddYears(1);
                                                                    break;
                                                                case 7:
                                                                    expirationDate = DateTime.UtcNow.AddYears(42);
                                                                    break;
                                                            }
                                                            user.UserActivityLog.Add(new UserActivityLogItem
                                                            {
                                                                Type = "Ban",
                                                                Date = DateTime.UtcNow,
                                                                Information = string.Format(
                                                                    "Ban given by '{0}'\n\nExpiration: {1}\n\nDetails: {2}",
                                                                    CommandResult.CurrentUser.Username,
                                                                    expirationDate.TimeUntil(),
                                                                    CommandResult.CommandContext.PromptData[1]
                                                                )
                                                            });
                                                            CommandResult.CurrentUser.BannedUsers.Add(new Ban
                                                            {
                                                                StartDate = DateTime.UtcNow,
                                                                EndDate = expirationDate,
                                                                Username = user.Username,
                                                                Reason = CommandResult.CommandContext.PromptData[1]
                                                            });
                                                            _dataBucket.UserRepository.UpdateUser(CommandResult.CurrentUser);
                                                            _dataBucket.SaveChanges();
                                                            CommandResult.CommandContext.Restore();
                                                            CommandResult.WriteLine("'{0}' banned successfully.", user.Username);
                                                        }
                                                        else
                                                            CommandResult.WriteLine("'{0}' is not a valid ban type.", banType);
                                                    }
                                                    else
                                                        CommandResult.WriteLine("'{0}' is not a valid ban type.", promptData);
                                                }
                                            }
                                            else
                                                CommandResult.WriteLine("User '{0}' is already banned.", user.Username);
                                        }
                                        else
                                        {
                                            if (user.BanInfo != null)
                                            {
                                                _dataBucket.UserRepository.UnbanUser(user.Username);
                                                _dataBucket.UserRepository.UpdateUser(user);
                                                _dataBucket.SaveChanges();
                                                CommandResult.WriteLine("User '{0}' successfully unbanned.", user.Username);
                                            }
                                            else
                                                CommandResult.WriteLine("User '{0}' is not banned.", user.Username);
                                        }
                                    }
                                    else
                                        CommandResult.WriteLine("You are not authorized to ban user '{0}'.", user.Username);
                                }
                                else
                                    CommandResult.WriteLine("There is no user with the username '{0}'.", parsedArgs[0]);
                            }
                            else
                                CommandResult.WriteLine("You must specify a username.");
                        }
                        else
                        {
                            if (history)
                            {
                                if (parsedArgs.Length == 1)
                                {
                                    var user = _dataBucket.UserRepository.GetUser(parsedArgs[0]);
                                    if (user != null)
                                    {
                                        var offenseLog = _dataBucket.UserRepository.GetOffenseHistory(user.Username);
                                        CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
                                        foreach (var logItem in offenseLog)
                                        {
                                            CommandResult.WriteLine();
                                            if (logItem.Type.Is("Ban"))
                                                CommandResult.WriteLine(DisplayMode.DontType, "'{0}' was banned {1}.", user.Username, logItem.Date.TimePassed());
                                            else if (logItem.Type.Is("Warning"))
                                                CommandResult.WriteLine(DisplayMode.DontType, "'{0}' was warned {1}.", user.Username, logItem.Date.TimePassed());
                                            CommandResult.WriteLine();
                                            CommandResult.WriteLine(DisplayMode.DontType, logItem.Information);
                                            CommandResult.WriteLine();
                                            CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
                                        }
                                        if (offenseLog.Count() == 0)
                                        {
                                            CommandResult.WriteLine();
                                            CommandResult.WriteLine("User '{0}' does not have any offenses.", user.Username);
                                        }
                                    }
                                    else
                                        CommandResult.WriteLine("There is no user with the username '{0}'.", parsedArgs[0]);
                                }
                                else
                                    CommandResult.WriteLine("You must specify a username.");
                            }
                            if (ignore != null)
                            {
                                if (parsedArgs.Length == 1)
                                {
                                    var user = _dataBucket.UserRepository.GetUser(parsedArgs[0]);
                                    if (user != null)
                                    {
                                        var ignoreItem = CommandResult.CurrentUser.Ignores
                                                .SingleOrDefault(x => x.IgnoredUser.Equals(user.Username));

                                        if ((bool)ignore)
                                            if (ignoreItem == null)
                                            {
                                                _dataBucket.UserRepository.IgnoreUser(CommandResult.CurrentUser.Username, user.Username);
                                                _dataBucket.UserRepository.UpdateUser(CommandResult.CurrentUser);
                                                _dataBucket.SaveChanges();
                                                CommandResult.WriteLine("'{0}' successfully ignored.", user.Username);
                                            }
                                            else
                                                CommandResult.WriteLine("You have already ignored '{0}'.", user.Username);
                                        else if (!(bool)ignore)
                                            if (ignoreItem != null)
                                            {
                                                _dataBucket.UserRepository.UnignoreUser(CommandResult.CurrentUser.Username, user.Username);
                                                _dataBucket.UserRepository.UpdateUser(CommandResult.CurrentUser);
                                                _dataBucket.SaveChanges();
                                                CommandResult.WriteLine("'{0}' successfully unignored.", user.Username);
                                            }
                                            else
                                                CommandResult.WriteLine("You have not ignored any users with the username '{0}'.", user.Username);
                                    }
                                    else
                                        CommandResult.WriteLine("There is no user with the username '{0}'.", parsedArgs[0]);
                                }
                                else
                                    CommandResult.WriteLine("You must specify a username.");
                            }
                            if (addRole != null)
                            {
                                if (parsedArgs.Length == 1)
                                {
                                    var user = _dataBucket.UserRepository.GetUser(parsedArgs[0]);
                                    if (user != null)
                                    {
                                        var role = _dataBucket.UserRepository.GetRole(addRole);
                                        if (role != null)
                                        {
                                            user.Roles.Add(role);
                                            _dataBucket.UserRepository.UpdateUser(user);
                                            _dataBucket.SaveChanges();
                                            CommandResult.WriteLine("Role '{0}' added to '{1}' successfully.", role.Name, user.Username);
                                        }
                                        else
                                            CommandResult.WriteLine("There is no role '{0}'.", addRole);
                                    }
                                    else
                                        CommandResult.WriteLine("There is no user with the username '{0}'.", parsedArgs[0]);
                                }
                                else
                                    CommandResult.WriteLine("You must specify a username.");
                            }
                            if (removeRole != null)
                            {
                                if (parsedArgs.Length == 1)
                                {
                                    var user = _dataBucket.UserRepository.GetUser(parsedArgs[0]);
                                    if (user != null)
                                    {
                                        var role = _dataBucket.UserRepository.GetRole(removeRole);
                                        if (role != null)
                                        {
                                            user.Roles.Remove(role);
                                            _dataBucket.UserRepository.UpdateUser(user);
                                            _dataBucket.SaveChanges();
                                            CommandResult.WriteLine("Role '{0}' removed from '{1}' successfully.", role.Name, user.Username);
                                        }
                                        else
                                            CommandResult.WriteLine("There is no role '{0}'.", addRole);
                                    }
                                    else
                                        CommandResult.WriteLine("There is no user with the username '{0}'.", parsedArgs[0]);
                                }
                                else
                                    CommandResult.WriteLine("You must specify a username.");
                            }
                            if (giveCredits != null)
                            {
                                if (parsedArgs.Length == 1)
                                {
                                    var user = _dataBucket.UserRepository.GetUser(parsedArgs[0]);
                                    if (user != null)
                                    {
                                        if (giveCredits.IsLong())
                                        {
                                            var creditsToGive = giveCredits.ToLong();
                                            user.Credits += creditsToGive;
                                            _dataBucket.UserRepository.UpdateUser(user);
                                            _dataBucket.SaveChanges();
                                            CommandResult.WriteLine("User '{0}' received {1} credits.", user.Username, creditsToGive);
                                        }
                                        else
                                            CommandResult.WriteLine("You must enter a valid amount of credits.");
                                    }
                                    else
                                        CommandResult.WriteLine("There is no user with the username '{0}'.", parsedArgs[0]);
                                }
                                else
                                    CommandResult.WriteLine("You must specify a username.");
                            }
                            if (giveInvites != null)
                            {
                                if (parsedArgs.Length == 1)
                                {
                                    var user = _dataBucket.UserRepository.GetUser(parsedArgs[0]);
                                    if (user != null)
                                    {
                                        if (giveInvites.IsLong())
                                        {
                                            var invitesToGive = giveInvites.ToLong();
                                            var random = new Random();
                                            for (int count = 0; count < invitesToGive; count++)
                                            {
                                                string code = random.Next(1 << 16).ToString("X4")
                                                        + random.Next(1 << 16).ToString("X4")
                                                        + random.Next(1 << 16).ToString("X4")
                                                        + random.Next(1 << 16).ToString("X4");
                                                var inviteCode = new InviteCode
                                                {
                                                    Code = code,
                                                    Username = user.Username
                                                };
                                                _dataBucket.InviteCodeRepository.AddInviteCode(inviteCode);
                                            }
                                            _dataBucket.UserRepository.UpdateUser(user);
                                            _dataBucket.SaveChanges();
                                            CommandResult.WriteLine("User '{0}' received {1} invintes.", user.Username, invitesToGive);
                                        }
                                        else
                                            CommandResult.WriteLine("You must enter a valid amount of invites.");
                                    }
                                    else
                                        CommandResult.WriteLine("There is no user with the username '{0}'.", parsedArgs[0]);
                                }
                                else
                                    CommandResult.WriteLine("You must specify a username.");
                            }
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
