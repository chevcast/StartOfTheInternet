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
    public class LINKS : ICommand
    {
        private ILinkRepository _linkRepository;
        private IReplyRepository _replyRepository;

        public LINKS(ILinkRepository linkRepository, IReplyRepository replyRepository)
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
            get { return "LINKS"; }
        }

        public string Parameters
        {
            get { return "[Page#/<Option(s)>]"; }
        }

        public string Description
        {
            get { return "View a variety of interesting links that have been rated by the community."; }
        }

        public bool ShowHelp
        {
            get { return true; }
        }

        public void Invoke(string[] args)
        {
            bool showHelp = false;
            string sortBy = null;
            string tags = null;
            string searchTerms = null;
            bool newLink = false;
            bool refresh = false;

            var options = new OptionSet();
            options.Add(
                "?|help",
                "Show help information.",
                x => showHelp = x != null
            );
            options.Add(
                "nl|newLink",
                "Create a new link.",
                x => newLink = x != null
            );
            options.Add(
                "sb|sortBy=",
                "Sort by DATE|RATING|CLICKS|REPLIES.",
                x => sortBy = x
            );
            options.Add(
                "t|tags=",
                "The tags to include. Ex: -t=games,news,food",
                x => tags = x
            );
            options.Add(
                "s|search=",
                "Filter links by search terms. Ex: -search=anime,pizza",
                x => searchTerms = x
            );
            options.Add(
                "R|refresh",
                "Refresh the current page.",
                x => refresh = x != null
            );

            if (args == null)
            {
                WriteLinks(0, null, null, sortBy);
            }
            else
            {
                try
                {
                    var parsedArgs = options.Parse(args).ToArray();

                    if (args.Length == 0)
                    {
                        WriteLinks(0, null, null, sortBy);
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
                        else if (refresh)
                        {
                            WriteLinks(
                                this.CommandResult.CommandContext.CurrentPage,
                                this.CommandResult.CommandContext.CurrentLinkTags,
                                this.CommandResult.CommandContext.CurrentSearchTerms,
                                this.CommandResult.CommandContext.CurrentSortOrder
                            );
                        }
                        else if (newLink)
                        {
                            if (this.CommandResult.CommandContext.PromptData == null)
                            {
                                this.CommandResult.WriteLine("Enter the title of your topic.");
                                this.CommandResult.CommandContext.SetPrompt(this.Name, args, "NEW LINK Title");
                            }
                            else if (this.CommandResult.CommandContext.PromptData.Length == 1)
                            {
                                this.CommandResult.WriteLine("Enter the URL of your link.");
                                this.CommandResult.CommandContext.SetPrompt(this.Name, args, "NEW LINK Url");
                            }
                            else if (this.CommandResult.CommandContext.PromptData.Length == 2)
                            {
                                this.CommandResult.WriteLine("Enter a description of your link.");
                                this.CommandResult.CommandContext.SetPrompt(this.Name, args, "NEW LINK Description");
                            }
                            else if (this.CommandResult.CommandContext.PromptData.Length == 3)
                            {
                                var allTags = _linkRepository.GetTags();
                                var tagString = new StringBuilder();
                                foreach (var tag in allTags)
                                    tagString.Append(tag.Name).Append(", ");
                                this.CommandResult.WriteLine("Available Tags: {0}", tagString.ToString().Trim(',', ' '));
                                this.CommandResult.WriteLine();
                                this.CommandResult.WriteLine("Enter up to five tags for your link (comma-delimited).");
                                this.CommandResult.CommandContext.SetPrompt(this.Name, args, "NEW LINK Tags");
                            }
                            else if (this.CommandResult.CommandContext.PromptData.Length == 4)
                            {
                                var link = new Link
                                {
                                    Title = this.CommandResult.CommandContext.PromptData[0],
                                    URL = this.CommandResult.CommandContext.PromptData[1],
                                    Description = BBCodeUtility.SimplifyComplexTags(
                                        this.CommandResult.CommandContext.PromptData[2],
                                        _replyRepository,
                                        this.CommandResult.CurrentUser.IsModerator || this.CommandResult.CurrentUser.IsAdministrator
                                    ),
                                    Username = this.CommandResult.CurrentUser.Username,
                                    Date = DateTime.UtcNow
                                };
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
                                _linkRepository.AddLink(link);
                                this.CommandResult.CommandContext.Restore();
                                //var LINK = this.AvailableCommands.SingleOrDefault(x => x.Name.Is("LINK"));
                                //LINK.Invoke(new string[] { link.LinkID.ToString() });
                                //this.CommandResult.WriteLine("New topic succesfully posted.");
                            }
                        }
                        else
                        {
                            var page = 0;
                            bool displayLinks = true;
                            if (parsedArgs.Length == 1)
                            {
                                if (parsedArgs[0].IsInt())
                                    page = parsedArgs[0].ToInt();
                                else if (PagingUtility.Shortcuts.Any(x => parsedArgs[0].Is(x)))
                                {
                                    page = PagingUtility.TranslateShortcut(parsedArgs[0], this.CommandResult.CommandContext.CurrentPage);
                                    if (parsedArgs[0].Is("last") || parsedArgs[0].Is("prev"))
                                        this.CommandResult.ScrollToBottom = false;
                                }
                                else
                                {
                                    this.CommandResult.WriteLine("'{0}' is not a valid page number.", parsedArgs[0]);
                                    displayLinks = false;
                                }
                            }

                            if (displayLinks)
                            {
                                List<string> tagList = null;
                                if (tags != null)
                                {
                                    tagList = new List<string>();
                                    foreach (var tagName in tags.Split(',').ToList())
                                        if (_linkRepository.GetTag(tagName) != null)
                                            tagList.Add(tagName);

                                }
                                else
                                    tagList = this.CommandResult.CommandContext.CurrentLinkTags;
                                List<string> searchTermList = null;
                                if (searchTerms != null)
                                    searchTermList = searchTerms.Split(',').ToList();
                                else
                                    searchTermList = this.CommandResult.CommandContext.CurrentSearchTerms;
                                var sortOptions = new List<string>
                                {
                                    "DATE",
                                    "RATINGS",
                                    "CLICKS",
                                    "REPLIES"
                                };
                                if (sortBy != null && !sortOptions.Any(x => sortBy.ToUpper() == x))
                                    sortBy = this.CommandResult.CommandContext.CurrentSortOrder;
                                WriteLinks(page, tagList, searchTermList, sortBy);
                            }
                        }
                    }
                }
                catch (OptionException ex)
                {
                    this.CommandResult.WriteLine(ex.Message);
                }
            }
        }

        private void WriteLinks(int page, List<string> tags, List<string> searchTerms, string sortBy)
        {
            if (sortBy == null)
                sortBy = "DATE";
            this.CommandResult.ClearScreen = true;
            this.CommandResult.ScrollToBottom = true;
            var linksPage = _linkRepository.GetLinks(page, AppSettings.LinksPerPage, tags, searchTerms, sortBy);
            if (page > linksPage.TotalPages)
                page = linksPage.TotalPages;
            else if (page < 1)
                page = 1;
            this.CommandResult.WriteLine(DisplayMode.Inverted | DisplayMode.Bold, "Welcome to the links board!");
            this.CommandResult.WriteLine();
            this.CommandResult.WriteLine(DisplayMode.Bold, "Active Filters");
            this.CommandResult.WriteLine("Tags: {0}", tags.ToCommaDelimitedString());
            this.CommandResult.WriteLine("Search Terms: {0}", searchTerms.ToCommaDelimitedString());
            this.CommandResult.WriteLine("Sorted By: {0}", sortBy.ToUpper());
            this.CommandResult.WriteLine();
            this.CommandResult.WriteLine(DisplayMode.DontType, "Page {0}/{1}", page, linksPage.TotalPages);
            this.CommandResult.WriteLine();
            this.CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
            foreach (var link in linksPage.Items)
            {
                var displayMode = DisplayMode.DontType;
                var lastLinkComment = link.LinkComments.LastOrDefault();
                this.CommandResult.WriteLine(displayMode | DisplayMode.Bold, "{{[transmit=LINK]{0}[/transmit]}} {1}", link.LinkID, link.Title);
                this.CommandResult.WriteLine(displayMode, "   by [transmit=USER]{0}[/transmit] {1} | {2} comments", link.Username, link.Date.TimePassed(), link.LinkComments.Count);
                if (lastLinkComment != null)
                    this.CommandResult.WriteLine(displayMode, "   last comment by [transmit=USER]{0}[/transmit] {1}", lastLinkComment.Username, lastLinkComment.Date.TimePassed());
                this.CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
            }
            if (linksPage.TotalItems == 0)
            {
                this.CommandResult.WriteLine("There are no links on the links board at this time.");
                this.CommandResult.WriteLine();
                this.CommandResult.WriteLine(DisplayMode.Dim | DisplayMode.DontType, new string('-', AppSettings.DividerLength));
                this.CommandResult.WriteLine();
            }
            this.CommandResult.WriteLine();
            this.CommandResult.WriteLine(DisplayMode.DontType, "Page {0}/{1}", page, linksPage.TotalPages);
            this.CommandResult.CommandContext.CurrentPage = page;
            this.CommandResult.CommandContext.CurrentLinkTags = tags;
            this.CommandResult.CommandContext.CurrentSearchTerms = searchTerms;
            this.CommandResult.CommandContext.CurrentSortOrder = sortBy;
            this.CommandResult.CommandContext.Set(ContextStatus.Passive, this.Name, null, null);
        }
    }
}
