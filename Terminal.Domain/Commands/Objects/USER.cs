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
using System.ServiceProcess;
using System.Threading;
using System.Diagnostics;

namespace Terminal.Domain.Commands.Objects
{
    public class USER : ICommand
    {
        private IUserRepository _userRepository;
        private IInviteCodeRepository _inviteCodeRepository;

        public USER(IUserRepository userRepository, IInviteCodeRepository inviteCodeRepository)
        {
            _userRepository = userRepository;
            _inviteCodeRepository = inviteCodeRepository;
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
            if (this.CommandResult.CurrentUser.IsModerator || this.CommandResult.CurrentUser.IsAdministrator)
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
            if (this.CommandResult.CurrentUser.IsAdministrator)
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
                this.CommandResult.WriteLine(DisplayTemplates.InvalidArguments);
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
                            var user = _userRepository.GetUser(parsedArgs[0]);
                            if (user != null)
                            {
                                // display user profile.
                            }
                            else
                                this.CommandResult.WriteLine("There is no user with the username '{0}'.", parsedArgs[0]);
                        }
                        else
                            this.CommandResult.WriteLine("You must specify a username.");
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
                        else if (warn)
                        {
                            if (parsedArgs.Length == 1)
                            {
                                var user = _userRepository.GetUser(parsedArgs[0]);
                                if (user != null)
                                {
                                    if ((!user.IsModerator && !user.IsAdministrator) || this.CommandResult.CurrentUser.IsAdministrator)
                                    {
                                        if (this.CommandResult.CommandContext.PromptData == null)
                                        {
                                            this.CommandResult.WriteLine("Type the details of your warning to '{0}'.", user.Username);
                                            this.CommandResult.CommandContext.SetPrompt(this.Name, args, string.Format("{0} WARNING", user.Username));
                                        }
                                        else if (this.CommandResult.CommandContext.PromptData.Length == 1)
                                        {
                                            user.UserActivityLog.Add(new UserActivityLogItem
                                            {
                                                Type = "Warning",
                                                Date = DateTime.UtcNow,
                                                Information = string.Format(
                                                    "Warning given by '{0}'\n\nDetails: {1}",
                                                    this.CommandResult.CurrentUser.Username,
                                                    this.CommandResult.CommandContext.PromptData[0]
                                                )
                                            });
                                            user.ReceivedMessages.Add(new Message
                                            {
                                                Sender = this.CommandResult.CurrentUser.Username,
                                                SentDate = DateTime.UtcNow,
                                                Subject = string.Format(
                                                    "{0} has given you a warning.",
                                                    this.CommandResult.CurrentUser.Username
                                                ),
                                                Body = string.Format(
                                                    "You have been given a warning by '{0}'.\n\n{1}",
                                                    this.CommandResult.CurrentUser.Username,
                                                    this.CommandResult.CommandContext.PromptData[0]
                                                )
                                            });
                                            _userRepository.UpdateUser(user);
                                            this.CommandResult.CommandContext.Restore();
                                            this.CommandResult.WriteLine("Warning successfully issued to '{0}'.", user.Username);
                                        }
                                    }
                                    else
                                        this.CommandResult.WriteLine("You are not authorized to give warnings to user '{0}'.", user.Username);
                                }
                                else
                                    this.CommandResult.WriteLine("There is no user with the username '{0}'.", parsedArgs[0]);
                            }
                            else
                                this.CommandResult.WriteLine("You must specify a username.");
                        }
                        else if (ban != null)
                        {
                            if (parsedArgs.Length == 1)
                            {
                                var user = _userRepository.GetUser(parsedArgs[0]);
                                if (user != null)
                                {
                                    if ((!user.IsModerator && !user.IsAdministrator) || this.CommandResult.CurrentUser.IsAdministrator)
                                    {
                                        if ((bool)ban)
                                        {
                                            if (user.BanInfo == null)
                                            {
                                                if (this.CommandResult.CommandContext.PromptData == null)
                                                {
                                                    this.CommandResult.WriteLine("How severe should the ban be?");
                                                    this.CommandResult.WriteLine();
                                                    this.CommandResult.WriteLine("1 = One Hour");
                                                    this.CommandResult.WriteLine("2 = Three Hours");
                                                    this.CommandResult.WriteLine("3 = One Day");
                                                    this.CommandResult.WriteLine("4 = One Week");
                                                    this.CommandResult.WriteLine("5 = One Month");
                                                    this.CommandResult.WriteLine("6 = One Year");
                                                    this.CommandResult.WriteLine("7 = 42 Years");
                                                    this.CommandResult.CommandContext.SetPrompt(this.Name, args, string.Format("{0} BAN SEVERITY", user.Username));
                                                }
                                                else if (this.CommandResult.CommandContext.PromptData.Length == 1)
                                                {
                                                    string promptData = this.CommandResult.CommandContext.PromptData[0];
                                                    if (promptData.IsShort())
                                                    {
                                                        var banType = promptData.ToShort();
                                                        if (banType >= 1 && banType <= 7)
                                                        {
                                                            this.CommandResult.WriteLine("Type a reason for this ban.");
                                                            this.CommandResult.CommandContext.SetPrompt(this.Name, args, string.Format("{0} BAN REASON", user.Username));
                                                        }
                                                        else
                                                            this.CommandResult.WriteLine("'{0}' is not a valid ban type.", banType);
                                                    }
                                                    else
                                                        this.CommandResult.WriteLine("'{0}' is not a valid ban type.", promptData);
                                                }
                                                else if (this.CommandResult.CommandContext.PromptData.Length == 2)
                                                {
                                                    string promptData = this.CommandResult.CommandContext.PromptData[0];
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
                                                                    this.CommandResult.CurrentUser.Username,
                                                                    expirationDate.TimeUntil(),
                                                                    this.CommandResult.CommandContext.PromptData[1]
                                                                )
                                                            });
                                                            this.CommandResult.CurrentUser.BannedUsers.Add(new Ban
                                                            {
                                                                StartDate = DateTime.UtcNow,
                                                                EndDate = expirationDate,
                                                                Username = user.Username,
                                                                Reason = this.CommandResult.CommandContext.PromptData[1]
                                                            });
                                                            _userRepository.UpdateUser(this.CommandResult.CurrentUser);
                                                            this.CommandResult.CommandContext.Restore();
                                                            this.CommandResult.WriteLine("'{0}' banned successfully.", user.Username);
                                                        }
                                                        else
                                                            this.CommandResult.WriteLine("'{0}' is not a valid ban type.", banType);
                                                    }
                                                    else
                                                        this.CommandResult.WriteLine("'{0}' is not a valid ban type.", promptData);
                                                }
                                            }
                                            else
                                                this.CommandResult.WriteLine("User '{0}' is already banned.", user.Username);
                                        }
                                        else
                                        {
                                            if (user.BanInfo != null)
                                            {
                                                _userRepository.UnbanUser(user.Username);
                                                _userRepository.UpdateUser(user);
                                                this.CommandResult.WriteLine("User '{0}' successfully unbanned.", user.Username);
                                            }
                                            else
                                                this.CommandResult.WriteLine("User '{0}' is not banned.", user.Username);
                                        }
                                    }
                                    else
                                        this.CommandResult.WriteLine("You are not authorized to ban user '{0}'.", user.Username);
                                }
                                else
                                    this.CommandResult.WriteLine("There is no user with the username '{0}'.", parsedArgs[0]);
                            }
                            else
                                this.CommandResult.WriteLine("You must specify a username.");
                        }
                        else
                        {
                            if (history)
                            {
                                if (parsedArgs.Length == 1)
                                {
                                    var user = _userRepository.GetUser(parsedArgs[0]);
                                    if (user != null)
                                    {
                                        var offenseLog = _userRepository.GetOffenseHistory(user.Username);
                                        this.CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
                                        foreach (var logItem in offenseLog)
                                        {
                                            this.CommandResult.WriteLine();
                                            if (logItem.Type.Is("Ban"))
                                                this.CommandResult.WriteLine(DisplayMode.DontType, "'{0}' was banned {1}.", user.Username, logItem.Date.TimePassed());
                                            else if (logItem.Type.Is("Warning"))
                                                this.CommandResult.WriteLine(DisplayMode.DontType, "'{0}' was warned {1}.", user.Username, logItem.Date.TimePassed());
                                            this.CommandResult.WriteLine();
                                            this.CommandResult.WriteLine(DisplayMode.DontType, logItem.Information);
                                            this.CommandResult.WriteLine();
                                            this.CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
                                        }
                                        if (offenseLog.Count() == 0)
                                        {
                                            this.CommandResult.WriteLine();
                                            this.CommandResult.WriteLine("User '{0}' does not have any offenses.", user.Username);
                                        }
                                    }
                                    else
                                        this.CommandResult.WriteLine("There is no user with the username '{0}'.", parsedArgs[0]);
                                }
                                else
                                    this.CommandResult.WriteLine("You must specify a username.");
                            }
                            if (ignore != null)
                            {
                                if (parsedArgs.Length == 1)
                                {
                                    var user = _userRepository.GetUser(parsedArgs[0]);
                                    if (user != null)
                                    {
                                        var ignoreItem = this.CommandResult.CurrentUser.Ignores
                                                .SingleOrDefault(x => x.IgnoredUser.Equals(user.Username));

                                        if ((bool)ignore)
                                            if (ignoreItem == null)
                                            {
                                                _userRepository.IgnoreUser(this.CommandResult.CurrentUser.Username, user.Username);
                                                _userRepository.UpdateUser(this.CommandResult.CurrentUser);
                                                this.CommandResult.WriteLine("'{0}' successfully ignored.", user.Username);
                                            }
                                            else
                                                this.CommandResult.WriteLine("You have already ignored '{0}'.", user.Username);
                                        else if (!(bool)ignore)
                                            if (ignoreItem != null)
                                            {
                                                _userRepository.UnignoreUser(this.CommandResult.CurrentUser.Username, user.Username);
                                                _userRepository.UpdateUser(this.CommandResult.CurrentUser);
                                                this.CommandResult.WriteLine("'{0}' successfully unignored.", user.Username);
                                            }
                                            else
                                                this.CommandResult.WriteLine("You have not ignored any users with the username '{0}'.", user.Username);
                                    }
                                    else
                                        this.CommandResult.WriteLine("There is no user with the username '{0}'.", parsedArgs[0]);
                                }
                                else
                                    this.CommandResult.WriteLine("You must specify a username.");
                            }
                            if (addRole != null)
                            {
                                if (parsedArgs.Length == 1)
                                {
                                    var user = _userRepository.GetUser(parsedArgs[0]);
                                    if (user != null)
                                    {
                                        var role = _userRepository.GetRole(addRole);
                                        if (role != null)
                                        {
                                            user.Roles.Add(role);
                                            _userRepository.UpdateUser(user);
                                            this.CommandResult.WriteLine("Role '{0}' added to '{1}' successfully.", role.Name, user.Username);
                                        }
                                        else
                                            this.CommandResult.WriteLine("There is no role '{0}'.", addRole);
                                    }
                                    else
                                        this.CommandResult.WriteLine("There is no user with the username '{0}'.", parsedArgs[0]);
                                }
                                else
                                    this.CommandResult.WriteLine("You must specify a username.");
                            }
                            if (removeRole != null)
                            {
                                if (parsedArgs.Length == 1)
                                {
                                    var user = _userRepository.GetUser(parsedArgs[0]);
                                    if (user != null)
                                    {
                                        var role = _userRepository.GetRole(removeRole);
                                        if (role != null)
                                        {
                                            user.Roles.Remove(role);
                                            _userRepository.UpdateUser(user);
                                            this.CommandResult.WriteLine("Role '{0}' removed from '{1}' successfully.", role.Name, user.Username);
                                        }
                                        else
                                            this.CommandResult.WriteLine("There is no role '{0}'.", addRole);
                                    }
                                    else
                                        this.CommandResult.WriteLine("There is no user with the username '{0}'.", parsedArgs[0]);
                                }
                                else
                                    this.CommandResult.WriteLine("You must specify a username.");
                            }
                            if (giveCredits != null)
                            {
                                if (parsedArgs.Length == 1)
                                {
                                    var user = _userRepository.GetUser(parsedArgs[0]);
                                    if (user != null)
                                    {
                                        if (giveCredits.IsLong())
                                        {
                                            var creditsToGive = giveCredits.ToLong();
                                            user.Credits += creditsToGive;
                                            _userRepository.UpdateUser(user);
                                            this.CommandResult.WriteLine("User '{0}' received {1} credits.", user.Username, creditsToGive);
                                        }
                                        else
                                            this.CommandResult.WriteLine("You must enter a valid amount of credits.");
                                    }
                                    else
                                        this.CommandResult.WriteLine("There is no user with the username '{0}'.", parsedArgs[0]);
                                }
                                else
                                    this.CommandResult.WriteLine("You must specify a username.");
                            }
                            if (giveInvites != null)
                            {
                                if (parsedArgs.Length == 1)
                                {
                                    var user = _userRepository.GetUser(parsedArgs[0]);
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
                                                _inviteCodeRepository.AddInviteCode(inviteCode);
                                            }
                                            _inviteCodeRepository.SaveChanges();
                                            _userRepository.UpdateUser(user);
                                            this.CommandResult.WriteLine("User '{0}' received {1} invintes.", user.Username, invitesToGive);
                                        }
                                        else
                                            this.CommandResult.WriteLine("You must enter a valid amount of invites.");
                                    }
                                    else
                                        this.CommandResult.WriteLine("There is no user with the username '{0}'.", parsedArgs[0]);
                                }
                                else
                                    this.CommandResult.WriteLine("You must specify a username.");
                            }
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
