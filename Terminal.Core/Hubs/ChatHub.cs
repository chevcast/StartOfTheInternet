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

        public void ConnectUser(string username)
        {
            if (username.IsNullOrEmpty())
                Caller.writeLine("You must be logged in to chat.");
            else
            {
                var connectionId = Guid.Parse(Context.ConnectionId);
                var user = _dataBucket.UserRepository.GetUser(username);
                if (user.ChannelStatuses.All(x => x.ConnectionId != connectionId))
                {
                    var channelStatus = new ChannelStatus
                    {
                        ConnectionId = connectionId,
                        ChannelName = "Default"
                    };
                    user.ChannelStatuses.Add(channelStatus);
                    _dataBucket.SaveChanges();
                    Clients.writeLine(string.Format("{0} has joined.", user.Username));
                }
                //var connectedUsers = _dataBucket.ChannelStatusRepository.GetChannelStatuses("Default").Select(x => x.User.Username).Distinct();
                //Caller.loadUsers(connectedUsers);
            }
        }

        public void Send(string text)
        {
            var channelStatus = _dataBucket.ChannelStatusRepository.GetChannelStatus(Guid.Parse(Context.ConnectionId));
            if (channelStatus != null)
                Clients.writeLine(string.Format("{0}: {1}", channelStatus.User.Username, text));
            else
                Caller.writeLine("You are not logged in.");
        }

        public void DisconnectUser()
        {
            var channel = _dataBucket.ChannelStatusRepository.GetChannelStatus(Guid.Parse(Context.ConnectionId));
            if (channel != null)
            {
                var user = channel.User;
                _dataBucket.ChannelStatusRepository.DeleteChannelStatus(channel);
                _dataBucket.SaveChanges();
                Clients.writeLine(string.Format("{0} has left.", user.Username));
            }
        }

        public Task Disconnect()
        {
            DisconnectUser();
            return null;
        }
    }
}
