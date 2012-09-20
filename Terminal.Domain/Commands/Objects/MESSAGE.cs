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
    public class MESSAGE : ICommand
    {
        private IMessageRepository _messageRepository;
        private IUserRepository _userRepository;

        public IEnumerable<ICommand> AvailableCommands { get; set; }

        public MESSAGE(IMessageRepository messageRepository, IUserRepository userRepository)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

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
                this.CommandResult.WriteLine(DisplayTemplates.InvalidArguments);
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
                                this.CommandResult.WriteLine("'{0}' is not a valid board ID.", parsedArgs[0]);
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
                        else if (reply)
                        {
                            if (parsedArgs.Length > 0)
                            {
                                if (parsedArgs[0].IsLong())
                                {
                                    var messageId = parsedArgs[0].ToLong();
                                    var message = _messageRepository.GetMessage(messageId);
                                    if (message != null)
                                    {
                                        if (message.Recipient.Is(this.CommandResult.CurrentUser.Username))
                                        {
                                            if (this.CommandResult.CommandContext.PromptData == null)
                                            {
                                                this.CommandResult.WriteLine("Write the body of your message.");
                                                this.CommandResult.CommandContext.SetPrompt(this.Name, args, string.Format("{0} REPLY Body", messageId));
                                            }
                                            else if (this.CommandResult.CommandContext.PromptData.Length == 1)
                                            {
                                                _messageRepository.AddMessage(new Message
                                                {
                                                    Body = this.CommandResult.CommandContext.PromptData[0],
                                                    RecipientLocked = false,
                                                    SenderLocked = false,
                                                    MessageRead = false,
                                                    Recipient = message.Sender,
                                                    Sender = this.CommandResult.CurrentUser.Username,
                                                    RecipientDeleted = false,
                                                    SenderDeleted = false,
                                                    Subject = string.Format("Re: {0}", message.Subject),
                                                    SentDate = DateTime.UtcNow
                                                });
                                                this.CommandResult.WriteLine("Reply sent succesfully.");
                                                this.CommandResult.CommandContext.Restore();
                                            }
                                        }
                                        else
                                            this.CommandResult.WriteLine("Message '{0}' was not sent to you.", messageId);
                                    }
                                    else
                                        this.CommandResult.WriteLine("There is no message with ID '{0}'.", messageId);
                                }
                                else
                                    this.CommandResult.WriteLine("'{0}' is not a valid message ID.", parsedArgs[0]);
                            }
                            else
                                this.CommandResult.WriteLine("You must supply a message ID.");
                        }
                        else if (delete)
                        {
                            if (parsedArgs.Length > 0)
                            {
                                if (parsedArgs[0].IsLong())
                                {
                                    var messageId = parsedArgs[0].ToLong();
                                    var message = _messageRepository.GetMessage(messageId);
                                    if (message != null)
                                    {
                                        if (message.Recipient.Is(this.CommandResult.CurrentUser.Username))
                                        {
                                            if (!message.RecipientLocked)
                                            {
                                                message.RecipientDeleted = true;
                                                _messageRepository.UpdateMessage(message);
                                                this.CommandResult.WriteLine("Message '{0}' deleted successfully.", messageId);
                                            }
                                            else
                                                this.CommandResult.WriteLine("You have locked message '{0}' and cannot delete it.", messageId);
                                        }
                                        else if (message.Sender.Is(this.CommandResult.CurrentUser.Username))
                                        {
                                            if (!message.SenderLocked)
                                            {
                                                message.SenderDeleted = true;
                                                _messageRepository.UpdateMessage(message);
                                                this.CommandResult.WriteLine("Message '{0}' deleted successfully.", messageId);
                                            }
                                            else
                                                this.CommandResult.WriteLine("You have locked message '{0}' and cannot delete it.", messageId);
                                        }
                                        else
                                            this.CommandResult.WriteLine("Message '{0}' does not belong to you.", messageId);
                                    }
                                    else
                                        this.CommandResult.WriteLine("There is no message with ID '{0}'.", messageId);
                                }
                                else
                                    this.CommandResult.WriteLine("'{0}' is not a valid message ID.", parsedArgs[0]);
                            }
                            else
                                this.CommandResult.WriteLine("You must supply a message ID.");
                        }
                        else if (newMessage)
                        {
                            if (this.CommandResult.CommandContext.PromptData == null)
                            {
                                this.CommandResult.WriteLine("Type the name of the recipient.");
                                this.CommandResult.CommandContext.SetPrompt(this.Name, args, "USERNAME");
                            }
                            else if (this.CommandResult.CommandContext.PromptData.Length == 1)
                            {
                                var user = _userRepository.GetUser(this.CommandResult.CommandContext.PromptData[0]);
                                if (user != null)
                                {
                                    this.CommandResult.WriteLine("Type the subject of your message.");
                                    this.CommandResult.CommandContext.SetPrompt(this.Name, args, "SUBJECT");
                                }
                                else
                                {
                                    this.CommandResult.WriteLine("'{0}' is not a valid username.", this.CommandResult.CommandContext.PromptData[0]);
                                    this.CommandResult.WriteLine("Re-type the name of the recipient.");
                                    this.CommandResult.CommandContext.PromptData = null;
                                    this.CommandResult.CommandContext.SetPrompt(this.Name, args, "USERNAME");
                                }
                            }
                            else if (this.CommandResult.CommandContext.PromptData.Length == 2)
                            {
                                this.CommandResult.WriteLine("Type the body of your message.");
                                this.CommandResult.CommandContext.SetPrompt(this.Name, args, "BODY");
                            }
                            else if (this.CommandResult.CommandContext.PromptData.Length == 3)
                            {
                                _messageRepository.AddMessage(new Message
                                {
                                    Sender = this.CommandResult.CurrentUser.Username,
                                    Recipient = this.CommandResult.CommandContext.PromptData[0],
                                    Subject = this.CommandResult.CommandContext.PromptData[1],
                                    Body = this.CommandResult.CommandContext.PromptData[2],
                                    SentDate = DateTime.UtcNow
                                });
                                this.CommandResult.WriteLine("Message sent succesfully.");
                                this.CommandResult.CommandContext.Restore();
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
                                        var message = _messageRepository.GetMessage(messageId);
                                        if (message != null)
                                        {
                                            if (message.Recipient.Is(this.CommandResult.CurrentUser.Username))
                                            {
                                                message.RecipientLocked = (bool)lockMessage;
                                                _messageRepository.UpdateMessage(message);
                                                this.CommandResult.WriteLine("Message '{0}' {1} successfully.", messageId, (bool)lockMessage ? "locked" : "unlocked");
                                            }
                                            else if (message.Sender.Is(this.CommandResult.CurrentUser.Username))
                                            {
                                                message.SenderLocked = (bool)lockMessage;
                                                _messageRepository.UpdateMessage(message);
                                                this.CommandResult.WriteLine("Message '{0}' {1} successfully.", messageId, (bool)lockMessage ? "locked" : "unlocked");
                                            }
                                            else
                                                this.CommandResult.WriteLine("Message '{0}' does not belong to you.", messageId);
                                        }
                                        else
                                            this.CommandResult.WriteLine("There is no message with ID '{0}'.", messageId);
                                    }
                                    else
                                        this.CommandResult.WriteLine("'{0}' is not a valid message ID.", parsedArgs[0]);
                                }
                                else
                                    this.CommandResult.WriteLine("You must supply a message ID.");
                            }
                    }
                }
                catch (OptionException ex)
                {
                    this.CommandResult.WriteLine(ex.Message);
                }
            }
        }

        private void WriteMessage(long messageId)
        {
            var message = _messageRepository.GetMessage(messageId);
            if (message != null)
            {
                var isRecipient = message.Recipient.Is(this.CommandResult.CurrentUser.Username);
                var isSender = message.Sender.Is(this.CommandResult.CurrentUser.Username);
                if (isRecipient || isSender || this.CommandResult.CurrentUser.IsAdministrator)
                {
                    var messageLocked = (isRecipient && message.RecipientLocked) || (isSender && message.SenderLocked);
                    this.CommandResult.WriteLine(DisplayMode.Inverted | DisplayMode.DontType, "{{[transmit=MESSAGE]{0}[/transmit]}} {1}{2}", messageId, messageLocked ? "[LOCKED] " : null, message.Subject);
                    this.CommandResult.WriteLine(DisplayMode.DontType, "sent to [transmit=USER]{0}[/transmit] from [transmit=USER]{1}[/transmit] on {2}", message.Recipient, message.Sender, message.SentDate);
                    this.CommandResult.WriteLine();
                    this.CommandResult.WriteLine(DisplayMode.Parse | DisplayMode.DontType, message.Body);
                    this.CommandResult.ClearScreen = true;
                    this.CommandResult.CommandContext.Set(ContextStatus.Passive, this.Name, new string[] { messageId.ToString() }, string.Format("{0}", messageId));
                    if (message.Recipient.Is(this.CommandResult.CurrentUser.Username))
                    {
                        message.MessageRead = true;
                        _messageRepository.UpdateMessage(message);
                    }
                    this.CommandResult.ScrollToBottom = false;
                }
                else
                    this.CommandResult.WriteLine("Message '{0}' does not belong to you.", messageId);
            }
            else
                this.CommandResult.WriteLine("There is no message with ID '{0}'.", messageId);
        }
    }
}
