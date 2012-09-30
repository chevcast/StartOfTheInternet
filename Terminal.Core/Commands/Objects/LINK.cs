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
    public class LINK : ICommand
    {
        private ILinkRepository _linkRepository;
        private IReplyRepository _replyRepository;

        public LINK(ILinkRepository linkRepository, IReplyRepository replyRepository)
        {
            _linkRepository = linkRepository;
            _replyRepository = replyRepository;
        }

        public CommandResult CommandResult { get; set; }

        public IEnumerable<ICommand> AvailableCommands { get; set; }

        public string[] Roles
        {
            get { return RoleTemplates.AllLoggedIn; }
        }

        public string Name
        {
            get { return "LINK"; }
        }

        public string Parameters
        {
            get { return "<LinkID> [Page#/Option(s)]"; }
        }

        public string Description
        {
            get { return "Displays a specified link."; }
        }

        public bool ShowHelp
        {
            get { return true; }
        }

        public void Invoke(string[] args)
        {
            bool showHelp = false;
            bool newComment = false;
            bool refresh = false;
            bool edit = false;
            bool delete = false;
            bool report = false;
            long? commentId = null;
            short? rating = null;

            var options = new OptionSet();
            options.Add(
                "?|help",
                "Show help information.",
                x => showHelp = x != null
            );
            options.Add(
                "c|comment",
                "Comment on the link.",
                x => newComment = x != null
            );
            options.Add(
                "R|refresh",
                "Refresh the current link.",
                x => refresh = x != null
            );
            options.Add(
                "e|edit:",
                "Edits the link or a comment if a {CommentID} is specified.",
                x =>
                {
                    edit = true;
                    if (x.IsLong())
                        commentId = x.ToLong();
                }
            );
            options.Add(
                "d|delete:",
                "Deletes the link or a comment if a {CommentID} is specified.",
                x =>
                {
                    delete = true;
                    if (x.IsLong())
                        commentId = x.ToLong();
                }
            );
            options.Add(
                "report:",
                "Report the link for abuse or comment if a {CommentID} is specified.",
                x =>
                {
                    report = true;
                    if (x.IsLong())
                        commentId = x.ToLong();
                }
            );
            options.Add(
                "rate=",
                "Rate the current link from 1 to 10.",
                x => rating = x.ToShort()
            );

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
                                var linkId = parsedArgs[0].ToLong();
                                var page = 1;
                                WriteLink(linkId, page);
                            }
                            else
                                this.CommandResult.WriteLine("'{0}' is not a valid link ID.", parsedArgs[0]);
                        }
                        else if (parsedArgs.Length == 2)
                        {
                            if (parsedArgs[0].IsInt())
                            {
                                var linkId = parsedArgs[0].ToLong();
                                if (parsedArgs[1].IsInt())
                                {
                                    var page = parsedArgs[1].ToInt();
                                    WriteLink(linkId, page);
                                }
                                else if (PagingUtility.Shortcuts.Any(x => parsedArgs[1].Is(x)))
                                {
                                    var page = PagingUtility.TranslateShortcut(parsedArgs[1], this.CommandResult.CommandContext.CurrentPage);
                                    WriteLink(linkId, page);
                                    if (parsedArgs[1].Is("last") || parsedArgs[1].Is("prev"))
                                        this.CommandResult.ScrollToBottom = true;
                                }
                                else
                                    this.CommandResult.WriteLine("'{0}' is not a valid page number.", parsedArgs[1]);
                            }
                            else
                                this.CommandResult.WriteLine("'{0}' is not a valid link ID.", parsedArgs[0]);
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
                        else if (rating != null)
                        {
                            if (parsedArgs.Length > 0)
                            {
                                if (parsedArgs[0].IsLong())
                                {
                                    var linkId = parsedArgs[0].ToLong();
                                    var link = _linkRepository.GetLink(linkId);
                                    if (link != null)
                                    {
                                        if (rating is short && (short)rating > 0 && (short)rating <= 10)
                                        {
                                            var existingRating = link.LinkVotes.SingleOrDefault(x => x.Username == this.CommandResult.CurrentUser.Username);
                                            if (existingRating == null)
                                            {
                                                var linkRating = new LinkVote
                                                {
                                                    Rating = (short)rating,
                                                    Username = this.CommandResult.CurrentUser.Username
                                                };
                                                link.LinkVotes.Add(linkRating);
                                            }
                                            else
                                                existingRating.Rating = (short)rating;
                                            _linkRepository.UpdateLink(link);
                                            this.CommandResult.WriteLine("You successfully gave link '{0}' a rating of '{1}'.", linkId, rating);
                                        }
                                        else
                                            this.CommandResult.WriteLine("Your rating must be a whole number between 1 and 10.");
                                    }
                                    else
                                        this.CommandResult.WriteLine("There is no link with ID '{0}'.", linkId);
                                }
                                else
                                    this.CommandResult.WriteLine("'{0}' is not a valid link ID.", parsedArgs[0]);
                            }
                            else
                                this.CommandResult.WriteLine("You must supply a link ID.");
                        }
                        else if (newComment)
                        {
                            if (parsedArgs.Length > 0)
                            {
                                if (parsedArgs[0].IsLong())
                                {
                                    var linkId = parsedArgs[0].ToLong();
                                    var link = _linkRepository.GetLink(linkId);
                                    if (link != null)
                                    {
                                        if (this.CommandResult.CommandContext.PromptData == null)
                                        {
                                            this.CommandResult.WriteLine("Type your comment.");
                                            this.CommandResult.CommandContext.SetPrompt(this.Name, args, string.Format("{0} COMMENT", linkId));
                                        }
                                        else
                                        {
                                            _linkRepository.AddComment(new LinkComment
                                            {
                                                Username = this.CommandResult.CurrentUser.Username,
                                                Date = DateTime.UtcNow,
                                                LinkID = linkId,
                                                Body = BBCodeUtility.SimplifyComplexTags(
                                                    this.CommandResult.CommandContext.PromptData[0],
                                                    _replyRepository,
                                                    this.CommandResult.CurrentUser.IsModerator || this.CommandResult.CurrentUser.IsAdministrator
                                                )
                                            });
                                            this.CommandResult.CommandContext.PromptData = null;
                                            var LINK = this.AvailableCommands.SingleOrDefault(x => x.Name.Is("LINK"));
                                            LINK.Invoke(new string[] { linkId.ToString(), "last" });
                                            this.CommandResult.WriteLine("Comment successfully posted.");
                                        }
                                    }
                                    else
                                        this.CommandResult.WriteLine("There is no link with ID '{0}'.", linkId);
                                }
                                else
                                    this.CommandResult.WriteLine("'{0}' is not a valid link ID.", parsedArgs[0]);
                            }
                            else
                                this.CommandResult.WriteLine("You must supply a link ID.");
                        }
                        else if (edit)
                        {
                            if (parsedArgs.Length > 0)
                            {
                                if (parsedArgs[0].IsLong())
                                {
                                    var linkId = parsedArgs[0].ToLong();
                                    if (commentId == null)
                                    {
                                        var link = _linkRepository.GetLink(linkId);
                                        if (link != null)
                                        {
                                            if (link.Username.Is(this.CommandResult.CurrentUser.Username)
                                                || (this.CommandResult.CurrentUser.IsModerator && !link.User.IsModerator && !link.User.IsAdministrator)
                                                || this.CommandResult.CurrentUser.IsAdministrator)
                                            {
                                                if (this.CommandResult.CommandContext.PromptData == null)
                                                {
                                                    this.CommandResult.WriteLine("Edit the link title.");
                                                    this.CommandResult.EditText = link.Title;
                                                    this.CommandResult.CommandContext.SetPrompt(this.Name, args, string.Format("{0} EDIT Title", linkId));
                                                }
                                                else if (this.CommandResult.CommandContext.PromptData.Length == 1)
                                                {
                                                    this.CommandResult.WriteLine("Edit the link URL.");
                                                    this.CommandResult.EditText = link.URL;
                                                    this.CommandResult.CommandContext.SetPrompt(this.Name, args, string.Format("{0} EDIT Url", linkId));
                                                }
                                                else if (this.CommandResult.CommandContext.PromptData.Length == 2)
                                                {
                                                    this.CommandResult.WriteLine("Edit the link description.");
                                                    this.CommandResult.EditText = link.Description;
                                                    this.CommandResult.CommandContext.SetPrompt(this.Name, args, string.Format("{0} EDIT Description", linkId));
                                                }
                                                else if (this.CommandResult.CommandContext.PromptData.Length == 3)
                                                {
                                                    var allTags = _linkRepository.GetTags();
                                                    var tagString = new StringBuilder();
                                                    foreach (var tag in allTags)
                                                        tagString.Append(tag.Name).Append(", ");
                                                    this.CommandResult.WriteLine("Available Tags: {0}", tagString.ToString().Trim(',', ' '));
                                                    this.CommandResult.WriteLine();
                                                    this.CommandResult.WriteLine("Edit the link tags.");
                                                    this.CommandResult.EditText = link.Tags.Select(x => x.Name).ToList().ToCommaDelimitedString();
                                                    this.CommandResult.CommandContext.SetPrompt(this.Name, args, string.Format("{0} EDIT Tags", linkId));
                                                }
                                                else if (this.CommandResult.CommandContext.PromptData.Length == 4)
                                                {
                                                    link.Title = this.CommandResult.CommandContext.PromptData[0];
                                                    link.URL = this.CommandResult.CommandContext.PromptData[1];
                                                    link.Description = BBCodeUtility.SimplifyComplexTags(
                                                        this.CommandResult.CommandContext.PromptData[2],
                                                        _replyRepository,
                                                        this.CommandResult.CurrentUser.IsModerator || this.CommandResult.CurrentUser.IsAdministrator
                                                    );
                                                    link.Tags.Clear();
                                                    var tagList = this.CommandResult.CommandContext.PromptData[3].Replace(" ", "").Split(',').ToList();
                                                    if (tagList.Count > 5)
                                                    {
                                                        tagList.RemoveRange(5, tagList.Count - 5);
                                                        this.CommandResult.WriteLine("You supplied more than five tags. Only the first five were used.");
                                                    }
                                                    foreach (var tagName in tagList)
                                                    {
                                                        var tag = _linkRepository.GetTag(tagName);
                                                        if (tag != null)
                                                            link.Tags.Add(tag);
                                                        else
                                                            this.CommandResult.WriteLine("'{0}' was not a valid tag and was not added.", tagName.ToUpper());
                                                    }
                                                    _linkRepository.UpdateLink(link);
                                                    this.CommandResult.CommandContext.PromptData = null;
                                                    this.CommandResult.WriteLine("Link '{0}' was edited successfully.", linkId);
                                                    this.CommandResult.CommandContext.Restore();
                                                }
                                            }
                                            else
                                                this.CommandResult.WriteLine("Link '{0}' belongs to '{1}'. You are not authorized to edit it.", linkId, link.Username);
                                        }
                                        else
                                            this.CommandResult.WriteLine("There is no link with ID '{0}'.", linkId);
                                    }
                                    else
                                    {
                                        var linkComment = _linkRepository.GetComment((int)commentId);
                                        if (linkComment != null)
                                        {
                                            if (linkComment.LinkID == linkId)
                                            {
                                                if (linkComment.Username.Is(this.CommandResult.CurrentUser.Username)
                                                    || (this.CommandResult.CurrentUser.IsModerator && !linkComment.User.IsModerator && !linkComment.User.IsAdministrator)
                                                    || this.CommandResult.CurrentUser.IsAdministrator)
                                                {
                                                    if (this.CommandResult.CommandContext.PromptData == null)
                                                    {
                                                        this.CommandResult.WriteLine("Edit the comment body.");
                                                        this.CommandResult.EditText = linkComment.Body;
                                                        this.CommandResult.CommandContext.SetPrompt(this.Name, args, string.Format("{0} EDIT Comment {1}", linkId, commentId));
                                                    }
                                                    else if (this.CommandResult.CommandContext.PromptData.Length == 1)
                                                    {
                                                        linkComment.Body = BBCodeUtility.SimplifyComplexTags(
                                                            this.CommandResult.CommandContext.PromptData[0],
                                                            _replyRepository,
                                                            this.CommandResult.CurrentUser.IsModerator || this.CommandResult.CurrentUser.IsAdministrator
                                                        );
                                                        _linkRepository.UpdateComment(linkComment);
                                                        this.CommandResult.CommandContext.PromptData = null;
                                                        this.CommandResult.WriteLine("Comment '{0}' was edited successfully.", commentId);
                                                        this.CommandResult.CommandContext.Restore();
                                                    }
                                                }
                                                else
                                                    this.CommandResult.WriteLine("Comment '{0}' belongs to '{1}'. You are not authorized to edit it.", commentId, linkComment.Username);
                                            }
                                            else
                                                this.CommandResult.WriteLine("Link '{0}' does not contain a comment with ID '{1}'.", linkId, commentId);
                                        }
                                        else
                                            this.CommandResult.WriteLine("Comment '{0}' does not exist.", commentId);
                                    }
                                }
                                else
                                    this.CommandResult.WriteLine("'{0}' is not a valid link ID.", parsedArgs[0]);
                            }
                            else
                                this.CommandResult.WriteLine("You must supply a link ID.");
                        }
                        else if (delete)
                        {
                            if (parsedArgs.Length > 0)
                            {
                                if (parsedArgs[0].IsLong())
                                {
                                    var linkId = parsedArgs[0].ToLong();
                                    if (commentId == null)
                                    {
                                        var link = _linkRepository.GetLink(linkId);
                                        if (link != null)
                                        {
                                            if (this.CommandResult.CurrentUser.Username.Is(link.Username) || this.CommandResult.CurrentUser.IsModerator || this.CommandResult.CurrentUser.IsAdministrator)
                                            {
                                                if (this.CommandResult.CommandContext.PromptData == null)
                                                {
                                                    this.CommandResult.WriteLine("Are you sure you want to delete the link titled \"{0}\"? (Y/N)", link.Title);
                                                    this.CommandResult.CommandContext.SetPrompt(this.Name, args, string.Format("{0} DELETE CONFIRM", linkId));
                                                }
                                                else if (this.CommandResult.CommandContext.PromptData.Length == 1)
                                                {
                                                    if (this.CommandResult.CommandContext.PromptData[0].Is("Y"))
                                                    {
                                                        _linkRepository.DeleteLink(link);
                                                        this.CommandResult.WriteLine("Link '{0}' was deleted successfully.", linkId);
                                                        this.CommandResult.CommandContext.PromptData = null;
                                                        if (this.CommandResult.CommandContext.PreviousContext != null
                                                            && this.CommandResult.CommandContext.PreviousContext.Command.Is("LINK")
                                                            && this.CommandResult.CommandContext.PreviousContext.Args.Contains(linkId.ToString())
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
                                                        this.CommandResult.WriteLine("Link '{0}' was not deleted.", linkId);
                                                        this.CommandResult.CommandContext.PromptData = null;
                                                        this.CommandResult.CommandContext.Restore();
                                                    }
                                                }
                                            }
                                            else
                                                this.CommandResult.WriteLine("Link '{0}' belongs to '{1}'. You are not authorized to edit it.", linkId, link.Username);
                                        }
                                        else
                                            this.CommandResult.WriteLine("There is no link with ID {0}.", linkId);
                                    }
                                    else
                                    {
                                        var linkComment = _linkRepository.GetComment((int)commentId);
                                        if (linkComment != null)
                                        {
                                            if (linkComment.LinkID == linkId)
                                            {
                                                if (linkComment.Username.Is(this.CommandResult.CurrentUser.Username)
                                                    || (this.CommandResult.CurrentUser.IsModerator && !linkComment.User.IsModerator && !linkComment.User.IsAdministrator)
                                                    || this.CommandResult.CurrentUser.IsAdministrator)
                                                {
                                                    _linkRepository.DeleteComment(linkComment);
                                                    this.CommandResult.WriteLine("Comment '{0}' was deleted successfully.", commentId);
                                                }
                                                else
                                                    this.CommandResult.WriteLine("Comment '{0}' belongs to '{1}'. You are not authorized to delete it.", commentId, linkComment.Username);
                                            }
                                            else
                                                this.CommandResult.WriteLine("Link '{0}' does not contain a comment with ID '{1}'.", linkId, commentId);
                                        }
                                        else
                                            this.CommandResult.WriteLine("Comment '{0}' does not exist.", commentId);
                                    }
                                }
                                else
                                    this.CommandResult.WriteLine("'{0}' is not a valid link ID.", parsedArgs[0]);
                            }
                            else
                                this.CommandResult.WriteLine("You must supply a link ID.");
                        }
                        else if (report)
                        {
                            // allow user to report abuse.
                        }
                        else
                        {
                            if (refresh)
                            {
                                if (parsedArgs.Length > 0)
                                {
                                    if (parsedArgs[0].IsLong())
                                    {
                                        var linkId = parsedArgs[0].ToLong();
                                        WriteLink(linkId, this.CommandResult.CommandContext.CurrentPage);
                                    }
                                    else
                                        this.CommandResult.WriteLine("'{0}' is not a valid link ID.", parsedArgs[0]);
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

        private void WriteLink(long linkId, int page)
        {
            var link = _linkRepository.GetLink(linkId);
            if (link != null)
            {
                this.CommandResult.ScrollToBottom = false;
                this.CommandResult.ClearScreen = true;
                var commentPage = _linkRepository.GetComments(linkId, page, AppSettings.LinkCommentsPerPage);
                if (page > commentPage.TotalPages)
                    page = commentPage.TotalPages;
                else if (page < 1)
                    page = 1;
                this.CommandResult.WriteLine(DisplayMode.Inverted, "{{[transmit=LINK]{0}[/transmit]}} {1}", link.LinkID, link.Title);
                this.CommandResult.WriteLine(DisplayMode.Italics, "Posted by [transmit=USER]{0}[/transmit] on {1} | {2} comments", link.Username, link.Date.TimePassed(), commentPage.TotalItems);
                var linkVote = link.LinkVotes.SingleOrDefault(x => x.Username == this.CommandResult.CurrentUser.Username);
                if (linkVote != null)
                {
                    this.CommandResult.WriteLine();
                    this.CommandResult.WriteLine(DisplayMode.Bold, "You have rated this link: {0}/10", linkVote.Rating);
                }
                this.CommandResult.WriteLine();
                this.CommandResult.WriteLine(DisplayMode.DontType | DisplayMode.Bold | DisplayMode.Parse, "[url]{0}[/url]", link.URL);
                this.CommandResult.WriteLine();
                this.CommandResult.WriteLine(DisplayMode.Parse | DisplayMode.DontType, link.Description);
                this.CommandResult.WriteLine();
                this.CommandResult.WriteLine();
                this.CommandResult.WriteLine();
                this.CommandResult.WriteLine(DisplayMode.DontType, "Page {0}/{1}", page, commentPage.TotalPages);
                this.CommandResult.WriteLine();
                this.CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
                this.CommandResult.WriteLine();
                for (int index = 0; index < commentPage.Items.Count(); index++)
                {
                    var linkComment = commentPage.Items[index];
                    var displayMode = DisplayMode.DontType;
                    this.CommandResult.WriteLine(displayMode, "{{[transmit]{0}[/transmit]}} | Comment by [transmit=USER]{1}[/transmit] {2}", linkComment.CommentID, linkComment.Username, linkComment.Date.TimePassed());
                    this.CommandResult.WriteLine();
                    this.CommandResult.WriteLine(displayMode | DisplayMode.Parse, "{0}", linkComment.Body);
                    this.CommandResult.WriteLine();
                    this.CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
                    this.CommandResult.WriteLine();
                }
                if (commentPage.TotalItems == 0)
                {
                    this.CommandResult.WriteLine("There are no comments on this link.");
                    this.CommandResult.WriteLine();
                    this.CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
                    this.CommandResult.WriteLine();
                }
                this.CommandResult.WriteLine(DisplayMode.DontType, "Page {0}/{1}", page, commentPage.TotalPages);
                this.CommandResult.CommandContext.CurrentPage = page;
                this.CommandResult.CommandContext.Set(ContextStatus.Passive, this.Name, new string[] { linkId.ToString() }, string.Format("{0}", linkId));
            }
            else
                this.CommandResult.WriteLine("'{0}' is not a valid link ID.", linkId);
        }
    }
}
