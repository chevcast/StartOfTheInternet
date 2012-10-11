using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Core.Data.Entities;
using Terminal.Core.Objects;
using Terminal.Core.ExtensionMethods;

namespace Terminal.Core.Data.Repositories
{
    /// <summary>
    /// Repository for persisting topics to the Entity Framework data context.
    /// </summary>
    public class TopicRepository : ITopicRepository
    {
        /// <summary>
        /// Every repository requires an instance of the Entity Framework data context.
        /// </summary>
        EntityContainer _entityContainer;

        public TopicRepository(EntityContainer entityContainer)
        {
            _entityContainer = entityContainer;
        }

        /// <summary>
        /// Adds a topic to the data context.
        /// </summary>
        /// <param name="topic">The topic to be added.</param>
        public void AddTopic(Topic topic)
        {
            _entityContainer.Topics.Add(topic);
        }

        /// <summary>
        /// Updates an existing topic in the data context.
        /// </summary>
        /// <param name="topic">The topic to be updated.</param>
        public void UpdateTopic(Topic topic)
        {
            
        }

        /// <summary>
        /// Deletes a topic from the data context.
        /// </summary>
        /// <param name="topic">The topic to be deleted.</param>
        public void DeleteTopic(Topic topic)
        {
            topic.Replies.ToList().ForEach(x => _entityContainer.Replies.Remove(x));
            _entityContainer.Topics.Remove(topic);
        }

        /// <summary>
        /// Gets a topic from the data context by its unique ID.
        /// </summary>
        /// <param name="topicId">The unique ID of the topic.</param>
        /// <returns>A topic entity.</returns>
        public Topic GetTopic(long topicId)
        {
            var query = _entityContainer.Topics.Where(x => x.TopicID == topicId);
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Retrieve all topics for the specified board.
        /// </summary>
        /// <param name="boardId">The unique ID of the board.</param>
        /// <param name="page">The page number.</param>
        /// <param name="itemsPerPage">The number of items to display per page.</param>
        /// <param name="isModerator">True if moderator-only topics should be included.</param>
        /// <returns>An enumerable list of topics.</returns>
        public CollectionPage<Topic> GetTopics(short boardId, int page, int itemsPerPage, bool isModerator)
        {
            var topics = _entityContainer.Topics.AsQueryable();
            var totalTopics = 0;
            if (isModerator)
            {
                if (boardId == 0)
                    topics = topics
                    .Where(x => x.Board.AllTopics)
                    .OrderByDescending(x => x.GlobalSticky)
                    .ThenByDescending(x => x.Replies.Any() ? x.Replies.Max(r => r.PostedDate) : x.PostedDate);
                else
                    topics = topics
                        .Where(x => x.BoardID == boardId)
                        .OrderByDescending(x => x.Stickied)
                        .ThenByDescending(x => x.Replies.Any() ? x.Replies.Max(r => r.PostedDate) : x.PostedDate);
                totalTopics = topics.Count();
            }
            else
            {
                if (boardId == 0)
                    topics = topics
                    .Where(x => x.Board.AllTopics)
                    .Where(x => !x.ModsOnly)
                    .Where(x => !x.Board.ModsOnly)
                    .Where(x => !x.Board.Hidden)
                    .OrderByDescending(x => x.GlobalSticky)
                    .ThenByDescending(x => x.Replies
                        .Where(r => !r.ModsOnly)
                        .Any() ? x.Replies
                            .Where(r => !r.ModsOnly)
                            .Max(r => r.PostedDate) : x.PostedDate);
                else
                    topics = topics
                        .Where(x => x.BoardID == boardId)
                        .Where(x => !x.ModsOnly)
                        .OrderByDescending(x => x.Stickied)
                        .ThenByDescending(x => x.Replies
                            .Where(r => !r.ModsOnly)
                            .Any() ? x.Replies
                                .Where(r => !r.ModsOnly)
                                .Max(r => r.PostedDate) : x.PostedDate);
                totalTopics = topics.Count();
            }

            var totalPages = totalTopics.NumberOfPages(itemsPerPage);
            if (totalPages <= 0)
                totalPages = 1;

            if (page > totalPages)
                return GetTopics(boardId, totalPages, itemsPerPage, isModerator);
            else if (page < 1)
                return GetTopics(boardId, 1, itemsPerPage, isModerator);
            else
            {
                if (totalTopics > itemsPerPage)
                    topics = topics
                        .Skip((page - 1) * itemsPerPage)
                        .Take(itemsPerPage);

                return new CollectionPage<Topic>
                {
                    Items = topics.AsEnumerable().Reverse().ToList(),
                    TotalItems = totalTopics,
                    TotalPages = totalPages
                };
            }
        }

        public long AllTopicsCount(bool isModerator)
        {
            if (isModerator)
                return _entityContainer.Topics
                    .Where(x => x.Board.AllTopics)
                    .Count();
            else
                return _entityContainer.Topics
                    .Where(x => x.Board.AllTopics)
                    .Where(x => !x.ModsOnly)
                    .Where(x => !x.Board.ModsOnly)
                    .Where(x => !x.Board.Hidden)
                    .Count();
        }

        public ForumStats GetForumStats()
        {
            var oneDayAgo = DateTime.UtcNow.AddHours(-24);
            var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
            var oneYearAgo = DateTime.UtcNow.AddYears(-1);

            var forumStats = new ForumStats
            {
                TotalTopics = _entityContainer.Topics.Count(),
                MostPopularTopic = _entityContainer.Topics.OrderByDescending(x => x.Replies.Count()).First().TopicID,
                TopicsInTheLast24Hours = _entityContainer.Topics.Count(x => x.PostedDate > oneDayAgo),
                TopicsInTheLastWeek = _entityContainer.Topics.Count(x => x.PostedDate > oneWeekAgo),
                TopicsInTheLastMonth = _entityContainer.Topics.Count(x => x.PostedDate > oneMonthAgo),
                TopicsInTheLastYear = _entityContainer.Topics.Count(x => x.PostedDate > oneYearAgo)
            };
            forumStats.TotalPosts = forumStats.TotalTopics + _entityContainer.Replies.Count();
            forumStats.PostsInTheLast24Hours = forumStats.TopicsInTheLast24Hours + _entityContainer.Replies.Count(x => x.PostedDate > oneDayAgo);
            forumStats.PostsInTheLastWeek = forumStats.TopicsInTheLastWeek + _entityContainer.Replies.Count(x => x.PostedDate > oneWeekAgo);
            forumStats.PostsInTheLastMonth = forumStats.TopicsInTheLastMonth + _entityContainer.Replies.Count(x => x.PostedDate > oneMonthAgo);
            forumStats.PostsInTheLastYear = forumStats.TopicsInTheLastYear + _entityContainer.Replies.Count(x => x.PostedDate > oneYearAgo);

            return forumStats;
        }
    }

    /// <summary>
    /// Repository for storing topics.
    /// </summary>
    public interface ITopicRepository
    {
        /// <summary>
        /// Adds a topic to the repository.
        /// </summary>
        /// <param name="topic">The topic to be added.</param>
        void AddTopic(Topic topic);

        /// <summary>
        /// Updates an existing topic in the repository.
        /// </summary>
        /// <param name="topic">The topic to be updated.</param>
        void UpdateTopic(Topic topic);

        /// <summary>
        /// Delete a topic from the repository.
        /// </summary>
        /// <param name="topic">The topic to be deleted.</param>
        void DeleteTopic(Topic topic);

        /// <summary>
        /// Get a topic by its unique ID.
        /// </summary>
        /// <param name="topicID">The unique ID of the topic.</param>
        /// <returns>A topic entity.</returns>
        Topic GetTopic(long topicID);

        /// <summary>
        /// Get all topics on the specified board.
        /// </summary>
        /// <param name="boardID">The unique ID of the board.</param>
        /// <param name="page">The specified page number.</param>
        /// <param name="itemsPerPage">The number of items to display per page.</param>
        /// <param name="isModerator">True if moderator-only topics should be included.</param>
        /// <returns>An enumerable list of topics.</returns>
        CollectionPage<Topic> GetTopics(short boardID, int page, int itemsPerPage, bool isModerator);

        long AllTopicsCount(bool isModerator);

        ForumStats GetForumStats();
    }
}