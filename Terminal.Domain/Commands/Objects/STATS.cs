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
    public class STATS : ICommand
    {
        private IUserRepository _userRepository;
        private ITopicRepository _topicRepository;

        public STATS(
            IUserRepository userRepository,
            ITopicRepository topicRepository
        )
        {
            _userRepository = userRepository;
            _topicRepository = topicRepository;
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
                this.CommandResult.WriteLine(DisplayTemplates.InvalidArguments);
            }
            else
            {
                try
                {
                    var parsedArgs = options.Parse(args).ToArray();

                    if (parsedArgs.Length == args.Length)
                    {
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
                        else
                        {
                            if (users)
                            {
                                var userStats = _userRepository.GetUserStatistics();
                                var loggedInUsers = _userRepository.GetLoggedInUsers();

                                var displayMode = DisplayMode.DontType;
                                this.CommandResult.WriteLine(displayMode, "There are {0} users registered.", userStats.TotalRegisteredUsers);
                                this.CommandResult.WriteLine(displayMode, "{0} users are currently banned for various acts of faggotry.", userStats.TotalBannedUsers);
                                this.CommandResult.WriteLine();
                                this.CommandResult.WriteLine(displayMode, "{0} users have registered within the last 24 hours.", userStats.NewUsersInTheLast24Hours);
                                this.CommandResult.WriteLine(displayMode, "{0} users have registered within the last week.", userStats.NewUsersInTheLastWeek);
                                this.CommandResult.WriteLine(displayMode, "{0} users have registered within the last month.", userStats.NewUsersInTheLastMonth);
                                this.CommandResult.WriteLine(displayMode, "{0} users have registered within the last year.", userStats.NewUsersInTheLastYear);
                                this.CommandResult.WriteLine();
                                this.CommandResult.WriteLine(displayMode, "{0} users have logged in within the last 24 hours.", userStats.LoggedInWithinTheLast24Hours);
                                this.CommandResult.WriteLine(displayMode, "{0} users have logged in within the last week.", userStats.LoggedInWithinTheLastWeek);
                                this.CommandResult.WriteLine(displayMode, "{0} users have logged in within the last month.", userStats.LoggedInWithinTheLastMonth);
                                this.CommandResult.WriteLine(displayMode, "{0} users have logged in within the last year.", userStats.LoggedInWithinTheLastYear);
                                this.CommandResult.WriteLine();
                                this.CommandResult.WriteLine(displayMode | DisplayMode.Dim, new string('-', AppSettings.DividerLength));
                                this.CommandResult.WriteLine();
                                this.CommandResult.WriteLine(displayMode, "There are currently {0} user(s) online.", loggedInUsers.Count());
                                this.CommandResult.WriteLine();
                                foreach (var user in loggedInUsers)
                                    this.CommandResult.WriteLine(displayMode, user.Username);
                            }
                            if (mods)
                            {
                                var staff = _userRepository.GetModeratorsAndAdministrators();
                                var displayMode = DisplayMode.DontType;
                                this.CommandResult.WriteLine(displayMode, "There are {0} moderators.", staff.Count());
                                this.CommandResult.WriteLine();
                                foreach (var user in staff)
                                {
                                    var tenMinutesAgo = DateTime.UtcNow.AddMinutes(-10);
                                    var isOnline = user.LastLogin > tenMinutesAgo;
                                    this.CommandResult.WriteLine(
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
                                var forumStats = _topicRepository.GetForumStats();
                                int numReplies = _topicRepository.GetTopic(forumStats.MostPopularTopic).Replies.Count();

                                var displayMode = DisplayMode.DontType;
                                this.CommandResult.WriteLine(displayMode, "There are {0} total topics.", forumStats.TotalTopics);
                                this.CommandResult.WriteLine(displayMode, "Topic {0} is the most popular topic with {1} replies.", forumStats.MostPopularTopic, numReplies);
                                this.CommandResult.WriteLine(displayMode, "{0} topics have been created within the last 24 hours.", forumStats.TopicsInTheLast24Hours);
                                this.CommandResult.WriteLine(displayMode, "{0} topics have been created within the last week.", forumStats.TopicsInTheLastWeek);
                                this.CommandResult.WriteLine(displayMode, "{0} topics have been created within the last month.", forumStats.TopicsInTheLastMonth);
                                this.CommandResult.WriteLine(displayMode, "{0} topics have been created within the last year.", forumStats.TopicsInTheLastYear);
                                this.CommandResult.WriteLine();
                                this.CommandResult.WriteLine(displayMode, "There are {0} total posts.", forumStats.TotalPosts);
                                this.CommandResult.WriteLine(displayMode, "{0} posts have been made within the last 24 hours.", forumStats.PostsInTheLast24Hours);
                                this.CommandResult.WriteLine(displayMode, "{0} posts have been made within the last week.", forumStats.PostsInTheLastWeek);
                                this.CommandResult.WriteLine(displayMode, "{0} posts have been made within the last month.", forumStats.PostsInTheLastMonth);
                                this.CommandResult.WriteLine(displayMode, "{0} posts have been made within the last year.", forumStats.PostsInTheLastYear);
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
    }
}
