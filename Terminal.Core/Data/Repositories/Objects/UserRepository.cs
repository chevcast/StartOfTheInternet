using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Core.Data.Repositories.Interfaces;
using Terminal.Core.Data.Entities;
using Terminal.Core.Objects;
using Terminal.Core.ExtensionMethods;

namespace Terminal.Core.Data.Repositories.Objects
{
    /// <summary>
    /// Repository for persisting users to the Entity Framework data context.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        /// <summary>
        /// Every repository requires an instance of the Entity Framework data context.
        /// </summary>
        EntityContainer _entityContainer;

        public UserRepository(EntityContainer entityContainer)
        {
            _entityContainer = entityContainer;
        }

        /// <summary>
        /// Adds a user to the data context.
        /// </summary>
        /// <param name="user">The user to be added.</param>
        public void AddUser(User user)
        {
            _entityContainer.Users.Add(user);
        }

        /// <summary>
        /// Updates an existing user in the data context.
        /// </summary>
        /// <param name="user">The user to be updated.</param>
        public void UpdateUser(User user)
        {
            
        }

        /// <summary>
        /// Deletes a user from the data context.
        /// </summary>
        /// <param name="user">The user to be deleted.</param>
        public void DeleteUser(User user)
        {
            _entityContainer.Users.Remove(user);
        }

        /// <summary>
        /// Retrieve user from the data context by the username.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <returns>A user entity.</returns>
        public User GetUser(string username)
        {
            var query = _entityContainer.Users.Where(x => x.Username.ToLower() == username.ToLower());
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Check if a user currently exists in the data context.
        /// </summary>
        /// <param name="username">The desired username.</param>
        /// <returns>True if the user already exists.</returns>
        public bool CheckUserExists(string username)
        {
            var query = _entityContainer.Users.Any(x => x.Username.ToLower() == username.ToLower());
            return query;
        }

        /// <summary>
        /// Get the stored version of a username.
        /// Note: Preserves casing.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <returns>A correctly cased username.</returns>
        public string GetStoredUsername(string username)
        {
            var query = _entityContainer.Users.SingleOrDefault(x => x.Username.ToLower() == username.ToLower()).Username;
            return query;
        }

        /// <summary>
        /// Retrieve all users from the data context that have been active within the last ten minutes.
        /// </summary>
        /// <returns>An enumerable list of users.</returns>
        public IEnumerable<User> GetLoggedInUsers()
        {
            DateTime tenMinutesAgo = DateTime.UtcNow.AddMinutes(-10);
            var users = from x in _entityContainer.Users
                        where x.LastLogin > tenMinutesAgo
                        orderby x.Username
                        select x;
            return users;
        }

        public Role GetRole(string roleName)
        {
            return _entityContainer.Roles.SingleOrDefault(x => x.Name.ToLower() == roleName.ToLower());
        }

        public UserStats GetUserStatistics()
        {
            var oneDayAgo = DateTime.UtcNow.AddHours(-24);
            var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
            var oneYearAgo = DateTime.UtcNow.AddYears(-1);

            var userStats = new UserStats
            {
                TotalBannedUsers = _entityContainer.Users.Count(x => x.BanInfo != null),
                LoggedInWithinTheLast24Hours = _entityContainer.Users.Count(x => x.LastLogin >= oneDayAgo),
                LoggedInWithinTheLastWeek = _entityContainer.Users.Count(x => x.LastLogin >= oneWeekAgo),
                LoggedInWithinTheLastMonth = _entityContainer.Users.Count(x => x.LastLogin >= oneMonthAgo),
                LoggedInWithinTheLastYear = _entityContainer.Users.Count(x => x.LastLogin >= oneYearAgo),
                NewUsersInTheLast24Hours = _entityContainer.Users.Count(x => x.JoinDate >= oneDayAgo),
                NewUsersInTheLastWeek = _entityContainer.Users.Count(x => x.JoinDate >= oneWeekAgo),
                NewUsersInTheLastMonth = _entityContainer.Users.Count(x => x.JoinDate >= oneMonthAgo),
                NewUsersInTheLastYear = _entityContainer.Users.Count(x => x.JoinDate >= oneYearAgo),
                TotalRegisteredUsers = _entityContainer.Users.Count()
            };

            return userStats;
        }

        public IEnumerable<User> GetModeratorsAndAdministrators()
        {
            return _entityContainer.Users.Where(x => x.Roles.Any(y => 
                y.Name.Equals("Moderator", StringComparison.InvariantCultureIgnoreCase)
                || y.Name.Equals("Administrator", StringComparison.InvariantCultureIgnoreCase)));
        }


        public void IgnoreUser(string initiatingUsername, string ignoredUsername)
        {
            _entityContainer.Ignores.Add(new Ignore
            {
                InitiatingUser = initiatingUsername,
                IgnoredUser = ignoredUsername
            });
        }

        public void UnignoreUser(string initiatingUsername, string ignoredUsername)
        {
            var ignoreItem = _entityContainer.Ignores
                .SingleOrDefault(x => x.InitiatingUser.Equals(initiatingUsername, StringComparison.InvariantCultureIgnoreCase)
                && x.IgnoredUser.Equals(ignoredUsername, StringComparison.InvariantCultureIgnoreCase));
            _entityContainer.Ignores.Remove(ignoreItem);
        }

        public void UnbanUser(string username)
        {
            var banInfo = _entityContainer.Bans
                .SingleOrDefault(x => x.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            _entityContainer.Bans.Remove(banInfo);
        }

        public IEnumerable<UserActivityLogItem> GetOffenseHistory(string username)
        {
            return _entityContainer.UserActivityLog
                .Where(x => x.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.Type.Equals("Warning", StringComparison.InvariantCultureIgnoreCase)
                    || x.Type.Equals("Ban", StringComparison.InvariantCultureIgnoreCase));
        }

        public LUEser GetLUEser(string username)
        {
            return _entityContainer.LUEsers.SingleOrDefault(x => x.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
        }

        public void AddLUEser(LUEser llUser)
        {
            _entityContainer.LUEsers.Add(llUser);
        }
    }
}