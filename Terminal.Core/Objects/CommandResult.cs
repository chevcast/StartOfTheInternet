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
        /// <summary>
        /// Set default values for certain properties.
        /// </summary>
        public CommandResult()
        {
            DisplayItems = new List<DisplayItem>();
            ScrollToBottom = true;
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

        #endregion
    }
}
