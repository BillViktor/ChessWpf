namespace Chess.Models
{
    public class Move
    {
        public ChessPiece PieceToMove { get; set; }
        public int FromRow { get; set; }
        public int FromCol { get; set; }
        public int ToRow { get; set; }
        public int ToCol { get; set; }
        public ChessPiece? PieceCaptured { get; set; }

        public int Score { get; set; } = 0;

        public Move(ChessPiece sPieceToMove, int aFromRow, int aFromCol, int aToRow, int aToCol, ChessPiece aPieceCaptured)
        {
            PieceToMove = sPieceToMove;
            FromRow = aFromRow;
            FromCol = aFromCol;
            ToRow = aToRow;
            ToCol = aToCol;
            PieceCaptured = aPieceCaptured;
        }
    }
}
