using Chess.Models;
using System.Windows.Input;
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
        private bool mSinglePlayer = false;

        //Properties
        public ChessPiece?[,] ChessBoard { get => mChessBoard; set => mChessBoard = value; }
        public ColorEnum ColorToMove { get => mColorToMove; set => mColorToMove = value; }
        public List<Rectangle> HighLightedCells { get => mHighLightedCells; set => mHighLightedCells = value; }
        public ChessPiece? SelectedPiece { get => mSelectedPiece; set => mSelectedPiece = value; }
        public int CurrentMove { get => mCurrentMove; set => mCurrentMove = value; }
        public bool SinglePlayer { get => mSinglePlayer; set => mSinglePlayer = value; }


        #region Movement Validation
        /// <summary>
        /// Checks if the move is valid or not
        /// </summary>
        /// <param name="aRowFrom">The row of the piece</param>
        /// <param name="aColFrom">The col of the piece</param>
        /// <param name="aRowTo">The target row</param>
        /// <param name="aColTo">The target col</param>
        /// <returns></returns>
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

        /// <summary>
        /// Checks if the path between two points are clear
        /// </summary>
        /// <param name="aRowFrom">The row to move from</param>
        /// <param name="aColFrom">The col to move from</param>
        /// <param name="aRowTo">The row to move to</param>
        /// <param name="aColTo">The col to move to</param>
        /// <param name="aPawn">The pawn to move</param>
        /// <returns>True if path is clear, false if not</returns>
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

        /// <summary>
        /// Checks if the move is valid for a rook
        /// </summary>
        /// <param name="aRowFrom">The row to move from</param>
        /// <param name="aColFrom">The col to move from</param>
        /// <param name="aRowTo">The row to move to</param>
        /// <param name="aColTo">The col to move to</param>
        /// <param name="aPawn">The pawn to move</param>
        private bool IsValidRookMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            if (aRowFrom != aRowTo && aColFrom != aColTo) return false; // Must move in a straight line

            return IsPathClear(aRowFrom, aColFrom, aRowTo, aColTo);
        }

        /// <summary>
        /// Checks if the move is valid for a bishop
        /// </summary>
        /// <param name="aRowFrom">The row to move from</param>
        /// <param name="aColFrom">The col to move from</param>
        /// <param name="aRowTo">The row to move to</param>
        /// <param name="aColTo">The col to move to</param>
        /// <param name="aPawn">The pawn to move</param>
        private bool IsValidBishopMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            if (Math.Abs(aRowFrom - aRowTo) != Math.Abs(aColFrom - aColTo)) return false; // Must move diagonally

            return IsPathClear(aRowFrom, aColFrom, aRowTo, aColTo);
        }

        /// <summary>
        /// Checks if the move is valid for a queen
        /// </summary>
        /// <param name="aRowFrom">The row to move from</param>
        /// <param name="aColFrom">The col to move from</param>
        /// <param name="aRowTo">The row to move to</param>
        /// <param name="aColTo">The col to move to</param>
        /// <param name="aPawn">The pawn to move</param>
        private bool IsValidQueenMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            return IsValidRookMove(aRowFrom, aColFrom, aRowTo, aColTo) || IsValidBishopMove(aRowFrom, aColFrom, aRowTo, aColTo);
        }

        /// <summary>
        /// Checks if the move is valid for a knight
        /// </summary>
        /// <param name="aRowFrom">The row to move from</param>
        /// <param name="aColFrom">The col to move from</param>
        /// <param name="aRowTo">The row to move to</param>
        /// <param name="aColTo">The col to move to</param>
        /// <param name="aPawn">The pawn to move</param>
        private bool IsValidKnightMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            int rowDiff = Math.Abs(aRowTo - aRowFrom);
            int colDiff = Math.Abs(aColTo - aColFrom);

            return (rowDiff == 2 && colDiff == 1) || (rowDiff == 1 && colDiff == 2);
        }

        /// <summary>
        /// Checks if the move is valid for a pawn
        /// </summary>
        /// <param name="aRowFrom">The row to move from</param>
        /// <param name="aColFrom">The col to move from</param>
        /// <param name="aRowTo">The row to move to</param>
        /// <param name="aColTo">The col to move to</param>
        /// <param name="aPawn">The pawn to move</param>
        /// <returns>True if valid, false if not</returns>
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

        /// <summary>
        /// Checks if the move is valid for a king
        /// </summary>
        /// <param name="aRowFrom">The row to move from</param>
        /// <param name="aColFrom">The col to move from</param>
        /// <param name="aRowTo">The row to move to</param>
        /// <param name="aColTo">The col to move to</param>
        private bool IsValidKingMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            return Math.Abs(aRowFrom - aRowTo) <= 1 && Math.Abs(aColFrom - aColTo) <= 1;
        }
        #endregion


        #region Helpers
        /// <summary>
        /// Find the position of the king
        /// </summary>
        /// <param name="aColor">The color of the king to find</param>
        /// <param name="aRow">The row the king is in</param>
        /// <param name="aCol">The col the king is in</param>
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

        /// <summary>
        /// Checks if the given color's king is in check
        /// </summary>
        /// <param name="aColor">The color of the king</param>
        /// <returns>True if in check, false if not</returns>
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

        /// <summary>
        /// Checks wether the given king is in checkmate or not
        /// </summary>
        /// <param name="aColor">The color of the king to check</param>
        /// <returns>True if king is in checkmate, false if not</returns>
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

        /// <summary>
        /// Checks if the given color is in a stalemate (no valid moves available)
        /// </summary>
        /// <param name="aColor">The color of the player</param>
        /// <returns>True if in stalemate, false if not</returns>
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

        /// <summary>
        /// Returns true if the game is over (checkmate or stalemate)
        /// </summary>
        /// <returns>True if game is over, false if not</returns>
        private bool IsGameOver()
        {
            return IsKingInCheckmate(ColorEnum.White) || IsKingInCheckmate(ColorEnum.Black) || IsStalemate(ColorEnum.White) || IsStalemate(ColorEnum.Black);
        }

        /// <summary>
        ///  Gets the coordinates of all pieces of a given color.
        /// </summary>
        /// <param name="aColor">The color of the pieces to fetch</param>
        /// <returns>A list of coordinates</returns>
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

        /// <summary>
        /// Returns the column coordinate (a-f) from a given col index of the game array
        /// </summary>
        /// <param name="aCol">The index of the col</param>
        /// <returns>The col coordinate</returns>
        public char GetColumnCoordinate(int aCol)
        {
            return (char)('a' + aCol);
        }

        /// <summary>
        /// Returns the row coordinate (1-8) from a given row index of the game array
        /// </summary>
        /// <param name="aRow">The index of the row</param>
        /// <returns>The row coordinate</returns>
        public int GetRowCoordinate(int aRow)
        {
            return 8 - aRow;
        }

        /// <summary>
        /// Evaluates the board and returns a score, a positive number means white is winning, a negative number means black is winning
        /// </summary>
        /// <returns>A score of the board</returns>
        public int EvaluateBoard()
        {
            int sScore = 0;

            for (int sRow = 0; sRow < 8; sRow++)
            {
                for (int sCol = 0; sCol < 8; sCol++)
                {
                    ChessPiece? sPiece = mChessBoard[sRow, sCol];
                    if (sPiece != null)
                    {
                        sScore += sPiece.Value;
                    }
                }
            }

            return sScore;
        }

        /// <summary>
        /// Minimax function with pruning to find the best move
        /// </summary>
        /// <param name="aDepth"></param>
        /// <param name="aIsMaximizingPlayer"></param>
        /// <param name="aMoveCount"></param>
        /// <param name="aAlpha"></param>
        /// <param name="aBeta"></param>
        /// <returns></returns>
        public (int BestEval, int RowFrom, int ColFrom, int RowTo, int ColTo) MiniMax(int aDepth, bool aIsMaximizingPlayer, int aAlpha = int.MinValue, int aBeta = int.MaxValue)
        {
            if (aDepth == 0 || IsGameOver())
            {
                return (EvaluateBoard(), -1, -1, -1, -1); // Include evaluation in return value
            }

            int sBestEval = aIsMaximizingPlayer ? int.MinValue : int.MaxValue;
            (int, int, int, int) sBestMove = (-1, -1, -1, -1);
            ColorEnum sColor = aIsMaximizingPlayer ? ColorEnum.Black : ColorEnum.White;
            Func<int, int, bool> sComparer = aIsMaximizingPlayer ? (x, y) => x > y : (x, y) => x < y;

            foreach (var (sRowFrom, sColFrom) in GetAllPiecesCoordinates(sColor))
            {
                for (int sRowTo = 0; sRowTo < 8; sRowTo++)
                {
                    for (int sColTo = 0; sColTo < 8; sColTo++)
                    {
                        if (!IsValidMove(sRowFrom, sColFrom, sRowTo, sColTo)) continue;

                        // Make the move
                        ChessPiece? sCapturedPiece = MakeMove(sRowFrom, sColFrom, sRowTo, sColTo);

                        // Recursively evaluate
                        int sChildMoveCount;
                        var (sEval, _, _, _, _) = MiniMax(aDepth - 1, !aIsMaximizingPlayer, aAlpha, aBeta);

                        // Undo the move
                        UndoMove(sRowFrom, sColFrom, sRowTo, sColTo, sCapturedPiece);

                        // Update best move if needed
                        if (sComparer(sEval, sBestEval))
                        {
                            sBestEval = sEval;
                            sBestMove = (sRowFrom, sColFrom, sRowTo, sColTo);
                        }

                        // Alpha-beta pruning
                        if (aIsMaximizingPlayer)
                        {
                            aAlpha = Math.Max(aAlpha, sBestEval);
                            if (aBeta <= aAlpha) return (sBestEval, sBestMove.Item1, sBestMove.Item2, sBestMove.Item3, sBestMove.Item4);
                        }
                        else
                        {
                            aBeta = Math.Min(aBeta, sBestEval);
                            if (aBeta <= aAlpha) return (sBestEval, sBestMove.Item1, sBestMove.Item2, sBestMove.Item3, sBestMove.Item4);
                        }
                    }
                }
            }

            return (sBestEval, sBestMove.Item1, sBestMove.Item2, sBestMove.Item3, sBestMove.Item4);
        }

        /// <summary>
        /// Helper method that makes a move, and returns the captured piece (null if none)
        /// </summary>
        /// <param name="aRowFrom">The row to move from</param>
        /// <param name="aColFrom">The col to move from</param>
        /// <param name="aRowTo">The row to move to</param>
        /// <param name="aColTo">The col to move to</param>
        /// <returns>The piece captured</returns>
        private ChessPiece? MakeMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            ChessPiece? capturedPiece = mChessBoard[aRowTo, aColTo];
            mChessBoard[aRowTo, aColTo] = mChessBoard[aRowFrom, aColFrom];
            mChessBoard[aRowFrom, aColFrom] = null;
            return capturedPiece;
        }

        /// <summary>
        /// Helper method to undo a move
        /// </summary>
        /// <param name="aRowFrom">The row to move from</param>
        /// <param name="aColFrom">The col to move from</param>
        /// <param name="aRowTo">The row to move to</param>
        /// <param name="aColTo">The col to move to</param>
        /// <param name="aCapturedPiece">The captured piece to return</param>
        private void UndoMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo, ChessPiece? aCapturedPiece)
        {
            mChessBoard[aRowFrom, aColFrom] = mChessBoard[aRowTo, aColTo];
            mChessBoard[aRowTo, aColTo] = aCapturedPiece;
        }
        #endregion
    }
}
