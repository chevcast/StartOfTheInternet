using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Core.Data.Entities;

namespace Terminal.Core.Data.Repositories.Interfaces
{
    /// <summary>
    /// A repository to store bans.
    /// </summary>
    public interface IBanRepository
    {
        /// <summary>
        /// Gets a ban by the banned user.
        /// </summary>
        /// <param name="username">The user who has been banned.</param>
        /// <returns>A ban entity.</returns>
        Ban GetBanBy_User(string username);

        /// <summary>
        /// Add a ban to the repository.
        /// </summary>
        /// <param name="ban">The ban to be added.</param>
        void AddBan(Ban ban);

        /// <summary>
        /// Updates an existing ban in the repository.
        /// </summary>
        /// <param name="ban">The ban to be updated.</param>
        void UpdateBan(Ban ban);

        /// <summary>
        /// Delete a ban from the repository.
        /// </summary>
        /// <param name="ban">The ban to be deleted.</param>
        void DeleteBan(Ban ban);
    }
}