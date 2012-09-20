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
    public class BOARDS : ICommand
    {
        private IBoardRepository _boardRepository;
        private ITopicRepository _topicRepository;

        public BOARDS(
            IBoardRepository boardRepository,
            ITopicRepository topicRepository)
        {
            _boardRepository = boardRepository;
            _topicRepository = topicRepository;
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

            if (args == null)
            {
                this.CommandResult.ScrollToBottom = false;
                this.CommandResult.CommandContext.Deactivate();
                this.CommandResult.ClearScreen = true;
                this.CommandResult.WriteLine(DisplayMode.Inverted, "Available Discussion Boards");
                var boards = _boardRepository.GetBoards(this.CommandResult.CurrentUser.IsModerator || this.CommandResult.CurrentUser.IsAdministrator);
                foreach (var board in boards.ToList())
                {
                    this.CommandResult.WriteLine();
                    var displayMode = DisplayMode.DontType;
                    if (board.ModsOnly || board.Hidden)
                        displayMode |= DisplayMode.Dim;
                    long topicCount = board.BoardID == 0
                        ? _topicRepository.AllTopicsCount(this.CommandResult.CurrentUser.IsModerator || this.CommandResult.CurrentUser.IsAdministrator)
                        : board.TopicCount(this.CommandResult.CurrentUser.IsModerator || this.CommandResult.CurrentUser.IsAdministrator);
                    this.CommandResult.WriteLine(displayMode, "{{[transmit=BOARD]{0}[/transmit]}} {1}{2}{3}{4} | {5} topics",
                        board.BoardID,
                        board.Hidden ? "[HIDDEN] " : string.Empty,
                        board.ModsOnly ? "[MODSONLY] " : string.Empty,
                        board.Locked ? "[LOCKED] " : string.Empty,
                        board.Name,
                        topicCount);
                    if (!board.Description.IsNullOrEmpty())
                        this.CommandResult.WriteLine(displayMode, "{0}", board.Description);
                }
                if (boards.Count() == 0)
                    this.CommandResult.WriteLine("There are no discussion boards.");
            }
            else
                try
                {
                    options.Parse(args);
                }
                catch (OptionException ex)
                {
                    this.CommandResult.WriteLine(ex.Message);
                }
        }
    }
}
