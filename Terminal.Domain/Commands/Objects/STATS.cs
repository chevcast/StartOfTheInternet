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
    public class STATS : ICommand
    {
        private IDataBucket _dataBucket;

        public STATS(IDataBucket dataBucket)
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
            get { return "STATS"; }
        }

        public string Parameters
        {
            get { return "[Option(s)]"; }
        }

        public string Description
        {
            get { return "Allows you to view various statistics."; }
        }

        public bool ShowHelp
        {
            get { return true; }
        }

        public void Invoke(string[] args)
        {
            bool showHelp = false;
            bool users = false;
            bool mods = false;
            bool forum = false;

            var options = new OptionSet();
            options.Add(
                "?|help",
                "Show help information.",
                x => showHelp = x != null
            );
            options.Add(
                "users",
                "Show statistics about members.",
                x => users = x != null
            );
            options.Add(
                "modsquad",
                "List all moderators.",
                x => mods = x != null
            );
            options.Add(
                "forum",
                "Display forum statistics.",
                x => forum = x != null
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
                        else
                        {
                            if (users)
                            {
                                var userStats = _dataBucket.UserRepository.GetUserStatistics();
                                var loggedInUsers = _dataBucket.UserRepository.GetLoggedInUsers();

                                var displayMode = DisplayMode.DontType;
                                CommandResult.WriteLine(displayMode, "There are {0} users registered.", userStats.TotalRegisteredUsers);
                                CommandResult.WriteLine(displayMode, "{0} users are currently banned for various acts of faggotry.", userStats.TotalBannedUsers);
                                CommandResult.WriteLine();
                                CommandResult.WriteLine(displayMode, "{0} users have registered within the last 24 hours.", userStats.NewUsersInTheLast24Hours);
                                CommandResult.WriteLine(displayMode, "{0} users have registered within the last week.", userStats.NewUsersInTheLastWeek);
                                CommandResult.WriteLine(displayMode, "{0} users have registered within the last month.", userStats.NewUsersInTheLastMonth);
                                CommandResult.WriteLine(displayMode, "{0} users have registered within the last year.", userStats.NewUsersInTheLastYear);
                                CommandResult.WriteLine();
                                CommandResult.WriteLine(displayMode, "{0} users have logged in within the last 24 hours.", userStats.LoggedInWithinTheLast24Hours);
                                CommandResult.WriteLine(displayMode, "{0} users have logged in within the last week.", userStats.LoggedInWithinTheLastWeek);
                                CommandResult.WriteLine(displayMode, "{0} users have logged in within the last month.", userStats.LoggedInWithinTheLastMonth);
                                CommandResult.WriteLine(displayMode, "{0} users have logged in within the last year.", userStats.LoggedInWithinTheLastYear);
                                CommandResult.WriteLine();
                                CommandResult.WriteLine(displayMode | DisplayMode.Dim, new string('-', AppSettings.DividerLength));
                                CommandResult.WriteLine();
                                CommandResult.WriteLine(displayMode, "There are currently {0} user(s) online.", loggedInUsers.Count());
                                CommandResult.WriteLine();
                                foreach (var user in loggedInUsers)
                                    CommandResult.WriteLine(displayMode, user.Username);
                            }
                            if (mods)
                            {
                                var staff = _dataBucket.UserRepository.GetModeratorsAndAdministrators();
                                var displayMode = DisplayMode.DontType;
                                CommandResult.WriteLine(displayMode, "There are {0} moderators.", staff.Count());
                                CommandResult.WriteLine();
                                foreach (var user in staff)
                                {
                                    var tenMinutesAgo = DateTime.UtcNow.AddMinutes(-10);
                                    var isOnline = user.LastLogin > tenMinutesAgo;
                                    CommandResult.WriteLine(
                                        displayMode,
                                        "{0}{1}{2}",
                                        user.Username,
                                        user.IsAdministrator ? " (admin)" : null,
                                        isOnline ? " (online)" : null
                                    );
                                }
                            }
                            if (forum)
                            {
                                var forumStats = _dataBucket.TopicRepository.GetForumStats();
                                int numReplies = _dataBucket.TopicRepository.GetTopic(forumStats.MostPopularTopic).Replies.Count();

                                var displayMode = DisplayMode.DontType;
                                CommandResult.WriteLine(displayMode, "There are {0} total topics.", forumStats.TotalTopics);
                                CommandResult.WriteLine(displayMode, "Topic {0} is the most popular topic with {1} replies.", forumStats.MostPopularTopic, numReplies);
                                CommandResult.WriteLine(displayMode, "{0} topics have been created within the last 24 hours.", forumStats.TopicsInTheLast24Hours);
                                CommandResult.WriteLine(displayMode, "{0} topics have been created within the last week.", forumStats.TopicsInTheLastWeek);
                                CommandResult.WriteLine(displayMode, "{0} topics have been created within the last month.", forumStats.TopicsInTheLastMonth);
                                CommandResult.WriteLine(displayMode, "{0} topics have been created within the last year.", forumStats.TopicsInTheLastYear);
                                CommandResult.WriteLine();
                                CommandResult.WriteLine(displayMode, "There are {0} total posts.", forumStats.TotalPosts);
                                CommandResult.WriteLine(displayMode, "{0} posts have been made within the last 24 hours.", forumStats.PostsInTheLast24Hours);
                                CommandResult.WriteLine(displayMode, "{0} posts have been made within the last week.", forumStats.PostsInTheLastWeek);
                                CommandResult.WriteLine(displayMode, "{0} posts have been made within the last month.", forumStats.PostsInTheLastMonth);
                                CommandResult.WriteLine(displayMode, "{0} posts have been made within the last year.", forumStats.PostsInTheLastYear);
                            }
                        }
                    }
                }
                catch (OptionException ex)
                {
                    CommandResult.WriteLine(ex.Message);
                }
            }
        }
    }
}
