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
    public class TOPIC : ICommand
    {
        private IDataBucket _dataBucket;

        public TOPIC(IDataBucket dataBucket)
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

            if (CommandResult.IsUserLoggedIn)
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

            if (CommandResult.UserLoggedAndModOrAdmin())
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
                    CommandResult.WriteLine(DisplayTemplates.InvalidArguments);
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
                                CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
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
                                    var page = PagingUtility.TranslateShortcut(parsedArgs[1], CommandResult.CommandContext.CurrentPage);
                                    WriteTopic(topicId, page);
                                    if (parsedArgs[1].Is("last") || parsedArgs[1].Is("prev"))
                                        CommandResult.ScrollToBottom = true;
                                }
                                else
                                    CommandResult.WriteLine("'{0}' is not a valid page number.", parsedArgs[1]);
                            }
                            else
                                CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                        }
                        else
                            CommandResult.WriteLine(DisplayTemplates.InvalidArguments);
                    }
                    else
                    {
                        if (showHelp)
                        {
                            HelpUtility.WriteHelpInformation(this, options);
                        }
                        else if (replyToTopic)
                        {
                            if (parsedArgs.Length > 0)
                            {
                                if (parsedArgs[0].IsLong())
                                {
                                    var topicId = parsedArgs[0].ToLong();
                                    var topic = _dataBucket.TopicRepository.GetTopic(topicId);
                                    if (topic != null)
                                    {
                                        if (!topic.Locked || (!topic.IsModsOnly() && modReply && CommandResult.CurrentUser.IsModerator) || CommandResult.CurrentUser.IsAdministrator)
                                        {
                                            if (!topic.IsModsOnly() || CommandResult.CurrentUser.IsModeratorOrAdministrator())
                                            {
                                                if (CommandResult.CommandContext.PromptData == null)
                                                {
                                                    CommandResult.WriteLine("Type your reply.");
                                                    if (replyId != null)
                                                        CommandResult.EditText = string.Format("[quote]{0}[/quote]\r\n\r\n", replyId);
                                                    CommandResult.CommandContext.SetPrompt(Name, args, string.Format("{0} REPLY", topicId));
                                                }
                                                else
                                                {
                                                    _dataBucket.ReplyRepository.AddReply(new Reply
                                                    {
                                                        Username = CommandResult.CurrentUser.Username,
                                                        PostedDate = DateTime.UtcNow,
                                                        LastEdit = DateTime.UtcNow,
                                                        TopicID = topicId,
                                                        Body = BBCodeUtility.SimplifyComplexTags(
                                                            CommandResult.CommandContext.PromptData[0],
                                                            _dataBucket.ReplyRepository,
                                                            CommandResult.CurrentUser.IsModeratorOrAdministrator()
                                                        ),
                                                        ModsOnly = modReply && !topic.IsModsOnly()
                                                    });
                                                    _dataBucket.SaveChanges();
                                                    CommandResult.CommandContext.PromptData = null;
                                                    //var TOPIC = AvailableCommands.SingleOrDefault(x => x.Name.Is("TOPIC"));
                                                    //TOPIC.Invoke(new string[] { topicId.ToString(), "last" });
                                                    CommandResult.CommandContext.Restore();
                                                    CommandResult.WriteLine("Reply successfully posted.");
                                                }
                                            }
                                            else
                                                CommandResult.WriteLine("Topic '{0}' is for moderators only.", topicId);
                                        }
                                        else
                                            CommandResult.WriteLine("Topic '{0}' is locked.", topicId);
                                    }
                                    else
                                        CommandResult.WriteLine("There is no topic with ID '{0}'.", topicId);
                                }
                                else
                                    CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                            }
                            else
                                CommandResult.WriteLine("You must supply a topic ID.");
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
                                        var topic = _dataBucket.TopicRepository.GetTopic(topicId);
                                        if (topic != null)
                                        {
                                            if (!topic.Locked || CommandResult.CurrentUser.IsAdministrator)
                                            {
                                                if (topic.Username.Is(CommandResult.CurrentUser.Username)
                                                    || (CommandResult.CurrentUser.IsModerator && !topic.IsModsOnly() && !topic.User.IsModerator && !topic.User.IsAdministrator)
                                                    || CommandResult.CurrentUser.IsAdministrator)
                                                {
                                                    if (CommandResult.CommandContext.PromptData == null)
                                                    {
                                                        CommandResult.WriteLine("Edit the topic title.");
                                                        CommandResult.EditText = topic.Title;
                                                        CommandResult.CommandContext.SetPrompt(Name, args, string.Format("{0} EDIT Title", topicId));
                                                    }
                                                    else if (CommandResult.CommandContext.PromptData.Length == 1)
                                                    {
                                                        CommandResult.WriteLine("Edit the topic body.");
                                                        CommandResult.EditText = topic.Body;
                                                        CommandResult.CommandContext.SetPrompt(Name, args, string.Format("{0} EDIT Body", topicId));
                                                    }
                                                    else if (CommandResult.CommandContext.PromptData.Length == 2)
                                                    {
                                                        topic.Title = CommandResult.CommandContext.PromptData[0];
                                                        topic.Body = BBCodeUtility.SimplifyComplexTags(
                                                            CommandResult.CommandContext.PromptData[1],
                                                            _dataBucket.ReplyRepository,
                                                            CommandResult.CurrentUser.IsModeratorOrAdministrator()
                                                        );
                                                        topic.LastEdit = DateTime.UtcNow;
                                                        topic.EditedBy = CommandResult.CurrentUser.Username;
                                                        _dataBucket.TopicRepository.UpdateTopic(topic);
                                                        _dataBucket.SaveChanges();
                                                        CommandResult.CommandContext.PromptData = null;
                                                        CommandResult.WriteLine("Topic '{0}' was edited successfully.", topicId);
                                                        CommandResult.CommandContext.Restore();
                                                    }
                                                }
                                                else
                                                    CommandResult.WriteLine("Topic '{0}' does not belong to you. You are not authorized to edit it.", topicId);
                                            }
                                            else
                                                CommandResult.WriteLine("Topic '{0}' is locked.", topicId);
                                        }
                                        else
                                            CommandResult.WriteLine("There is no topic with ID '{0}'.", topicId);
                                    }
                                    else
                                    {
                                        var reply = _dataBucket.ReplyRepository.GetReply((long)replyId);
                                        if (reply != null)
                                        {
                                            if (reply.TopicID == topicId)
                                            {
                                                if (!reply.Topic.Locked || (reply.ModsOnly && !reply.Topic.IsModsOnly()) || CommandResult.CurrentUser.IsAdministrator)
                                                {
                                                    if (reply.Username.Is(CommandResult.CurrentUser.Username)
                                                        || (CommandResult.CurrentUser.IsModerator && !reply.IsModsOnly() && !reply.User.IsModeratorOrAdministrator())
                                                        || CommandResult.CurrentUser.IsAdministrator)
                                                    {
                                                        if (CommandResult.CommandContext.PromptData == null)
                                                        {
                                                            CommandResult.WriteLine("Edit the reply body.");
                                                            CommandResult.EditText = reply.Body;
                                                            CommandResult.CommandContext.SetPrompt(Name, args, string.Format("{0} EDIT Reply {1}", topicId, replyId));
                                                        }
                                                        else if (CommandResult.CommandContext.PromptData.Length == 1)
                                                        {
                                                            reply.Body = BBCodeUtility.SimplifyComplexTags(
                                                                CommandResult.CommandContext.PromptData[0],
                                                                _dataBucket.ReplyRepository,
                                                                CommandResult.CurrentUser.IsModeratorOrAdministrator()
                                                            );
                                                            reply.LastEdit = DateTime.UtcNow;
                                                            reply.EditedBy = CommandResult.CurrentUser.Username;
                                                            _dataBucket.ReplyRepository.UpdateReply(reply);
                                                            _dataBucket.SaveChanges();
                                                            CommandResult.CommandContext.PromptData = null;
                                                            CommandResult.WriteLine("Reply '{0}' was edited successfully.", replyId);
                                                            CommandResult.CommandContext.Restore();
                                                        }
                                                    }
                                                    else
                                                        CommandResult.WriteLine("Reply '{0}' does not belong to you. You are not authorized to edit it.", replyId);
                                                }
                                                else
                                                    CommandResult.WriteLine("Topic '{0}' is locked.", topicId);
                                            }
                                            else
                                                CommandResult.WriteLine("Topic '{0}' does not contain a reply with ID '{1}'.", topicId, replyId);
                                        }
                                        else
                                            CommandResult.WriteLine("Reply '{0}' does not exist.", replyId);
                                    }
                                }
                                else
                                    CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                            }
                            else
                                CommandResult.WriteLine("You must supply a topic ID.");
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
                                        var topic = _dataBucket.TopicRepository.GetTopic(topicId);
                                        if (topic != null)
                                        {
                                            if (!topic.Locked || CommandResult.CurrentUser.IsAdministrator)
                                            {
                                                if (CommandResult.CurrentUser.IsAdministrator)
                                                {
                                                    if (CommandResult.CommandContext.PromptData == null)
                                                    {
                                                        CommandResult.WriteLine("Are you sure you want to delete the topic titled \"{0}\"? (Y/N)", topic.Title);
                                                        CommandResult.CommandContext.SetPrompt(Name, args, string.Format("{0} DELETE CONFIRM", topicId));
                                                    }
                                                    else if (CommandResult.CommandContext.PromptData.Length == 1)
                                                    {
                                                        if (CommandResult.CommandContext.PromptData[0].Is("Y"))
                                                        {
                                                            _dataBucket.TopicRepository.DeleteTopic(topic);
                                                            _dataBucket.SaveChanges();
                                                            CommandResult.WriteLine("Topic '{0}' was deleted successfully.", topicId);
                                                            CommandResult.CommandContext.PromptData = null;
                                                            if (CommandResult.CommandContext.PreviousContext != null
                                                                && CommandResult.CommandContext.PreviousContext.Command.Is("TOPIC")
                                                                && CommandResult.CommandContext.PreviousContext.Args.Contains(topicId.ToString())
                                                                && CommandResult.CommandContext.PreviousContext.Status == ContextStatus.Passive)
                                                            {
                                                                CommandResult.ClearScreen = true;
                                                                CommandResult.CommandContext.Deactivate();
                                                            }
                                                            else
                                                                CommandResult.CommandContext.Restore();
                                                        }
                                                        else
                                                        {
                                                            CommandResult.WriteLine("Topic '{0}' was not deleted.", topicId);
                                                            CommandResult.CommandContext.PromptData = null;
                                                            CommandResult.CommandContext.Restore();
                                                        }
                                                    }
                                                }
                                                else
                                                    CommandResult.WriteLine("You are not an administrator. You are not authorized to delete topics.");
                                            }
                                            else
                                                CommandResult.WriteLine("Topic '{0}' is locked.", topicId);
                                        }
                                        else
                                            CommandResult.WriteLine("There is no topic with ID {0}.", topicId);
                                    }
                                    else
                                    {
                                        var reply = _dataBucket.ReplyRepository.GetReply((long)replyId);
                                        if (reply != null)
                                        {
                                            if (reply.TopicID == topicId)
                                            {
                                                if (!reply.Topic.Locked || (reply.ModsOnly && !reply.Topic.IsModsOnly()) || CommandResult.CurrentUser.IsAdministrator)
                                                {
                                                    if (reply.Username.Is(CommandResult.CurrentUser.Username)
                                                        || (CommandResult.CurrentUser.IsModerator && !reply.IsModsOnly() && !reply.User.IsModeratorOrAdministrator())
                                                        || CommandResult.CurrentUser.IsAdministrator)
                                                    {
                                                        _dataBucket.ReplyRepository.DeleteReply(reply);
                                                        _dataBucket.SaveChanges();
                                                        CommandResult.WriteLine("Reply '{0}' was deleted successfully.", replyId);
                                                    }
                                                    else
                                                        CommandResult.WriteLine("Reply '{0}' does not belong to you. You are not authorized to delete it.", replyId);
                                                }
                                                else
                                                    CommandResult.WriteLine("Topic '{0}' is locked.", topicId);
                                            }
                                            else
                                                CommandResult.WriteLine("Topic '{0}' does not contain a reply with ID '{1}'.", topicId, replyId);
                                        }
                                        else
                                            CommandResult.WriteLine("Reply '{0}' does not exist.", replyId);
                                    }
                                }
                                else
                                    CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                            }
                            else
                                CommandResult.WriteLine("You must supply a topic ID.");
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
                                        var topic = _dataBucket.TopicRepository.GetTopic(topicId);
                                        if (topic != null)
                                        {
                                            if (!topic.IsModsOnly() || CommandResult.CurrentUser.IsAdministrator)
                                            {
                                                if ((bool)lockTopic || (!(bool)lockTopic && CommandResult.CurrentUser.IsAdministrator))
                                                {
                                                    topic.Locked = (bool)lockTopic;
                                                    _dataBucket.TopicRepository.UpdateTopic(topic);
                                                    _dataBucket.SaveChanges();
                                                    string status = (bool)lockTopic ? "locked" : "unlocked";
                                                    CommandResult.WriteLine("Topic '{0}' was successfully {1}.", topicId, status);
                                                }
                                                else
                                                    CommandResult.WriteLine("Only administrators can unlock topics.");
                                            }
                                            else
                                                CommandResult.WriteLine("You are not authorized to lock moderator topics.");
                                        }
                                        else
                                            CommandResult.WriteLine("There is no topic with ID {0}.", topicId);
                                    }
                                    else
                                        CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                                }
                                else
                                    CommandResult.WriteLine("You must supply a topic ID.");
                            }
                            if (stickyTopic != null)
                            {
                                if (parsedArgs.Length > 0)
                                {
                                    if (parsedArgs[0].IsLong())
                                    {
                                        var topicId = parsedArgs[0].ToLong();
                                        var topic = _dataBucket.TopicRepository.GetTopic(topicId);
                                        if (topic != null)
                                        {
                                            if (!topic.IsModsOnly() || CommandResult.CurrentUser.IsAdministrator)
                                            {
                                                topic.Stickied = (bool)stickyTopic;
                                                _dataBucket.TopicRepository.UpdateTopic(topic);
                                                _dataBucket.SaveChanges();
                                                string status = (bool)stickyTopic ? "stickied" : "unstickied";
                                                CommandResult.WriteLine("Topic '{0}' was successfully {1}.", topicId, status);
                                            }
                                            else
                                                CommandResult.WriteLine("You are not authorized to sticky/unsticky moderator topics.");
                                        }
                                        else
                                            CommandResult.WriteLine("There is no topic with ID '{0}'.", topicId);
                                    }
                                    else
                                        CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                                }
                                else
                                    CommandResult.WriteLine("You must supply a topic ID.");
                            }
                            if (globalStickyTopic != null)
                            {
                                if (parsedArgs.Length > 0)
                                {
                                    if (parsedArgs[0].IsLong())
                                    {
                                        var topicId = parsedArgs[0].ToLong();
                                        var topic = _dataBucket.TopicRepository.GetTopic(topicId);
                                        if (topic != null)
                                        {
                                            if (!topic.IsModsOnly() || CommandResult.CurrentUser.IsAdministrator)
                                            {
                                                topic.GlobalSticky = (bool)globalStickyTopic;
                                                _dataBucket.TopicRepository.UpdateTopic(topic);
                                                _dataBucket.SaveChanges();
                                                string status = (bool)globalStickyTopic ? "stickied" : "unstickied";
                                                CommandResult.WriteLine("Topic '{0}' was successfully globally {1}.", topicId, status);
                                            }
                                            else
                                                CommandResult.WriteLine("You are not authorized to globally sticky/unsticky moderator topics.");
                                        }
                                        else
                                            CommandResult.WriteLine("There is no topic with ID '{0}'.", topicId);
                                    }
                                    else
                                        CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                                }
                                else
                                    CommandResult.WriteLine("You must supply a topic ID.");
                            }
                            if (moveToBoard != null)
                            {
                                if (parsedArgs.Length > 0)
                                {
                                    if (parsedArgs[0].IsLong())
                                    {
                                        var topicId = parsedArgs[0].ToLong();
                                        var topic = _dataBucket.TopicRepository.GetTopic(topicId);
                                        if (topic != null)
                                        {
                                            var board = _dataBucket.BoardRepository.GetBoard((short)moveToBoard);
                                            if (board != null)
                                            {
                                                if (!board.Locked || CommandResult.CurrentUser.IsAdministrator)
                                                {
                                                    if (!topic.IsModsOnly() || CommandResult.CurrentUser.IsAdministrator)
                                                    {
                                                        if (!board.ModsOnly || CommandResult.CurrentUser.IsAdministrator)
                                                        {
                                                            topic.BoardID = (short)moveToBoard;
                                                            _dataBucket.TopicRepository.UpdateTopic(topic);
                                                            _dataBucket.SaveChanges();
                                                            CommandResult.WriteLine("Topic '{0}' was successfully moved to board '{1}'.", topicId, moveToBoard);
                                                        }
                                                        else
                                                            CommandResult.WriteLine("You are not authorized to move topics onto moderator boards.");
                                                    }
                                                    else
                                                        CommandResult.WriteLine("You are not authorized to move moderator topics.");
                                                }
                                                else
                                                    CommandResult.WriteLine("Board '{0}' is locked.", moveToBoard);
                                            }
                                            else
                                                CommandResult.WriteLine("There is no board with ID '{0}'.", moveToBoard);
                                        }
                                        else
                                            CommandResult.WriteLine("There is no topic with ID '{0}'.", topicId);
                                    }
                                    else
                                        CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                                }
                                else
                                    CommandResult.WriteLine("You must supply a board ID.");
                            }
                            if (refresh)
                            {
                                if (parsedArgs.Length > 0)
                                {
                                    if (parsedArgs[0].IsLong())
                                    {
                                        var topicId = parsedArgs[0].ToLong();
                                        WriteTopic(topicId, CommandResult.CommandContext.CurrentPage);
                                    }
                                    else
                                        CommandResult.WriteLine("'{0}' is not a valid topic ID.", parsedArgs[0]);
                                }
                                else
                                    CommandResult.WriteLine("You must supply a topic ID.");
                            }
                        }
                    }
                }
            }
            catch (OptionException ex)
            {
                CommandResult.WriteLine(ex.Message);
            }
        }

        private void WriteTopic(long topicId, int page)
        {
            var topic = _dataBucket.TopicRepository.GetTopic(topicId);
            if (topic != null)
            {
                if (!topic.IsModsOnly() || CommandResult.UserLoggedAndModOrAdmin())
                {
                    CommandResult.ScrollToBottom = false;
                    CommandResult.ClearScreen = true;
                    var topicPage = _dataBucket.ReplyRepository.GetReplies(topicId, page, AppSettings.RepliesPerPage, CommandResult.UserLoggedAndModOrAdmin());
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
                    CommandResult.WriteLine(DisplayMode.Inverted | DisplayMode.DontType, "{{[transmit=BOARD][topicboardid={1}]{0}[/topicboardid][/transmit]}} > {{[transmit=TOPIC]{1}[/transmit]}} [topicstatus={1}]{2}[/topicstatus][topictitle={1}]{3}[/topictitle]", topic.BoardID, topic.TopicID, status, topic.Title);
                    var topicAuthor = topic.Board.Anonymous && (!CommandResult.IsUserLoggedIn || (!CommandResult.CurrentUser.IsModeratorOrAdministrator() && topic.Username != CommandResult.CurrentUser.Username)) ? "Anon" : topic.Username;
                    CommandResult.WriteLine(DisplayMode.Italics | DisplayMode.DontType, "Posted by [transmit=USER]{0}[/transmit] on {1} | [replycount={3}]{2}[/replycount] replies", topicAuthor, topic.PostedDate.TimePassed(), topicPage.TotalItems, topic.TopicID);
                    if (topic.EditedBy != null)
                    {
                        var editedBy = topic.Board.Anonymous && (!CommandResult.IsUserLoggedIn || (!CommandResult.CurrentUser.IsModeratorOrAdministrator() && topic.EditedBy != CommandResult.CurrentUser.Username)) ? "Anon" : topic.EditedBy;
                        CommandResult.WriteLine(DisplayMode.Italics | DisplayMode.DontType, "[Edited by [transmit=USER][editedbyuser={2}]{0}[/editedbyuser][/transmit] [editedbydate={2}]{1}[/editedbydate]]", editedBy, topic.LastEdit.TimePassed(), topic.TopicID);
                    }
                    CommandResult.WriteLine();
                    CommandResult.WriteLine(DisplayMode.Parse | DisplayMode.DontType, "[topicbody={0}]{1}[/topicbody]", topic.TopicID, topic.Body);
                    CommandResult.WriteLine();
                    CommandResult.WriteLine();
                    CommandResult.WriteLine();
                    CommandResult.WriteLine(DisplayMode.DontType, "Page {0}/[topicmaxpages={2}]{1}[/topicmaxpages]", page, topicPage.TotalPages, topic.TopicID);
                    CommandResult.WriteLine();
                    CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
                    CommandResult.WriteLine();
                    for (int index = 0; index < topicPage.Items.Count(); index++)
                    {
                        var reply = topicPage.Items[index];
                        var displayMode = DisplayMode.DontType;
                        if (reply.ModsOnly && !topic.IsModsOnly())
                            displayMode |= DisplayMode.Dim;
                        var replyAuthor = reply.Topic.Board.Anonymous && (!CommandResult.IsUserLoggedIn || (!CommandResult.CurrentUser.IsModeratorOrAdministrator() && reply.Username != CommandResult.CurrentUser.Username)) ? "Anon" : reply.Username;
                        CommandResult.WriteLine(displayMode, "{{[transmit=-r=]{0}[/transmit]}} | Reply by [transmit=USER]{1}[/transmit] {2}", reply.ReplyID, replyAuthor, reply.PostedDate.TimePassed());
                        CommandResult.WriteLine();
                        CommandResult.WriteLine(displayMode | DisplayMode.Parse, "{0}", reply.Body);
                        CommandResult.WriteLine();
                        if (reply.EditedBy != null)
                        {
                            var editedBy = reply.Topic.Board.Anonymous && (!CommandResult.IsUserLoggedIn || (!CommandResult.CurrentUser.IsModeratorOrAdministrator() && reply.EditedBy != CommandResult.CurrentUser.Username)) ? "Anon" : reply.EditedBy;
                            CommandResult.WriteLine(displayMode | DisplayMode.Italics, "[Edited by [transmit=USER]{0}[/transmit] {1}]", editedBy, reply.LastEdit.TimePassed());
                            CommandResult.WriteLine();
                        }
                        CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
                        CommandResult.WriteLine();
                    }
                    if (topicPage.TotalItems == 0)
                    {
                        CommandResult.WriteLine("There are no replies to this topic.");
                        CommandResult.WriteLine();
                        CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
                        CommandResult.WriteLine();
                    }
                    CommandResult.WriteLine(DisplayMode.DontType, "Page {0}/{1}", page, topicPage.TotalPages);
                    CommandResult.CommandContext.CurrentPage = page;
                    CommandResult.CommandContext.Set(ContextStatus.Passive, Name, new string[] { topicId.ToString() }, string.Format("{0}", topicId));
                }
                else
                    CommandResult.WriteLine("Topic '{0}' is for moderators only.", topicId);
            }
            else
                CommandResult.WriteLine("'{0}' is not a valid topic ID.", topicId);
        }
    }
}
