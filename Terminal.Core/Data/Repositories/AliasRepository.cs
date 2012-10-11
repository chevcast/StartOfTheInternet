using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Core;
using Terminal.Core.Data.Entities;

namespace Terminal.Core.Data.Repositories
{
    /// <summary>
    /// Repository for persisting aliases to the Entity Framework data context.
    /// </summary>
    public class AliasRepository : IAliasRepository
    {
        /// <summary>
        /// Every repository requires an instance of the Entity Framework data context.
        /// </summary>
        EntityContainer _entityContainer;

        public AliasRepository(EntityContainer entityContainer)
        {
            _entityContainer = entityContainer;
        }

        /// <summary>
        /// Adds an alias to the data context.
        /// </summary>
        /// <param name="alias">The alias to be added.</param>
        public void AddAlias(Alias alias)
        {
            _entityContainer.Aliases.Add(alias);
        }

        /// <summary>
        /// Deletes an alias from the data context.
        /// </summary>
        /// <param name="alias">The alias to be deleted.</param>
        public void DeleteAlias(Alias alias)
        {
            _entityContainer.Aliases.Remove(alias);
        }

        /// <summary>
        /// Retrieves an alias from the data context by username and shortcut.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <param name="shortcut">The shortcut name.</param>
        /// <returns>An alias entity.</returns>
        public Alias GetAlias(string username, string shortcut)
        {
            var query = _entityContainer.Aliases
                .Where(x => x.Username.ToLower() == username.ToLower())
                .Where(x => x.Shortcut.ToLower() == shortcut.ToLower())
                .FirstOrDefault();
            return query;
        }

        /// <summary>
        /// Retrieves all aliases by username.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <returns>An enumerable list of aliases.</returns>
        public IEnumerable<Alias> GetAliases(string username)
        {
            var query = _entityContainer.Aliases
                .Where(x => x.Username.ToLower() == username.ToLower());
            return query;
        }
    }

    /// <summary>
    /// A repository for storing command aliases.
    /// </summary>
    public interface IAliasRepository
    {
        /// <summary>
        /// Adds an alias to the repository.
        /// </summary>
        /// <param name="alias">The alias to be added.</param>
        void AddAlias(Alias alias);

        /// <summary>
        /// Deletes an alias from the repository.
        /// </summary>
        /// <param name="alias">The alias to be deleted.</param>
        void DeleteAlias(Alias alias);

        /// <summary>
        /// Obtains an alias by username and shortcut.
        /// </summary>
        /// <param name="username">The name of the user specifying the alias.</param>
        /// <param name="shortcut">The shortcut defined by the user.</param>
        /// <returns>A command alias.</returns>
        Alias GetAlias(string username, string shortcut);

        /// <summary>
        /// Get all aliases by username.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <returns>An enumerable list of aliases.</returns>
        IEnumerable<Alias> GetAliases(string username);
    }
}