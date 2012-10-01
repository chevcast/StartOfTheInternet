using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Core.Data.Entities;

namespace Terminal.Core.Data.Repositories.Interfaces
{
    public interface IChannelStatusRepository
    {
        ChannelStatus GetChannelStatus(Guid connectionid);
        List<ChannelStatus> GetChannelStatuses(string channelName);
        void DeleteChannelStatus(ChannelStatus channel);
    }
}