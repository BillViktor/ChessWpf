using Chess;
using Chess.Models;

namespace ChessTest
{
    /// <summary>
    /// Tests for moving chess pieces.
    /// </summary>
    [TestClass]
    public sealed class MoveTests 
    {
        private ChessGame mChessGame;
        private string mStartingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        /// <summary>
        /// Checks some basic moves for the pieces on the chessboard.
        /// </summary>
        [TestMethod]
        public void GenerateMoves_ShouldReturnValidMovesForPieces()
        {
            mChessGame = new ChessGame(mStartingFen);

            //Check all moves
            List<Move> sMoves = mChessGame.GetAllMoves(ColorEnum.White);
            Assert.AreEqual(20, sMoves.Count);

            //Check for white pawn at row 6, column 0
            List<Move> sWhitePawnMoves = sMoves.Where(m => m.FromRow == 6 && m.FromCol == 0).ToList();
            Assert.AreEqual(2, sWhitePawnMoves.Count); // Pawn can move one or two squares

            //Check for white knight at row 7, column 1
            List<Move> sBlackKnightMoves = sMoves.Where(m => m.FromRow == 7 && m.FromCol == 1).ToList();
            Assert.AreEqual(2, sBlackKnightMoves.Count); // Knight has 2 valid moves
        }

        /// <summary>
        /// Assert that king side castling is not allowed
        /// in this FEN position because the kings path is attacked.
        /// </summary>
        [TestMethod]
        public void Castling_PassingCheck()
        {
            string sFen = "rn1qk1nr/pp1N4/B2p4/5P1p/2b5/b7/PPPP2PP/RNBQK2R w KQkq - 0 1";
            mChessGame = new ChessGame(sFen);

            List<Move> sMoves = mChessGame.GetAllMoves(ColorEnum.White);

            Move sMove = sMoves.FirstOrDefault(x => x.PieceToMove is King sKing && sKing.Color == ColorEnum.White && x.MoveType == MoveTypeEnum.CastlingKingSide);

            Assert.IsNull(sMove);
        }



        #region Pinned Pieces
        /// <summary>
        /// Assert that a pinned piece cannot move.
        /// </summary>
        [TestMethod]
        public void PinnedRook_ShouldHaveNoLegalMoves()
        {
            string sFen = "4k3/8/8/8/8/8/3q4/4K2R w K - 0 1";
            mChessGame = new ChessGame(sFen);
            List<Move> sMoves = mChessGame.GetAllMoves(ColorEnum.White);

            // Check that no rook move from h1 is allowed
            bool sRookMove = sMoves.Any(x => x.FromRow == 0 && x.FromCol == 7);

            Assert.IsFalse(sRookMove, "Pinned rook on h1 should not have any legal moves.");
        }

        /// <summary>
        /// Assert that a pinned piece cannot move.
        /// </summary>
        [TestMethod]
        public void PinnedKnight_ShouldHaveNoLegalMoves()
        {
            string sFen = "4k3/8/8/8/8/3b4/4N3/4K3 w - - 0 1";
            mChessGame = new ChessGame(sFen);
            List<Move> sMoves = mChessGame.GetAllMoves(ColorEnum.White);

            // Check that no rook move from h1 is allowed
            bool sRookMove = sMoves.Any(x => x.FromRow == 1 && x.FromCol == 4);

            Assert.IsFalse(sRookMove, "Pinned rook on h1 should not have any legal moves.");
        }

        /// <summary>
        /// Assert that a pinned piece cannot move.
        /// </summary>
        [TestMethod]
        public void PinnedPawn_ShouldNotBeAbleToPush()
        {
            // FEN: 4k3/8/8/8/8/4P3/4r3/4K3 w - - 0 1
            string sFen = "4k3/8/8/8/8/4P3/4r3/4K3 w - - 0 1";
            mChessGame = new ChessGame(sFen);
            List<Move> sMoves = mChessGame.GetAllMoves(ColorEnum.White);

            // Pawn on e3 = (2, 4)
            bool sPawnMoves = sMoves.Any(m => m.FromRow == 2 && m.FromCol == 4);

            Assert.IsFalse(sPawnMoves, "Pinned pawn on e3 should not be able to move.");
        }
        #endregion
    }
}
