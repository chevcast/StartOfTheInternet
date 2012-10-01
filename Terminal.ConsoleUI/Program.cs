using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Terminal.Core.Utilities;
using Terminal.Core.Enums;
using Terminal.Core;
using Terminal.Core.Commands.Objects;
using Ninject;
using Terminal.Core.Ninject;
using Terminal.Core.Data.Entities;
using Terminal.Core.Objects;
using System.Threading;
using Terminal.Core.Commands.Interfaces;
using Terminal.Core.Settings;
using Terminal.Core.ExtensionMethods;
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
        private static TerminalApi _terminalApi;
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
            // Set the database initializer and migrate database to latest version as specified by EF migrations
            // in Terminal.Core.Data.Entities.Migrations.
            Database.SetInitializer<EntityContainer>(new MigrateDatabaseToLatestVersion<EntityContainer, Terminal.Core.Data.Entities.Migrations.Configuration>());

            // Load up a typing sound to be played as the console is printing letters to the screen.
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

            // Grab console input stream.
            Console.SetIn(new StreamReader(Console.OpenStandardInput(4000)));
            _beep.Load();

            // Set console aesthetic properties.
            Console.Title = "Terminal - Visitor";
            Console.ForegroundColor = _foregroundColor;
            Console.BackgroundColor = _backgroundColor;
            Console.Clear();

            // Play a fun dial up sound and pretend like the console is connecting to the server.
            //if (Properties.Settings.Default.FirstLoad)
            //{
            //    Console.WriteLine("Connecting...");
            //    SoundPlayer dialUp = new SoundPlayer(Assembly.GetExecutingAssembly()
            //        .GetManifestResourceStream("Terminal.ConsoleUI.dialup.wav"));
            //    dialUp.PlaySync();
            //    Console.WriteLine("Connection established.");
            //    Console.WriteLine();
            //    Properties.Settings.Default.FirstLoad = false;
            //    Properties.Settings.Default.Save();
            //}

            // Adjust height and width to 75% of screen area.
            int width = Convert.ToInt32(Console.LargestWindowWidth * .75);
            int height = Convert.ToInt32(Console.LargestWindowHeight * .75);
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(Console.LargestWindowWidth, 300);

            // Now that the UI is ready, execute the INITIALIZE command in the core to output some welcome
            // text.
            InvokeCommand("INITIALIZE");
        }

        /// <summary>
        /// Pass the command string to the terminal core for execution, then examine the resulting instructions.
        /// </summary>
        /// <param name="commandString">The command string passed from the command line.</param>
        private static void InvokeCommand(string commandString)
        {
            // Instantiate the Ninject kernel and pass in the pre-defined Ninject Module from Terminal.Core.Ninject.
            var kernel = new StandardKernel(new TerminalBindings(false));
            // Grab the terminal API object from Ninject.
            _terminalApi = kernel.Get<TerminalApi>();

            // Set the username on the API. This tells the API if someone is logged in or not. If nobody is logged in
            // then _username is null by default;
            _terminalApi.Username = _username;
            // Set the CommandContext object. This persists state information between API requests.
            _terminalApi.CommandContext = _commandContext;

            // Launch a separate thread to print a loading message while waiting for the terminal API to execute commands.
            var loadingThread = new Thread(ShowLoading);
            loadingThread.Start();

            // Pass command string to the terminal API. No pre-parsing necessary, just pass the string.
            var commandResult = _terminalApi.ExecuteCommand(commandString);

            // Stop the loading message thread when API returns.
            loadingThread.Abort();
            loadingThread.Join();

            // Pass result object to special method that will determine how to display the results.
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
        /// Evaluates the display item flags and displays the formatted text.
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
                displayItem.Text = Terminal.Core.Utilities.BBCodeUtility.ConvertTagsForConsole(displayItem.Text);

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
