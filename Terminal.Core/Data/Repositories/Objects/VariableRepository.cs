using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Core.Data.Repositories.Interfaces;
using Terminal.Core.Data.Entities;
using Terminal.Core.ExtensionMethods;

namespace Terminal.Core.Data.Repositories.Objects
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
}
