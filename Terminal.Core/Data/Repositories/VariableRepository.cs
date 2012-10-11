using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Core.Data.Entities;
using Terminal.Core.ExtensionMethods;

namespace Terminal.Core.Data.Repositories
{
    public class VariableRepository : IVariableRepository
    {
        private EntityContainer _entityContainer;

        public VariableRepository(EntityContainer entityContainer)
        {
            _entityContainer = entityContainer;
        }

        public void ModifyVariable(string name, string value)
        {
            var variable = _entityContainer.Variables.SingleOrDefault(x => x.Name.Equals(name));
            if (variable == null)
                _entityContainer.Variables.Add(new Variable { Name = name, Value = value });
            else
                variable.Value = value;
        }

        public string GetVariable(string name)
        {
            var variable = _entityContainer.Variables.SingleOrDefault(x => x.Name.Equals(name));
            if (variable != null)
                return variable.Value;
            return null;
        }
    }

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
