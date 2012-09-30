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
    public class ALIAS : ICommand
    {
        private IDataBucket _dataBucket;

        public ALIAS(IDataBucket dataBucket)
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
                CommandResult.WriteLine(DisplayTemplates.InvalidArguments);
            }
            else
            {
                try
                {
                    var parsedArgs = options.Parse(args).ToArray();

                    if (parsedArgs.Length == args.Length)
                    {
                        CommandResult.WriteLine(DisplayTemplates.InvalidArguments);
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
                        else if (newAlias != null)
                        {
                            if (!newAlias.Is("HELP") && !AvailableCommands.Any(x => x.Name.Is(newAlias)))
                            {
                                var alias = _dataBucket.AliasRepository.GetAlias(CommandResult.CurrentUser.Username, newAlias);
                                if (alias == null)
                                {
                                    if (CommandResult.CommandContext.PromptData == null)
                                    {
                                        CommandResult.WriteLine("Type the value that should be sent to the terminal when you use your new alias.");
                                        CommandResult.CommandContext.SetPrompt(Name, args, string.Format("{0} VALUE", newAlias.ToUpper()));
                                    }
                                    else if (CommandResult.CommandContext.PromptData.Length == 1)
                                    {
                                        _dataBucket.AliasRepository.AddAlias(new Alias
                                        {
                                            Username = CommandResult.CurrentUser.Username,
                                            Shortcut = newAlias,
                                            Command = CommandResult.CommandContext.PromptData[0]
                                        });
                                        _dataBucket.SaveChanges();
                                        CommandResult.WriteLine("Alias '{0}' was successfully defined.", newAlias.ToUpper());
                                        CommandResult.CommandContext.Restore();
                                    }
                                }
                                else
                                    CommandResult.WriteLine("You have already defined an alias named '{0}'.", newAlias.ToUpper());
                            }
                            else
                                CommandResult.WriteLine("'{0}' is an existing command. You cannot create aliases with the same name as existing commands.", newAlias.ToUpper());
                        }
                        else if (deleteAlias != null)
                        {
                            var alias = _dataBucket.AliasRepository.GetAlias(CommandResult.CurrentUser.Username, deleteAlias);
                            if (alias != null)
                            {
                                _dataBucket.AliasRepository.DeleteAlias(alias);
                                _dataBucket.SaveChanges();
                                CommandResult.WriteLine("Alias '{0}' was successfully deleted.", deleteAlias.ToUpper());
                            }
                            else
                                CommandResult.WriteLine("You have not defined an alias named '{0}'.", deleteAlias.ToUpper());
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
