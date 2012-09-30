using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Domain.Data.Entities;

namespace Terminal.Domain.Data.Repositories.Interfaces
{
    /// <summary>
    /// A repository for storing application variables.
    /// </summary>
    public interface IVariableRepository
    {
        /// <summary>
        /// Adds or edits a variable in the repository.
        /// </summary>
        /// <param name="name">The name of the variable to be added or modified.</param>
        /// <param name="value">The value of the variable to be added or modified.</param>
        void ModifyVariable(string name, string value);

        /// <summary>
        /// Gets a variable from the variable repository.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        string GetVariable(string name);
    }
}