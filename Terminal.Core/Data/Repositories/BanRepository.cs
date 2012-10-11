using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Core.Data.Entities;

namespace Terminal.Core.Data.Repositories
{
    /// <summary>
    /// A repository for persisting bans to the Entity Framework data context.
    /// </summary>
    public class BanRepository : IBanRepository
    {
        #region dependencies

       /// <summary>
        /// Every repository requires an instance of the Entity Framework data context.
        /// </summary>
        EntityContainer _entityContainer;

        public BanRepository(EntityContainer entityContainer)
        {
            _entityContainer = entityContainer;
        }

        #endregion

        #region Interface Members

        /// <summary>
        /// Get a ban by the banned user.
        /// </summary>
        /// <param name="username">The banned user.</param>
        /// <returns>A ban entity.</returns>
        public Ban GetBanBy_User(string username)
        {
            var query = _entityContainer.Bans
                .SingleOrDefault(x => x.Username == username);
            return query;
        }

        /// <summary>
        /// Add a ban to the data context.
        /// </summary>
        /// <param name="ban">The ban to be added.</param>
        public void AddBan(Ban ban)
        {
            _entityContainer.Bans.Add(ban);
        }

        /// <summary>
        /// This method is not used. Please just call SaveChanges().
        /// </summary>
        /// <param name="message">The ban to be updated.</param>
        [Obsolete]
        public void UpdateBan(Ban ban)
        {
            // Do not throw a not implemented exception.
            // If this method is called, do nothing.
            //
            // This allows the code above this layer to use this method
            // in case a future data store needs it.
        }

        /// <summary>
        /// Deletes a ban from the data context.
        /// </summary>
        /// <param name="ban">The ban to be deleted.</param>
        public void DeleteBan(Ban ban)
        {
            _entityContainer.Bans.Remove(ban);
        }

        #endregion

        #region Expressions

        #endregion
    }

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
