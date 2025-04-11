namespace Chess.Models
{
    /// <summary>
    /// Class that represents a move in chess.
    /// </summary>
    public class Move
    {
        public ChessPiece PieceToMove { get; set; }
        public int FromRow { get; set; }
        public int FromCol { get; set; }
        public int ToRow { get; set; }
        public int ToCol { get; set; }
        public ChessPiece? PieceCaptured { get; set; }
        public MoveTypeEnum MoveType { get; set; } = MoveTypeEnum.Normal;

        public int Score { get; set; } = 0;

        public Move(ChessPiece sPieceToMove, int aFromRow, int aFromCol, int aToRow, int aToCol, ChessPiece aPieceCaptured)
        {
            PieceToMove = sPieceToMove;
            FromRow = aFromRow;
            FromCol = aFromCol;
            ToRow = aToRow;
            ToCol = aToCol;
            PieceCaptured = aPieceCaptured;

            SetMoveType();
        }

        private void SetMoveType()
        {
            //Promotion
            if(PieceToMove is Pawn && PieceToMove.Color == ColorEnum.White && ToRow == 0)
            {
                MoveType = MoveTypeEnum.Promotion;
            }
            else if(PieceToMove is Pawn && PieceToMove.Color == ColorEnum.Black && ToRow == 7)
            {
                MoveType = MoveTypeEnum.Promotion;
            }

            //Capture
            else if(PieceCaptured != null)
            {
                MoveType = MoveTypeEnum.Capture;
            }

            //En Passant
            else if (PieceToMove is Pawn && PieceCaptured == null && Math.Abs(ToCol - FromCol) == 1 && Math.Abs(ToRow - FromRow) == 1)
            {
                MoveType = MoveTypeEnum.EnPassant;
            }

            //Castling
            else if(PieceToMove is King && Math.Abs(ToCol - FromCol) == 2)
            {
                if (ToCol > FromCol)
                {
                    MoveType = MoveTypeEnum.CastlingKingSide;
                }
                else
                {
                    MoveType = MoveTypeEnum.CastlingQueenSide;
                }
            }
        }
    }
}
