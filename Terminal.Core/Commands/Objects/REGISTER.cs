using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Core.Enums;
using Terminal.Core.Objects;
using Terminal.Core.Commands.Interfaces;
using Terminal.Core.Data.Entities;
using Terminal.Core.ExtensionMethods;
using Terminal.Core.Settings;
using System.IO;
using Mono.Options;
using Terminal.Core.Utilities;
using System.Net;
using Terminal.Core.Data;

namespace Terminal.Core.Commands.Objects
{
    public class REGISTER : ICommand
    {
        private IDataBucket _dataBucket;

        public REGISTER(IDataBucket dataBucket)
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
            get { return "REGISTER"; }
        }

        public string Parameters
        {
            get { return "<Username> <Password> <ConfirmPassword> [Option(s)]"; }
        }

        public string Description
        {
            get { return "Allows a visitor to register for a username."; }
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

            List<string> parsedArgs = null;
            if (args != null)
            {
                try
                {
                    parsedArgs = options.Parse(args);
                }
                catch (OptionException ex)
                {
                    CommandResult.WriteLine(ex.Message);
                }
            }

            var registrationStatus = _dataBucket.VariableRepository.GetVariable("Registration");
            if (registrationStatus.Equals("Open", StringComparison.InvariantCultureIgnoreCase) || CommandResult.CommandContext.PromptData != null)
            {
                CommandResult.CommandContext.Prompt = false;
                InviteCode inviteCode = null;
                if (CommandResult.CommandContext.PromptData != null)
                    inviteCode = _dataBucket.InviteCodeRepository.GetInviteCode(CommandResult.CommandContext.PromptData[0]);

                if (CommandResult.CommandContext.PromptData == null || inviteCode != null)
                {
                    if (inviteCode != null)
                    {
                        if (parsedArgs == null)
                            parsedArgs = new List<string> { inviteCode.Username };
                        else
                            parsedArgs.Insert(0, inviteCode.Username);
                    }
                    if ((parsedArgs == null || parsedArgs.Count == 0))
                    {
                        CommandResult.WriteLine("Enter your desired username. (no spaces. sorry.)");
                        CommandResult.SetContext(ContextStatus.Forced, Name, args, "Username");
                    }
                    else if (parsedArgs.Count == 1)
                    {
                        if (parsedArgs[0].Length >= 3 && parsedArgs[0].Length <= 30)
                        {
                            if (!_dataBucket.UserRepository.CheckUserExists(parsedArgs[0]))
                            {
                                CommandResult.WriteLine("Enter your desired password.");
                                CommandResult.PasswordField = true;
                                CommandResult.SetContext(ContextStatus.Forced, Name, args, "Password");
                            }
                            else
                            {
                                CommandResult.WriteLine("Username already exists.");
                                CommandResult.WriteLine("Enter a different username.");
                                CommandResult.SetContext(ContextStatus.Forced, Name, null, "Username");
                            }
                        }
                        else
                        {
                            CommandResult.WriteLine("Username must be between 3 and 15 characters.");
                            CommandResult.WriteLine("Enter a different username.");
                            CommandResult.SetContext(ContextStatus.Forced, Name, null, "Username");
                        }
                    }
                    else if (parsedArgs.Count == 2)
                    {
                        if (parsedArgs[0].Length >= 3 && parsedArgs[0].Length <= 15)
                        {
                            if (!_dataBucket.UserRepository.CheckUserExists(parsedArgs[0]))
                            {
                                CommandResult.WriteLine("Re-enter your desired password.");
                                CommandResult.PasswordField = true;
                                CommandResult.SetContext(ContextStatus.Forced, Name, args, "Confirm Password");
                            }
                            else
                            {
                                CommandResult.WriteLine("Username already exists.");
                                CommandResult.WriteLine("Enter your desired username.");
                                CommandResult.SetContext(ContextStatus.Forced, Name, null, "Username");
                            }
                        }
                        else
                        {
                            CommandResult.WriteLine("Username must be between 3 and 15 characters.");
                            CommandResult.WriteLine("Enter a different username.");
                            CommandResult.SetContext(ContextStatus.Forced, Name, null, "Username");
                        }
                    }
                    else if (parsedArgs.Count == 3)
                    {
                        if (parsedArgs[0].Length >= 3 && parsedArgs[0].Length <= 15)
                        {
                            var user = _dataBucket.UserRepository.GetUser(parsedArgs[0]);
                            if (user == null)
                            {
                                if (parsedArgs[1] == parsedArgs[2])
                                {
                                    user = new User
                                    {
                                        Username = parsedArgs[0],
                                        Password = parsedArgs[1],
                                        JoinDate = DateTime.UtcNow,
                                        LastLogin = DateTime.UtcNow,
                                        TimeZone = "UTC",
                                        Sound = true,
                                    };
                                    var role = _dataBucket.UserRepository.GetRole("User");
                                    user.Roles = new List<Role> { role };

                                    if (inviteCode != null)
                                        _dataBucket.InviteCodeRepository.DeleteInviteCode(inviteCode);

                                    _dataBucket.UserRepository.AddUser(user);
                                    _dataBucket.SaveChanges();


                                    var defaultAliases = new List<Alias>
                                        {
                                            new Alias
                                            {
                                                Username = user.Username,
                                                Shortcut = "lb",
                                                Command = "BOARDS"
                                            },
                                            new Alias
                                            {
                                                Username = user.Username,
                                                Shortcut = "b",
                                                Command = "BOARD"
                                            },
                                            new Alias
                                            {
                                                Username = user.Username,
                                                Shortcut = "t",
                                                Command = "TOPIC"
                                            },
                                            new Alias
                                            {
                                                Username = user.Username,
                                                Shortcut = "lm",
                                                Command = "MESSAGES"
                                            },
                                            new Alias
                                            {
                                                Username = user.Username,
                                                Shortcut = "m",
                                                Command = "MESSAGE"
                                            }
                                        };

                                    defaultAliases.ForEach(x => _dataBucket.AliasRepository.AddAlias(x));
                                    _dataBucket.SaveChanges();

                                    CommandResult.CurrentUser = user;
                                    CommandResult.WriteLine("Thank you for registering.");
                                    //CommandResult.WriteLine();
                                    //var STATS = AvailableCommands.SingleOrDefault(x => x.Name.Is("STATS"));
                                    //STATS.Invoke(new string[] { "-users" });
                                    CommandResult.WriteLine();
                                    CommandResult.WriteLine("You are now logged in as {0}.", CommandResult.CurrentUser.Username);
                                    CommandResult.DeactivateContext();
                                }
                                else
                                {
                                    CommandResult.WriteLine("Passwords did not match.");
                                    CommandResult.WriteLine("Enter your desired password.");
                                    CommandResult.PasswordField = true;
                                    CommandResult.SetContext(ContextStatus.Forced, Name, new string[] { parsedArgs[0] }, "Password");
                                }
                            }
                            else
                            {
                                CommandResult.WriteLine("Username already exists.");
                                CommandResult.WriteLine("Enter your desired username.");
                                CommandResult.SetContext(ContextStatus.Forced, Name, null, "Username");
                            }
                        }
                        else
                        {
                            CommandResult.WriteLine("Username must be between 3 and 15 characters.");
                            CommandResult.WriteLine("Enter a different username.");
                            CommandResult.SetContext(ContextStatus.Forced, Name, null, "Username");
                        }
                    }
                }
                else
                {
                    CommandResult.WriteLine("You did not supply a valid invite code.");
                    CommandResult.DeactivateContext();
                }
            }
            else if (registrationStatus.Equals("Invite-Only", StringComparison.InvariantCultureIgnoreCase))
            {
                CommandResult.WriteLine("Enter your invite code.");
                CommandResult.SetPrompt(Name, args, "Invite Code");
            }
            else if (registrationStatus.Equals("Closed", StringComparison.InvariantCultureIgnoreCase))
                CommandResult.WriteLine("Registration is currently closed.");
        }
    }
}
