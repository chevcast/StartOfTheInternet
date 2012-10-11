using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Terminal.MvcUI.Data
{
    public class UIContext : DbContext
    {
        public DbSet<SignalRConnection> SignalRConnections { get; set; }
    }
}