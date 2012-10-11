using System.Data.Entity;

namespace Terminal.MvcUI.Data
{
    public class UIContext : DbContext
    {
        public DbSet<SignalRConnection> SignalRConnections { get; set; }
    }
}