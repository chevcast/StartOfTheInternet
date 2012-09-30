using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Domain.Data.Entities;
using Terminal.Domain.Data.Repositories.Interfaces;
using Terminal.Domain.Data.Repositories.Objects;

namespace Terminal.Domain.Data
{
    public class DataBucket : IDataBucket
    {
        private EntityContainer _dataContext;

        public DataBucket(EntityContainer dataContext)
        {
            _dataContext = dataContext;
        }

        private IAliasRepository _aliasRepository;
        public IAliasRepository AliasRepository
        {
            get
            {
                if (_aliasRepository == null)
                    _aliasRepository = new AliasRepository(_dataContext);
                return _aliasRepository;
            }
        }

        private IBanRepository _banRepository;
        public IBanRepository BanRepository
        {
            get
            {
                if (_banRepository == null)
                    _banRepository = new BanRepository(_dataContext);
                return _banRepository;
            }
        }

        private IBoardRepository _boardRepository;
        public IBoardRepository BoardRepository
        {
            get
            {
                if (_boardRepository == null)
                    _boardRepository = new BoardRepository(_dataContext);
                return _boardRepository;
            }
        }

        private IInviteCodeRepository _inviteCodeRepository;
        public IInviteCodeRepository InviteCodeRepository
        {
            get
            {
                if (_inviteCodeRepository == null)
                    _inviteCodeRepository = new InviteCodeRepository(_dataContext);
                return _inviteCodeRepository;
            }
        }

        private IMessageRepository _messageRepository;
        public IMessageRepository MessageRepository
        {
            get
            {
                if (_messageRepository == null)
                    _messageRepository = new MessageRepository(_dataContext);
                return _messageRepository;
            }
        }

        private IReplyRepository _replyRepository;
        public IReplyRepository ReplyRepository
        {
            get
            {
                if (_replyRepository == null)
                    _replyRepository = new ReplyRepository(_dataContext);
                return _replyRepository;
            }
        }

        private ITopicRepository _topicRepository;
        public ITopicRepository TopicRepository
        {
            get
            {
                if (_topicRepository == null)
                    _topicRepository = new TopicRepository(_dataContext);
                return _topicRepository;
            }
        }

        private IUserRepository _userRepository;
        public IUserRepository UserRepository
        {
            get
            {
                if (_userRepository == null)
                    _userRepository = new UserRepository(_dataContext);
                return _userRepository;
            }
        }

        private IVariableRepository _variableRepository;
        public IVariableRepository VariableRepository
        {
            get
            {
                if (_variableRepository == null)
                    _variableRepository = new VariableRepository(_dataContext);
                return _variableRepository;
            }
        }

        public void SaveChanges()
        {
            _dataContext.SaveChanges();
        }
    }
}
