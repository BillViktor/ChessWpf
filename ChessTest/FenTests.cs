using Chess;
using Chess.Models;

namespace ChessTest
{
    /// <summary>
    /// Tests for the FEN (Forsyth-Edwards Notation) functionality in the ChessGame class.
    /// </summary>
    [TestClass]
    public sealed class FenTests 
    {
        private ChessGame mChessGame;
        private string mStartingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        /// <summary>
        /// Tests the parsing of a FEN string to ensure that the chessboard is populated correctly.
        /// </summary>
        [TestMethod]
        public void ParseFen_PopulateStartingBoardCorrectly()
        {
            mChessGame = new ChessGame(mStartingFen);

            //Check all pieces for Black (row 0 and row 1)
            Assert.IsInstanceOfType(mChessGame.ChessBoard[0, 0], typeof(Rook));  // a8
            Assert.AreEqual(ColorEnum.Black, (mChessGame.ChessBoard[0, 0] as Rook).Color);

            Assert.IsInstanceOfType(mChessGame.ChessBoard[0, 1], typeof(Knight));  // b8
            Assert.AreEqual(ColorEnum.Black, (mChessGame.ChessBoard[0, 1] as Knight).Color);

            Assert.IsInstanceOfType(mChessGame.ChessBoard[0, 2], typeof(Bishop));  // c8
            Assert.AreEqual(ColorEnum.Black, (mChessGame.ChessBoard[0, 2] as Bishop).Color);

            Assert.IsInstanceOfType(mChessGame.ChessBoard[0, 3], typeof(Queen));  // d8
            Assert.AreEqual(ColorEnum.Black, (mChessGame.ChessBoard[0, 3] as Queen).Color);

            Assert.IsInstanceOfType(mChessGame.ChessBoard[0, 4], typeof(King));  // e8
            Assert.AreEqual(ColorEnum.Black, (mChessGame.ChessBoard[0, 4] as King).Color);

            Assert.IsInstanceOfType(mChessGame.ChessBoard[0, 5], typeof(Bishop));  // f8
            Assert.AreEqual(ColorEnum.Black, (mChessGame.ChessBoard[0, 5] as Bishop).Color);

            Assert.IsInstanceOfType(mChessGame.ChessBoard[0, 6], typeof(Knight));  // g8
            Assert.AreEqual(ColorEnum.Black, (mChessGame.ChessBoard[0, 6] as Knight).Color);

            Assert.IsInstanceOfType(mChessGame.ChessBoard[0, 7], typeof(Rook));  // h8
            Assert.AreEqual(ColorEnum.Black, (mChessGame.ChessBoard[0, 7] as Rook).Color);

            //Check all pawns for Black (row 1)
            for (int col = 0; col < 8; col++)
            {
                Assert.IsInstanceOfType(mChessGame.ChessBoard[1, col], typeof(Pawn));  // 2nd row
                Assert.AreEqual(ColorEnum.Black, (mChessGame.ChessBoard[1, col] as Pawn).Color);
            }

            //Check empty squares (rows 2 to 5)
            for (int row = 2; row < 6; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Assert.IsNull(mChessGame.ChessBoard[row, col]);  // Empty squares
                }
            }

            //Check all pawns for White (row 6)
            for (int col = 0; col < 8; col++)
            {
                Assert.IsInstanceOfType(mChessGame.ChessBoard[6, col], typeof(Pawn));  // 7th row
                Assert.AreEqual(ColorEnum.White, (mChessGame.ChessBoard[6, col] as Pawn).Color);
            }

            //Check all pieces for White (row 7)
            Assert.IsInstanceOfType(mChessGame.ChessBoard[7, 0], typeof(Rook));  // a1
            Assert.AreEqual(ColorEnum.White, (mChessGame.ChessBoard[7, 0] as Rook).Color);

            Assert.IsInstanceOfType(mChessGame.ChessBoard[7, 1], typeof(Knight));  // b1
            Assert.AreEqual(ColorEnum.White, (mChessGame.ChessBoard[7, 1] as Knight).Color);

            Assert.IsInstanceOfType(mChessGame.ChessBoard[7, 2], typeof(Bishop));  // c1
            Assert.AreEqual(ColorEnum.White, (mChessGame.ChessBoard[7, 2] as Bishop).Color);

            Assert.IsInstanceOfType(mChessGame.ChessBoard[7, 3], typeof(Queen));  // d1
            Assert.AreEqual(ColorEnum.White, (mChessGame.ChessBoard[7, 3] as Queen).Color);

            Assert.IsInstanceOfType(mChessGame.ChessBoard[7, 4], typeof(King));  // e1
            Assert.AreEqual(ColorEnum.White, (mChessGame.ChessBoard[7, 4] as King).Color);

            Assert.IsInstanceOfType(mChessGame.ChessBoard[7, 5], typeof(Bishop));  // f1
            Assert.AreEqual(ColorEnum.White, (mChessGame.ChessBoard[7, 5] as Bishop).Color);

            Assert.IsInstanceOfType(mChessGame.ChessBoard[7, 6], typeof(Knight));  // g1
            Assert.AreEqual(ColorEnum.White, (mChessGame.ChessBoard[7, 6] as Knight).Color);

            Assert.IsInstanceOfType(mChessGame.ChessBoard[7, 7], typeof(Rook));  // h1
            Assert.AreEqual(ColorEnum.White, (mChessGame.ChessBoard[7, 7] as Rook).Color);

            //Make sure white is to move
            Assert.IsTrue(mChessGame.ColorToMove == ColorEnum.White);
        }

        /// <summary>
        /// Tests the parsing of a FEN string to ensure that the color to move is correctly identified.
        /// </summary>
        [TestMethod]
        public void ParseFen_AssertColorToMove()
        {
            //Black
            string sFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1";

            mChessGame = new ChessGame(sFen);

            Assert.AreEqual(ColorEnum.Black, mChessGame.ColorToMove);
        }

        /// <summary>
        /// Tests the export of the chessboard to a FEN string to ensure it matches the initial position.
        /// </summary>
        [TestMethod]
        public void ExportStartingFen_SholdMatchInitialPosition()
        {
            mChessGame = new ChessGame(mStartingFen);

            //Export FEN
            string sFen = mChessGame.GetFenString();

            Assert.AreEqual(mStartingFen, sFen, false);
        }

    }
}
