using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Core.Data.Entities;
using Terminal.Core.Enums;
using Terminal.Core.ExtensionMethods;
using Terminal.Core.Settings;

namespace Terminal.Core.Objects
{
    /// <summary>
    /// This object contains information that is usable by a UI project in determining what and how to display the results of the terminal core.
    /// </summary>
    public class CommandResult
    {
        private TerminalEvents _terminalEvents;

        /// <summary>
        /// Set default values for certain properties.
        /// </summary>
        public CommandResult(TerminalEvents terminalEvents)
        {
            DisplayItems = new List<DisplayItem>();
            ScrollToBottom = true;
            _terminalEvents = terminalEvents;
        }

        /// <summary>
        /// The command that was executed.
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// The command context as set by the executed command.
        /// This should usually be passed back into the terminal core on the next execution.
        /// </summary>
        public CommandContext CommandContext { get; set; }

        /// <summary>
        /// True if the UI should close down the terminal.
        /// </summary>
        public bool Exit { get; set; }

        /// <summary>
        /// True if the UI should clear the screen.
        /// </summary>
        public bool ClearScreen { get; set; }

        /// <summary>
        /// The currently logged in user.
        /// </summary>
        public User CurrentUser { get; set; }

        public string IPAddress { get; set; }

        /// <summary>
        /// True if there is a password prompt pending.
        /// This is useful if the UI wishes to obfuscate the password being entered by the user.
        /// </summary>
        public bool PasswordField { get; set; }

        public bool ScrollToBottom { get; set; }

        /// <summary>
        /// Text to be edited. UI should populate command line with this text.
        /// </summary>
        public string EditText { get; set; }

        public string TerminalTitle { get; set; }

        /// <summary>
        /// The display array contains various objects to be displayed.
        /// The UI should know how to interpret each one and display them correctly.
        /// 
        /// The available types are:
        /// 
        /// string - should output directly to terminal.
        /// int - should pause for a specified time.
        /// </summary>
        public List<DisplayItem> DisplayItems { get; set; }

        #region Shortcut Methods

        public bool IsUserLoggedIn
        {
            get { return CurrentUser != null; }
        }

        public bool UserLoggedAndModOrAdmin()
        {
            return IsUserLoggedIn && (CurrentUser.IsModeratorOrAdministrator());
        }

        public bool UserLoggedAndMod()
        {
            return IsUserLoggedIn && (CurrentUser.IsModerator);
        }

        public bool UserLoggedAndAdmin()
        {
            return IsUserLoggedIn && (CurrentUser.IsAdministrator);
        }

        /// <summary>
        /// Writes a blank line to the display.
        /// </summary>
        public void WriteLine()
        {
            WriteLine("");
        }

        /// <summary>
        /// Writes a line of text to the display.
        /// </summary>
        /// <param name="text">The text to be written.</param>
        /// <param name="args">An object or objects to write using format.</param>
        public void WriteLine(string text, params object[] args)
        {
            WriteLine(DisplayMode.None, text, args);
        }

        /// <summary>
        /// Writes a line of text to the display with custom display options.
        /// </summary>
        /// /// <param name="displayMode">The custom display mode.</param>
        /// <param name="text">The text to be written.</param>
        /// <param name="args">An object or objects to write using format.</param>
        public void WriteLine(DisplayMode displayMode, string text, params object[] args)
        {
            if (args.Length == 0)
                text = text.Replace("{", "{{").Replace("}", "}}");
            DisplayItems.Add(new DisplayItem
            {
                Text = string.Format(text, args),
                DisplayMode = displayMode
            });
        }

        /// <summary>
        /// Sets up a prompt where all data from the command line will be dumped into the PromptData collection.
        /// </summary>
        /// <param name="command">The command to be set as the contexted command.</param>
        /// <param name="args">The arguments to be set as the contexted arguments.</param>
        /// <param name="text">The custom text to display next to the command line.</param>
        public void SetPrompt(string command, string[] args, string text)
        {
            CommandContext.Prompt = true;
            SetContext(ContextStatus.Forced, command, args, text);
        }

        /// <summary>
        /// Set the current command context.
        /// </summary>
        /// <param name="status">The status of the context you are setting.</param>
        /// <param name="command">The command to be set as the contexted command.</param>
        /// <param name="args">The arguments to be set as the contexted arguments.</param>
        /// <param name="text">The custom text to display next to the command line.</param>
        public void SetContext(ContextStatus status, string command, string[] args, string text)
        {
            if (_terminalEvents.OnBeforeSetContext != null)
                _terminalEvents.OnBeforeSetContext(this);
            if (CommandContext.Status == ContextStatus.Passive)
                if (status == ContextStatus.Forced)
                    BackupContext();
            CommandContext.Status = status;
            CommandContext.Command = command;
            CommandContext.Args = args;
            CommandContext.Text = text;
            if (_terminalEvents.OnSetContext != null)
                _terminalEvents.OnSetContext(this);
        }

        /// <summary>
        /// Save the current command context as the previous command context.
        /// </summary>
        private void BackupContext()
        {
            if (_terminalEvents.OnBeforeBackupContext != null)
                _terminalEvents.OnBeforeBackupContext(this);
            CommandContext.PreviousContext = new CommandContext
            {
                Status = CommandContext.Status,
                Command = CommandContext.Command,
                Args = CommandContext.Args,
                Text = CommandContext.Text,
                Prompt = CommandContext.Prompt,
                PromptData = CommandContext.PromptData,
                PreviousContext = CommandContext.PreviousContext
            };
            if (_terminalEvents.OnBackupContext != null)
                _terminalEvents.OnBackupContext(this);
        }

        /// <summary>
        /// Restore the current command context to the state of the previous command context.
        /// </summary>
        public void RestoreContext()
        {
            if (CommandContext.PreviousContext != null)
            {
                if (_terminalEvents.OnBeforeRestoreContext != null)
                    _terminalEvents.OnBeforeRestoreContext(this);
                CommandContext.Status = CommandContext.PreviousContext.Status;
                CommandContext.Command = CommandContext.PreviousContext.Command;
                CommandContext.Args = CommandContext.PreviousContext.Args;
                CommandContext.Text = CommandContext.PreviousContext.Text;
                CommandContext.Prompt = CommandContext.PreviousContext.Prompt;
                CommandContext.PromptData = CommandContext.PreviousContext.PromptData;
                CommandContext.PreviousContext = CommandContext.PreviousContext.PreviousContext;
                if (_terminalEvents.OnRestoreContext != null)
                    _terminalEvents.OnRestoreContext(this);
            }
            else
            {
                DeactivateContext();
            }
        }

        /// <summary>
        /// Disable the current command context.
        /// </summary>
        public void DeactivateContext()
        {
            if (_terminalEvents.OnBeforeDeactivateContext != null)
                _terminalEvents.OnBeforeDeactivateContext(this);
            CommandContext.Status = ContextStatus.Disabled;
            CommandContext.Command = null;
            CommandContext.Args = null;
            CommandContext.Text = null;
            CommandContext.Prompt = false;
            CommandContext.PromptData = null;
            CommandContext.CurrentPage = 0;
            CommandContext.CurrentLinkTags = null;
            CommandContext.CurrentSearchTerms = null;
            CommandContext.CurrentSortOrder = null;
            if (_terminalEvents.OnDeactivateContext != null)
                _terminalEvents.OnDeactivateContext(this);
        }

        #endregion
    }
}
