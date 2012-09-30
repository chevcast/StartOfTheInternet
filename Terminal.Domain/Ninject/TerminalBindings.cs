using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using Terminal.Domain.Commands.Objects;
using Terminal.Domain.Repositories.Interfaces;
using Terminal.Domain.Repositories.Objects;
using Terminal.Domain.Commands.Interfaces;
using Terminal.Domain.Entities;
using Terminal.Domain.Settings;
using System.Data.EntityClient;
using System.Web;
using Ninject.Web.Common;

namespace Terminal.Domain.Ninject
{
    /// <summary>
    /// This module will automatically register all Terminal.Domain related bindings.
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
            BindRepositories();
            BindCommands();

            if (_isWebApplication)
                this.Bind<TerminalCore>().ToSelf().InRequestScope();
            else
                this.Bind<TerminalCore>().ToSelf().InSingletonScope();
        }

        /// <summary>
        /// Register the entity container and pass in a hard-coded connection string.
        /// </summary>
        private void BindEntityContainer()
        {
            if (!_isWebApplication)
                this.Bind<EntityContainer>().ToSelf();
            else
                this.Bind<EntityContainer>().ToSelf().InRequestScope();
        }

        /// <summary>
        /// Register all available terminal commands.
        /// </summary>
        private void BindCommands()
        {
            this.Bind<ICommand>().To<ALIAS>();
            this.Bind<ICommand>().To<BOARD>();
            this.Bind<ICommand>().To<BOARDS>();
            this.Bind<ICommand>().To<CLS>();
            this.Bind<ICommand>().To<EXIT>();
            this.Bind<ICommand>().To<INITIALIZE>();
            this.Bind<ICommand>().To<LOGIN>();
            this.Bind<ICommand>().To<LOGOUT>();
            this.Bind<ICommand>().To<MARKET>();
            this.Bind<ICommand>().To<MESSAGE>();
            this.Bind<ICommand>().To<MESSAGES>();
            this.Bind<ICommand>().To<PROFILE>();
            this.Bind<ICommand>().To<REGISTER>();
            this.Bind<ICommand>().To<SETTINGS>();
            this.Bind<ICommand>().To<STATS>();
            this.Bind<ICommand>().To<TOPIC>();
            this.Bind<ICommand>().To<USER>();
        }

        /// <summary>
        /// Register all available repositories.
        /// </summary>
        private void BindRepositories()
        {
            this.Bind<IAliasRepository>().To<AliasRepository>();
            this.Bind<IBanRepository>().To<BanRepository>();
            this.Bind<IBoardRepository>().To<BoardRepository>();
            this.Bind<IInviteCodeRepository>().To<InviteCodeRepository>();
            this.Bind<ILinkRepository>().To<LinkRepository>();
            this.Bind<IMessageRepository>().To<MessageRepository>();
            this.Bind<IReplyRepository>().To<ReplyRepository>();
            this.Bind<ITopicRepository>().To<TopicRepository>();
            this.Bind<IUserRepository>().To<UserRepository>();
            this.Bind<IVariableRepository>().To<VariableRepository>();
        }
    }
}
