using Chess;

namespace ChessTest
{
    /// <summary>
    /// Tests for the Minimax algorithm functionality in the ChessGame class.
    /// </summary>
    [TestClass]
    public sealed class MinimaxTests
    {
        private ChessGame mChessGame;
        private string mStartingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private string mPositionFiveFen = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8"; //https://www.chessprogramming.org/Perft_Results

        /// <summary>
        /// Tests the MiniMax algorithm with a depth of 0.
        /// Should equal 1.
        /// </summary>
        [TestMethod]
        public void AssertNodes_DepthZero()
        {
            mChessGame = new ChessGame(mStartingFen);
            int sPosTried = 0;
            var sBestMove = mChessGame.MiniMax(0, true, out sPosTried, int.MinValue, int.MaxValue, false);
            Assert.AreEqual(1, sPosTried);
        }

        /// <summary>
        /// Tests the MiniMax algorithm with a depth of 1.
        /// Should equal 20.
        /// </summary>
        [TestMethod]
        public void AssertNodes_DepthOne()
        {
            mChessGame = new ChessGame(mStartingFen);
            int sPosTried = 0;
            var sBestMove = mChessGame.MiniMax(1, true, out sPosTried, int.MinValue, int.MaxValue, false);
            Assert.AreEqual(20, sPosTried);
        }

        /// <summary>
        /// Tests the MiniMax algorithm with a depth of 2.
        /// Should equal 400.
        /// </summary>
        [TestMethod]
        public void AssertNodes_DepthTwo()
        {
            mChessGame = new ChessGame(mStartingFen);
            int sPosTried = 0;
            var sBestMove = mChessGame.MiniMax(2, true, out sPosTried, int.MinValue, int.MaxValue, false);
            Assert.AreEqual(400, sPosTried);
        }

        /// <summary>
        /// Tests the MiniMax algorithm with a depth of 3.
        /// Should equal 8,902.
        /// </summary>
        [TestMethod]
        public void AssertNodes_DepthThree()
        {
            mChessGame = new ChessGame(mStartingFen);
            int sPosTried = 0;
            var sBestMove = mChessGame.MiniMax(3, true, out sPosTried, int.MinValue, int.MaxValue, false);
            Assert.AreEqual(8092, sPosTried);
        }

        /// <summary>
        /// Tests the MiniMax algorithm with a depth of 4.
        /// Should equal 197,281	.
        /// </summary>
        [TestMethod]
        public void AssertNodes_DepthFour()
        {
            mChessGame = new ChessGame(mStartingFen);
            int sPosTried = 0;
            var sBestMove = mChessGame.MiniMax(4, true, out sPosTried, int.MinValue, int.MaxValue, false);
            Assert.AreEqual(197281, sPosTried);
        }

        /// <summary>
        /// This is position 5 from the chess programming wiki.
        /// Which caught bugs in engines several year sold.
        /// This is a test to see if our engine can find the correct number of positions.
        /// At a depth of one.
        /// Should equal 44.
        /// </summary>
        [TestMethod]
        public void AssertPositionFiveDepthOne()
        {
            mChessGame = new ChessGame(mPositionFiveFen);
            int sPosTried = 0;
            var sBestMove = mChessGame.MiniMax(1, true, out sPosTried, int.MinValue, int.MaxValue, false);
            Assert.AreEqual(44, sPosTried);
        }

        /// <summary>
        /// This is position 5 from the chess programming wiki.
        /// Which caught bugs in engines several year sold.
        /// This is a test to see if our engine can find the correct number of positions.
        /// At a depth of two
        /// Should equal 1486.
        /// </summary>
        [TestMethod]
        public void AssertPositionFiveDepthTwo()
        {
            mChessGame = new ChessGame(mPositionFiveFen);
            int sPosTried = 0;
            var sBestMove = mChessGame.MiniMax(2, true, out sPosTried, int.MinValue, int.MaxValue, false);
            Assert.AreEqual(1486, sPosTried);
        }

        /// <summary>
        /// This is position 5 from the chess programming wiki.
        /// Which caught bugs in engines several year sold.
        /// This is a test to see if our engine can find the correct number of positions.
        /// At a depth of one
        /// Should equal 62379.
        /// </summary>
        [TestMethod]
        public void AssertPositionFiveDepthThree()
        {
            mChessGame = new ChessGame(mPositionFiveFen);
            int sPosTried = 0;
            var sBestMove = mChessGame.MiniMax(3, true, out sPosTried, int.MinValue, int.MaxValue, false);
            Assert.AreEqual(62379, sPosTried);
        }

        /// <summary>
        /// This test verifies that the Alpha-Beta pruning implementation in the MiniMax algorithm works
        /// It should reduce the number of nodes visited in the search tree compared to a naive implementation.
        /// </summary>
        [TestMethod]
        public void AlphaBetaPruning_ShouldReduceSearchSpace()
        {
            mChessGame = new ChessGame(mPositionFiveFen); //Use a position where there are better moves to prune

            int sPosWithPruning;
            _ = mChessGame.MiniMax(3, false, out sPosWithPruning, int.MinValue, int.MaxValue, true);
            int sPosWithoutPruning;
            _ = mChessGame.MiniMax(3, false, out sPosWithoutPruning, int.MinValue, int.MaxValue, false);

            Assert.IsTrue(sPosWithPruning < sPosWithoutPruning);
        }
    }
}
