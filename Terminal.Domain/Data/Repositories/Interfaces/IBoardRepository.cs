using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Core.Data.Entities;

namespace Terminal.Core.Data.Repositories.Interfaces
{
    /// <summary>
    /// A repository for storing boards.
    /// </summary>
    public interface IBoardRepository
    {
        /// <summary>
        /// Adds a board to the repository.
        /// </summary>
        /// <param name="board">The board to be added.</param>
        void AddBoard(Board board);

        /// <summary>
        /// Updates an existing board in the repository.
        /// </summary>
        /// <param name="board">The board to be updated.</param>
        void UpdateBoard (Board board);

        /// <summary>
        /// Deletes a board from the repository.
        /// </summary>
        /// <param name="board">The board to be deleted.</param>
        void DeleteBoard(Board board);

        /// <summary>
        /// Retrieve a board by its unique ID.
        /// </summary>
        /// <param name="boardID">The unique ID of the desired board.</param>
        /// <returns>A board entity.</returns>
        Board GetBoard(short boardID);

        /// <summary>
        /// Retrieve an enumerable list of all boards.
        /// </summary>
        /// <param name="loadModeratorBoards">True if moderator-only boards should be included.</param>
        /// <returns>An enumerable list of boards.</returns>
        IEnumerable<Board> GetBoards(bool loadModeratorBoards);
    }
}