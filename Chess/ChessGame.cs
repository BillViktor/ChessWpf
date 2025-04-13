using Chess.Models;
using System.Windows.Shapes;

namespace Chess
{
    public class ChessGame
    {
        //Fields
        private string mStartingString = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        private ChessPiece?[,] mChessBoard = new ChessPiece?[8, 8];

        //History of positions, the key is the FEN string and the value is the number of times it has appeared in the game
        private Dictionary<string, int> mPositionHistory = new Dictionary<string, int>(); 

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

        //Constructors
        public ChessGame()
        {
            LoadBoardFromFen(mStartingString);
        }
        public ChessGame(string aFen)
        {
            LoadBoardFromFen(aFen);
        }

        /// <summary>
        /// Updates the position history
        /// </summary>
        public void UpdatePositionHistory()
        {
            string sFenString = GetFenString();

            if(mPositionHistory.ContainsKey(sFenString))
            {
                mPositionHistory[GetFenString()]++;
            }
            else
            {
                mPositionHistory.Add(sFenString, 1);
            }
        }

        /// <summary>
        /// Sets up the chess board from a FEN string.
        /// </summary>
        /// <param name="aFen">The FEN string</param>
        public void LoadBoardFromFen(string aFen)
        {
            //Reset board
            mChessBoard = new ChessPiece?[8, 8]; 

            string[] sParts = aFen.Split(' ');
            string[] sRows = sParts[0].Split('/');

            for (int sRow = 0; sRow < 8; sRow++)
            {
                int sFile = 0;
                foreach (char c in sRows[sRow])
                {
                    if (char.IsDigit(c))
                    {
                        //Skip empty squares
                        sFile += c - '0'; 
                    }
                    else
                    {
                        ColorEnum color = char.IsUpper(c) ? ColorEnum.White : ColorEnum.Black;
                        mChessBoard[sRow, sFile] = GetPieceFromFenChar(char.ToLower(c), color);
                        sFile++;
                    }
                }
            }

            //Set color to move
            mColorToMove = sParts[1] == "w" ? ColorEnum.White : ColorEnum.Black;
        }

        /// <summary>
        /// Creates a chess piece from a FEN character.
        /// </summary>
        /// <param name="aChar"></param>
        /// <param name="aColor"></param>
        /// <returns>A chesspiece</returns>
        private ChessPiece? GetPieceFromFenChar(char aChar, ColorEnum aColor)
        {
            return aChar switch
            {
                'p' => new Pawn(aColor),
                'r' => new Rook(aColor),
                'n' => new Knight(aColor),
                'b' => new Bishop(aColor),
                'q' => new Queen(aColor),
                'k' => new King(aColor),
                _ => null
            };
        }

        /// <summary>
        /// Gets the FEN character from a chess piece.
        /// </summary>
        /// <param name="aPiece">The chess piece</param>
        /// <returns>The FEN char</returns>
        private char GetFenCharFromPiece(ChessPiece aPiece)
        {
            char c = aPiece switch
            {
                Pawn => 'p',
                Rook => 'r',
                Knight => 'n',
                Bishop => 'b',
                Queen => 'q',
                King => 'k',
                _ => '?'
            };

            return aPiece.Color == ColorEnum.White ? char.ToUpper(c) : c;
        }

        /// <summary>
        /// Gets the current state of the board as a FEN string
        /// </summary>
        public string GetFenString()
        {
            string sFen = "";

            for (int sRow = 0; sRow < 8; sRow++)
            {
                int sEmptyCount = 0;

                for (int sCol = 0; sCol < 8; sCol++)
                {
                    ChessPiece? sPiece = mChessBoard[sRow, sCol];

                    if (sPiece == null)
                    {
                        sEmptyCount++;
                    }
                    else
                    {
                        if (sEmptyCount > 0)
                        {
                            sFen += sEmptyCount.ToString();
                            sEmptyCount = 0;
                        }

                        char sPieceChar = GetFenCharFromPiece(sPiece);
                        sFen += sPieceChar;
                    }
                }

                if (sEmptyCount > 0)
                {
                    sFen += sEmptyCount.ToString();
                }

                if (sRow < 7)
                {
                    sFen += "/";
                }
            }

            string sActiveColor = mColorToMove == ColorEnum.White ? "w" : "b";

            //Rest of FEN: turn, castling, en passant, halfmove, fullmove
            //TODO: Add castling, en passant, halfmove, fullmove, hardcoded for now
            sFen += $" {sActiveColor} KQkq - 0 1";
            return sFen;
        }

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

            // Determine castling side
            bool sIsKingSide = sColDiff > 0;
            int sRookCol = sIsKingSide ? 7 : 0;

            //Make sure the rook hasn't moved
            ChessPiece sChessPiece = mChessBoard[aRowFrom, sRookCol];

            if (sChessPiece == null || sChessPiece is not Rook sRook || sRook.MoveCount > 0) return false;

            //Make sure the path between them is clear
            if (!IsPathClear(aRowFrom, aColFrom, aRowFrom, sRookCol)) return false;

            //Squares the king passes through must not be under attack
            int step = sIsKingSide ? 1 : -1;
            for (int i = 1; i <= 2; i++)
            {
                int colToCheck = aColFrom + step * i;
                if (IsSquareAttacked(aRowFrom, colToCheck, sKing.Color == ColorEnum.White ? ColorEnum.Black : ColorEnum.White))
                    return false;
            }

            // Queenside special case: also check the square next to the rook isn't under attack (e.g. d1 or d8)
            if (!sIsKingSide && IsSquareAttacked(aRowFrom, aColFrom - 1, sKing.Color == ColorEnum.White ? ColorEnum.Black : ColorEnum.White))
                return false;

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
        /// Checks if the square is attacked by any piece of the given color.
        /// </summary>
        /// <param name="aRow">The row to attack</param>
        /// <param name="aCol">The col to attack</param>
        /// <param name="aColor">The color to attack</param>
        /// <returns>True if under attack, false if not</returns>
        private bool IsSquareAttacked(int aRow, int aCol, ColorEnum aColor)
        {
            List<(int, int)> sOtherColoredPieces = GetAllPiecesCoordinates(aColor);

            foreach (var (sRow, sCol) in sOtherColoredPieces)
            {
                if (CanAttackSquare(sRow, sCol, aRow, aCol))
                {
                    return true;
                }
            }
            return false;
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
        /// Checks if the game is a draw by threefold repetition.
        /// </summary>
        /// <returns></returns>
        public bool IsThreefoldRule()
        {
            if(mPositionHistory.Any(x => x.Value >= 3))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the game is over (checkmate or stalemate).
        /// </summary>
        /// <returns>True if game is over, false if not</returns>
        private bool IsGameOver()
        {
            return IsKingInCheckmate(ColorEnum.White) || IsKingInCheckmate(ColorEnum.Black) || IsStalemate(ColorEnum.White) || IsStalemate(ColorEnum.Black) || IsThreefoldRule();
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
        /// Gets the coordinates of all pieces on the board
        /// </summary>
        /// <returns>A list of tuples containing coordinates</returns>
        public List<(int, int)> GetAllPiecesCoordinates()
        {
            List<(int, int)> sPieces = new List<(int, int)>();

            for (int sRow = 0; sRow < 8; sRow++)
            {
                for (int sCol = 0; sCol < 8; sCol++)
                {
                    if (mChessBoard[sRow, sCol] != null)
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
        /// Positive means white is in the lead, negative means black is.
        /// </summary>
        public int EvaluateBoard()
        {
            int sScore = 0;

            if (IsKingInCheckmate(ColorEnum.White)) return int.MinValue;
            if (IsKingInCheckmate(ColorEnum.Black)) return int.MaxValue;
            if (IsThreefoldRule()) return 0; //Draw
            if (IsStalemate(ColorEnum.White)) return 0; //Draw
            if (IsStalemate(ColorEnum.Black)) return 0; //Draw

            var sCoordinates = GetAllPiecesCoordinates();

            foreach (var (sRow, sCol) in sCoordinates)
            {
                ChessPiece sPiece = mChessBoard[sRow, sCol];
                int sPositionBonus = 0;

                // Flip row for black pieces for position tables
                int sAdjustedRow = sPiece.Color == ColorEnum.White ? sRow : 7 - sRow;

                switch (sPiece)
                {
                    case Pawn:
                        sPositionBonus = ScoreTables.PawnTable[sAdjustedRow, sCol];
                        break;
                    case Knight:
                        sPositionBonus = ScoreTables.KnightTable[sAdjustedRow, sCol];
                        break;
                    case Bishop:
                        sPositionBonus = ScoreTables.BishopTable[sAdjustedRow, sCol];
                        break;
                    case Rook:
                        sPositionBonus = ScoreTables.RookTable[sAdjustedRow, sCol];
                        break;
                    case Queen:
                        sPositionBonus = ScoreTables.QueenTable[sAdjustedRow, sCol];
                        break;
                    case King:
                        sPositionBonus = ScoreTables.KingTable[sAdjustedRow, sCol];
                        break;
                }

                // Add or subtract based on piece color
                sScore += sPiece.Value + (sPiece.Color == ColorEnum.White ? sPositionBonus : -sPositionBonus);
            }

            return sScore;
        }

        /// <summary>
        /// Gets the evaluation of the board for both colors.
        /// </summary>
        /// <returns>A tuple containing the score of both colors</returns>
        public (int White, int Black) GetEvaluation()
        {
            int sWhiteScore = 0;
            int sBlackScore = 0;

            var sCoordinates = GetAllPiecesCoordinates();

            foreach (var (sRow, sCol) in sCoordinates)
            {
                ChessPiece sPiece = mChessBoard[sRow, sCol];
                int sPositionBonus = 0;

                int sAdjustedRow = sPiece.Color == ColorEnum.White ? sRow : 7 - sRow;

                switch (sPiece)
                {
                    case Pawn:
                        sPositionBonus = ScoreTables.PawnTable[sAdjustedRow, sCol];
                        break;
                    case Knight:
                        sPositionBonus = ScoreTables.KnightTable[sAdjustedRow, sCol];
                        break;
                    case Bishop:
                        sPositionBonus = ScoreTables.BishopTable[sAdjustedRow, sCol];
                        break;
                    case Rook:
                        sPositionBonus = ScoreTables.RookTable[sAdjustedRow, sCol];
                        break;
                    case Queen:
                        sPositionBonus = ScoreTables.QueenTable[sAdjustedRow, sCol];
                        break;
                    case King:
                        sPositionBonus = ScoreTables.KingTable[sAdjustedRow, sCol];
                        break;
                }

                if (sPiece.Color == ColorEnum.White)
                {
                    sWhiteScore += sPiece.Value + sPositionBonus;
                }
                else
                {
                    sBlackScore += sPiece.Value + sPositionBonus*-1;
                }
            }

            return (sWhiteScore, sBlackScore);
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
        public (int BestEval, Move BestMove) MiniMax(int aDepth, bool aIsMaximizingPlayer, out int aPosTried, int aAlpha = int.MinValue, int aBeta = int.MaxValue, bool aUsePruning = true)
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
                var sNext = MiniMax(aDepth - 1, !aIsMaximizingPlayer, out sSubPosTried, aAlpha, aBeta, aUsePruning);
                aPosTried += sSubPosTried;
                UndoMove(sMove);

                //Update value and best move
                if (aIsMaximizingPlayer)
                {
                    if (sNext.BestEval > sValue)
                    {
                        sValue = sNext.BestEval;
                        sBestMove = sMove;
                    }

                    if (aUsePruning) aAlpha = Math.Max(aAlpha, sValue);
                }
                else
                {
                    if (sNext.BestEval < sValue)
                    {
                        sValue = sNext.BestEval;
                        sBestMove = sMove;
                    }

                    if (aUsePruning) aBeta = Math.Min(aBeta, sValue);
                }

                //Prune if needed
                if (aUsePruning && aBeta <= aAlpha)
                    break;
            }

            return (sValue, sBestMove);
        }

        /// <summary>
        /// Gets all possible moves for a given color.
        /// </summary>
        /// <param name="aColorEnum">The color to get all moves for</param>
        /// <returns>List of moves</returns>
        public List<Move> GetAllMoves(ColorEnum aColorEnum)
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
                    sScore += 900; //Assume Queen promotion 
                }

                //Castling bonuses
                if (sMove.MoveType == MoveTypeEnum.CastlingKingSide || sMove.MoveType == MoveTypeEnum.CastlingQueenSide)
                {
                    sScore += 30; //Favor castling a bit
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
