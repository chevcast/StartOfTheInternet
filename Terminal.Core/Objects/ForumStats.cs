using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terminal.Core.Objects
{
    public class ForumStats
    {
        public long TotalTopics { get; set; }
        public long TopicsInTheLast24Hours { get; set; }
        public long TopicsInTheLastWeek { get; set; }
        public long TopicsInTheLastMonth { get; set; }
        public long TopicsInTheLastYear { get; set; }
        public long TotalPosts { get; set; }
        public long PostsInTheLast24Hours { get; set; }
        public long PostsInTheLastWeek { get; set; }
        public long PostsInTheLastMonth { get; set; }
        public long PostsInTheLastYear { get; set; }
        public long MostPopularTopic { get; set; }
    }
}
