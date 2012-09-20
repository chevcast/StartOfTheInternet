using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Domain.Enums;
using Terminal.Domain.Objects;
using Terminal.Domain.Commands.Interfaces;
using Terminal.Domain.Entities;
using Terminal.Domain.ExtensionMethods;
using Terminal.Domain.Settings;
using System.IO;
using Mono.Options;
using Terminal.Domain.Repositories.Interfaces;
using Terminal.Domain.Utilities;
using System.Net;

namespace Terminal.Domain.Commands.Objects
{
    public class REGISTER : ICommand
    {
        private IUserRepository _userRepository;
        private IVariableRepository _variableRepository;
        private IAliasRepository _aliasRepository;
        private IInviteCodeRepository _inviteCodeRepository;

        public REGISTER(
            IUserRepository userRepository,
            IVariableRepository variableRepository,
            IAliasRepository aliasRepository,
            IInviteCodeRepository inviteCodeRepository
        )
        {
            _userRepository = userRepository;
            _variableRepository = variableRepository;
            _aliasRepository = aliasRepository;
            _inviteCodeRepository = inviteCodeRepository;
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
            bool lue = false;

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
            options.Add(
                "LUE|lue",
                "Perform an LL specific registration.",
                x => lue = x != null
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
                    this.CommandResult.WriteLine(ex.Message);
                }
            }

            var registrationStatus = _variableRepository.GetVariable("Registration");
            if (registrationStatus.Equals("Open", StringComparison.InvariantCultureIgnoreCase) || this.CommandResult.CommandContext.PromptData != null)
            {
                this.CommandResult.CommandContext.Prompt = false;
                bool verified = false;
                if (this.CommandResult.CommandContext.PromptData != null)
                {
                    if (lue)
                    {
                        var llUsername = this.CommandResult.CommandContext.PromptData[0];
                        if (_userRepository.GetLUEser(llUsername) == null)
                        {
                            var llApiUrl = string.Format(
                                "http://boards.endoftheinter.net/scripts/login.php?username={0}&ip={1}",
                                llUsername,
                                this.CommandResult.IPAddress
                            );
                            var llResult = new WebClient().DownloadString(llApiUrl);
                            var split = llResult.Split(':');
                            verified = split[0] == "1" && split[1].ToUpper() == llUsername.ToUpper();
                        }
                        else
                            this.CommandResult.WriteLine("The LL username '{0}' has already been used before.", llUsername);
                    }
                    else
                    {
                        var inviteCode = _inviteCodeRepository.GetInviteCode(this.CommandResult.CommandContext.PromptData[0]);
                        verified = inviteCode != null && (inviteCode.User.BanInfo == null || DateTime.UtcNow > inviteCode.User.BanInfo.EndDate);
                    }
                }

                if (this.CommandResult.CommandContext.PromptData == null || verified)
                {
                    if (parsedArgs == null || parsedArgs.Count == 0)
                    {
                        this.CommandResult.WriteLine("Enter your desired username. (no spaces. sorry.)");
                        this.CommandResult.CommandContext.Set(ContextStatus.Forced, this.Name, args, "Username");
                    }
                    else if (parsedArgs.Count == 1)
                    {
                        if (parsedArgs[0].Length >= 3 && parsedArgs[0].Length <= 15)
                        {
                            if (!_userRepository.CheckUserExists(args[0]))
                            {
                                this.CommandResult.WriteLine("Enter your desired password.");
                                this.CommandResult.PasswordField = true;
                                this.CommandResult.CommandContext.Set(ContextStatus.Forced, this.Name, args, "Password");
                            }
                            else
                            {
                                this.CommandResult.WriteLine("Username already exists.");
                                this.CommandResult.WriteLine("Enter a different username.");
                                this.CommandResult.CommandContext.Set(ContextStatus.Forced, this.Name, null, "Username");
                            }
                        }
                        else
                        {
                            this.CommandResult.WriteLine("Username must be between 3 and 15 characters.");
                            this.CommandResult.WriteLine("Enter a different username.");
                            this.CommandResult.CommandContext.Set(ContextStatus.Forced, this.Name, null, "Username");
                        }
                    }
                    else if (parsedArgs.Count == 2)
                    {
                        if (parsedArgs[0].Length >= 3 && parsedArgs[0].Length <= 15)
                        {
                            if (!_userRepository.CheckUserExists(parsedArgs[0]))
                            {
                                this.CommandResult.WriteLine("Re-enter your desired password.");
                                this.CommandResult.PasswordField = true;
                                this.CommandResult.CommandContext.Set(ContextStatus.Forced, this.Name, args, "Confirm Password");
                            }
                            else
                            {
                                this.CommandResult.WriteLine("Username already exists.");
                                this.CommandResult.WriteLine("Enter your desired username.");
                                this.CommandResult.CommandContext.Set(ContextStatus.Forced, this.Name, null, "Username");
                            }
                        }
                        else
                        {
                            this.CommandResult.WriteLine("Username must be between 3 and 15 characters.");
                            this.CommandResult.WriteLine("Enter a different username.");
                            this.CommandResult.CommandContext.Set(ContextStatus.Forced, this.Name, null, "Username");
                        }
                    }
                    else if (parsedArgs.Count == 3)
                    {
                        if (parsedArgs[0].Length >= 3 && parsedArgs[0].Length <= 15)
                        {
                            var user = this._userRepository.GetUser(parsedArgs[0]);
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
                                    var role = _userRepository.GetRole("User");
                                    user.Roles = new List<Role> { role };

                                    if (lue)
                                    {
                                        var llUser = new LUEser { Username = this.CommandResult.CommandContext.PromptData[0] };
                                        _userRepository.AddLUEser(llUser);
                                        user.Credits = 1000;
                                    }
                                    else if (this.CommandResult.CommandContext.PromptData != null)
                                    {
                                        var inviteCode = _inviteCodeRepository.GetInviteCode(this.CommandResult.CommandContext.PromptData[0]);
                                        _inviteCodeRepository.DeleteInviteCode(inviteCode);
                                        _inviteCodeRepository.SaveChanges();
                                    }

                                    _userRepository.AddUser(user);


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
                                            },
                                            new Alias
                                            {
                                                Username = user.Username,
                                                Shortcut = "ll",
                                                Command = "LINKS"
                                            },
                                            new Alias
                                            {
                                                Username = user.Username,
                                                Shortcut = "l",
                                                Command = "LINK"
                                            }
                                        };

                                    defaultAliases.ForEach(x => _aliasRepository.AddAlias(x));

                                    this.CommandResult.CurrentUser = user;
                                    this.CommandResult.WriteLine("Thank you for registering.");
                                    //this.CommandResult.WriteLine();
                                    //var STATS = this.AvailableCommands.SingleOrDefault(x => x.Name.Is("STATS"));
                                    //STATS.Invoke(new string[] { "-users" });
                                    this.CommandResult.WriteLine();
                                    this.CommandResult.WriteLine("You are now logged in as {0}.", this.CommandResult.CurrentUser.Username);
                                    this.CommandResult.CommandContext.Deactivate();
                                }
                                else
                                {
                                    this.CommandResult.WriteLine("Passwords did not match.");
                                    this.CommandResult.WriteLine("Enter your desired password.");
                                    this.CommandResult.PasswordField = true;
                                    this.CommandResult.CommandContext.Set(ContextStatus.Forced, this.Name, new string[] { parsedArgs[0] }, "Password");
                                }
                            }
                            else
                            {
                                this.CommandResult.WriteLine("Username already exists.");
                                this.CommandResult.WriteLine("Enter your desired username.");
                                this.CommandResult.CommandContext.Set(ContextStatus.Forced, this.Name, null, "Username");
                            }
                        }
                        else
                        {
                            this.CommandResult.WriteLine("Username must be between 3 and 15 characters.");
                            this.CommandResult.WriteLine("Enter a different username.");
                            this.CommandResult.CommandContext.Set(ContextStatus.Forced, this.Name, null, "Username");
                        }
                    }
                }
                else
                {
                    if (lue)
                        this.CommandResult.WriteLine("Your LL username could not be verified.");
                    else
                        this.CommandResult.WriteLine("You did not supply a valid invite code.");
                    this.CommandResult.CommandContext.Deactivate();
                }
            }
            else if (registrationStatus.Equals("Invite-Only", StringComparison.InvariantCultureIgnoreCase))
            {
                if (lue)
                {
                    this.CommandResult.WriteLine("What is your LL username? (This is for verification only)");
                    this.CommandResult.CommandContext.SetPrompt(this.Name, args, "LL Username");
                }
                else
                {
                    this.CommandResult.WriteLine("Enter your invite code.");
                    this.CommandResult.CommandContext.SetPrompt(this.Name, args, "Invite Code");
                }
            }
            else if (registrationStatus.Equals("Closed", StringComparison.InvariantCultureIgnoreCase))
                this.CommandResult.WriteLine("Registration is currently closed.");
        }
    }
}
