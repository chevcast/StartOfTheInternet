using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Terminal.Domain.Utilities;
using Terminal.Domain.Enums;
using Terminal.Domain;
using Terminal.Domain.Commands.Objects;
using Ninject;
using Terminal.Domain.Ninject;
using Terminal.Domain.Entities;
using Terminal.Domain.Repositories.Interfaces;
using Terminal.Domain.Objects;
using System.Threading;
using Terminal.Domain.Commands.Interfaces;
using Terminal.Domain.Settings;
using Terminal.Domain.ExtensionMethods;
using System.Windows.Forms;
using System.IO;
using System.Windows.Media;
using System.Globalization;
using System.Media;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Data.Entity;

namespace Terminal.ConsoleUI
{
    class Program
    {
        private static string _username;
        private static CommandContext _commandContext;
        private static TerminalCore _terminalApi;
        private static bool _appRunning = true;
        private static bool _passwordField = false;
        private static ConsoleColor _foregroundColor;
        private static ConsoleColor _backgroundColor;
        private static ConsoleColor _dimColor;
        private static SoundPlayer _beep;

        /// <summary>
        /// The main entry point to the Terminal.ConsoleUI application.
        /// </summary>
        /// <param name="args">Arguments supplied from the command prompt when initializing this application.</param>
        static void Main(string[] args)
        {
            SetupConsole();

            while (_appRunning)
            {
                Console.WriteLine();
                Console.Write(_terminalApi.CommandContext.Command);
                if (!_terminalApi.CommandContext.Text.IsNullOrEmpty())
                    Console.Write(string.Format(" {0}", _terminalApi.CommandContext.Text));
                Console.Write("> ");

                if (_passwordField)
                    Console.ForegroundColor = _backgroundColor;

                string commandString = Console.ReadLine().Replace("--", "\n");

                AppSettings.MaxLineLength = Console.WindowWidth - 5;

                if (_passwordField)
                    Console.ForegroundColor = _foregroundColor;

                if (!commandString.IsNullOrEmpty())
                    InvokeCommand(commandString);
            }
        }

        /// <summary>
        /// Take actions to initialize the application.
        /// </summary>
        private static void SetupConsole()
        {
            Database.SetInitializer<EntityContainer>(new MigrateDatabaseToLatestVersion<EntityContainer, Terminal.Domain.Migrations.Configuration>());
            var typingSound = "Terminal.ConsoleUI.beeps.wav";
            _foregroundColor = ConsoleColor.Gray;
            _backgroundColor = ConsoleColor.Black;
            _dimColor = ConsoleColor.DarkGray;
            if (DateTime.UtcNow.Month == (int)Month.October)
            {
                typingSound = "Terminal.ConsoleUI.typewriter.wav";
                _foregroundColor = ConsoleColor.Yellow;
                _dimColor = ConsoleColor.DarkYellow;
            }
            else if (DateTime.UtcNow.Month == (int)Month.December)
            {
                _backgroundColor = ConsoleColor.White;
                _foregroundColor = ConsoleColor.Blue;
                _dimColor = ConsoleColor.DarkBlue;
            }
            _beep = new SoundPlayer(Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(typingSound));
            Console.SetIn(new StreamReader(Console.OpenStandardInput(4000)));
            _beep.Load();
            Console.Title = "Terminal - Visitor";
            Console.ForegroundColor = _foregroundColor;
            Console.BackgroundColor = _backgroundColor;
            Console.Clear();
            if (Properties.Settings.Default.FirstLoad)
            {
                Console.WriteLine("Connecting...");
                SoundPlayer dialUp = new SoundPlayer(Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("Terminal.ConsoleUI.dialup.wav"));
                dialUp.PlaySync();
                Console.WriteLine("Connection established.");
                Console.WriteLine();
                Properties.Settings.Default.FirstLoad = false;
                Properties.Settings.Default.Save();
            }
            int width = Convert.ToInt32(Console.LargestWindowWidth * .75);
            int height = Convert.ToInt32(Console.LargestWindowHeight * .75);
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(Console.LargestWindowWidth, 300);
            InvokeCommand("INITIALIZE");
        }

        /// <summary>
        /// Pass the command string to the terminal core for execution, then examine the resulting instructions.
        /// </summary>
        /// <param name="commandString">The command string passed from the command line.</param>
        private static void InvokeCommand(string commandString)
        {
            var kernel = new StandardKernel(new TerminalBindings(false));
            _terminalApi = kernel.Get<TerminalCore>();
            _terminalApi.Username = _username;
            _terminalApi.CommandContext = _commandContext;

            var loadingThread = new Thread(ShowLoading);
            loadingThread.Start();

            var commandResult = _terminalApi.ExecuteCommand(commandString);

            loadingThread.Abort();
            loadingThread.Join();

            InterpretResult(commandResult);
        }

        /// <summary>
        /// Method to be invoked by another thread.
        /// Loading will be displayed until the thread is aborted.
        /// 
        /// I know this is not the most graceful way to accomplish this.
        /// </summary>
        private static void ShowLoading()
        {
            bool displayed = false;
            try
            {
                Thread.Sleep(1000);
                displayed = true;
                Console.Write("Loading...");
                while (true)
                {
                    Thread.Sleep(1000);
                    Console.Write(".");
                }
            }
            catch
            {
                if (displayed) Console.WriteLine("done!");
            }
        }

        /// <summary>
        /// Examine command result and perform relevant actions.
        /// </summary>
        /// <param name="commandResult">The command result returned by the terminal core.</param>
        private static void InterpretResult(CommandResult commandResult)
        {
            // Set the terminal core command context to the one returned in the result.
            _commandContext = commandResult.CommandContext;

            // If the result calls for the screen to be cleared, clear it.
            if (commandResult.ClearScreen)
                Console.Clear();

            // Set the terminal core current user to the one returned in the result and display the username in the console title bar.
            _username = commandResult.CurrentUser != null ? commandResult.CurrentUser.Username : null;
            Console.Title = commandResult.TerminalTitle;

            // Add a blank line to the console before displaying results.
            if (commandResult.Display.Count > 0)
                Console.WriteLine();

            // Iterate over the display collection and perform relevant display actions based on the type of the object.
            foreach (var displayInstruction in commandResult.Display)
                Display(displayInstruction);

            if (!commandResult.EditText.IsNullOrEmpty())
                SendKeys.SendWait(commandResult.EditText.Replace("\n", "--"));


            // If the terminal is prompting for a password then set the global PasswordField bool to true.
            _passwordField = commandResult.PasswordField;

            // If the terminal is asking to be closed then kill the runtime loop for the console.
            _appRunning = !commandResult.Exit;
        }

        /// <summary>
        /// Alternative to Console.WriteLine() so that we have control over how text is written to the screen from one location.
        /// </summary>
        /// <param name="text">The text to be displayed.</param>
        private static void Display(string text)
        {
            Display(new DisplayItem
            {
                Text = text,
                DisplayMode = DisplayMode.None
            });
        }

        /// <summary>
        /// Evaluates the display item and displays the formatted text.
        /// </summary>
        /// <param name="displayItem">The display item to be evaluated.</param>
        private static void Display(DisplayItem displayItem)
        {
            Console.ForegroundColor = _foregroundColor;
            Console.BackgroundColor = _backgroundColor;

            if ((displayItem.DisplayMode & DisplayMode.Inverted) != 0)
            {
                Console.ForegroundColor = _backgroundColor;
                Console.BackgroundColor = _foregroundColor;
                displayItem.Text = string.Format(" {0} ", displayItem.Text);
            }
            if ((displayItem.DisplayMode & DisplayMode.Dim) != 0)
                Console.ForegroundColor = _dimColor;
            if ((displayItem.DisplayMode & DisplayMode.Parse) != 0)
                displayItem.Text = Terminal.Domain.Utilities.BBCodeUtility.ConvertTagsForConsole(displayItem.Text);

            if ((displayItem.DisplayMode & DisplayMode.DontType) != 0)
                Console.WriteLine(displayItem.Text);
            else
            {
                if ((displayItem.DisplayMode & DisplayMode.Mute) == 0)
                    _beep.PlayLooping();
                foreach (char c in displayItem.Text)
                {
                    Console.Write(c);
                    Thread.Sleep(10);
                }
                Console.Write(' ');
                Console.WriteLine();
                if ((displayItem.DisplayMode & DisplayMode.Mute) == 0)
                    _beep.Stop();
            }

            Console.ForegroundColor = _foregroundColor;
            Console.BackgroundColor = _backgroundColor;
        }
    }
}
