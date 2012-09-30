using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Core.Data.Entities;
using Terminal.Core.Objects;

namespace Terminal.Core.Data.Repositories.Interfaces
{
    /// <summary>
    /// Repository for storing topic replies.
    /// </summary>
    public interface IReplyRepository
    {
        /// <summary>
        /// Adds a reply to the repository.
        /// </summary>
        /// <param name="reply">The reply to be added.</param>
        void AddReply(Reply reply);

        /// <summary>
        /// Updates an existing reply in the repository.
        /// </summary>
        /// <param name="reply">The reply to be added.</param>
        void UpdateReply (Reply reply);

        /// <summary>
        /// Deletes a reply from the repository.
        /// </summary>
        /// <param name="reply">The reply to be deleted.</param>
        void DeleteReply(Reply reply);

        /// <summary>
        /// Get a reply by its unique ID.
        /// </summary>
        /// <param name="replyID">The unique ID of the reply.</param>
        /// <returns>A reply entity.</returns>
        Reply GetReply(long replyID);

        /// <summary>
        /// Get all replies for a single topic.
        /// </summary>
        /// <param name="topicID">The unique ID of the topic.</param>
        /// <param name="isModerator">True if moderator-only replies should be returned.</param>
        /// <returns>An enumerable list of replies.</returns>
        CollectionPage<Reply> GetReplies(long topicID, int page, int itemsPerPage, bool isModerator);
    }
}