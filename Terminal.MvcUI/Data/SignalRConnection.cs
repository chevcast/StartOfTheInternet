using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Terminal.MvcUI.Data
{
    public class SignalRConnection
    {
        public int Id { get; set; }
        public string SessionId { get; set; }
        public string ConnectionId { get; set; }
        public string Username { get; set; }
    }
}