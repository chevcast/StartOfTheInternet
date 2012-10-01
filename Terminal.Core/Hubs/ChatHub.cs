using SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terminal.Core.Data;
using Terminal.Core.Data.Entities;
using Terminal.Core.ExtensionMethods;
using System.Linq;

namespace Terminal.Core.Hubs
{
    public class ChatHub : Hub, IDisconnect
    {
        private IDataBucket _dataBucket;

        public ChatHub(IDataBucket dataBucket)
        {
            _dataBucket = dataBucket;
        }

        public void Connect(string username)
        {
            var user = _dataBucket.UserRepository.GetUser(username);
            var channelStatus = new ChannelStatus
            {
                ConnectionId = Guid.Parse(Context.ConnectionId),
                ChannelName = "Default"
            };
            user.ChannelStatuses.Add(channelStatus);
            _dataBucket.SaveChanges();
            Clients.joinUser(user.Username);
            var connectedUsers = _dataBucket.ChannelStatusRepository.GetChannelStatuses("Default").Select(x => x.User.Username).Distinct();
            Caller.loadUsers(connectedUsers);
        }

        public Task Disconnect()
        {
            var channel = _dataBucket.ChannelStatusRepository.GetChannelStatus(Guid.Parse(Context.ConnectionId));
            var user = channel.User;
            _dataBucket.ChannelStatusRepository.DeleteChannelStatus(channel);
            _dataBucket.SaveChanges();
            return Clients.removeUser(user.Username);
        }
    }
}
