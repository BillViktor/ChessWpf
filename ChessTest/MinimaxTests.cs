using Chess;

namespace ChessTest
{
    /// <summary>
    /// Tests for the Minimax algorithm functionality in the ChessGame class. 
    /// Using these positions from the Chess Programming Wiki:
    /// https://www.chessprogramming.org/Perft_Results
    /// Please note that these tests are also very dependent on the correctness of the MoveGenerator classes.
    /// </summary>
    [TestClass]
    public sealed class MinimaxTests
    {
        private ChessGame mChessGame;
        private string mFenInitialPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private string mFenPositionTwo = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
        private string mFenPositionFive = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8";

        #region Initial Position
        [TestMethod]
        public void InitialPosition_AssertNodes_DepthZero()
        {
            mChessGame = new ChessGame(mFenInitialPosition);
            int sDepth = 0;
            int sPosTried = 0;
            _ = mChessGame.MiniMax(sDepth, true, out sPosTried, int.MinValue, int.MaxValue, false, sDepth);
            Assert.AreEqual(1, sPosTried);
        }

        [TestMethod]
        public void InitialPosition_AssertNodes_DepthOne()
        {
            mChessGame = new ChessGame(mFenInitialPosition);
            int sDepth = 1;
            int sPosTried = 0;
            var sBestMove = mChessGame.MiniMax(sDepth, true, out sPosTried, int.MinValue, int.MaxValue, false, sDepth);
            Assert.AreEqual(20, sPosTried);
        }

        [TestMethod]
        public void InitialPosition_AssertNodes_DepthTwo()
        {
            mChessGame = new ChessGame(mFenInitialPosition);
            int sDepth = 2;
            int sPosTried = 0;
            var sBestMove = mChessGame.MiniMax(sDepth, true, out sPosTried, int.MinValue, int.MaxValue, false, sDepth);
            Assert.AreEqual(400, sPosTried);
        }

        [TestMethod]
        public void InitialPosition_AssertNodes_DepthThree()
        {
            mChessGame = new ChessGame(mFenInitialPosition);
            int sDepth = 3;
            int sPosTried = 0;
            var sBestMove = mChessGame.MiniMax(sDepth, true, out sPosTried, int.MinValue, int.MaxValue, false, sDepth);
            Assert.AreEqual(8902, sPosTried);
        }

        [TestMethod]
        public void InitialPosition_AssertNodes_DepthFour()
        {
            mChessGame = new ChessGame(mFenInitialPosition);
            int sDepth = 4;
            int sPosTried = 0;
            var sBestMove = mChessGame.MiniMax(sDepth, true, out sPosTried, int.MinValue, int.MaxValue, false, sDepth);
            Assert.AreEqual(197281, sPosTried);
        }
        #endregion


        #region Position 2
        [TestMethod]
        public void PositionTwo_AssertNodes_DepthOne()
        {
            mChessGame = new ChessGame(mFenPositionTwo);
            int sDepth = 1;
            int sPosTried = 0;
            var sBestMove = mChessGame.MiniMax(sDepth, true, out sPosTried, int.MinValue, int.MaxValue, false, sDepth);
            Assert.AreEqual(48, sPosTried);
        }

        [TestMethod]
        public void PositionTwo_AssertNodes_DepthTwo()
        {
            mChessGame = new ChessGame(mFenPositionTwo);
            int sDepth = 2;
            int sPosTried = 0;
            var sBestMove = mChessGame.MiniMax(sDepth, true, out sPosTried, int.MinValue, int.MaxValue, false, sDepth);
            Assert.AreEqual(2039, sPosTried);
        }

        [TestMethod]
        public void PositionTwo_AssertNodes_DepthThree()
        {
            mChessGame = new ChessGame(mFenPositionTwo);
            int sDepth = 3;
            int sPosTried = 0;
            var sBestMove = mChessGame.MiniMax(sDepth, true, out sPosTried, int.MinValue, int.MaxValue, false, sDepth);
            Assert.AreEqual(97862, sPosTried);
        }
        #endregion


        #region Position 5
        [TestMethod]
        public void PositionFive_AssertNodes_DepthOne()
        {
            mChessGame = new ChessGame(mFenPositionFive);
            int sDepth = 1;
            int sPosTried = 0;
            var sBestMove = mChessGame.MiniMax(sDepth, true, out sPosTried, int.MinValue, int.MaxValue, false, sDepth);
            Assert.AreEqual(44, sPosTried);
        }

        [TestMethod]
        public void PositionFive_AssertNodes_DepthTwo()
        {
            mChessGame = new ChessGame(mFenPositionFive);
            int sDepth = 2;
            int sPosTried = 0;
            var sBestMove = mChessGame.MiniMax(sDepth, true, out sPosTried, int.MinValue, int.MaxValue, false, sDepth);
            Assert.AreEqual(1486, sPosTried);
        }

        [TestMethod]
        public void PositionFive_AssertNodes_DepthThree()
        {
            mChessGame = new ChessGame(mFenPositionFive);
            int sDepth = 3;
            int sPosTried = 0;
            var sBestMove = mChessGame.MiniMax(sDepth, true, out sPosTried, int.MinValue, int.MaxValue, false, sDepth);
            Assert.AreEqual(62379, sPosTried);
        }
        #endregion

        /// <summary>
        /// This test verifies that the Alpha-Beta pruning implementation in the MiniMax algorithm works
        /// It should reduce the number of nodes visited in the search tree compared to a naive implementation.
        /// </summary>
        [TestMethod]
        public void AlphaBetaPruning_ShouldReduceSearchSpace()
        {
            mChessGame = new ChessGame(mFenPositionFive); //Use a position where there are better moves to prune

            int sPosWithPruning;
            _ = mChessGame.MiniMax(3, false, out sPosWithPruning, int.MinValue, int.MaxValue, true);
            int sPosWithoutPruning;
            _ = mChessGame.MiniMax(3, false, out sPosWithoutPruning, int.MinValue, int.MaxValue, false);

            Assert.IsTrue(sPosWithPruning < sPosWithoutPruning);
        }
    }
}
