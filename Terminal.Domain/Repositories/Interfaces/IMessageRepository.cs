using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Domain.Entities;
using Terminal.Domain.Objects;

namespace Terminal.Domain.Repositories.Interfaces
{
    /// <summary>
    /// A repository to store messages.
    /// </summary>
    public interface IMessageRepository
    {
        /// <summary>
        /// Gets a message by its unique ID.
        /// </summary>
        /// <param name="messageId">The unique ID of the message.</param>
        /// <returns>A message entity.</returns>
        Message GetMessage(long messageId);

        /// <summary>
        /// Get received messages for user.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <param name="page">The page number.</param>
        /// <param name="itemsPerPage">The number of items to display per page.</param>
        /// <returns>An enumerable list of messages.</returns>
        CollectionPage<Message> GetMessages(string username, int page, int itemsPerPage, bool sent);

        int UnreadMessages(string username);

        /// <summary>
        /// Retrieves a list of all messages from the data context for user.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <param name="sent">True if retrieving sent messages.</param>
        /// <returns>An enumerable list of messages.</returns>
        IEnumerable<Message> GetAllMessages(string username, bool sent);

        /// <summary>
        /// Add a message to the repository.
        /// </summary>
        /// <param name="message">The message to be added.</param>
        void AddMessage(Message message);

        /// <summary>
        /// Updates an existing message in the repository.
        /// </summary>
        /// <param name="message">The message to be updated.</param>
        void UpdateMessage(Message message);

        /// <summary>
        /// Delete a message from the repository.
        /// </summary>
        /// <param name="message">The message to be deleted.</param>
        void DeleteMessage(Message message);
    }
}