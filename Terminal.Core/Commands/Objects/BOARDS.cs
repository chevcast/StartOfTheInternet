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
    public class BOARDS : ICommand
    {
        private IDataBucket _dataBucket;

        public BOARDS(IDataBucket dataBucket)
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
            get { return "BOARDS"; }
        }

        public string Parameters
        {
            get { return "[Option(s)]"; }
        }

        public string Description
        {
            get { return "Displays a list of available discussion boards."; }
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

            if (args == null)
            {
                CommandResult.ScrollToBottom = false;
                CommandResult.DeactivateContext();
                CommandResult.ClearScreen = true;
                CommandResult.WriteLine(DisplayMode.Inverted, "Available Discussion Boards");
                var boards = _dataBucket.BoardRepository.GetBoards(CommandResult.UserLoggedAndModOrAdmin());
                foreach (var board in boards.ToList())
                {
                    CommandResult.WriteLine();
                    var displayMode = DisplayMode.DontType;
                    if (board.ModsOnly || board.Hidden)
                        displayMode |= DisplayMode.Dim;
                    long topicCount = board.BoardID == 0
                        ? _dataBucket.TopicRepository.AllTopicsCount(CommandResult.UserLoggedAndModOrAdmin())
                        : board.TopicCount(CommandResult.UserLoggedAndModOrAdmin());
                    CommandResult.WriteLine(displayMode, "{{[transmit=BOARD]{0}[/transmit]}} {1}{2}{3}{4} | {5} topics",
                        board.BoardID,
                        board.Hidden ? "[HIDDEN] " : string.Empty,
                        board.ModsOnly ? "[MODSONLY] " : string.Empty,
                        board.Locked ? "[LOCKED] " : string.Empty,
                        board.Name,
                        topicCount);
                    if (!board.Description.IsNullOrEmpty())
                        CommandResult.WriteLine(displayMode, "{0}", board.Description);
                }
                if (boards.Count() == 0)
                    CommandResult.WriteLine("There are no discussion boards.");
            }
            else
                try
                {
                    options.Parse(args);
                }
                catch (OptionException ex)
                {
                    CommandResult.WriteLine(ex.Message);
                }
        }
    }
}
