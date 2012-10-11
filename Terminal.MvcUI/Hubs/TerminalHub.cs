using SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terminal.Core.Data;
using Terminal.Core.Data.Entities;
using Terminal.Core.ExtensionMethods;
using System.Linq;
using Terminal.MvcUI.Data;

namespace Terminal.MvcUI.Hubs
{
    public class TerminalHub : Hub, IDisconnect
    {
        private UIContext _uiContext;

        public TerminalHub(UIContext uiContext)
        {
            _uiContext = uiContext;
        }

        public void Send(string text)
        {
            if (Context.User.Identity.IsAuthenticated)
                Clients.writeLine(Context.User.Identity.Name, text);
            else
                Caller.message("You are not logged in.");
        }

        public void DisconnectUser()
        {
            var signalRConnection = _uiContext.SignalRConnections.SingleOrDefault(x => x.ConnectionId == Context.ConnectionId);
            
            if (signalRConnection != null)
            {
                var username = signalRConnection.Username;
                _uiContext.SignalRConnections.Remove(signalRConnection);
                _uiContext.SaveChanges();
                if (!_uiContext.SignalRConnections.Any(x => x.Username.Is(username)))
                    Clients.leaveUser(username);
            }
        }

        public Task Disconnect()
        {
            // If the user simply refreshed the page then we want to let the connect event add another channelstatus entity
            // before we delete the current one. This way the "user left" message will not appear.
            System.Threading.Thread.Sleep(3000);
            DisconnectUser();
            return null;
        }
    }
}
