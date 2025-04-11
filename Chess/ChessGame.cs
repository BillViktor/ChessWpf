using Chess.Models;
using System.DirectoryServices.ActiveDirectory;
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
        private Move mLastMove;

        //Properties
        public ChessPiece?[,] ChessBoard { get => mChessBoard; set => mChessBoard = value; }
        public ColorEnum ColorToMove { get => mColorToMove; set => mColorToMove = value; }
        public List<Rectangle> HighLightedCells { get => mHighLightedCells; set => mHighLightedCells = value; }
        public ChessPiece? SelectedPiece { get => mSelectedPiece; set => mSelectedPiece = value; }
        public int CurrentMove { get => mCurrentMove; set => mCurrentMove = value; }
        public Move LastMove { get => mLastMove; set => mLastMove = value; }


        public bool IsValidMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            ChessPiece? sPieceToMove = mChessBoard[aRowFrom, aColFrom];
            if (sPieceToMove == null) return false;

            ChessPiece? sPieceToCapture = mChessBoard[aRowTo, aColTo];
            if (sPieceToCapture?.Color == sPieceToMove.Color || sPieceToCapture is King) return false;

            bool sValidMove = sPieceToMove switch
            {
                Pawn sPawn => IsValidPawnMove(aRowFrom, aColFrom, aRowTo, aColTo, sPawn),
                King sKing => IsValidKingMove(aRowFrom, aColFrom, aRowTo, aColTo),
                Rook sRook => IsValidRookMove(aRowFrom, aColFrom, aRowTo, aColTo),
                Knight sKnight => IsValidKnightMove(aRowFrom, aColFrom, aRowTo, aColTo),
                Queen sQueen => IsValidQueenMove(aRowFrom, aColFrom, aRowTo, aColTo),
                Bishop sBishop => IsValidBishopMove(aRowFrom, aColFrom, aRowTo, aColTo),
                _ => false
            };

            if (!sValidMove) return false;

            Move sMove = new Move(sPieceToMove, aRowFrom, aColFrom, aRowTo, aColTo, sPieceToCapture);
            MakeMove(sMove);
            bool sInCheck = IsKingInCheck(sPieceToMove.Color);
            UndoMove(sMove);

            return !sInCheck;
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

            //En Passant
            if (Math.Abs(aColTo - aColFrom) == 1 && aRowTo == aRowFrom + sDirection)
            {
                if (mLastMove != null && mLastMove.PieceToMove is Pawn sLastMovedPawn && sLastMovedPawn.Color != mColorToMove)
                {
                    // Last move was a 2-square pawn advance
                    int sFromRow = (sLastMovedPawn.Color == ColorEnum.White) ? 6 : 1;
                    int sToRow = sFromRow + 2 * ((sLastMovedPawn.Color == ColorEnum.White) ? -1 : 1);

                    if (mLastMove.FromRow == sFromRow && mLastMove.ToRow == sToRow && mLastMove.ToCol == aColTo && mLastMove.ToRow == aRowFrom)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsValidKingMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            return Math.Abs(aRowFrom - aRowTo) <= 1 && Math.Abs(aColFrom - aColTo) <= 1;
        }


        #region Helpers
        public (int Row, int Col) GetKingPosition(ColorEnum aColor)
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

            List<(int, int)> sOtherColoredPieces = GetAllPiecesCoordinates(aColor == ColorEnum.White ? ColorEnum.Black : ColorEnum.White);

            foreach (var (sRow, sCol) in sOtherColoredPieces)
            {
                if (CanAttackSquare(sRow, sCol, sKingPos.Row, sKingPos.Col))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CanAttackSquare(int aFromRow, int aFromCol, int aToRow, int aToCol)
        {
            ChessPiece? piece = mChessBoard[aFromRow, aFromCol];
            if (piece == null) return false;

            ChessPiece? target = mChessBoard[aToRow, aToCol];
            if (target?.Color == piece.Color) return false;

            return piece switch
            {
                Pawn sPawn => IsValidPawnMove(aFromRow, aFromCol, aToRow, aToCol, sPawn),
                King sKing => IsValidKingMove(aFromRow, aFromCol, aToRow, aToCol),
                Rook sRook => IsValidRookMove(aFromRow, aFromCol, aToRow, aToCol),
                Knight sKnight => IsValidKnightMove(aFromRow, aFromCol, aToRow, aToCol),
                Queen sQueen => IsValidQueenMove(aFromRow, aFromCol, aToRow, aToCol),
                Bishop sBishop => IsValidBishopMove(aFromRow, aFromCol, aToRow, aToCol),
                _ => false
            };
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
                        if (CanAttackSquare(sRowPiece, sColPiece, sRow, sCol))
                        {
                            Move sMove = new Move(sChessPiece, sRowPiece, sColPiece, sRow, sCol, mChessBoard[sRow, sCol]);

                            //It could, simulate the move & check if we're still in check
                            MakeMove(sMove);

                            //Check if we're still in check
                            bool sStillInCheck = IsKingInCheck(aColor);

                            //Undo the move
                            UndoMove(sMove);

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

        public int EvaluateBoard()
        {
            List<ChessPiece> sPieces = GetAllPieces();

            return sPieces.Sum(x => x.Value);
        }

        public (int White, int Black) GetEvaluation()
        {
            List<ChessPiece> sPieces = GetAllPieces();

            return (sPieces.Where(x => x.Color == ColorEnum.White).Sum(x => x.Value),
                sPieces.Where(x => x.Color == ColorEnum.Black).Sum(x => x.Value));
        }

        public (int BestEval, Move BestMove) MiniMax(int aDepth, bool aIsMaximizingPlayer, out int aPosTried, int aAlpha = int.MinValue, int aBeta = int.MaxValue)
        {
            aPosTried = 0;

            if (aDepth == 0 || IsGameOver())
            {
                aPosTried = 1;
                return (EvaluateBoard(), null); // Include evaluation in return value
            }

            int sValue;
            Move sBestMove = null;

            sValue = aIsMaximizingPlayer ? int.MinValue : int.MaxValue;

            List<Move> sMoves = GetAllMoves(aIsMaximizingPlayer ? ColorEnum.White : ColorEnum.Black);
            OrderMoves(sMoves);

            foreach (Move sMove in sMoves)
            {
                MakeMove(sMove);

                int sSubPosTried;
                var sNext = MiniMax(aDepth - 1, !aIsMaximizingPlayer, out sSubPosTried);
                aPosTried += sSubPosTried;

                //Undo move
                UndoMove(sMove);

                if (aIsMaximizingPlayer ? sNext.BestEval > sValue : sNext.BestEval < sValue)
                {
                    sValue = sNext.BestEval;
                    sBestMove = sMove;
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

            return (sValue, sBestMove);
        }

        private List<Move> GetAllMoves(ColorEnum aColorEnum)
        {
            List<Move> sMoves = new List<Move>();

            for (int sRow = 0; sRow < 8; sRow++)
            {
                for (int sCol = 0; sCol < 8; sCol++)
                {
                    if(mChessBoard[sRow, sCol]?.Color == aColorEnum)
                    {
                        sMoves.AddRange(GetMovesForPiece(sRow, sCol));
                    }
                }
            }

            return sMoves;
        }

        public List<Move> GetMovesForPiece(int aRow, int aCol)
        {
            List<Move> sMoves = new List<Move>();

            ChessPiece sPiece = mChessBoard[aRow, aCol];

            for (int sRow = 0; sRow < 8; sRow++)
            {
                for (int sCol = 0; sCol < 8; sCol++)
                {
                    if(sRow == aRow && sCol == aCol) continue; // Skip the piece's current position

                    if(IsValidMove(aRow, aCol, sRow, sCol))
                    {
                        Move sMove = new Move(sPiece, aRow, aCol, sRow, sCol, mChessBoard[sRow, sCol]);
                        sMoves.Add(sMove);
                    }
                }
            }

            return sMoves;
        }

        private List<ChessPiece> GetAllPieces()
        {
            List<ChessPiece> sPieces = new List<ChessPiece>();
            for (int sRow = 0; sRow < 8; sRow++)
            {
                for (int sCol = 0; sCol < 8; sCol++)
                {
                    if (mChessBoard[sRow, sCol] != null)
                    {
                        sPieces.Add(mChessBoard[sRow, sCol]);
                    }
                }
            }
            return sPieces;
        }

        private void OrderMoves(List<Move> aMovesList)
        {
            foreach(Move sMove in aMovesList)
            {
                //Capturing a piece is good
                if(sMove.PieceCaptured != null)
                {
                    sMove.Score = 10 * sMove.PieceCaptured.Value - sMove.PieceToMove.Value;
                }

                if(sMove.MoveType == MoveTypeEnum.Promotion)
                {
                    sMove.Score += 900;
                }
            }

            aMovesList.Sort((m1, m2) => m2.Score.CompareTo(m1.Score));
        }

        private void MakeMove(Move aMove)
        {
            mChessBoard[aMove.ToRow, aMove.ToCol] = aMove.PieceToMove;
            mChessBoard[aMove.FromRow, aMove.FromCol] = null;
        }

        private void UndoMove(Move aMove)
        {
            mChessBoard[aMove.FromRow, aMove.FromCol] = mChessBoard[aMove.ToRow, aMove.ToCol];
            mChessBoard[aMove.ToRow, aMove.ToCol] = aMove.PieceCaptured;
        }
        #endregion
    }
}
