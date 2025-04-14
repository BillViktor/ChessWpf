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

        public ChessPiece? PromotionPiece; //For promotion
        public ChessPiece? OriginalPawn;   //To restore during undo

        //Used in move ordering
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

        /// <summary>
        /// Sets the move type based on the properties of the move.
        /// Used when ordering moves
        /// </summary>
        private void SetMoveType()
        {
            //Promotion
            if (PromotionPiece != null)
            {
                MoveType = MoveTypeEnum.Promotion;
                return;
            }

            //En Passant
            else if (PieceToMove is Pawn && PieceCaptured == null && Math.Abs(ToCol - FromCol) == 1 && Math.Abs(ToRow - FromRow) == 1)
            {
                MoveType = MoveTypeEnum.EnPassant;
            }

            //Capture
            else if(PieceCaptured != null)
            {
                MoveType = MoveTypeEnum.Capture;
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

        /// <summary>
        /// Returns a string representation of the move.
        /// Format: a2a4
        /// </summary>
        public override string ToString()
        {
            char sFromCol = (char)('a' + FromCol);
            int sFromRow = 8 - FromRow;
            char sToCol = (char)('a' + ToCol);
            int sToRow = 8 - ToRow; 

            return $"{sFromCol}{sFromRow}{sToCol}{sToRow}";
        }
    }
}
