using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Core.Data.Repositories.Interfaces;

namespace Terminal.Core.Data
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
        IChannelStatusRepository ChannelStatusRepository { get; }

        void SaveChanges();
    }
}
