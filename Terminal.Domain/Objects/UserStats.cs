using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terminal.Core.Objects
{
    public class UserStats
    {
        public long TotalRegisteredUsers { get; set; }
        public long NewUsersInTheLast24Hours { get; set; }
        public long NewUsersInTheLastWeek { get; set; }
        public long NewUsersInTheLastMonth { get; set; }
        public long NewUsersInTheLastYear { get; set; }
        public long LoggedInWithinTheLast24Hours { get; set; }
        public long LoggedInWithinTheLastWeek { get; set; }
        public long LoggedInWithinTheLastMonth { get; set; }
        public long LoggedInWithinTheLastYear { get; set; }
        public long TotalBannedUsers { get; set; }
    }
}
