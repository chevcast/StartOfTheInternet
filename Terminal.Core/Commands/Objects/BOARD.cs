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
    public class BOARD : ICommand
    {
        private IDataBucket _dataBucket;

        public BOARD(IDataBucket dataBucket)
        {
            _dataBucket = dataBucket;
        }

        public CommandResult CommandResult { get; set; }

        public IEnumerable<ICommand> AvailableCommands { get; set; }

        public string[] Roles
        {
            get { return RoleTemplates.Everyone; }
        }

        public string Name
        {
            get { return "BOARD"; }
        }

        public string Parameters
        {
            get { return "<BoardID> [Page#/Option(s)]"; }
        }

        public string Description
        {
            get { return "Displays a list of topics posted on the specified board."; }
        }

        public bool ShowHelp
        {
            get { return true; }
        }

        public void Invoke(string[] args)
        {
            bool showHelp = false;
            bool newTopic = false;
            bool modTopic = false;
            bool refresh = false;
            bool? lockBoard = null;

            var options = new OptionSet();
            options.Add(
                "?|help",
                "Show help information.",
                x => showHelp = x != null
            );
            options.Add(
                "R|refresh",
                "Refresh the current board.",
                x => refresh = x != null
            );
            if (CommandResult.IsUserLoggedIn)
            {
                options.Add(
                    "nt|newTopic",
                    "Create new topic on the specified board.",
                    x => newTopic = x != null
                );
            }
            if (CommandResult.UserLoggedAndModOrAdmin())
            {
                options.Add(
                    "mt|modTopic",
                    "Create a topic that only moderators can see.",
                    x =>
                    {
                        newTopic = x != null;
                        modTopic = x != null;
                    }
                );
            }
            if (CommandResult.UserLoggedAndAdmin())
            {
                options.Add(
                    "l|lock",
                    "Lock the board to prevent creation of topics.",
                    x => lockBoard = x != null
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
                            if (parsedArgs[0].IsShort())
                            {
                                var boardId = parsedArgs[0].ToShort();
                                var page = 1;
                                WriteTopics(boardId, page);
                            }
                            else
                                CommandResult.WriteLine("'{0}' is not a valid board ID.", parsedArgs[0]);
                        }
                        else if (parsedArgs.Length == 2)
                        {
                            if (parsedArgs[0].IsShort())
                            {
                                var boardId = parsedArgs[0].ToShort();
                                if (parsedArgs[1].IsInt())
                                {
                                    var page = parsedArgs[1].ToInt();
                                    WriteTopics(boardId, page);
                                }
                                else if (PagingUtility.Shortcuts.Any(x => parsedArgs[1].Is(x)))
                                {
                                    var page = PagingUtility.TranslateShortcut(parsedArgs[1], CommandResult.CommandContext.CurrentPage);
                                    WriteTopics(boardId, page);
                                    if (parsedArgs[1].Is("last") || parsedArgs[1].Is("prev"))
                                        CommandResult.ScrollToBottom = false;
                                }
                                else
                                    CommandResult.WriteLine("'{0}' is not a valid page number.", parsedArgs[1]);
                            }
                            else
                                CommandResult.WriteLine("'{0}' is not a valid board ID.", parsedArgs[0]);
                        }
                        else
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
                        else if (newTopic)
                        {
                            if (parsedArgs.Length >= 1)
                            {
                                if (parsedArgs[0].IsShort())
                                {
                                    var boardId = parsedArgs[0].ToShort();
                                    var board = _dataBucket.BoardRepository.GetBoard(boardId);
                                    if (board != null)
                                    {
                                        if (!board.Locked || CommandResult.CurrentUser.IsModeratorOrAdministrator())
                                        {
                                            if (!board.ModsOnly || CommandResult.CurrentUser.IsModeratorOrAdministrator())
                                            {
                                                if (CommandResult.CommandContext.PromptData == null)
                                                {
                                                    CommandResult.WriteLine("Create a title for your topic.");
                                                    CommandResult.CommandContext.SetPrompt(Name, args, string.Format("{0} NEW TOPIC Title", boardId));
                                                }
                                                else if (CommandResult.CommandContext.PromptData.Length == 1)
                                                {
                                                    CommandResult.WriteLine("Create the body for your topic.");
                                                    CommandResult.CommandContext.SetPrompt(Name, args, string.Format("{0} NEW TOPIC Body", boardId));
                                                }
                                                else if (CommandResult.CommandContext.PromptData.Length == 2)
                                                {
                                                    var topic = new Topic
                                                    {
                                                        BoardID = boardId,
                                                        Title = CommandResult.CommandContext.PromptData[0],
                                                        Body = BBCodeUtility.SimplifyComplexTags(
                                                            CommandResult.CommandContext.PromptData[1],
                                                            _dataBucket.ReplyRepository,
                                                            CommandResult.CurrentUser.IsModeratorOrAdministrator()
                                                        ),
                                                        Username = CommandResult.CurrentUser.Username,
                                                        PostedDate = DateTime.UtcNow,
                                                        LastEdit = DateTime.UtcNow,
                                                        ModsOnly = modTopic && !board.ModsOnly
                                                    };
                                                    _dataBucket.TopicRepository.AddTopic(topic);
                                                    _dataBucket.SaveChanges();
                                                    CommandResult.CommandContext.Restore();
                                                    var TOPIC = AvailableCommands.SingleOrDefault(x => x.Name.Is("TOPIC"));
                                                    TOPIC.Invoke(new string[] { topic.TopicID.ToString() });
                                                    CommandResult.WriteLine("New topic succesfully posted.");
                                                }
                                            }
                                            else
                                                CommandResult.WriteLine("Board '{0}' is for moderators only.", boardId);
                                        }
                                        else
                                            CommandResult.WriteLine("Board '{0}' is locked. You cannot create topics on this board.", boardId);
                                    }
                                    else
                                        CommandResult.WriteLine("There is no board with ID '{0}'.", boardId);

                                }
                                else
                                    CommandResult.WriteLine("'{0}' is not a valid board ID.", parsedArgs[0]);
                            }
                            else
                                CommandResult.WriteLine("You must supply a board ID.");
                        }
                        else
                        {
                            if (lockBoard != null)
                            {
                                if (parsedArgs.Length > 0)
                                {
                                    if (parsedArgs[0].IsShort())
                                    {
                                        var boardId = parsedArgs[0].ToShort();
                                        var board = _dataBucket.BoardRepository.GetBoard(boardId);
                                        if (board != null)
                                        {
                                            board.Locked = (bool)lockBoard;
                                            _dataBucket.BoardRepository.UpdateBoard(board);
                                            _dataBucket.SaveChanges();
                                            string status = (bool)lockBoard ? "locked" : "unlocked";
                                            CommandResult.WriteLine("Board '{0}' was successfully {1}.", boardId, status);
                                        }
                                        else
                                            CommandResult.WriteLine("There is no board with ID '{0}'.", boardId);
                                    }
                                    else
                                        CommandResult.WriteLine("'{0}' is not a valid board ID.", parsedArgs[0]);
                                }
                                else
                                    CommandResult.WriteLine("You must supply a board ID.");
                            }
                            if (refresh)
                            {
                                if (parsedArgs.Length > 0)
                                {
                                    if (parsedArgs[0].IsShort())
                                    {
                                        var boardId = parsedArgs[0].ToShort();
                                        WriteTopics(boardId, CommandResult.CommandContext.CurrentPage);
                                    }
                                    else
                                        CommandResult.WriteLine("'{0}' is not a valid board ID.", parsedArgs[0]);
                                }
                                else
                                    CommandResult.WriteLine("You must supply a board ID.");
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

        private void WriteTopics(short boardId, int page)
        {
            var board = _dataBucket.BoardRepository.GetBoard(boardId);
            if (board != null)
            {
                if (!board.ModsOnly || CommandResult.UserLoggedAndModOrAdmin())
                {
                    CommandResult.ClearScreen = true;
                    CommandResult.ScrollToBottom = true;
                    var boardPage = _dataBucket.TopicRepository.GetTopics(boardId, page, AppSettings.TopicsPerPage, CommandResult.UserLoggedAndModOrAdmin());
                    if (page > boardPage.TotalPages)
                        page = boardPage.TotalPages;
                    else if (page < 1)
                        page = 1;
                    CommandResult.WriteLine(DisplayMode.Inverted | DisplayMode.DontType, "{{[transmit=BOARD]{0}[/transmit]}} {1}{2}", board.BoardID, board.Locked ? "[LOCKED] " : string.Empty, board.Name);
                    CommandResult.WriteLine();
                    CommandResult.WriteLine(DisplayMode.DontType, "Page {0}/{1}", page, boardPage.TotalPages);
                    CommandResult.WriteLine();
                    CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
                    CommandResult.WriteLine();
                    foreach (var topic in boardPage.Items)
                    {
                        var displayMode = DisplayMode.DontType;
                        if ((topic.IsModsOnly() && !board.ModsOnly) || (topic.Board.Hidden && boardId == 0))
                            displayMode |= DisplayMode.Dim;
                        var replies = topic.GetReplies(CommandResult.UserLoggedAndModOrAdmin());
                        var lastReply = replies.LastOrDefault();
                        var status = new StringBuilder();
                        if (topic.Board.Hidden && !board.Hidden)
                            status.Append("[HIDDEN] ");
                        if (topic.IsModsOnly() && !board.ModsOnly)
                            status.Append("[MODSONLY] ");
                        if ((boardId != 0 && topic.Stickied) || (boardId == 0 && topic.GlobalSticky))
                            status.Append("[STICKY] ");
                        if (topic.Locked)
                            status.Append("[LOCKED] ");
                        string boardNumber = boardId == 0 ? string.Format("{{[transmit=BOARD]{0}[/transmit]}} > ", topic.BoardID) : string.Empty;
                        CommandResult.WriteLine(displayMode, "{0}{{[transmit=TOPIC]{1}[/transmit]}} {2}{3}", boardNumber, topic.TopicID, status, topic.Title);
                        var topicAuthor = topic.Board.Anonymous && (!CommandResult.IsUserLoggedIn || (!CommandResult.CurrentUser.IsModeratorOrAdministrator() && topic.Username != CommandResult.CurrentUser.Username)) ? "Anon" : topic.Username;
                        CommandResult.WriteLine(displayMode, "   by [transmit=USER]{0}[/transmit] {1} | {2} replies", topicAuthor, topic.PostedDate.TimePassed(), replies.Count());
                        if (lastReply != null)
                        {
                            var lastReplyAuthor = topic.Board.Anonymous && (!CommandResult.IsUserLoggedIn || (!CommandResult.CurrentUser.IsModeratorOrAdministrator() && lastReply.Username != CommandResult.CurrentUser.Username)) ? "Anon" : lastReply.Username;
                            CommandResult.WriteLine(displayMode, "   last reply by [transmit=USER]{0}[/transmit] {1}", lastReplyAuthor, lastReply.PostedDate.TimePassed());
                        }
                        CommandResult.WriteLine();
                        CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
                        CommandResult.WriteLine();
                    }
                    if (boardPage.TotalItems == 0)
                    {
                        CommandResult.WriteLine("There are no topics on this board.");
                        CommandResult.WriteLine();
                        CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
                        CommandResult.WriteLine();
                    }
                    CommandResult.WriteLine(DisplayMode.DontType, "Page {0}/{1}", page, boardPage.TotalPages);
                    CommandResult.CommandContext.CurrentPage = page;
                    CommandResult.CommandContext.Set(ContextStatus.Passive, Name, new string[] { boardId.ToString() }, string.Format("{0}", boardId));
                }
                else
                    CommandResult.WriteLine("Board '{0}' is for moderators only.", boardId);
            }
            else
                CommandResult.WriteLine("There is no board with ID '{0}'.", boardId);
        }
    }
}
