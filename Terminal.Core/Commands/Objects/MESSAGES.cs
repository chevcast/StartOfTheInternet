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
    public class MESSAGES : ICommand
    {
        private IDataBucket _dataBucket;

        public MESSAGES(IDataBucket dataBucket)
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
            string username = CommandResult.CurrentUser.Username;

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
            if (CommandResult.CurrentUser.IsAdministrator)
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
                                var page = PagingUtility.TranslateShortcut(parsedArgs[0], CommandResult.CommandContext.CurrentPage);
                                WriteMessages(username, page, false);
                                if (parsedArgs[0].Is("last") || parsedArgs[0].Is("prev"))
                                    CommandResult.ScrollToBottom = false;
                            }
                            else
                                CommandResult.WriteLine("'{0}' is not a valid page number.", parsedArgs[0]);
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
                        else if (deleteAll)
                        {
                            if (CommandResult.CommandContext.PromptData == null)
                            {
                                CommandResult.WriteLine("Are you sure you want to delete all {0} messages? (Y/N)", sent ? "sent" : "received");
                                CommandResult.CommandContext.SetPrompt(Name, args, string.Format("DELETE {0} CONFIRM", sent ? "SENT" : "RECEIVED"));
                            }
                            else if (CommandResult.CommandContext.PromptData.Length == 1)
                            {
                                if (CommandResult.CommandContext.PromptData[0].Is("Y"))
                                {
                                    var messages = _dataBucket.MessageRepository.GetAllMessages(username, sent)
                                        .Where(x => sent ? !x.SenderLocked : !x.RecipientLocked).ToList();
                                    foreach (var message in messages)
                                    {
                                        if (sent)
                                            message.SenderDeleted = true;
                                        else
                                            message.RecipientDeleted = true;
                                        _dataBucket.MessageRepository.UpdateMessage(message);
                                        _dataBucket.SaveChanges();
                                    }
                                    CommandResult.WriteLine("All {0} messages for '{1}' have been deleted.", sent ? "sent" : "received", username);
                                    CommandResult.CommandContext.PromptData = null;
                                    CommandResult.CommandContext.Restore();
                                }
                                else
                                {
                                    CommandResult.WriteLine("{0} messages were not deleted.", sent ? "Sent" : "Received");
                                    CommandResult.CommandContext.PromptData = null;
                                    CommandResult.CommandContext.Restore();
                                }
                            }
                        }
                        else
                            if (refresh)
                                WriteMessages(username, CommandResult.CommandContext.CurrentPage, sent);
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
                                        var page = PagingUtility.TranslateShortcut(parsedArgs[0], CommandResult.CommandContext.CurrentPage);
                                        WriteMessages(username, page, sent);
                                        if (parsedArgs[1].Is("last") || parsedArgs[1].Is("prev"))
                                            CommandResult.ScrollToBottom = false;
                                    }
                                    else
                                        CommandResult.WriteLine("'{0}' is not a valid page number.", parsedArgs[0]);
                                }
                                else
                                    WriteMessages(username, 1, sent);
                    }
                }
                catch (OptionException ex)
                {
                    CommandResult.WriteLine(ex.Message);
                }
            }
        }

        private void WriteMessages(string username, int page, bool sent)
        {
            var user = _dataBucket.UserRepository.GetUser(username);
            if (user != null)
            {
                CommandResult.ClearScreen = true;
                CommandResult.ScrollToBottom = true;
                var messagesPage = _dataBucket.MessageRepository.GetMessages(user.Username, page, AppSettings.MessagesPerPage, sent);
                if (page > messagesPage.TotalPages)
                    page = messagesPage.TotalPages;
                else if (page < 1)
                    page = 1;
                CommandResult.WriteLine(DisplayMode.Inverted | DisplayMode.DontType, "{0} messages for [transmit=USER]{1}[/transmit]", sent ? "Sent" : "Received", user.Username);
                CommandResult.WriteLine();
                CommandResult.WriteLine(DisplayMode.DontType, "Page {0}/{1}", page, messagesPage.TotalPages);
                CommandResult.WriteLine();
                foreach (var message in messagesPage.Items)
                {
                    var displayMode = DisplayMode.DontType;
                    if (message.MessageRead)
                        displayMode |= DisplayMode.Dim;
                    var messageLocked = (sent && message.SenderLocked) || (!sent && message.RecipientLocked);
                    CommandResult.WriteLine(displayMode, "{{[transmit=MESSAGE]{0}[/transmit]}} {1}{2}", message.MessageID, messageLocked ? "[LOCKED] " : null, message.Subject);
                    CommandResult.WriteLine(displayMode, "   sent {0} [transmit=USER]{1}[/transmit] {2}", sent ? "to" : "from", sent ? message.Recipient : message.Sender, message.SentDate.TimePassed());
                    CommandResult.WriteLine();
                }
                if (messagesPage.TotalItems == 0)
                {
                    CommandResult.WriteLine("There are no messages in your {0}.", sent ? "outbox" : "inbox");
                    CommandResult.WriteLine();
                }
                CommandResult.WriteLine(DisplayMode.DontType, "Page {0}/{1}", page, messagesPage.TotalPages);
                CommandResult.WriteLine();
                CommandResult.CommandContext.CurrentPage = page;
                CommandResult.CommandContext.Set(ContextStatus.Passive, Name, sent ? new string[] { "-s" } : null, sent ? "SENT" : "INBOX");
            }
            else
                CommandResult.WriteLine("There is no user with username '{0}'.", username);
        }
    }
}
