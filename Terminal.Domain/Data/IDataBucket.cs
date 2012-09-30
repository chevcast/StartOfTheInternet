using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Domain.Data.Repositories.Interfaces;

namespace Terminal.Domain.Data
{
    public interface IDataBucket
    {
        IAliasRepository AliasRepository { get; }
        IBanRepository BanRepository { get; }
        IBoardRepository BoardRepository { get; }
        IInviteCodeRepository InviteCodeRepository { get; }
        IMessageRepository MessageRepository { get; }
        IReplyRepository ReplyRepository { get; }
        ITopicRepository TopicRepository { get; }
        IUserRepository UserRepository { get; }
        IVariableRepository VariableRepository { get; }

        void SaveChanges();
    }
}
