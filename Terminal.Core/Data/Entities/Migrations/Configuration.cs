namespace Terminal.Core.Data.Entities.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Terminal.Core.Data.Entities;
    using System.Collections.Generic;

    public sealed class Configuration : DbMigrationsConfiguration<Terminal.Core.Data.Entities.EntityContainer>
    {
        public Configuration()
        {
            this.MigrationsDirectory = @"Data\Entities\Migrations";
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Terminal.Core.Data.Entities.EntityContainer context)
        {
            if (context.Variables.Find("Registration") == null)
            {
                context.Variables.Add(
                    new Variable { Name = "Registration", Value = "Open" }
                );
                context.SaveChanges();
            }

            if (!context.Roles.Any())
            {
                context.Roles.Add(new Role { Name = "Administrator" });
                context.Roles.Add(new Role { Name = "Moderator" });
                context.Roles.Add(new Role { Name = "User" });
                context.SaveChanges();
            }

            if (!context.Users.Any())
            {
                var adminRole = context.Roles.Find("Administrator");
                context.Users.Add(
                    new User
                    {
                        Username = "Admin",
                        Password = "12345",
                        Credits = 1000000,
                        JoinDate = DateTime.Now,
                        LastLogin = DateTime.Now,
                        Sound = true,
                        Roles = new List<Role> { adminRole },
                        Aliases = new List<Alias>
                        {
                            new Alias { Shortcut = "lb", Command = "BOARDS" },
                            new Alias { Shortcut = "b", Command = "BOARD" },
                            new Alias { Shortcut = "t", Command = "TOPIC" },
                            new Alias { Shortcut = "lm", Command = "MESSAGES" },
                            new Alias { Shortcut = "m", Command = "MESSAGE" },
                        }
                    }
                );
                context.SaveChanges();
            }

            if (!context.Boards.Any())
            {
                context.Boards.Add(new Board
                {
                    BoardID = -1,
                    Name = "Negative",
                    Description = "Only negative discussion is allowed. Express your inner pessimist.",
                    Hidden = true,
                    AllTopics = true
                });
                context.Boards.Add(new Board
                {
                    BoardID = 0,
                    Name = "All Topics",
                    Description = "Read-Only board that shows all topics from all boards.",
                    Locked = true,
                    AllTopics = true
                });
                context.Boards.Add(new Board
                {
                    BoardID = 1,
                    Name = "General",
                    Description = "Any topic goes. This is a place for resistance members to talk about anything, Ingress-related or not.",
                    AllTopics = true
                });
                context.Boards.Add(new Board
                {
                    BoardID = 2,
                    Name = "Utah Resistance",
                    Description = "Talk about Ingress strategy in Utah.",
                    AllTopics = true
                });
                context.Boards.Add(new Board
                {
                    BoardID = 3,
                    Name = "Salt Lake Resistance",
                    Description = "Talk about Ingress strategy in Salt Lake County.",
                    AllTopics = true
                });
                context.Boards.Add(new Board
                {
                    BoardID = 4,
                    Name = "Utah County Resistance",
                    Description = "Talk about Ingress strategy in Utah County.",
                    AllTopics = true
                });
                context.Boards.Add(new Board
                {
                    BoardID = 69,
                    Name = "NSFW",
                    Description = "If you must post NSFW content then post it here."
                });
                context.Boards.Add(new Board
                {
                    BoardID = 70,
                    Name = "Moderators",
                    Description = "This board is for moderators only.",
                    ModsOnly = true
                });
                context.Boards.Add(new Board
                {
                    BoardID = 71,
                    Name = "Board Suggestions",
                    Description = "Want a board for your area/group? Suggest it here and we'll try to make it happen :D",
                });
                context.Boards.Add(new Board
                {
                    BoardID = 72,
                    Name = "Site Feedback",
                    Description = "We are currently working on a new version of the site but feel free to report any issues here and we will do our best to take care of them.",
                });
                context.SaveChanges();
            }
        }
    }
}
