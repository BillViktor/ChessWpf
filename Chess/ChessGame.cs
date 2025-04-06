using Chess.Models;
using System.Windows.Shapes;

namespace Chess
{
    public class ChessGame
    {
        //Fields
        private ChessPiece?[,] mChessBoard = new ChessPiece?[8, 8]
        {
            { new Rook(ColorEnum.Black), new Knight(ColorEnum.Black), new Bishop(ColorEnum.Black), new Queen(ColorEnum.Black), new King(ColorEnum.Black), new Bishop(ColorEnum.Black), new Knight(ColorEnum.Black), new Rook(ColorEnum.Black)  },
            { new Pawn(ColorEnum.Black), new Pawn(ColorEnum.Black), new Pawn(ColorEnum.Black), new Pawn(ColorEnum.Black), new Pawn(ColorEnum.Black), new Pawn(ColorEnum.Black), new Pawn(ColorEnum.Black), new Pawn(ColorEnum.Black)  },
            { null, null, null, null, null, null, null, null  },
            { null, null, null, null, null, null, null, null  },
            { null, null, null, null, null, null, null, null  },
            { null, null, null, null, null, null, null, null  },
            { new Pawn(ColorEnum.White), new Pawn(ColorEnum.White), new Pawn(ColorEnum.White), new Pawn(ColorEnum.White), new Pawn(ColorEnum.White), new Pawn(ColorEnum.White), new Pawn(ColorEnum.White), new Pawn(ColorEnum.White)  },
            { new Rook(ColorEnum.White), new Knight(ColorEnum.White), new Bishop(ColorEnum.White), new Queen(ColorEnum.White), new King(ColorEnum.White), new Bishop(ColorEnum.White), new Knight(ColorEnum.White), new Rook(ColorEnum.White)  }
        };

        private ColorEnum mColorToMove = ColorEnum.White;
        private List<Rectangle> mHighLightedCells = new List<Rectangle>();
        private ChessPiece? mSelectedPiece = null;
        private int mCurrentMove = 1;

        //Properties
        public ChessPiece?[,] ChessBoard { get => mChessBoard; set => mChessBoard = value; }
        public ColorEnum ColorToMove { get => mColorToMove; set => mColorToMove = value; }
        public List<Rectangle> HighLightedCells { get => mHighLightedCells; set => mHighLightedCells = value; }
        public ChessPiece? SelectedPiece { get => mSelectedPiece; set => mSelectedPiece = value; }
        public int CurrentMove { get => mCurrentMove; set => mCurrentMove = value; }


        #region Movement Validation
        public bool IsValidMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            ChessPiece? sChessPiece = mChessBoard[aRowFrom, aColFrom];

            if (sChessPiece == null) return false;

            if (mChessBoard[aRowFrom, aColFrom] != null && mChessBoard[aRowTo, aColTo]?.Color == sChessPiece.Color)
            {
                return false;
            }

            bool sValidMove = false;

            switch (sChessPiece)
            {
                case Pawn sPawn:
                    sValidMove = IsValidPawnMove(aRowFrom, aColFrom, aRowTo, aColTo, sPawn);
                    break;
                case King sKing:
                    sValidMove = IsValidKingMove(aRowFrom, aColFrom, aRowTo, aColTo);
                    break;
                case Rook sRook:
                    sValidMove = IsValidRookMove(aRowFrom, aColFrom, aRowTo, aColTo);
                    break;
                case Knight sKnight:
                    sValidMove = IsValidKnightMove(aRowFrom, aColFrom, aRowTo, aColTo);
                    break;
                case Queen sQueen:
                    sValidMove = IsValidQueenMove(aRowFrom, aColFrom, aRowTo, aColTo);
                    break;
                case Bishop sBishop:
                    sValidMove = IsValidBishopMove(aRowFrom, aColFrom, aRowTo, aColTo);
                    break;
                default:
                    return false;
            }

            if (!sValidMove)
            {
                return false;
            }
            else
            {
                //Simulate the move
                ChessPiece? sCapturedPiece = MakeMove(aRowFrom, aColFrom, aRowTo, aColTo);

                //Check if we're still in check
                bool sInCheck = IsKingInCheck(sChessPiece.Color);

                //Undo the move
                UndoMove(aRowFrom, aColFrom, aRowTo, aColTo, sCapturedPiece);

                return !sInCheck;
            }
        }

        private bool IsPathClear(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            int sRowStep = (aRowTo - aRowFrom) == 0 ? 0 : (aRowTo - aRowFrom) / Math.Abs(aRowTo - aRowFrom);
            int sColStep = (aColTo - aColFrom) == 0 ? 0 : (aColTo - aColFrom) / Math.Abs(aColTo - aColFrom);

            int sCurrentRow = aRowFrom + sRowStep;
            int sCurrentCol = aColFrom + sColStep;

            while (sCurrentRow != aRowTo || sCurrentCol != aColTo)
            {
                if (mChessBoard[sCurrentRow, sCurrentCol] != null) return false;

                sCurrentRow += sRowStep;
                sCurrentCol += sColStep;
            }

            return true;
        }

        private bool IsValidRookMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            if (aRowFrom != aRowTo && aColFrom != aColTo) return false; // Must move in a straight line

            return IsPathClear(aRowFrom, aColFrom, aRowTo, aColTo);
        }

        private bool IsValidBishopMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            if (Math.Abs(aRowFrom - aRowTo) != Math.Abs(aColFrom - aColTo)) return false; // Must move diagonally

            return IsPathClear(aRowFrom, aColFrom, aRowTo, aColTo);
        }

        private bool IsValidQueenMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            return IsValidRookMove(aRowFrom, aColFrom, aRowTo, aColTo) || IsValidBishopMove(aRowFrom, aColFrom, aRowTo, aColTo);
        }

        private bool IsValidKnightMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            int rowDiff = Math.Abs(aRowTo - aRowFrom);
            int colDiff = Math.Abs(aColTo - aColFrom);

            return (rowDiff == 2 && colDiff == 1) || (rowDiff == 1 && colDiff == 2);
        }

        private bool IsValidPawnMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo, Pawn aPawn)
        {
            //White moves up, Black moves down
            int sDirection = (aPawn.Color == ColorEnum.White) ? -1 : 1;
            int sStartRow = (aPawn.Color == ColorEnum.White) ? 6 : 1;

            //Normal one-step move
            if (aColTo == aColFrom && aRowTo == aRowFrom + sDirection && mChessBoard[aRowTo, aColTo] == null)
            {
                return true;
            }

            //First move, allow two steps forward
            if (aColTo == aColFrom && aRowFrom == sStartRow && aRowTo == aRowFrom + 2 * sDirection && mChessBoard[aRowTo, aColTo] == null && mChessBoard[aRowFrom + sDirection, aColFrom] == null)
            {
                return true;
            }

            //Capturing diagonally
            if (Math.Abs(aColTo - aColFrom) == 1 && aRowTo == aRowFrom + sDirection && mChessBoard[aRowTo, aColTo] != null && mChessBoard[aRowTo, aColTo]?.Color != aPawn.Color)
            {
                return true;
            }

            return false;
        }

        private bool IsValidKingMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            return Math.Abs(aRowFrom - aRowTo) <= 1 && Math.Abs(aColFrom - aColTo) <= 1;
        }
        #endregion


        #region Helpers
        public (int aRow, int aCol) GetKingPosition(ColorEnum aColor)
        {
            for (int sRow = 0; sRow < 8; sRow++)
            {
                for (int sCol = 0; sCol < 8; sCol++)
                {
                    if (mChessBoard[sRow, sCol] is King sKing && sKing.Color == aColor)
                    {
                        return (sRow, sCol);
                    }
                }
            }

            return (-1, -1);
        }

        public bool IsKingInCheck(ColorEnum aColor)
        {
            //Find the king
            var sKingPos = GetKingPosition(aColor);

            int sRowKing = sKingPos.aRow;
            int sColKing = sKingPos.aCol;

            //If the king is not found, it's been taken in a simulated move!
            if (sRowKing == -1 && sColKing == -1)
            {
                return true;
            }

            List<(int, int)> sOtherColoredPieces = GetAllPiecesCoordinates(aColor == ColorEnum.White ? ColorEnum.Black : ColorEnum.White);

            foreach (var (sRow, sCol) in sOtherColoredPieces)
            {
                if (IsValidMove(sRow, sCol, sRowKing, sColKing))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsKingInCheckmate(ColorEnum aColor)
        {
            //Not even in check => No checkmate
            if (!IsKingInCheck(aColor))
            {
                return false;
            }

            //Try all possible moves
            //Loop through the coordinates of all pieces
            foreach (var (sRowPiece, sColPiece) in GetAllPiecesCoordinates(aColor))
            {
                ChessPiece sChessPiece = mChessBoard[sRowPiece, sColPiece];

                //Loop through all coordinates
                for (int sRow = 0; sRow < 8; sRow++)
                {
                    for (int sCol = 0; sCol < 8; sCol++)
                    {
                        //Check if the chess piece can move there
                        if (IsValidMove(sRowPiece, sColPiece, sRow, sCol))
                        {
                            //It could, simulate the move & check if we're still in check
                            ChessPiece? sCapturedPiece = MakeMove(sRowPiece, sColPiece, sRow, sCol);

                            //Check if we're still in check
                            bool sStillInCheck = IsKingInCheck(aColor);

                            //Undo the move
                            UndoMove(sRowPiece, sColPiece, sRow, sCol, sCapturedPiece);

                            if (!sStillInCheck)
                            {
                                //We found a move, not in checkmate
                                return false;
                            }
                        }
                    }
                }
            }

            //No valid moves found, checkmate
            return true;
        }

        public bool IsStalemate(ColorEnum aColor)
        {
            if (IsKingInCheck(aColor)) return false; //Check => not stalemate

            foreach (var (sRowPiece, sColPiece) in GetAllPiecesCoordinates(aColor))
            {
                //Loop through all coordinates
                for (int sRow = 0; sRow < 8; sRow++)
                {
                    for (int sCol = 0; sCol < 8; sCol++)
                    {
                        //Check if the chess piece can move there
                        if (IsValidMove(sRowPiece, sColPiece, sRow, sCol))
                        {
                            //It could, so it's not a stalemate
                            return false;
                        }
                    }
                }
            }

            //No valid moves found, it's a stalemate
            return true;
        }

        private bool IsGameOver()
        {
            return IsKingInCheckmate(ColorEnum.White) || IsKingInCheckmate(ColorEnum.Black) || IsStalemate(ColorEnum.White) || IsStalemate(ColorEnum.Black);
        }

        public List<(int, int)> GetAllPiecesCoordinates(ColorEnum aColor)
        {
            List<(int, int)> sPieces = new List<(int, int)>();

            for (int sRow = 0; sRow < 8; sRow++)
            {
                for (int sCol = 0; sCol < 8; sCol++)
                {
                    if (mChessBoard[sRow, sCol]?.Color == aColor)
                    {
                        sPieces.Add((sRow, sCol));
                    }
                }
            }

            return sPieces;
        }

        public char GetColumnCoordinate(int aCol)
        {
            return (char)('a' + aCol);
        }

        public int GetRowCoordinate(int aRow)
        {
            return 8 - aRow;
        }

        public (int Total, int Black, int White) EvaluateBoard()
        {
            int sBlack = 0;
            int sWhite = 0;

            if (IsKingInCheckmate(ColorEnum.White)) return (int.MinValue, 0, 0);
            if(IsKingInCheckmate(ColorEnum.Black)) return (int.MaxValue, 0, 0);
            if (IsStalemate(ColorEnum.White)) return (0, 0, 0);

            for (int sRow = 0; sRow < 8; sRow++)
            {
                for (int sCol = 0; sCol < 8; sCol++)
                {
                    ChessPiece? sPiece = mChessBoard[sRow, sCol];
                    if (sPiece != null)
                    {
                        if (sPiece.Color == ColorEnum.White)
                        {
                            sWhite += sPiece.Value;
                        }
                        else
                        {
                            sBlack += sPiece.Value;
                        }
                    }
                }
            }

            return (sWhite - sBlack, sBlack, sWhite);
        }

        public (int BestEval, int RowFrom, int ColFrom, int RowTo, int ColTo) MiniMax(int aDepth, bool aIsMaximizingPlayer, out int aPosTried, int aAlpha = int.MinValue, int aBeta = int.MaxValue)
        {
            aPosTried = 0;

            if (aDepth == 0 || IsGameOver())
            {
                return (EvaluateBoard().Total, -1, -1, -1, -1); // Include evaluation in return value
            }

            int sValue;
            int sBestRowFrom = -1, sBestColFrom = -1, sBestRowTo = -1, sBestColTo = -1;

            sValue = aIsMaximizingPlayer ? int.MinValue : int.MaxValue;

            foreach(var sPos in GetAllPiecesCoordinates(aIsMaximizingPlayer ? ColorEnum.White : ColorEnum.Black))
            {
                for (int sRow = 0; sRow < 8; sRow++)
                {
                    for (int sCol = 0; sCol < 8; sCol++)
                    {
                        if (!IsValidMove(sPos.Item1, sPos.Item2, sRow, sCol)) continue;

                        aPosTried++;

                        //Make move
                        ChessPiece? sCapturedPiece = MakeMove(sPos.Item1, sPos.Item2, sRow, sCol);

                        int sSubPosTried;
                        var sNext = MiniMax(aDepth - 1, !aIsMaximizingPlayer, out sSubPosTried);
                        aPosTried += sSubPosTried;

                        //Undo move
                        UndoMove(sPos.Item1, sPos.Item2, sRow, sCol, sCapturedPiece);

                        if (aIsMaximizingPlayer ? sNext.BestEval > sValue : sNext.BestEval < sValue)
                        {
                            sValue = sNext.BestEval;
                            sBestRowFrom = sPos.Item1;
                            sBestColFrom = sPos.Item2;
                            sBestRowTo = sRow;
                            sBestColTo = sCol;
                        }

                        if (aIsMaximizingPlayer)
                        {
                            aAlpha = Math.Max(aAlpha, sValue);
                        }
                        else
                        {
                            aBeta = Math.Min(aBeta, sValue);
                        }

                        if (aBeta <= aAlpha) break; // Alpha cutoff
                    }
                }
            }

            return (sValue, sBestRowFrom, sBestColFrom, sBestRowTo, sBestColTo);
        }

        private ChessPiece? MakeMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            ChessPiece? sCapturedPiece = mChessBoard[aRowTo, aColTo];
            mChessBoard[aRowTo, aColTo] = mChessBoard[aRowFrom, aColFrom];
            mChessBoard[aRowFrom, aColFrom] = null;
            return sCapturedPiece;
        }

        private void UndoMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo, ChessPiece? aCapturedPiece)
        {
            mChessBoard[aRowFrom, aColFrom] = mChessBoard[aRowTo, aColTo];
            mChessBoard[aRowTo, aColTo] = aCapturedPiece;
        }
        #endregion
    }
}
