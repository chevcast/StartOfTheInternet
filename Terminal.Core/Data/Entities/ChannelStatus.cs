using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terminal.Core.Data.Entities
{
    public class ChannelStatus
    {
        public int Id { get; set; }
        public Guid ConnectionId { get; set; }
        public string ChannelName { get; set; }

        public virtual User User { get; set; }
    }
}
