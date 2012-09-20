using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Domain.Entities;
using Terminal.Domain.Objects;

namespace Terminal.Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository for storing users.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Add user to the repository.
        /// </summary>
        /// <param name="user">The user to be added.</param>
        void AddUser(User user);

        /// <summary>
        /// Update an existing user in the repository.
        /// </summary>
        /// <param name="user">The user to be updated.</param>
        void UpdateUser (User user);

        /// <summary>
        /// Delete a user from the repository.
        /// </summary>
        /// <param name="user">The user to be deleted.</param>
        void DeleteUser(User user);

        /// <summary>
        /// Get a user by the username.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <returns>A user entity.</returns>
        User GetUser(string username);

        /// <summary>
        /// Check if the username exists.
        /// </summary>
        /// <param name="username">The desired username.</param>
        /// <returns>True if the username exists.</returns>
        bool CheckUserExists(string username);

        /// <summary>
        /// Get the stored version of a username.
        /// Note: Preserves casing.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <returns>A correctly cased username.</returns>
        string GetStoredUsername(string username);

        /// <summary>
        /// Get all users active within the last ten minutes.
        /// </summary>
        /// <returns>An enumerable list of users.</returns>
        IEnumerable<User> GetLoggedInUsers();

        IEnumerable<User> GetModeratorsAndAdministrators();

        Role GetRole(string roleName);

        UserStats GetUserStatistics();

        void IgnoreUser(string initiatingUsername, string ignoredUsername);

        void UnignoreUser(string initiatingUsername, string ignoredUsername);

        void UnbanUser(string username);

        IEnumerable<UserActivityLogItem> GetOffenseHistory(string username);

        LUEser GetLUEser(string username);

        void AddLUEser(LUEser llUser);
    }
}