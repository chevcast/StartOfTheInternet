using Ninject.Modules;
using Ninject.Web.Common;
using Terminal.Core.Commands.Interfaces;
using Terminal.Core.Commands.Objects;
using Terminal.Core.Data;
using Terminal.Core.Data.Entities;

namespace Terminal.Core.Ninject
{
    /// <summary>
    /// This module will automatically register all Terminal.Core related bindings.
    /// </summary>
    public class TerminalBindings : NinjectModule
    {
        bool _isWebApplication = false;

        public TerminalBindings(bool isWebApplication)
        {
            _isWebApplication = isWebApplication;
        }

        /// <summary>
        /// Load all terminal bindings.
        /// </summary>
        public override void Load()
        {
            BindEntityContainer();
            BindCommands();

            if (_isWebApplication)
                Bind<TerminalApi>().ToSelf().InRequestScope();
            else
                Bind<TerminalApi>().ToSelf().InSingletonScope();

            Bind<IDataBucket>().To<DataBucket>();
        }

        /// <summary>
        /// Register the entity container and pass in a hard-coded connection string.
        /// </summary>
        private void BindEntityContainer()
        {
            if (!_isWebApplication)
                Bind<EntityContainer>().ToSelf();
            else
                Bind<EntityContainer>().ToSelf().InRequestScope();
        }

        /// <summary>
        /// Register all available terminal commands.
        /// </summary>
        private void BindCommands()
        {
            Bind<ICommand>().To<ALIAS>();
            Bind<ICommand>().To<BOARD>();
            Bind<ICommand>().To<BOARDS>();
            Bind<ICommand>().To<CLS>();
            Bind<ICommand>().To<EXIT>();
            Bind<ICommand>().To<INITIALIZE>();
            Bind<ICommand>().To<LOGIN>();
            Bind<ICommand>().To<LOGOUT>();
            Bind<ICommand>().To<MESSAGE>();
            Bind<ICommand>().To<MESSAGES>();
            Bind<ICommand>().To<REGISTER>();
            Bind<ICommand>().To<SETTINGS>();
            Bind<ICommand>().To<STATS>();
            Bind<ICommand>().To<TOPIC>();
            Bind<ICommand>().To<USER>();
            Bind<ICommand>().To<ABOUT>();
            Bind<ICommand>().To<INVITE>();
        }
    }
}
