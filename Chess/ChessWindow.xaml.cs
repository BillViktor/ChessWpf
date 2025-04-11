using Chess.Models;
using System.Diagnostics;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using ColorEnum = Chess.Models.ColorEnum;

namespace Chess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ChessWindow : Window
    {
        private ChessGame mChessGame = new ChessGame();

        //Settings
        private TimeSpan? mTimerTimeSpan;
        private bool mSinglePlayer = false;

        private DispatcherTimer mTimerWhite;
        private TimeSpan mTimeWhite;

        private DispatcherTimer mTimerBlack;
        private TimeSpan mTimeBlack;

        public ChessWindow(bool aSinglePlayer, TimeSpan? aTimerTimeSpan)
        {
            InitializeComponent();

            mSinglePlayer = aSinglePlayer;

            if (aTimerTimeSpan != null)
            {
                mTimerTimeSpan = aTimerTimeSpan;
                WhiteTimer.Text = aTimerTimeSpan.Value.ToString(@"mm\:ss");
                BlackTimer.Text = aTimerTimeSpan.Value.ToString(@"mm\:ss");
                InitializeTimer();
            }

            CreateChessBoard();

            PlaySound("game-start.wav");
        }

        /// <summary>
        /// Updates the evaluation bar
        /// </summary>
        private void UpdateEvalBar()
        {
            //Get the eval
            var sEval = mChessGame.GetEvaluation();

            // Convert to 0.0 to 1.0: -10 = 0 (white losing), +10 = 1 (white winning)
            double sWhitePortion = ((double)sEval.White / (sEval.White - sEval.Black));
            double sBlackPortion = 1.0 - sWhitePortion;

            // Assign correctly: bottom is white, top is black
            WhiteEvalRow.Height = new GridLength(sWhitePortion, GridUnitType.Star);
            BlackEvalRow.Height = new GridLength(sBlackPortion, GridUnitType.Star);
        }

        /// <summary>
        /// Initializes timers for both colors and starts the white timer
        /// </summary>
        private void InitializeTimer()
        {
            mTimeWhite = mTimerTimeSpan.Value;
            mTimeBlack = mTimerTimeSpan.Value;

            mTimerWhite = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, delegate
            {
                mTimeWhite = mTimeWhite.Add(TimeSpan.FromSeconds(-1));
                WhiteTimer.Text = mTimeWhite.ToString(@"mm\:ss");
                if(mTimeWhite == TimeSpan.Zero)
                {
                    mTimerWhite.Stop();
                    MessageBox.Show("White ran out of time. Game over!", "Time's up!");
                    Close();
                }

            }, Dispatcher.CurrentDispatcher);

            mTimerBlack = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, delegate
            {
                mTimeBlack = mTimeBlack.Add(TimeSpan.FromSeconds(-1));
                BlackTimer.Text = mTimeBlack.ToString(@"mm\:ss");
                if (mTimeBlack == TimeSpan.Zero)
                {
                    mTimerBlack.Stop();
                    MessageBox.Show("White ran out of time. Game over!", "Time's up!");
                    Close();
                }
            }, Dispatcher.CurrentDispatcher);

            mTimerWhite.Stop();
            mTimerBlack.Stop();
        }

        /// <summary>
        /// Creates the chess game board by adding colored rectangles and the images of the pieces
        /// </summary>
        private void CreateChessBoard()
        {
            SolidColorBrush sLightBrush = new SolidColorBrush(Colors.Beige);
            SolidColorBrush sDarkBrush = new SolidColorBrush(Colors.DarkOliveGreen);

            for (int sRow = 0; sRow < 8; sRow++)
            {
                for (int sCol = 0; sCol < 8; sCol++)
                {
                    //Create a rectangle for each cell
                    Rectangle sRectangle = new Rectangle();
                    sRectangle.Fill = (sRow + sCol) % 2 == 0 ? sLightBrush : sDarkBrush;
                    sRectangle.Stretch = Stretch.Fill;

                    //Add to grid
                    Grid.SetRow(sRectangle, sRow);
                    Grid.SetColumn(sRectangle, sCol);
                    ChessBoardGrid.Children.Add(sRectangle);

                    //Add column numbers
                    if(sCol == 0)
                    {
                        TextBlock sRowLabel = new TextBlock
                        {
                            Text = (8 - sRow).ToString(),
                            FontWeight = FontWeights.Bold,
                            FontSize = 24,
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Margin = new Thickness(3),
                            Foreground = sRow % 2 == 0 ? sDarkBrush : sLightBrush
                        };

                        Grid.SetRow(sRowLabel, sRow);
                        Grid.SetColumn(sRowLabel, 0);
                        ChessBoardGrid.Children.Add(sRowLabel);
                    }
                    //Add row letters
                    if(sRow == 7)
                    {
                        TextBlock sRowLabel = new TextBlock
                        {
                            Text = ((char)('a' + sCol)).ToString(),
                            FontWeight = FontWeights.Bold,
                            FontSize = 24,
                            VerticalAlignment = VerticalAlignment.Bottom,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Margin = new Thickness(3),
                            Foreground = sCol % 2 == 0 ? sLightBrush : sDarkBrush
                        };

                        Grid.SetRow(sRowLabel, sRow);
                        Grid.SetColumn(sRowLabel, sCol);
                        ChessBoardGrid.Children.Add(sRowLabel);
                    }

                    //Add the chess piece
                    ChessPiece sChessPiece = mChessGame.ChessBoard[sRow, sCol];

                    if (sChessPiece != null)
                    {
                        Image sPieceImage = new Image
                        {
                            Source = new BitmapImage(new Uri(sChessPiece.ImagePath)),
                            Stretch = Stretch.Uniform,
                        };

                        Grid.SetRow(sPieceImage, sRow);
                        Grid.SetColumn(sPieceImage, sCol);
                        Panel.SetZIndex(sPieceImage, 999);
                        ChessBoardGrid.Children.Add(sPieceImage);
                    }
                }
            }
        }

        /// <summary>
        /// Refreshses the board by removing it's content and creating it again
        /// </summary>
        private void RefreshBoard()
        {
            ChessBoardGrid.Children.Clear();

            CreateChessBoard();
        }

        /// <summary>
        /// Gets click events from the grid. Hightlights selected pieces & possible moves
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChessBoardGrid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point sClickPosition = e.GetPosition(ChessBoardGrid);

            double sCellWidth = ChessBoardGrid.ActualWidth / 8;
            double sCellHeight = ChessBoardGrid.ActualHeight / 8;

            int sClickedColumn = (int)(sClickPosition.X / sCellWidth);
            int sClickedRow = (int)(sClickPosition.Y / sCellHeight);

            ChessPiece? sSelectedPiece = mChessGame.ChessBoard[sClickedRow, sClickedColumn];

            //In single player, you can not move black pieces
            if (mSinglePlayer && mChessGame.SelectedPiece == null && sSelectedPiece != null && sSelectedPiece.Color == ColorEnum.Black)
            {
                return;
            }

            if (mChessGame.SelectedPiece != null)
            {
                MovePiece(sClickedRow, sClickedColumn);
            }
            else
            {
                if(sSelectedPiece == null || sSelectedPiece.Color != mChessGame.ColorToMove)
                {
                    return;
                }
                else
                {
                    mChessGame.SelectedPiece = sSelectedPiece;
                    HighlightCell(sClickedRow, sClickedColumn, Colors.Yellow, "SelectedPiece");
                    HighlightValidMoves(sClickedRow, sClickedColumn);
                }
            }
        }

        /// <summary>
        /// Highlights valid moves for the selected piece
        /// </summary>
        /// <param name="aRowFrom">The row of the chess piece</param>
        /// <param name="aColFrom">The col of the chess piece</param>
        private void HighlightValidMoves(int aRowFrom, int aColFrom)
        {
            List<Move> sMoves = mChessGame.GetMovesForPiece(aRowFrom, aColFrom);

            foreach(Move sMove in sMoves)
            {
                HighlightCell(sMove.ToRow, sMove.ToCol, Colors.LightBlue, "ValidMove");
            }
        }

        /// <summary>
        /// Highlights a cell
        /// </summary>
        /// <param name="aRow">The row index of the cell to highlight</param>
        /// <param name="aCol">The col index of the cell to highlight</param>
        private void HighlightCell(int aRow, int aCol, System.Windows.Media.Color aColor, string? aTag = null)
        {
            Rectangle sHighLight = new Rectangle
            {
                Fill = new SolidColorBrush(aColor),
                Opacity = 0.5,
                Tag = aTag
            };

            Grid.SetRow(sHighLight, aRow);
            Grid.SetColumn(sHighLight, aCol);

            Panel.SetZIndex(sHighLight, 1);

            ChessBoardGrid.Children.Add(sHighLight);
            mChessGame.HighLightedCells.Add(sHighLight);
        }

        /// <summary>
        /// Removes the highlight
        /// </summary>
        private void ClearHighLights(bool aClearAll = true)
        {
            List<Rectangle> sRectanglesToRemove = new List<Rectangle>();

            foreach (Rectangle sHighLight in mChessGame.HighLightedCells)
            {
                if (!aClearAll && (sHighLight.Tag?.ToString() == "Check" || sHighLight.Tag?.ToString() == "LastMove")) continue;

                sRectanglesToRemove.Add(sHighLight);
                ChessBoardGrid.Children.Remove(sHighLight);
            }

            foreach(var sHighlight in sRectanglesToRemove)
            {
                mChessGame.HighLightedCells.Remove(sHighlight);
            }
        }

        /// <summary>
        /// Makes the move
        /// </summary>
        /// <param name="aRowTo">The row to move the piece to</param>
        /// <param name="aColTo">The col to move the piece to</param>
        private void MovePiece(Move aMove)
        {
            //Keep track if a piece was captured
            bool sPieceCaptured = false;

            //Make sure the move is valid
            if (!mChessGame.IsValidMove(aMove.FromRow, aMove.FromCol, aMove.ToRow, aMove.ToCol))
            {
                PlaySound("illegal-move.wav");
                ClearHighLights(false);
                mChessGame.SelectedPiece = null;
                return;
            }

            // Handle en passant
            if(aMove.MoveType == MoveTypeEnum.EnPassant)
            {
                int sCapturedPawnRow = aMove.FromRow;
                mChessGame.ChessBoard[sCapturedPawnRow, aMove.ToCol] = null;
                sPieceCaptured = true;
            }
            else
            {
                sPieceCaptured = aMove.MoveType == MoveTypeEnum.Capture;
            }

            //Handle castling
            if(aMove.MoveType == MoveTypeEnum.CastlingQueenSide)
            {
                mChessGame.ChessBoard[aMove.ToRow, 3] = mChessGame.ChessBoard[aMove.ToRow, 0]; // Move rook
                mChessGame.ChessBoard[aMove.ToRow, 0] = null;
            }
            else if(aMove.MoveType == MoveTypeEnum.CastlingKingSide)
            {
                mChessGame.ChessBoard[aMove.ToRow, 5] = mChessGame.ChessBoard[aMove.ToRow, 7]; // Move rook
                mChessGame.ChessBoard[aMove.ToRow, 7] = null;
            }

            mChessGame.ChessBoard[aMove.FromRow, aMove.FromCol] = null;
            mChessGame.ChessBoard[aMove.ToRow, aMove.ToCol] = mChessGame.SelectedPiece;

            RefreshBoard();

            if (sPieceCaptured)
            {
                PlaySound("capture.wav");
            }
            else
            {
                PlaySound("move-self.wav");
            }

            //Check if pawn promotion
            string sPromotion = "";

            if(aMove.MoveType == MoveTypeEnum.Promotion)
            {
                if (mSinglePlayer && mChessGame.ColorToMove == ColorEnum.Black)
                {
                    mChessGame.ChessBoard[aMove.ToRow, aMove.ToCol] = new Queen(ColorEnum.Black);
                }
                else
                {
                    PawnPromotion sPawnPromotion = new PawnPromotion(mChessGame.ColorToMove);
                    sPawnPromotion.ShowDialog();

                    mChessGame.ChessBoard[aMove.ToRow, aMove.ToCol] = sPawnPromotion.SelectedPromotionPiece;
                }
                RefreshBoard();
                PlaySound("promotion.wav");
                sPromotion = $"={mChessGame.ChessBoard[aMove.ToRow, aMove.ToCol].Abbreviation}";
            }

            //Increment the move count
            mChessGame.CurrentMove++;
            aMove.PieceToMove.MoveCount++;

            //Toggle the color to move
            mChessGame.ColorToMove = mChessGame.ColorToMove == ColorEnum.White ? ColorEnum.Black : ColorEnum.White;

            //Clear the old highlights
            ClearHighLights();

            //Highlight the move made
            HighlightCell(aMove.FromRow, aMove.FromCol, Colors.Yellow, "LastMove");
            HighlightCell(aMove.ToRow, aMove.ToCol, Colors.Yellow, "LastMove");

            //Check if it resulted in a check
            bool sCheck = mChessGame.IsKingInCheck(mChessGame.ColorToMove);

            if (sCheck)
            {
                if (mChessGame.IsKingInCheckmate(mChessGame.ColorToMove))
                {
                    PlaySound("checkmate.wav");
                    MessageBox.Show($"{mChessGame.ColorToMove} is in checkmate! Game over.", "Game over!");
                    Close();
                }
                else
                {
                    PlaySound("check.wav");

                    var sKingPos = mChessGame.GetKingPosition(mChessGame.ColorToMove);
                    HighlightCell(sKingPos.Row, sKingPos.Col, Colors.Red, "Check");
                }
            }
            else if (mChessGame.IsStalemate(mChessGame.ColorToMove))
            {
                PlaySound("game-end.wav");
                MessageBox.Show("Stalemate! The game is a draw!", "Game over!");
                Close();
            }

            //Add move to list
            AddMoveToList(aMove, sCheck, sPromotion);

            //Set last move
            mChessGame.LastMove = aMove;

            mChessGame.SelectedPiece = null;

            //Update eval bar
            UpdateEvalBar();

            //Change the timer
            if (mTimerTimeSpan != null)
            {
                if (mChessGame.ColorToMove == ColorEnum.White)
                {
                    mTimerBlack.Stop();
                    mTimerWhite.Start();
                }
                else
                {
                    mTimerWhite.Stop();
                    mTimerBlack.Start();
                }
            }

            if(mSinglePlayer && mChessGame.ColorToMove == ColorEnum.Black)
            {
                Dispatcher.InvokeAsync(() => ComputerMove(), DispatcherPriority.Background);
            }
        }

        /// <summary>
        /// Moves the selected piece to the given row and col
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="aCol"></param>
        private void MovePiece(int aRow, int aCol)
        {
            int sRowFrom, sColFrom;
            GetPositionForPiece(out sRowFrom, out sColFrom);
            Move sMove = new Move(mChessGame.SelectedPiece, sRowFrom, sColFrom, aRow, aCol, mChessGame.ChessBoard[aRow, aCol]);
            MovePiece(sMove);
        }

        /// <summary>
        /// Makes a move for the computer
        /// </summary>
        private void ComputerMove()
        {
            int sPositionsTried = 0;

            Stopwatch sStopWatch = new Stopwatch();
            sStopWatch.Start();

            //Use the minimax algorithm
            var sBestMove = mChessGame.MiniMax(4, false, out sPositionsTried);

            sStopWatch.Stop();
            Console.WriteLine($"Tried {sPositionsTried} in {sStopWatch.ElapsedMilliseconds}ms");

            //Make the move
            if (sBestMove.BestMove != null)
            {
                mChessGame.SelectedPiece = mChessGame.ChessBoard[sBestMove.BestMove.FromRow, sBestMove.BestMove.FromCol];
                MovePiece(sBestMove.BestMove);
            }
        }

        #region Sound
        /// <summary>
        /// Plays a sound from the resources folder
        /// </summary>
        /// <param name="aFileName">The name of the sound to play</param>
        private void PlaySound(string aFileName)
        {
            try
            {
                SoundPlayer sSoundPlayer = new SoundPlayer(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", aFileName));
                sSoundPlayer.Load();
                sSoundPlayer.Play();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error at PlaySound(): {ex.Message}", "Error!");
            }
        }
        #endregion


        #region Helpers
        /// <summary>
        /// Gets the position of the selected piece
        /// </summary>
        /// <param name="aRow">The row index of the piece</param>
        /// <param name="aCol">The col index of the piece</param>
        private void GetPositionForPiece(out int aRow, out int aCol)
        {
            for (int sRow = 0; sRow < 8; sRow++)
            {
                for (int sCol = 0; sCol < 8; sCol++)
                {
                    if (mChessGame.ChessBoard[sRow, sCol] == mChessGame.SelectedPiece)
                    {
                        aRow = sRow;
                        aCol = sCol;
                        return;
                    }
                }
            }

            aRow = -1;
            aCol = -1;
        }

        /// <summary>
        /// Adds a move to the list of moves
        /// </summary>
        /// <param name="aMove">The move made</param>
        /// <param name="aCheck">Did the move result in a check?</param>
        /// <param name="aPromotion">Did the move result in a promotion? (pawns only)</param>
        private void AddMoveToList(Move aMove, bool aCheck, string aPromotion = "")
        {
            string sMoveNumber = $"{(mChessGame.CurrentMove + 1) / 2}.";

            string sMoveText = "";

            if(aMove.MoveType == MoveTypeEnum.CastlingKingSide)
            {
                sMoveText = "O-O";
            }
            else if(aMove.MoveType == MoveTypeEnum.CastlingQueenSide)
            {
                sMoveText = "O-O-O";
            }
            else
            {
                bool sIsPawnCapture = aMove.PieceToMove is Pawn && aMove.PieceCaptured != null;
                string sFromFile = sIsPawnCapture ? $"{mChessGame.GetColumnCoordinate(aMove.FromCol)}" : "";
                string sCapture = aMove.PieceCaptured != null ? "x" : "";
                string sToSquare = $"{mChessGame.GetColumnCoordinate(aMove.ToCol)}{mChessGame.GetRowCoordinate(aMove.ToRow)}";
                string sCheck = aCheck ? "+" : "";
                string sEnPassant = aMove.MoveType == MoveTypeEnum.EnPassant ? " e.p" : "";

                sMoveText = $"{sFromFile}{aMove.PieceToMove.Abbreviation}{sCapture}{sToSquare}{aPromotion}{sCheck}{sEnPassant}";
            }

            if (aMove.PieceToMove.Color == ColorEnum.White)
            {
                string fullMove = string.Format("{0, -4} {1, -8}", sMoveNumber, sMoveText);
                Moves.Items.Add(fullMove);
            }
            else
            {
                string previous = Moves.Items[Moves.Items.Count - 1].ToString();
                Moves.Items[Moves.Items.Count - 1] = $"{previous} {sMoveText}";
            }
        }
        #endregion
    }
}