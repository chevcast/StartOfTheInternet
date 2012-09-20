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
    public class SETTINGS : ICommand
    {
        private IUserRepository _userRepository;
        private IVariableRepository _variableRepository;

        public SETTINGS(IUserRepository userRepository, IVariableRepository variableRepository)
        {
            _userRepository = userRepository;
            _variableRepository = variableRepository;
        }

        public CommandResult CommandResult { get; set; }

        public IEnumerable<ICommand> AvailableCommands { get; set; }

        public string[] Roles
        {
            get { return RoleTemplates.AllLoggedIn; }
        }

        public string Name
        {
            get { return "SETTINGS"; }
        }

        public string Parameters
        {
            get { return "[Option(s)]"; }
        }

        public string Description
        {
            get { return "Allows you to set various options."; }
        }

        public bool ShowHelp
        {
            get { return true; }
        }

        public void Invoke(string[] args)
        {
            bool showHelp = false;
            bool? mute = null;
            bool changePassword = false;
            bool setTimeZone = false;
            bool? timeStamps = null;
            bool? autoFollow = null;
            bool? replyNotify = null;
            bool? messageNotify = null;
            string registration = null;

            var options = new OptionSet();
            options.Add(
                "?|help",
                "Show help information.",
                x => showHelp = x != null
            );
            options.Add(
                "m|mute",
                "Mute/unmute the terminal typing sound.",
                x => mute = x != null
            );
            options.Add(
                "changePassword",
                "Change your current password.",
                x => changePassword = x != null
            );
            options.Add(
                "setTimeZone",
                "Set your current time zone.",
                x => setTimeZone = x != null
            );
            options.Add(
                "timeStamps",
                "Turn timestamps on/off.",
                x => timeStamps = x != null
            );
            options.Add(
                "autoFollow",
                "Auto-follow topics you create or reply to.",
                x => autoFollow = x != null
            );
            options.Add(
                "replyNotify",
                "Display notifications about replies to your followed topics.",
                x => replyNotify = x != null
            );
            options.Add(
                "msgNotify",
                "Display notifications for unread messages in your inbox.",
                x => messageNotify = x != null
            );
            if (this.CommandResult.CurrentUser.IsAdministrator)
            {
                options.Add(
                    "reg|registration=",
                    "1=OPEN, 2=INVITE-ONLY, 3=CLOSED",
                    x => registration = x
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
                        else if (changePassword)
                        {
                            if (this.CommandResult.CommandContext.PromptData == null)
                            {
                                this.CommandResult.WriteLine("Type your new password.");
                                this.CommandResult.PasswordField = true;
                                this.CommandResult.CommandContext.SetPrompt(this.Name, args, "NEW PASSWORD");
                            }
                            else if (this.CommandResult.CommandContext.PromptData.Length == 1)
                            {
                                this.CommandResult.WriteLine("Confirm your new password.");
                                this.CommandResult.PasswordField = true;
                                this.CommandResult.CommandContext.SetPrompt(this.Name, args, "CONFIRM PASSWORD");
                            }
                            else if (this.CommandResult.CommandContext.PromptData.Length == 2)
                            {
                                string password = this.CommandResult.CommandContext.PromptData[0];
                                string confirmPassword = this.CommandResult.CommandContext.PromptData[1];
                                if (password == confirmPassword)
                                {
                                    this.CommandResult.CurrentUser.Password = password;
                                    _userRepository.UpdateUser(this.CommandResult.CurrentUser);
                                    this.CommandResult.WriteLine("Password changed successfully.");
                                }
                                else
                                    this.CommandResult.WriteLine("Passwords did not match.");
                                this.CommandResult.CommandContext.Restore();
                            }
                        }
                        else if (setTimeZone)
                        {
                            var timeZones = TimeZoneInfo.GetSystemTimeZones();
                            if (this.CommandResult.CommandContext.PromptData == null)
                            {
                                for (int index = 0; index < timeZones.Count; index++)
                                {
                                    var timeZone = timeZones[index];
                                    this.CommandResult.WriteLine(DisplayMode.DontType, "{{{0}}} {1}", index, timeZone.DisplayName);
                                }
                                this.CommandResult.WriteLine();
                                this.CommandResult.WriteLine("Enter time zone ID.");
                                this.CommandResult.CommandContext.SetPrompt(this.Name, args, "CHANGE TIME ZONE");
                            }
                            else if (this.CommandResult.CommandContext.PromptData.Length == 1)
                            {
                                string promptData = this.CommandResult.CommandContext.PromptData[0];
                                if (promptData.IsInt())
                                {
                                    int timeZoneId = promptData.ToInt();
                                    if (timeZoneId >= 0 && timeZoneId < timeZones.Count)
                                    {
                                        var timeZone = timeZones[timeZoneId];
                                        this.CommandResult.CurrentUser.TimeZone = timeZone.Id;
                                        _userRepository.UpdateUser(this.CommandResult.CurrentUser);
                                        this.CommandResult.WriteLine("'{0}' successfully set as your current time zone.", timeZone.Id);
                                        this.CommandResult.CommandContext.Restore();
                                    }
                                    else
                                    {
                                        this.CommandResult.WriteLine("'{0}' does not match any available time zone ID.", timeZoneId);
                                        this.CommandResult.CommandContext.Restore();
                                        this.CommandResult.WriteLine("Enter time zone ID.");
                                        this.CommandResult.CommandContext.SetPrompt(this.Name, args, "CHANGE TIME ZONE");
                                    }
                                }
                                else
                                {
                                    this.CommandResult.WriteLine("'{0}' is not a valid time zone ID.", promptData);
                                    this.CommandResult.CommandContext.Restore();
                                    this.CommandResult.WriteLine("Enter time zone ID.");
                                    this.CommandResult.CommandContext.SetPrompt(this.Name, args, "CHANGE TIME ZONE");
                                }
                            }
                        }
                        else
                        {
                            if (mute != null)
                            {
                                this.CommandResult.CurrentUser.Sound = !(bool)mute;
                                _userRepository.UpdateUser(this.CommandResult.CurrentUser);
                                this.CommandResult.WriteLine("Sound successfully {0}.", (bool)mute ? "muted" : "unmuted");
                            }
                            if (timeStamps != null)
                            {
                                this.CommandResult.CurrentUser.ShowTimestamps = (bool)timeStamps;
                                _userRepository.UpdateUser(this.CommandResult.CurrentUser);
                                this.CommandResult.WriteLine("Timestamps were successfully {0}.", (bool)timeStamps ? "enabled" : "disabled");
                            }
                            if (autoFollow != null)
                            {
                                this.CommandResult.CurrentUser.AutoFollow = (bool)autoFollow;
                                _userRepository.UpdateUser(this.CommandResult.CurrentUser);
                                this.CommandResult.WriteLine("Auto-follow {0}.", (bool)autoFollow ? "activated" : "deactivated");
                            }
                            if (replyNotify != null)
                            {
                                this.CommandResult.CurrentUser.NotifyReplies = (bool)replyNotify;
                                _userRepository.UpdateUser(this.CommandResult.CurrentUser);
                                this.CommandResult.WriteLine("Reply notifications were successfully turned {0}.", (bool)replyNotify ? "on" : "off");
                            }
                            if (messageNotify != null)
                            {
                                this.CommandResult.CurrentUser.NotifyMessages = (bool)messageNotify;
                                _userRepository.UpdateUser(this.CommandResult.CurrentUser);
                                this.CommandResult.WriteLine("Message notifications were successfully turned {0}.", (bool)messageNotify ? "on" : "off");
                            }
                            if (registration != null)
                            {
                                var registrationStatus = _variableRepository.GetVariable("Registration");
                                if (registration == "1")
                                {
                                    registrationStatus = "Open";
                                    this.CommandResult.WriteLine("Registration opened successfully.");
                                }
                                else if (registration == "2")
                                {
                                    registrationStatus = "Invite-Only";
                                    this.CommandResult.WriteLine("Registration set to invite only.");
                                }
                                else if (registration == "3")
                                {
                                    registrationStatus = "Closed";
                                    this.CommandResult.WriteLine("Registration closed successfully.");
                                }
                                _variableRepository.ModifyVariable("Registration", registrationStatus);
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
