using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terminal.Domain.Objects
{
    public class TopicUpdate
    {
        public short BoardId { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public int ReplyCount { get; set; }
        public string Body { get; set; }
        public int MaxPages { get; set; }
    }
}
