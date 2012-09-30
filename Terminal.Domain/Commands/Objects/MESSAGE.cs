using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Domain.Enums;
using Terminal.Domain.Objects;
using Terminal.Domain.Commands.Interfaces;
using Terminal.Domain.Data.Entities;
using Terminal.Domain.Settings;
using System.IO;
using Mono.Options;
using Terminal.Domain.ExtensionMethods;
using Terminal.Domain.Utilities;
using Terminal.Domain.Data;

namespace Terminal.Domain.Commands.Objects
{
    public class MESSAGE : ICommand
    {
        private IDataBucket _dataBucket;

        public MESSAGE(IDataBucket dataBucket)
        {
            _dataBucket = dataBucket;
        }

        public IEnumerable<ICommand> AvailableCommands { get; set; }

        public CommandResult CommandResult { get; set; }

        public string[] Roles
        {
            get { return RoleTemplates.AllLoggedIn; }
        }

        public string Name
        {
            get { return "MESSAGE"; }
        }

        public string Parameters
        {
            get { return "<MessageID> [Options]"; }
        }

        public string Description
        {
            get { return "Allows you to read and reply to messages."; }
        }

        public bool ShowHelp
        {
            get { return true; }
        }

        public void Invoke(string[] args)
        {
            bool showHelp = false;
            bool delete = false;
            bool? lockMessage = null;
            bool reply = false;
            bool newMessage = false;

            var options = new OptionSet();
            options.Add(
                "?|help",
                "Show help information.",
                x => showHelp = x != null
            );
            options.Add(
                "r|reply",
                "Reply to the specified message.",
                x => reply = x != null
            );
            options.Add(
                "d|delete",
                "Deletes the specified message.",
                x => delete = x != null
            );
            options.Add(
                "l|lock",
                "Locks the message to prevent accidental deltion.",
                x => lockMessage = x != null
            );
            options.Add(
                "nm|newMessage",
                "Send a new message to a specific user.",
                x => newMessage = x != null
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
                        if (parsedArgs.Length == 1)
                        {
                            if (parsedArgs[0].IsLong())
                            {
                                var messageId = parsedArgs[0].ToLong();
                                WriteMessage(messageId);
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
                        else if (reply)
                        {
                            if (parsedArgs.Length > 0)
                            {
                                if (parsedArgs[0].IsLong())
                                {
                                    var messageId = parsedArgs[0].ToLong();
                                    var message = _dataBucket.MessageRepository.GetMessage(messageId);
                                    if (message != null)
                                    {
                                        if (message.Recipient.Is(CommandResult.CurrentUser.Username))
                                        {
                                            if (CommandResult.CommandContext.PromptData == null)
                                            {
                                                CommandResult.WriteLine("Write the body of your message.");
                                                CommandResult.CommandContext.SetPrompt(Name, args, string.Format("{0} REPLY Body", messageId));
                                            }
                                            else if (CommandResult.CommandContext.PromptData.Length == 1)
                                            {
                                                _dataBucket.MessageRepository.AddMessage(new Message
                                                {
                                                    Body = CommandResult.CommandContext.PromptData[0],
                                                    RecipientLocked = false,
                                                    SenderLocked = false,
                                                    MessageRead = false,
                                                    Recipient = message.Sender,
                                                    Sender = CommandResult.CurrentUser.Username,
                                                    RecipientDeleted = false,
                                                    SenderDeleted = false,
                                                    Subject = string.Format("Re: {0}", message.Subject),
                                                    SentDate = DateTime.UtcNow
                                                });
                                                _dataBucket.SaveChanges();
                                                CommandResult.WriteLine("Reply sent succesfully.");
                                                CommandResult.CommandContext.Restore();
                                            }
                                        }
                                        else
                                            CommandResult.WriteLine("Message '{0}' was not sent to you.", messageId);
                                    }
                                    else
                                        CommandResult.WriteLine("There is no message with ID '{0}'.", messageId);
                                }
                                else
                                    CommandResult.WriteLine("'{0}' is not a valid message ID.", parsedArgs[0]);
                            }
                            else
                                CommandResult.WriteLine("You must supply a message ID.");
                        }
                        else if (delete)
                        {
                            if (parsedArgs.Length > 0)
                            {
                                if (parsedArgs[0].IsLong())
                                {
                                    var messageId = parsedArgs[0].ToLong();
                                    var message = _dataBucket.MessageRepository.GetMessage(messageId);
                                    if (message != null)
                                    {
                                        if (message.Recipient.Is(CommandResult.CurrentUser.Username))
                                        {
                                            if (!message.RecipientLocked)
                                            {
                                                message.RecipientDeleted = true;
                                                _dataBucket.MessageRepository.UpdateMessage(message);
                                                _dataBucket.SaveChanges();
                                                CommandResult.WriteLine("Message '{0}' deleted successfully.", messageId);
                                            }
                                            else
                                                CommandResult.WriteLine("You have locked message '{0}' and cannot delete it.", messageId);
                                        }
                                        else if (message.Sender.Is(CommandResult.CurrentUser.Username))
                                        {
                                            if (!message.SenderLocked)
                                            {
                                                message.SenderDeleted = true;
                                                _dataBucket.MessageRepository.UpdateMessage(message);
                                                _dataBucket.SaveChanges();
                                                CommandResult.WriteLine("Message '{0}' deleted successfully.", messageId);
                                            }
                                            else
                                                CommandResult.WriteLine("You have locked message '{0}' and cannot delete it.", messageId);
                                        }
                                        else
                                            CommandResult.WriteLine("Message '{0}' does not belong to you.", messageId);
                                    }
                                    else
                                        CommandResult.WriteLine("There is no message with ID '{0}'.", messageId);
                                }
                                else
                                    CommandResult.WriteLine("'{0}' is not a valid message ID.", parsedArgs[0]);
                            }
                            else
                                CommandResult.WriteLine("You must supply a message ID.");
                        }
                        else if (newMessage)
                        {
                            if (CommandResult.CommandContext.PromptData == null)
                            {
                                CommandResult.WriteLine("Type the name of the recipient.");
                                CommandResult.CommandContext.SetPrompt(Name, args, "USERNAME");
                            }
                            else if (CommandResult.CommandContext.PromptData.Length == 1)
                            {
                                var user = _dataBucket.UserRepository.GetUser(CommandResult.CommandContext.PromptData[0]);
                                if (user != null)
                                {
                                    CommandResult.WriteLine("Type the subject of your message.");
                                    CommandResult.CommandContext.SetPrompt(Name, args, "SUBJECT");
                                }
                                else
                                {
                                    CommandResult.WriteLine("'{0}' is not a valid username.", CommandResult.CommandContext.PromptData[0]);
                                    CommandResult.WriteLine("Re-type the name of the recipient.");
                                    CommandResult.CommandContext.PromptData = null;
                                    CommandResult.CommandContext.SetPrompt(Name, args, "USERNAME");
                                }
                            }
                            else if (CommandResult.CommandContext.PromptData.Length == 2)
                            {
                                CommandResult.WriteLine("Type the body of your message.");
                                CommandResult.CommandContext.SetPrompt(Name, args, "BODY");
                            }
                            else if (CommandResult.CommandContext.PromptData.Length == 3)
                            {
                                _dataBucket.MessageRepository.AddMessage(new Message
                                {
                                    Sender = CommandResult.CurrentUser.Username,
                                    Recipient = CommandResult.CommandContext.PromptData[0],
                                    Subject = CommandResult.CommandContext.PromptData[1],
                                    Body = CommandResult.CommandContext.PromptData[2],
                                    SentDate = DateTime.UtcNow
                                });
                                _dataBucket.SaveChanges();
                                CommandResult.WriteLine("Message sent succesfully.");
                                CommandResult.CommandContext.Restore();
                            }
                        }
                        else
                            if (lockMessage != null)
                            {
                                if (parsedArgs.Length > 0)
                                {
                                    if (parsedArgs[0].IsLong())
                                    {
                                        var messageId = parsedArgs[0].ToLong();
                                        var message = _dataBucket.MessageRepository.GetMessage(messageId);
                                        if (message != null)
                                        {
                                            if (message.Recipient.Is(CommandResult.CurrentUser.Username))
                                            {
                                                message.RecipientLocked = (bool)lockMessage;
                                                _dataBucket.MessageRepository.UpdateMessage(message);
                                                _dataBucket.SaveChanges();
                                                CommandResult.WriteLine("Message '{0}' {1} successfully.", messageId, (bool)lockMessage ? "locked" : "unlocked");
                                            }
                                            else if (message.Sender.Is(CommandResult.CurrentUser.Username))
                                            {
                                                message.SenderLocked = (bool)lockMessage;
                                                _dataBucket.MessageRepository.UpdateMessage(message);
                                                _dataBucket.SaveChanges();
                                                CommandResult.WriteLine("Message '{0}' {1} successfully.", messageId, (bool)lockMessage ? "locked" : "unlocked");
                                            }
                                            else
                                                CommandResult.WriteLine("Message '{0}' does not belong to you.", messageId);
                                        }
                                        else
                                            CommandResult.WriteLine("There is no message with ID '{0}'.", messageId);
                                    }
                                    else
                                        CommandResult.WriteLine("'{0}' is not a valid message ID.", parsedArgs[0]);
                                }
                                else
                                    CommandResult.WriteLine("You must supply a message ID.");
                            }
                    }
                }
                catch (OptionException ex)
                {
                    CommandResult.WriteLine(ex.Message);
                }
            }
        }

        private void WriteMessage(long messageId)
        {
            var message = _dataBucket.MessageRepository.GetMessage(messageId);
            if (message != null)
            {
                var isRecipient = message.Recipient.Is(CommandResult.CurrentUser.Username);
                var isSender = message.Sender.Is(CommandResult.CurrentUser.Username);
                if (isRecipient || isSender || CommandResult.CurrentUser.IsAdministrator)
                {
                    var messageLocked = (isRecipient && message.RecipientLocked) || (isSender && message.SenderLocked);
                    CommandResult.WriteLine(DisplayMode.Inverted | DisplayMode.DontType, "{{[transmit=MESSAGE]{0}[/transmit]}} {1}{2}", messageId, messageLocked ? "[LOCKED] " : null, message.Subject);
                    CommandResult.WriteLine(DisplayMode.DontType, "sent to [transmit=USER]{0}[/transmit] from [transmit=USER]{1}[/transmit] on {2}", message.Recipient, message.Sender, message.SentDate);
                    CommandResult.WriteLine();
                    CommandResult.WriteLine(DisplayMode.Parse | DisplayMode.DontType, message.Body);
                    CommandResult.ClearScreen = true;
                    CommandResult.CommandContext.Set(ContextStatus.Passive, Name, new string[] { messageId.ToString() }, string.Format("{0}", messageId));
                    if (message.Recipient.Is(CommandResult.CurrentUser.Username))
                    {
                        message.MessageRead = true;
                        _dataBucket.MessageRepository.UpdateMessage(message);
                        _dataBucket.SaveChanges();
                    }
                    CommandResult.ScrollToBottom = false;
                }
                else
                    CommandResult.WriteLine("Message '{0}' does not belong to you.", messageId);
            }
            else
                CommandResult.WriteLine("There is no message with ID '{0}'.", messageId);
        }
    }
}
