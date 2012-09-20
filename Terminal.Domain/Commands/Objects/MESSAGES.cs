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
    public class MESSAGES : ICommand
    {
        private IMessageRepository _messageRepository;
        private IUserRepository _userRepository;

        public MESSAGES(IMessageRepository messageRepository, IUserRepository userRepository)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        public CommandResult CommandResult { get; set; }

        public IEnumerable<ICommand> AvailableCommands { get; set; }

        public string[] Roles
        {
            get { return RoleTemplates.AllLoggedIn; }
        }

        public string Name
        {
            get { return "MESSAGES"; }
        }

        public string Parameters
        {
            get { return "[Page#] [Options]"; }
        }

        public string Description
        {
            get { return "Displays a a list of sent or received messages."; }
        }

        public bool ShowHelp
        {
            get { return true; }
        }

        public void Invoke(string[] args)
        {
            bool showHelp = false;
            bool sent = false;
            bool refresh = false;
            bool deleteAll = false;
            string username = this.CommandResult.CurrentUser.Username;

            var options = new OptionSet();
            options.Add(
                "?|help",
                "Show help information.",
                x => showHelp = x != null
            );
            options.Add(
                "R|refresh",
                "Refresh the current list of messages.",
                x => refresh = x != null
            );
            options.Add(
                "s|sent",
                "Display sent messages.",
                x => sent = x != null
            );
            options.Add(
                "i|inbox",
                "Display received messages.",
                x => sent = x == null
            );
            options.Add(
                "da|deleteAll",
                "Delete all messages, excluding locked messages.",
                x => deleteAll = x != null
            );
            if (this.CommandResult.CurrentUser.IsAdministrator)
            {
                options.Add(
                    "u|user=",
                    "Get messages for a specified {Username}.",
                    x => username = x
                );
            }

            if (args == null)
            {
                WriteMessages(username, 1, false);
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
                            if (parsedArgs[0].IsInt())
                            {
                                var page = parsedArgs[0].ToInt();
                                WriteMessages(username, page, false);
                            }
                            else if (PagingUtility.Shortcuts.Any(x => parsedArgs[0].Is(x)))
                            {
                                var page = PagingUtility.TranslateShortcut(parsedArgs[0], this.CommandResult.CommandContext.CurrentPage);
                                WriteMessages(username, page, false);
                                if (parsedArgs[0].Is("last") || parsedArgs[0].Is("prev"))
                                    this.CommandResult.ScrollToBottom = false;
                            }
                            else
                                this.CommandResult.WriteLine("'{0}' is not a valid page number.", parsedArgs[0]);
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
                        else if (deleteAll)
                        {
                            if (this.CommandResult.CommandContext.PromptData == null)
                            {
                                this.CommandResult.WriteLine("Are you sure you want to delete all {0} messages? (Y/N)", sent ? "sent" : "received");
                                this.CommandResult.CommandContext.SetPrompt(this.Name, args, string.Format("DELETE {0} CONFIRM", sent ? "SENT" : "RECEIVED"));
                            }
                            else if (this.CommandResult.CommandContext.PromptData.Length == 1)
                            {
                                if (this.CommandResult.CommandContext.PromptData[0].Is("Y"))
                                {
                                    var messages = _messageRepository.GetAllMessages(username, sent)
                                        .Where(x => sent ? !x.SenderLocked : !x.RecipientLocked).ToList();
                                    foreach (var message in messages)
                                    {
                                        if (sent)
                                            message.SenderDeleted = true;
                                        else
                                            message.RecipientDeleted = true;
                                        _messageRepository.UpdateMessage(message);
                                    }
                                    this.CommandResult.WriteLine("All {0} messages for '{1}' have been deleted.", sent ? "sent" : "received", username);
                                    this.CommandResult.CommandContext.PromptData = null;
                                    this.CommandResult.CommandContext.Restore();
                                }
                                else
                                {
                                    this.CommandResult.WriteLine("{0} messages were not deleted.", sent ? "Sent" : "Received");
                                    this.CommandResult.CommandContext.PromptData = null;
                                    this.CommandResult.CommandContext.Restore();
                                }
                            }
                        }
                        else
                            if (refresh)
                                WriteMessages(username, this.CommandResult.CommandContext.CurrentPage, sent);
                            else
                                if (parsedArgs.Length >= 1)
                                {
                                    if (parsedArgs[0].IsInt())
                                    {
                                        var page = parsedArgs[0].ToInt();
                                        WriteMessages(username, page, sent);
                                    }
                                    else if (PagingUtility.Shortcuts.Any(x => parsedArgs[0].Is(x)))
                                    {
                                        var page = PagingUtility.TranslateShortcut(parsedArgs[0], this.CommandResult.CommandContext.CurrentPage);
                                        WriteMessages(username, page, sent);
                                        if (parsedArgs[1].Is("last") || parsedArgs[1].Is("prev"))
                                            this.CommandResult.ScrollToBottom = false;
                                    }
                                    else
                                        this.CommandResult.WriteLine("'{0}' is not a valid page number.", parsedArgs[0]);
                                }
                                else
                                    WriteMessages(username, 1, sent);
                    }
                }
                catch (OptionException ex)
                {
                    this.CommandResult.WriteLine(ex.Message);
                }
            }
        }

        private void WriteMessages(string username, int page, bool sent)
        {
            var user = _userRepository.GetUser(username);
            if (user != null)
            {
                this.CommandResult.ClearScreen = true;
                this.CommandResult.ScrollToBottom = true;
                var messagesPage = _messageRepository.GetMessages(user.Username, page, AppSettings.MessagesPerPage, sent);
                if (page > messagesPage.TotalPages)
                    page = messagesPage.TotalPages;
                else if (page < 1)
                    page = 1;
                this.CommandResult.WriteLine(DisplayMode.Inverted | DisplayMode.DontType, "{0} messages for [transmit=USER]{1}[/transmit]", sent ? "Sent" : "Received", user.Username);
                this.CommandResult.WriteLine();
                this.CommandResult.WriteLine(DisplayMode.DontType, "Page {0}/{1}", page, messagesPage.TotalPages);
                this.CommandResult.WriteLine();
                foreach (var message in messagesPage.Items)
                {
                    var displayMode = DisplayMode.DontType;
                    if (message.MessageRead)
                        displayMode |= DisplayMode.Dim;
                    var messageLocked = (sent && message.SenderLocked) || (!sent && message.RecipientLocked);
                    this.CommandResult.WriteLine(displayMode, "{{[transmit=MESSAGE]{0}[/transmit]}} {1}{2}", message.MessageID, messageLocked ? "[LOCKED] " : null, message.Subject);
                    this.CommandResult.WriteLine(displayMode, "   sent {0} [transmit=USER]{1}[/transmit] {2}", sent ? "to" : "from", sent ? message.Recipient : message.Sender, message.SentDate.TimePassed());
                    this.CommandResult.WriteLine();
                }
                if (messagesPage.TotalItems == 0)
                {
                    this.CommandResult.WriteLine("There are no messages in your {0}.", sent ? "outbox" : "inbox");
                    this.CommandResult.WriteLine();
                }
                this.CommandResult.WriteLine(DisplayMode.DontType, "Page {0}/{1}", page, messagesPage.TotalPages);
                this.CommandResult.WriteLine();
                this.CommandResult.CommandContext.CurrentPage = page;
                this.CommandResult.CommandContext.Set(ContextStatus.Passive, this.Name, sent ? new string[] { "-s" } : null, sent ? "SENT" : "INBOX");
            }
            else
                this.CommandResult.WriteLine("There is no user with username '{0}'.", username);
        }
    }
}
