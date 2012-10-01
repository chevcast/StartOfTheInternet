using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Core.Data.Repositories.Interfaces;
using Terminal.Core.Data.Entities;
using Terminal.Core.ExtensionMethods;

namespace Terminal.Core.Data.Repositories.Objects
{
    public class ChannelStatusRepository : IChannelStatusRepository
    {
        private EntityContainer _dataContext;

        public ChannelStatusRepository(EntityContainer dataContext)
        {
            _dataContext = dataContext;
        }

        public ChannelStatus GetChannelStatus(Guid connectionid)
        {
            return _dataContext.ChannelStatuses.SingleOrDefault(x => x.ConnectionId == connectionid);
        }

        public List<ChannelStatus> GetChannelStatuses(string channelName)
        {
            return _dataContext.ChannelStatuses.Where(x => x.ChannelName.Is(channelName)).ToList();
        }

        public void DeleteChannelStatus(ChannelStatus channel)
        {
            _dataContext.ChannelStatuses.Remove(channel);
        }
    }
}
