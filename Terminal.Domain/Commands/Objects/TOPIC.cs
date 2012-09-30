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
    public class TOPIC : ICommand
    {
        private IReplyRepository _replyRepository;
        private ITopicRepository _topicRepository;
        private IBoardRepository _boardRepository;

        public TOPIC(
            IReplyRepository replyRepository,
            ITopicRepository topicRepository,
            IBoardRepository boardRepository
        )
        {
            _replyRepository = replyRepository;
            _topicRepository = topicRepository;
            _boardRepository = boardRepository;
        }

        public CommandResult CommandResult { get; set; }

        public IEnumerable<ICommand> AvailableCommands { get; set; }

        public string[] Roles
        {
            get { return RoleTemplates.Everyone; }
        }

        public string Name
        {
            get { return "TOPIC"; }
        }

        public string Parameters
        {
            get { return "<TopicID> [Page#/Option(s)]"; }
        }

        public string Description
        {
            get { return "Displays a specified topic."; }
        }

        public bool ShowHelp
        {
            get { return true; }
        }

        public void Invoke(string[] args)
        {
            bool showHelp = false;
            bool replyToTopic = false;
            bool refresh = false;
            bool edit = false;
            bool delete = false;
            bool report = false;
            bool modReply = false;
            bool? lockTopic = null;
            bool? stickyTopic = null;
            bool? globalStickyTopic = null;
            long? replyId = null;
            short? moveToBoard = null;

            var options = new OptionSet();
            options.Add(
                "?|help",
                "Show help information.",
                x => showHelp = x != null
            );
            options.Add(
                "R|refresh",
                "Refresh the current topic.",
                x => refresh = x != null
            );

            if (this.CommandResult.IsUserLoggedIn)
            {
                options.Add(
                    "r|reply:",
                    "Reply to the topic. Optionally quotes a reply if a {ReplyID} is specified.",
                    x =>
                    {
                        replyToTopic = true;
                        if (x.IsLong())
                            replyId = x.ToLong();
                    }
                );
                options.Add(
                    "e|edit:",
                    "Edits the topic or a reply if a {ReplyID} is specified.",
                    x =>
                    {
                        edit = true;
                        if (x.IsLong())
                            replyId = x.ToLong();
                    }
                );
                options.Add(
                    "d|delete:",
                    "Deletes the topic or a reply if a {ReplyID} is specified.",
                    x =>
                    {
                        delete = true;
                        if (x.IsLong())
                            replyId = x.ToLong();
                    }
                );
                options.Add(
                    "report:",
                    "Report abuse for the topic or a reply if a {ReplyID} is specified.",
                    x =>
                    {
                        report = true;
                        if (x.IsLong())
                            replyId = x.ToLong();
                    }
                );
            }

            if (this.CommandResult.UserLoggedAndModOrAdmin())
            {
                options.Add(
                    "mr|modReply",
                    "A reply only moderators can see.",
                    x =>
                    {
                        modReply = x != null;
                        replyToTopic = x != null;
                    }
                );
                options.Add(
                    "l|lock",
                    "Locks the topic preventing further replies except modreplies.",
                    x => lockTopic = x != null
                );
                options.Add(
                    "s|sticky",
                    "Sticky's the topic keeping it at the top of the board regardless of last reply date.",
                    x => stickyTopic = x != null
                );
                options.Add(
                    "g|globalSticky",
                    "Sticky's the topic keeping it at the top of \"All Activity Board\".",
                    x => globalStickyTopic = x != null
                );
                options.Add(
                    "m|move=",
                    "Moves the topic to the board with the specified {BoardID}.",
                    x => moveToBoard = x.ToShort()
                );
            }

            try
            {
                if (args == null)
                {
                    this.CommandResult.WriteLine(DisplayTemplates.InvalidArguments);
                }
                else
                {
                    var parsedArgs = options.Parse(args).ToArray();
                    if (parsedArgs.Length == args.Length)
                    {
                        if (parsedArgs.Length == 1)
                        {
                            if (parsedArgs[0].IsLong())
                            {
                                var topicId = parsedArgs[0].ToLong();
                                var page = 1;
                                WriteTopic(topicId, page);
                            }
                            else
                                this.CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                        }
                        else if (parsedArgs.Length == 2)
                        {
                            if (parsedArgs[0].IsInt())
                            {
                                var topicId = parsedArgs[0].ToLong();
                                if (parsedArgs[1].IsInt())
                                {
                                    var page = parsedArgs[1].ToInt();
                                    WriteTopic(topicId, page);
                                }
                                else if (PagingUtility.Shortcuts.Any(x => parsedArgs[1].Is(x)))
                                {
                                    var page = PagingUtility.TranslateShortcut(parsedArgs[1], this.CommandResult.CommandContext.CurrentPage);
                                    WriteTopic(topicId, page);
                                    if (parsedArgs[1].Is("last") || parsedArgs[1].Is("prev"))
                                        this.CommandResult.ScrollToBottom = true;
                                }
                                else
                                    this.CommandResult.WriteLine("'{0}' is not a valid page number.", parsedArgs[1]);
                            }
                            else
                                this.CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                        }
                        else
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
                        else if (replyToTopic)
                        {
                            if (parsedArgs.Length > 0)
                            {
                                if (parsedArgs[0].IsLong())
                                {
                                    var topicId = parsedArgs[0].ToLong();
                                    var topic = _topicRepository.GetTopic(topicId);
                                    if (topic != null)
                                    {
                                        if (!topic.Locked || (!topic.IsModsOnly() && modReply && this.CommandResult.CurrentUser.IsModerator) || this.CommandResult.CurrentUser.IsAdministrator)
                                        {
                                            if (!topic.IsModsOnly() || this.CommandResult.CurrentUser.IsModeratorOrAdministrator())
                                            {
                                                if (this.CommandResult.CommandContext.PromptData == null)
                                                {
                                                    this.CommandResult.WriteLine("Type your reply.");
                                                    if (replyId != null)
                                                        this.CommandResult.EditText = string.Format("[quote]{0}[/quote]\r\n\r\n", replyId);
                                                    this.CommandResult.CommandContext.SetPrompt(this.Name, args, string.Format("{0} REPLY", topicId));
                                                }
                                                else
                                                {
                                                    _replyRepository.AddReply(new Reply
                                                    {
                                                        Username = this.CommandResult.CurrentUser.Username,
                                                        PostedDate = DateTime.UtcNow,
                                                        LastEdit = DateTime.UtcNow,
                                                        TopicID = topicId,
                                                        Body = BBCodeUtility.SimplifyComplexTags(
                                                            this.CommandResult.CommandContext.PromptData[0],
                                                            _replyRepository,
                                                            this.CommandResult.CurrentUser.IsModeratorOrAdministrator()
                                                        ),
                                                        ModsOnly = modReply && !topic.IsModsOnly()
                                                    });
                                                    this.CommandResult.CommandContext.PromptData = null;
                                                    //var TOPIC = this.AvailableCommands.SingleOrDefault(x => x.Name.Is("TOPIC"));
                                                    //TOPIC.Invoke(new string[] { topicId.ToString(), "last" });
                                                    this.CommandResult.CommandContext.Restore();
                                                    this.CommandResult.WriteLine("Reply successfully posted.");
                                                }
                                            }
                                            else
                                                this.CommandResult.WriteLine("Topic '{0}' is for moderators only.", topicId);
                                        }
                                        else
                                            this.CommandResult.WriteLine("Topic '{0}' is locked.", topicId);
                                    }
                                    else
                                        this.CommandResult.WriteLine("There is no topic with ID '{0}'.", topicId);
                                }
                                else
                                    this.CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                            }
                            else
                                this.CommandResult.WriteLine("You must supply a topic ID.");
                        }
                        else if (edit)
                        {
                            if (parsedArgs.Length > 0)
                            {
                                if (parsedArgs[0].IsLong())
                                {
                                    var topicId = parsedArgs[0].ToLong();
                                    if (replyId == null)
                                    {
                                        var topic = _topicRepository.GetTopic(topicId);
                                        if (topic != null)
                                        {
                                            if (!topic.Locked || this.CommandResult.CurrentUser.IsAdministrator)
                                            {
                                                if (topic.Username.Is(this.CommandResult.CurrentUser.Username)
                                                    || (this.CommandResult.CurrentUser.IsModerator && !topic.IsModsOnly() && !topic.User.IsModerator && !topic.User.IsAdministrator)
                                                    || this.CommandResult.CurrentUser.IsAdministrator)
                                                {
                                                    if (this.CommandResult.CommandContext.PromptData == null)
                                                    {
                                                        this.CommandResult.WriteLine("Edit the topic title.");
                                                        this.CommandResult.EditText = topic.Title;
                                                        this.CommandResult.CommandContext.SetPrompt(this.Name, args, string.Format("{0} EDIT Title", topicId));
                                                    }
                                                    else if (this.CommandResult.CommandContext.PromptData.Length == 1)
                                                    {
                                                        this.CommandResult.WriteLine("Edit the topic body.");
                                                        this.CommandResult.EditText = topic.Body;
                                                        this.CommandResult.CommandContext.SetPrompt(this.Name, args, string.Format("{0} EDIT Body", topicId));
                                                    }
                                                    else if (this.CommandResult.CommandContext.PromptData.Length == 2)
                                                    {
                                                        topic.Title = this.CommandResult.CommandContext.PromptData[0];
                                                        topic.Body = BBCodeUtility.SimplifyComplexTags(
                                                            this.CommandResult.CommandContext.PromptData[1],
                                                            _replyRepository,
                                                            this.CommandResult.CurrentUser.IsModeratorOrAdministrator()
                                                        );
                                                        topic.LastEdit = DateTime.UtcNow;
                                                        topic.EditedBy = this.CommandResult.CurrentUser.Username;
                                                        _topicRepository.UpdateTopic(topic);
                                                        this.CommandResult.CommandContext.PromptData = null;
                                                        this.CommandResult.WriteLine("Topic '{0}' was edited successfully.", topicId);
                                                        this.CommandResult.CommandContext.Restore();
                                                    }
                                                }
                                                else
                                                    this.CommandResult.WriteLine("Topic '{0}' does not belong to you. You are not authorized to edit it.", topicId);
                                            }
                                            else
                                                this.CommandResult.WriteLine("Topic '{0}' is locked.", topicId);
                                        }
                                        else
                                            this.CommandResult.WriteLine("There is no topic with ID '{0}'.", topicId);
                                    }
                                    else
                                    {
                                        var reply = _replyRepository.GetReply((long)replyId);
                                        if (reply != null)
                                        {
                                            if (reply.TopicID == topicId)
                                            {
                                                if (!reply.Topic.Locked || (reply.ModsOnly && !reply.Topic.IsModsOnly()) || this.CommandResult.CurrentUser.IsAdministrator)
                                                {
                                                    if (reply.Username.Is(this.CommandResult.CurrentUser.Username)
                                                        || (this.CommandResult.CurrentUser.IsModerator && !reply.IsModsOnly() && !reply.User.IsModeratorOrAdministrator())
                                                        || this.CommandResult.CurrentUser.IsAdministrator)
                                                    {
                                                        if (this.CommandResult.CommandContext.PromptData == null)
                                                        {
                                                            this.CommandResult.WriteLine("Edit the reply body.");
                                                            this.CommandResult.EditText = reply.Body;
                                                            this.CommandResult.CommandContext.SetPrompt(this.Name, args, string.Format("{0} EDIT Reply {1}", topicId, replyId));
                                                        }
                                                        else if (this.CommandResult.CommandContext.PromptData.Length == 1)
                                                        {
                                                            reply.Body = BBCodeUtility.SimplifyComplexTags(
                                                                this.CommandResult.CommandContext.PromptData[0],
                                                                _replyRepository,
                                                                this.CommandResult.CurrentUser.IsModeratorOrAdministrator()
                                                            );
                                                            reply.LastEdit = DateTime.UtcNow;
                                                            reply.EditedBy = this.CommandResult.CurrentUser.Username;
                                                            _replyRepository.UpdateReply(reply);
                                                            this.CommandResult.CommandContext.PromptData = null;
                                                            this.CommandResult.WriteLine("Reply '{0}' was edited successfully.", replyId);
                                                            this.CommandResult.CommandContext.Restore();
                                                        }
                                                    }
                                                    else
                                                        this.CommandResult.WriteLine("Reply '{0}' does not belong to you. You are not authorized to edit it.", replyId);
                                                }
                                                else
                                                    this.CommandResult.WriteLine("Topic '{0}' is locked.", topicId);
                                            }
                                            else
                                                this.CommandResult.WriteLine("Topic '{0}' does not contain a reply with ID '{1}'.", topicId, replyId);
                                        }
                                        else
                                            this.CommandResult.WriteLine("Reply '{0}' does not exist.", replyId);
                                    }
                                }
                                else
                                    this.CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                            }
                            else
                                this.CommandResult.WriteLine("You must supply a topic ID.");
                        }
                        else if (delete)
                        {
                            if (parsedArgs.Length > 0)
                            {
                                if (parsedArgs[0].IsLong())
                                {
                                    var topicId = parsedArgs[0].ToLong();
                                    if (replyId == null)
                                    {
                                        var topic = _topicRepository.GetTopic(topicId);
                                        if (topic != null)
                                        {
                                            if (!topic.Locked || this.CommandResult.CurrentUser.IsAdministrator)
                                            {
                                                if (this.CommandResult.CurrentUser.IsAdministrator)
                                                {
                                                    if (this.CommandResult.CommandContext.PromptData == null)
                                                    {
                                                        this.CommandResult.WriteLine("Are you sure you want to delete the topic titled \"{0}\"? (Y/N)", topic.Title);
                                                        this.CommandResult.CommandContext.SetPrompt(this.Name, args, string.Format("{0} DELETE CONFIRM", topicId));
                                                    }
                                                    else if (this.CommandResult.CommandContext.PromptData.Length == 1)
                                                    {
                                                        if (this.CommandResult.CommandContext.PromptData[0].Is("Y"))
                                                        {
                                                            _topicRepository.DeleteTopic(topic);
                                                            this.CommandResult.WriteLine("Topic '{0}' was deleted successfully.", topicId);
                                                            this.CommandResult.CommandContext.PromptData = null;
                                                            if (this.CommandResult.CommandContext.PreviousContext != null
                                                                && this.CommandResult.CommandContext.PreviousContext.Command.Is("TOPIC")
                                                                && this.CommandResult.CommandContext.PreviousContext.Args.Contains(topicId.ToString())
                                                                && this.CommandResult.CommandContext.PreviousContext.Status == ContextStatus.Passive)
                                                            {
                                                                this.CommandResult.ClearScreen = true;
                                                                this.CommandResult.CommandContext.Deactivate();
                                                            }
                                                            else
                                                                this.CommandResult.CommandContext.Restore();
                                                        }
                                                        else
                                                        {
                                                            this.CommandResult.WriteLine("Topic '{0}' was not deleted.", topicId);
                                                            this.CommandResult.CommandContext.PromptData = null;
                                                            this.CommandResult.CommandContext.Restore();
                                                        }
                                                    }
                                                }
                                                else
                                                    this.CommandResult.WriteLine("You are not an administrator. You are not authorized to delete topics.");
                                            }
                                            else
                                                this.CommandResult.WriteLine("Topic '{0}' is locked.", topicId);
                                        }
                                        else
                                            this.CommandResult.WriteLine("There is no topic with ID {0}.", topicId);
                                    }
                                    else
                                    {
                                        var reply = _replyRepository.GetReply((long)replyId);
                                        if (reply != null)
                                        {
                                            if (reply.TopicID == topicId)
                                            {
                                                if (!reply.Topic.Locked || (reply.ModsOnly && !reply.Topic.IsModsOnly()) || this.CommandResult.CurrentUser.IsAdministrator)
                                                {
                                                    if (reply.Username.Is(this.CommandResult.CurrentUser.Username)
                                                        || (this.CommandResult.CurrentUser.IsModerator && !reply.IsModsOnly() && !reply.User.IsModeratorOrAdministrator())
                                                        || this.CommandResult.CurrentUser.IsAdministrator)
                                                    {
                                                        _replyRepository.DeleteReply(reply);
                                                        this.CommandResult.WriteLine("Reply '{0}' was deleted successfully.", replyId);
                                                    }
                                                    else
                                                        this.CommandResult.WriteLine("Reply '{0}' does not belong to you. You are not authorized to delete it.", replyId);
                                                }
                                                else
                                                    this.CommandResult.WriteLine("Topic '{0}' is locked.", topicId);
                                            }
                                            else
                                                this.CommandResult.WriteLine("Topic '{0}' does not contain a reply with ID '{1}'.", topicId, replyId);
                                        }
                                        else
                                            this.CommandResult.WriteLine("Reply '{0}' does not exist.", replyId);
                                    }
                                }
                                else
                                    this.CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                            }
                            else
                                this.CommandResult.WriteLine("You must supply a topic ID.");
                        }
                        else if (report)
                        {
                            // allow user to report abuse.
                        }
                        else
                        {
                            if (lockTopic != null)
                            {
                                if (parsedArgs.Length > 0)
                                {
                                    if (parsedArgs[0].IsLong())
                                    {
                                        var topicId = parsedArgs[0].ToLong();
                                        var topic = _topicRepository.GetTopic(topicId);
                                        if (topic != null)
                                        {
                                            if (!topic.IsModsOnly() || this.CommandResult.CurrentUser.IsAdministrator)
                                            {
                                                if ((bool)lockTopic || (!(bool)lockTopic && this.CommandResult.CurrentUser.IsAdministrator))
                                                {
                                                    topic.Locked = (bool)lockTopic;
                                                    _topicRepository.UpdateTopic(topic);
                                                    string status = (bool)lockTopic ? "locked" : "unlocked";
                                                    this.CommandResult.WriteLine("Topic '{0}' was successfully {1}.", topicId, status);
                                                }
                                                else
                                                    this.CommandResult.WriteLine("Only administrators can unlock topics.");
                                            }
                                            else
                                                this.CommandResult.WriteLine("You are not authorized to lock moderator topics.");
                                        }
                                        else
                                            this.CommandResult.WriteLine("There is no topic with ID {0}.", topicId);
                                    }
                                    else
                                        this.CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                                }
                                else
                                    this.CommandResult.WriteLine("You must supply a topic ID.");
                            }
                            if (stickyTopic != null)
                            {
                                if (parsedArgs.Length > 0)
                                {
                                    if (parsedArgs[0].IsLong())
                                    {
                                        var topicId = parsedArgs[0].ToLong();
                                        var topic = _topicRepository.GetTopic(topicId);
                                        if (topic != null)
                                        {
                                            if (!topic.IsModsOnly() || this.CommandResult.CurrentUser.IsAdministrator)
                                            {
                                                topic.Stickied = (bool)stickyTopic;
                                                _topicRepository.UpdateTopic(topic);
                                                string status = (bool)stickyTopic ? "stickied" : "unstickied";
                                                this.CommandResult.WriteLine("Topic '{0}' was successfully {1}.", topicId, status);
                                            }
                                            else
                                                this.CommandResult.WriteLine("You are not authorized to sticky/unsticky moderator topics.");
                                        }
                                        else
                                            this.CommandResult.WriteLine("There is no topic with ID '{0}'.", topicId);
                                    }
                                    else
                                        this.CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                                }
                                else
                                    this.CommandResult.WriteLine("You must supply a topic ID.");
                            }
                            if (globalStickyTopic != null)
                            {
                                if (parsedArgs.Length > 0)
                                {
                                    if (parsedArgs[0].IsLong())
                                    {
                                        var topicId = parsedArgs[0].ToLong();
                                        var topic = _topicRepository.GetTopic(topicId);
                                        if (topic != null)
                                        {
                                            if (!topic.IsModsOnly() || this.CommandResult.CurrentUser.IsAdministrator)
                                            {
                                                topic.GlobalSticky = (bool)globalStickyTopic;
                                                _topicRepository.UpdateTopic(topic);
                                                string status = (bool)globalStickyTopic ? "stickied" : "unstickied";
                                                this.CommandResult.WriteLine("Topic '{0}' was successfully globally {1}.", topicId, status);
                                            }
                                            else
                                                this.CommandResult.WriteLine("You are not authorized to globally sticky/unsticky moderator topics.");
                                        }
                                        else
                                            this.CommandResult.WriteLine("There is no topic with ID '{0}'.", topicId);
                                    }
                                    else
                                        this.CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                                }
                                else
                                    this.CommandResult.WriteLine("You must supply a topic ID.");
                            }
                            if (moveToBoard != null)
                            {
                                if (parsedArgs.Length > 0)
                                {
                                    if (parsedArgs[0].IsLong())
                                    {
                                        var topicId = parsedArgs[0].ToLong();
                                        var topic = _topicRepository.GetTopic(topicId);
                                        if (topic != null)
                                        {
                                            var board = _boardRepository.GetBoard((short)moveToBoard);
                                            if (board != null)
                                            {
                                                if (!board.Locked || this.CommandResult.CurrentUser.IsAdministrator)
                                                {
                                                    if (!topic.IsModsOnly() || this.CommandResult.CurrentUser.IsAdministrator)
                                                    {
                                                        if (!board.ModsOnly || this.CommandResult.CurrentUser.IsAdministrator)
                                                        {
                                                            topic.BoardID = (short)moveToBoard;
                                                            _topicRepository.UpdateTopic(topic);
                                                            this.CommandResult.WriteLine("Topic '{0}' was successfully moved to board '{1}'.", topicId, moveToBoard);
                                                        }
                                                        else
                                                            this.CommandResult.WriteLine("You are not authorized to move topics onto moderator boards.");
                                                    }
                                                    else
                                                        this.CommandResult.WriteLine("You are not authorized to move moderator topics.");
                                                }
                                                else
                                                    this.CommandResult.WriteLine("Board '{0}' is locked.", moveToBoard);
                                            }
                                            else
                                                this.CommandResult.WriteLine("There is no board with ID '{0}'.", moveToBoard);
                                        }
                                        else
                                            this.CommandResult.WriteLine("There is no topic with ID '{0}'.", topicId);
                                    }
                                    else
                                        this.CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                                }
                                else
                                    this.CommandResult.WriteLine("You must supply a board ID.");
                            }
                            if (refresh)
                            {
                                if (parsedArgs.Length > 0)
                                {
                                    if (parsedArgs[0].IsLong())
                                    {
                                        var topicId = parsedArgs[0].ToLong();
                                        WriteTopic(topicId, this.CommandResult.CommandContext.CurrentPage);
                                    }
                                    else
                                        this.CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                                }
                                else
                                    this.CommandResult.WriteLine("You must supply a topic ID.");
                            }
                        }
                    }
                }
            }
            catch (OptionException ex)
            {
                this.CommandResult.WriteLine(ex.Message);
            }
        }

        private void WriteTopic(long topicId, int page)
        {
            var topic = _topicRepository.GetTopic(topicId);
            if (topic != null)
            {
                if (!topic.IsModsOnly() || this.CommandResult.UserLoggedAndModOrAdmin())
                {
                    this.CommandResult.ScrollToBottom = false;
                    this.CommandResult.ClearScreen = true;
                    var topicPage = _replyRepository.GetReplies(topicId, page, AppSettings.RepliesPerPage, this.CommandResult.UserLoggedAndModOrAdmin());
                    if (page > topicPage.TotalPages)
                        page = topicPage.TotalPages;
                    else if (page < 1)
                        page = 1;
                    StringBuilder status = new StringBuilder();
                    if (topic.Board.Hidden)
                        status.Append("[HIDDEN] ");
                    if (topic.IsModsOnly())
                        status.Append("[MODSONLY] ");
                    if (topic.Stickied || topic.GlobalSticky)
                        status.Append("[STICKY] ");
                    if (topic.Locked)
                        status.Append("[LOCKED] ");
                    this.CommandResult.WriteLine(DisplayMode.Inverted | DisplayMode.DontType, "{{[transmit=BOARD][topicboardid={1}]{0}[/topicboardid][/transmit]}} > {{[transmit=TOPIC]{1}[/transmit]}} [topicstatus={1}]{2}[/topicstatus][topictitle={1}]{3}[/topictitle]", topic.BoardID, topic.TopicID, status, topic.Title);
                    var topicAuthor = topic.Board.Anonymous && (!this.CommandResult.IsUserLoggedIn || (!this.CommandResult.CurrentUser.IsModeratorOrAdministrator() && topic.Username != this.CommandResult.CurrentUser.Username)) ? "Anon" : topic.Username;
                    this.CommandResult.WriteLine(DisplayMode.Italics | DisplayMode.DontType, "Posted by [transmit=USER]{0}[/transmit] on {1} | [replycount={3}]{2}[/replycount] replies", topicAuthor, topic.PostedDate.TimePassed(), topicPage.TotalItems, topic.TopicID);
                    if (topic.EditedBy != null)
                    {
                        var editedBy = topic.Board.Anonymous && (!this.CommandResult.IsUserLoggedIn || (!this.CommandResult.CurrentUser.IsModeratorOrAdministrator() && topic.EditedBy != this.CommandResult.CurrentUser.Username)) ? "Anon" : topic.EditedBy;
                        this.CommandResult.WriteLine(DisplayMode.Italics | DisplayMode.DontType, "[Edited by [transmit=USER][editedbyuser={2}]{0}[/editedbyuser][/transmit] [editedbydate={2}]{1}[/editedbydate]]", editedBy, topic.LastEdit.TimePassed(), topic.TopicID);
                    }
                    this.CommandResult.WriteLine();
                    this.CommandResult.WriteLine(DisplayMode.Parse | DisplayMode.DontType, "[topicbody={0}]{1}[/topicbody]", topic.TopicID, topic.Body);
                    this.CommandResult.WriteLine();
                    this.CommandResult.WriteLine();
                    this.CommandResult.WriteLine();
                    this.CommandResult.WriteLine(DisplayMode.DontType, "Page {0}/[topicmaxpages={2}]{1}[/topicmaxpages]", page, topicPage.TotalPages, topic.TopicID);
                    this.CommandResult.WriteLine();
                    this.CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
                    this.CommandResult.WriteLine();
                    for (int index = 0; index < topicPage.Items.Count(); index++)
                    {
                        var reply = topicPage.Items[index];
                        var displayMode = DisplayMode.DontType;
                        if (reply.ModsOnly && !topic.IsModsOnly())
                            displayMode |= DisplayMode.Dim;
                        var replyAuthor = reply.Topic.Board.Anonymous && (!this.CommandResult.IsUserLoggedIn || (!this.CommandResult.CurrentUser.IsModeratorOrAdministrator() && reply.Username != this.CommandResult.CurrentUser.Username)) ? "Anon" : reply.Username;
                        this.CommandResult.WriteLine(displayMode, "{{[transmit=-r=]{0}[/transmit]}} | Reply by [transmit=USER]{1}[/transmit] {2}", reply.ReplyID, replyAuthor, reply.PostedDate.TimePassed());
                        this.CommandResult.WriteLine();
                        this.CommandResult.WriteLine(displayMode | DisplayMode.Parse, "{0}", reply.Body);
                        this.CommandResult.WriteLine();
                        if (reply.EditedBy != null)
                        {
                            var editedBy = reply.Topic.Board.Anonymous && (!this.CommandResult.IsUserLoggedIn || (!this.CommandResult.CurrentUser.IsModeratorOrAdministrator() && reply.EditedBy != this.CommandResult.CurrentUser.Username)) ? "Anon" : reply.EditedBy;
                            this.CommandResult.WriteLine(displayMode | DisplayMode.Italics, "[Edited by [transmit=USER]{0}[/transmit] {1}]", editedBy, reply.LastEdit.TimePassed());
                            this.CommandResult.WriteLine();
                        }
                        this.CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
                        this.CommandResult.WriteLine();
                    }
                    if (topicPage.TotalItems == 0)
                    {
                        this.CommandResult.WriteLine("There are no replies to this topic.");
                        this.CommandResult.WriteLine();
                        this.CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
                        this.CommandResult.WriteLine();
                    }
                    this.CommandResult.WriteLine(DisplayMode.DontType, "Page {0}/{1}", page, topicPage.TotalPages);
                    this.CommandResult.CommandContext.CurrentPage = page;
                    this.CommandResult.CommandContext.Set(ContextStatus.Passive, this.Name, new string[] { topicId.ToString() }, string.Format("{0}", topicId));
                }
                else
                    this.CommandResult.WriteLine("Topic '{0}' is for moderators only.", topicId);
            }
            else
                this.CommandResult.WriteLine("'{0}' is not a valid topic ID.", topicId);
        }
    }
}
