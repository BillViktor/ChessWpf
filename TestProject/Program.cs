using Chess;

namespace TestProject
{
    public class Program
    {
        /// <summary>
        /// Main method to run the chess game minimax algorithm at depth 4.
        /// Used for testing purposes.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            string sFen = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1";
            ChessGame sChessGame = new ChessGame(sFen);
            int sDepth = 4;
            int sPosTried = 0;
            _ = sChessGame.MiniMax(sDepth, true, out sPosTried, int.MinValue, int.MaxValue, false, sDepth);
            Console.WriteLine("Total positions tried: " + sPosTried);
        }
    }
}
