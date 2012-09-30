using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Core;
using Terminal.Core.Data.Repositories.Interfaces;
using Terminal.Core.Data.Entities;

namespace Terminal.Core.Data.Repositories.Objects
{
    /// <summary>
    /// A repository for persisting boards to the Entity Framework data context.
    /// </summary>
    public class BoardRepository : IBoardRepository
    {
        /// <summary>
        /// Every repository requires an instance of the Entity Framework data context.
        /// </summary>
        EntityContainer _entityContainer;

        public BoardRepository(EntityContainer entityContainer)
        {
            _entityContainer = entityContainer;
        }

        /// <summary>
        /// Adds a board to the data context.
        /// </summary>
        /// <param name="board">The board to be added.</param>
        public void AddBoard(Board board)
        {
            _entityContainer.Boards.Add(board);
        }

        /// <summary>
        /// UPdates an existing board in the data context.
        /// </summary>
        /// <param name="board">The board to be updated.</param>
        public void UpdateBoard(Board board)
        {
            
        }

        /// <summary>
        /// Deletes a board from the data context.
        /// </summary>
        /// <param name="board">The board to be deleted.</param>
        public void DeleteBoard(Board board)
        {
            foreach (var topic in board.Topics.ToList())
            {
                topic.Replies.ToList().ForEach(x => _entityContainer.Replies.Remove(x));
                _entityContainer.Topics.Remove(topic);
            }
            _entityContainer.Boards.Remove(board);
        }

        /// <summary>
        /// Retrive a board from the data context by its unique ID.
        /// </summary>
        /// <param name="boardID">The unique ID of the board.</param>
        /// <returns>A board entity.</returns>
        public Board GetBoard(short boardID)
        {
            var query = _entityContainer.Boards.Where(x => x.BoardID == boardID);
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Get all available discussion boards from the data context.
        /// </summary>
        /// <param name="isModerator">True if moderator-only boards should be included.</param>
        /// <returns>An enumerable list of boards.</returns>
        public IEnumerable<Board> GetBoards(bool isModerator)
        {
            var query = _entityContainer.Boards.OrderBy(x => x.BoardID).AsEnumerable();
            if (!isModerator)
                query = query
                    .Where(x => !x.ModsOnly)
                    .Where(x => !x.Hidden);
            return query;
        }
    }
}