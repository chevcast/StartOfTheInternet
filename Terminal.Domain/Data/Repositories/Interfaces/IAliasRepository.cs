using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Domain.Data.Entities;

namespace Terminal.Domain.Data.Repositories.Interfaces
{
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