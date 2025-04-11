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

        /// <summary>
        /// Checks if the move is valid.
        /// </summary>
        /// <param name="aRowFrom">The row to move from</param>
        /// <param name="aColFrom">The col to move from</param>
        /// <param name="aRowTo">The row to move to</param>
        /// <param name="aColTo">The col to move to</param>
        /// <returns>True if valid, false if not</returns>
        public bool IsValidMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            //Get the piece to move
            ChessPiece? sPieceToMove = mChessBoard[aRowFrom, aColFrom];
            if (sPieceToMove == null) return false;

            //Check if the piece to capture is the right color and not a king (or null)
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

            //Check if the move would put the king in check
            Move sMove = new Move(sPieceToMove, aRowFrom, aColFrom, aRowTo, aColTo, sPieceToCapture);
            MakeMove(sMove);
            bool sInCheck = IsKingInCheck(sPieceToMove.Color);
            UndoMove(sMove);

            return !sInCheck;
        }

        /// <summary>
        /// Checks if the path between two squares is clear.
        /// </summary>
        /// <param name="aRowFrom">The row to move from</param>
        /// <param name="aColFrom">The col to move from</param>
        /// <param name="aRowTo">The row to move to</param>
        /// <param name="aColTo">The col to move to</param>
        /// <returns>True if clear, false if not</returns>
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
        /// Checks if the move is valid.
        /// </summary>
        /// <param name="aRowFrom">The row to move from</param>
        /// <param name="aColFrom">The col to move from</param>
        /// <param name="aRowTo">The row to move to</param>
        /// <param name="aColTo">The col to move to</param>
        /// <returns>True if valid, false if not</returns>
        private bool IsValidRookMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            if (aRowFrom != aRowTo && aColFrom != aColTo) return false; // Must move in a straight line

            return IsPathClear(aRowFrom, aColFrom, aRowTo, aColTo);
        }

        /// <summary>
        /// Checks if the move is valid.
        /// </summary>
        /// <param name="aRowFrom">The row to move from</param>
        /// <param name="aColFrom">The col to move from</param>
        /// <param name="aRowTo">The row to move to</param>
        /// <param name="aColTo">The col to move to</param>
        /// <returns>True if valid, false if not</returns>
        private bool IsValidBishopMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            if (Math.Abs(aRowFrom - aRowTo) != Math.Abs(aColFrom - aColTo)) return false; // Must move diagonally

            return IsPathClear(aRowFrom, aColFrom, aRowTo, aColTo);
        }

        /// <summary>
        /// Checks if the move is valid.
        /// </summary>
        /// <param name="aRowFrom">The row to move from</param>
        /// <param name="aColFrom">The col to move from</param>
        /// <param name="aRowTo">The row to move to</param>
        /// <param name="aColTo">The col to move to</param>
        /// <returns>True if valid, false if not</returns>
        private bool IsValidQueenMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            return IsValidRookMove(aRowFrom, aColFrom, aRowTo, aColTo) || IsValidBishopMove(aRowFrom, aColFrom, aRowTo, aColTo);
        }

        /// <summary>
        /// Checks if the move is valid.
        /// </summary>
        /// <param name="aRowFrom">The row to move from</param>
        /// <param name="aColFrom">The col to move from</param>
        /// <param name="aRowTo">The row to move to</param>
        /// <param name="aColTo">The col to move to</param>
        /// <returns>True if valid, false if not</returns>
        private bool IsValidKnightMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            int rowDiff = Math.Abs(aRowTo - aRowFrom);
            int colDiff = Math.Abs(aColTo - aColFrom);

            return (rowDiff == 2 && colDiff == 1) || (rowDiff == 1 && colDiff == 2);
        }

        /// <summary>
        /// Checks if the move is valid.
        /// </summary>
        /// <param name="aRowFrom">The row to move from</param>
        /// <param name="aColFrom">The col to move from</param>
        /// <param name="aRowTo">The row to move to</param>
        /// <param name="aColTo">The col to move to</param>
        /// <param name="aCoaPawnlTo">The pawn to move</param>
        /// <returns>True if valid, false if not</returns>
        private bool IsValidPawnMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo, Pawn aPawn)
        {
            //White moves up, Black moves down
            int sDirection = (aPawn.Color == ColorEnum.White) ? -1 : 1;

            //Normal one-step move
            if (aColTo == aColFrom && aRowTo == aRowFrom + sDirection && mChessBoard[aRowTo, aColTo] == null)
            {
                return true;
            }

            //First move, allow two steps forward
            if (aColTo == aColFrom && aPawn.MoveCount == 0 && aRowTo == aRowFrom + 2 * sDirection && mChessBoard[aRowTo, aColTo] == null && mChessBoard[aRowFrom + sDirection, aColFrom] == null)
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

        /// <summary>
        /// Checks if the move is valid.
        /// </summary>
        /// <param name="aRowFrom">The row to move from</param>
        /// <param name="aColFrom">The col to move from</param>
        /// <param name="aRowTo">The row to move to</param>
        /// <param name="aColTo">The col to move to</param>
        /// <returns>True if valid, false if not</returns>
        private bool IsValidKingMove(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            int sColDiff = aColTo - aColFrom;

            //Regular king move
            if (Math.Abs(aRowFrom - aRowTo) <= 1 && Math.Abs(sColDiff) <= 1) return true;

            //Castling
            //If not on the same row, return false
            if(aRowFrom != aRowTo || Math.Abs(sColDiff) != 2) return false;

            //Get the king
            ChessPiece sKing = mChessBoard[aRowFrom, aColFrom];

            //Make sure the king hasn't moved
            if (sKing.MoveCount > 0) return false;

            //Make sure we're not in check
            if (IsKingInCheck(sKing.Color)) return false;

            //Get the column of the rook
            int sRookCol = sColDiff > 0 ? 7 : 0;

            //Make sure the rook hasn't moved
            ChessPiece sChessPiece = mChessBoard[aRowFrom, sRookCol];

            if (sChessPiece == null || sChessPiece is not Rook sRook || sRook.MoveCount > 0) return false;

            //Make sure the path between them is clear
            if (!IsPathClear(aRowFrom, aColFrom, aRowFrom, sRookCol)) return false;

            return true;
        }


        #region Helpers
        /// <summary>
        /// Gets the position of the king of the given color.
        /// </summary>
        /// <param name="aColor">The color of the king to get</param>
        /// <returns>A tuple with the coordinates of the king</returns>
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

        /// <summary>
        /// Checks if the king of the given color is in check.
        /// </summary>
        /// <param name="aColor">The color of the king to check</param>
        /// <returns>True if in check, false if not</returns>
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

        /// <summary>
        /// Checks if the piece can attack the square.
        /// </summary>
        /// <param name="aRowFrom">The row to move from</param>
        /// <param name="aColFrom">The col to move from</param>
        /// <param name="aRowTo">The row to move to</param>
        /// <param name="aColTo">The col to move to</param>
        /// <returns>True if the source can attack the target</returns>
        private bool CanAttackSquare(int aRowFrom, int aColFrom, int aRowTo, int aColTo)
        {
            ChessPiece? sPiece = mChessBoard[aRowFrom, aColFrom];
            if (sPiece == null) return false;

            ChessPiece? target = mChessBoard[aRowTo, aColTo];
            if (target?.Color == sPiece.Color) return false;

            return sPiece switch
            {
                Pawn sPawn => IsValidPawnMove(aRowFrom, aColFrom, aRowTo, aColTo, sPawn),
                King sKing => IsValidKingMove(aRowFrom, aColFrom, aRowTo, aColTo),
                Rook sRook => IsValidRookMove(aRowFrom, aColFrom, aRowTo, aColTo),
                Knight sKnight => IsValidKnightMove(aRowFrom, aColFrom, aRowTo, aColTo),
                Queen sQueen => IsValidQueenMove(aRowFrom, aColFrom, aRowTo, aColTo),
                Bishop sBishop => IsValidBishopMove(aRowFrom, aColFrom, aRowTo, aColTo),
                _ => false
            };
        }

        /// <summary>
        /// Checks if the king of the given color is in checkmate.
        /// </summary>
        /// <param name="aColor">The color of the king to check</param>
        /// <returns>True if in checkmate, false if not</returns>
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

        /// <summary>
        /// Checks if the king of the given color is in stalemate.
        /// </summary>
        /// <param name="aColor">The color to check</param>
        /// <returns>True if stalemate (no available moves), false if not</returns>
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
        /// Checks if the game is over (checkmate or stalemate).
        /// </summary>
        /// <returns>True if game is over, false if not</returns>
        private bool IsGameOver()
        {
            return IsKingInCheckmate(ColorEnum.White) || IsKingInCheckmate(ColorEnum.Black) || IsStalemate(ColorEnum.White) || IsStalemate(ColorEnum.Black);
        }

        /// <summary>
        /// Gets the coordinates of all pieces of a given color
        /// </summary>
        /// <param name="aColor">The color to get all coordinates for</param>
        /// <returns>A list of tuples containing coordinates</returns>
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
        /// Gets the column coordinate from a given column index.
        /// </summary>
        /// <param name="aCol">The column index</param>
        /// <returns>The column index as a char</returns>
        public char GetColumnCoordinate(int aCol)
        {
            return (char)('a' + aCol);
        }

        /// <summary>
        /// Gets the row coordinate from a given row index.
        /// </summary>
        /// <param name="aRow">The row index</param>
        /// <returns>The row</returns>
        public int GetRowCoordinate(int aRow)
        {
            return 8 - aRow;
        }

        /// <summary>
        /// Evaluates the board and returns a score.
        /// Used by the MiniMax algorithm
        /// </summary>
        /// <returns>The board evaluation, positive means white is in the lead, negative means black is</returns>
        public int EvaluateBoard()
        {
            List<ChessPiece> sPieces = GetAllPieces();

            return sPieces.Sum(x => x.Value);
        }

        /// <summary>
        /// Gets the evaluation of the board for both colors.
        /// </summary>
        /// <returns>A tuple containing the score of both colors</returns>
        public (int White, int Black) GetEvaluation()
        {
            List<ChessPiece> sPieces = GetAllPieces();

            return (sPieces.Where(x => x.Color == ColorEnum.White).Sum(x => x.Value),
                sPieces.Where(x => x.Color == ColorEnum.Black).Sum(x => x.Value));
        }

        /// <summary>
        /// MiniMax algorithm to find the best move.
        /// </summary>
        /// <param name="aDepth">The depth of the search</param>
        /// <param name="aIsMaximizingPlayer">If the player is maximizing (white)</param>
        /// <param name="aPosTried">Number of positions tried</param>
        /// <param name="aAlpha">Alpha beta pruning</param>
        /// <param name="aBeta">Alpha beta pruning</param>
        /// <returns>Tuple containing the evaluation and the move</returns>
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

        /// <summary>
        /// Gets all possible moves for a given color.
        /// </summary>
        /// <param name="aColorEnum">The color to get all moves for</param>
        /// <returns>List of moves</returns>
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

        /// <summary>
        /// Gets all possible moves for a given piece.
        /// </summary>
        /// <param name="aRow">The row of the piece</param>
        /// <param name="aCol">The col of the piece</param>
        /// <returns>List of moves</returns>
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

        /// <summary>
        /// Gets a list of all pieces
        /// </summary>
        /// <returns>List of all pieces</returns>
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

        /// <summary>
        /// Orders the moves based to make the minimax algorithm more efficient (thanks to alpha beta pruning)
        /// </summary>
        /// <param name="aMovesList">The list of moves to order</param>
        private void OrderMoves(List<Move> aMovesList)
        {
            foreach (Move sMove in aMovesList)
            {
                int sScore = 0;

                //Captures: MVV-LVA (Most Valuable Victim - Least Valuable Attacker)
                if (sMove.PieceCaptured != null)
                {
                    sScore += 10 * (int)sMove.PieceCaptured.Value - (int)sMove.PieceToMove.Value;
                }

                //Promotions
                if (sMove.MoveType == MoveTypeEnum.Promotion)
                {
                    sScore += 900; // Queen promotion — customize if you support other promotions
                }

                //Castling bonuses
                if (sMove.MoveType == MoveTypeEnum.CastlingKingSide || sMove.MoveType == MoveTypeEnum.CastlingQueenSide)
                {
                    sScore += 30; // Favor castling a bit
                }

                //Center control (d4, e4, d5, e5)
                if ((sMove.ToRow == 3 || sMove.ToRow == 4) && (sMove.ToCol == 3 || sMove.ToCol == 4))
                {
                    sScore += 10;
                }

                sMove.Score = sScore;
            }

            aMovesList.Sort((m1, m2) => m2.Score.CompareTo(m1.Score));
        }

        /// <summary>
        /// Makes a move on the board.
        /// </summary>
        /// <param name="aMove">The move to make</param>
        private void MakeMove(Move aMove)
        {
            mChessBoard[aMove.ToRow, aMove.ToCol] = aMove.PieceToMove;
            mChessBoard[aMove.FromRow, aMove.FromCol] = null;
        }

        /// <summary>
        /// Undoes a move on the board.
        /// </summary>
        /// <param name="aMove">The move to undo</param>
        private void UndoMove(Move aMove)
        {
            mChessBoard[aMove.FromRow, aMove.FromCol] = mChessBoard[aMove.ToRow, aMove.ToCol];
            mChessBoard[aMove.ToRow, aMove.ToCol] = aMove.PieceCaptured;
        }
        #endregion
    }
}
