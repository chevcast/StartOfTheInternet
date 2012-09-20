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
    public class ALIAS : ICommand
    {
        private IAliasRepository _aliasRepository;

        public ALIAS(IAliasRepository aliasRepository)
        {
            _aliasRepository = aliasRepository;
        }

        public CommandResult CommandResult { get; set; }

        public IEnumerable<ICommand> AvailableCommands { get; set; }

        public string[] Roles
        {
            get { return RoleTemplates.AllLoggedIn; }
        }

        public string Name
        {
            get { return "ALIAS"; }
        }

        public string Parameters
        {
            get { return "<Options>"; }
        }

        public string Description
        {
            get { return "Allows you to define an alias/macro to simplify and customize commands."; }
        }

        public bool ShowHelp
        {
            get { return true; }
        }

        public void Invoke(string[] args)
        {
            bool showHelp = false;
            string newAlias = null;
            string deleteAlias = null;

            var options = new OptionSet();
            options.Add(
                "?|help",
                "Show help information.",
                x => showHelp = x != null
            );
            options.Add(
                "n|new=",
                "Create a new {alias}.",
                x => newAlias = x
            );
            options.Add(
                "d|delete=",
                "Delete an existing {alias}.",
                x => deleteAlias = x
            );

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
                        else if (newAlias != null)
                        {
                            if (!newAlias.Is("HELP") && !this.AvailableCommands.Any(x => x.Name.Is(newAlias)))
                            {
                                var alias = _aliasRepository.GetAlias(this.CommandResult.CurrentUser.Username, newAlias);
                                if (alias == null)
                                {
                                    if (this.CommandResult.CommandContext.PromptData == null)
                                    {
                                        this.CommandResult.WriteLine("Type the value that should be sent to the terminal when you use your new alias.");
                                        this.CommandResult.CommandContext.SetPrompt(this.Name, args, string.Format("{0} VALUE", newAlias.ToUpper()));
                                    }
                                    else if (this.CommandResult.CommandContext.PromptData.Length == 1)
                                    {
                                        _aliasRepository.AddAlias(new Alias
                                        {
                                            Username = this.CommandResult.CurrentUser.Username,
                                            Shortcut = newAlias,
                                            Command = this.CommandResult.CommandContext.PromptData[0]
                                        });
                                        this.CommandResult.WriteLine("Alias '{0}' was successfully defined.", newAlias.ToUpper());
                                        this.CommandResult.CommandContext.Restore();
                                    }
                                }
                                else
                                    this.CommandResult.WriteLine("You have already defined an alias named '{0}'.", newAlias.ToUpper());
                            }
                            else
                                this.CommandResult.WriteLine("'{0}' is an existing command. You cannot create aliases with the same name as existing commands.", newAlias.ToUpper());
                        }
                        else if (deleteAlias != null)
                        {
                            var alias = _aliasRepository.GetAlias(this.CommandResult.CurrentUser.Username, deleteAlias);
                            if (alias != null)
                            {
                                _aliasRepository.DeleteAlias(alias);
                                this.CommandResult.WriteLine("Alias '{0}' was successfully deleted.", deleteAlias.ToUpper());
                            }
                            else
                                this.CommandResult.WriteLine("You have not defined an alias named '{0}'.", deleteAlias.ToUpper());
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
