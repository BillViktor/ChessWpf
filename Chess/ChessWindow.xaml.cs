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
    public partial class ChessWindow : Window, IDisposable
    {
        private ChessGame mChessGame = new ChessGame();
        private int mMinimaxDepth = 4;

        //Settings
        private TimerSetting mTimerSetting;
        private bool mSinglePlayer = false;

        private DispatcherTimer mTimerWhite;
        private TimeSpan mTimeWhite;

        private DispatcherTimer mTimerBlack;
        private TimeSpan mTimeBlack;

        private ColorEnum mSinglePlayerColor = ColorEnum.White;

        public ChessWindow(bool aSinglePlayer, TimerSetting aTimerSetting, ColorEnum aColor = ColorEnum.White, int aMinimaxDepth = 4)
        {
            InitializeComponent();

            mSinglePlayer = aSinglePlayer;
            mMinimaxDepth = aMinimaxDepth;
            mSinglePlayerColor = aColor;
            mTimerSetting = aTimerSetting;

            InitializeTimer();

            CreateChessBoard();

            PlaySound("game-start.wav");

            //Should we start the computer?
            if(aSinglePlayer && aColor == ColorEnum.Black)
            {
                Dispatcher.InvokeAsync(() => ComputerMove(), DispatcherPriority.Background);
            }
        }

        /// <summary>
        /// Updates the evaluation bar
        /// </summary>
        private void UpdateEvalBar()
        {
            var sEval = mChessGame.GetEvaluation();
            int sSum = Math.Abs(sEval.White - sEval.Black);

            double sWhitePortion = 0;
            double sBlackPortion = 0;

            if(sSum == 0)
            {
                sWhitePortion = 0.5;
                sBlackPortion = 0.5;
            }
            else
            {
                sWhitePortion = (double)sEval.White / sSum;

                //In end game the eval can be negative for white, we need to make sure its not negative
                if (sWhitePortion < 0)
                {
                    sWhitePortion = 0;
                }

                sBlackPortion = 1 - sWhitePortion;
            }

            // Convert to relative "star" values — total always adds to 1.0
            WhiteEvalRow.Height = new GridLength(sWhitePortion, GridUnitType.Star);
            BlackEvalRow.Height = new GridLength(sBlackPortion, GridUnitType.Star);
        }

        /// <summary>
        /// Initializes timers for both colors and starts the white timer
        /// </summary>
        private void InitializeTimer()
        {
            mTimeWhite = TimeSpan.FromSeconds(mTimerSetting.WhiteTimer);
            mTimeBlack = TimeSpan.FromSeconds(mTimerSetting.BlackTimer);

            if(mTimerSetting.WhiteEnabled)
            {
                WhiteTimer.Text = mTimeWhite.ToString(@"mm\:ss");

                mTimerWhite = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, delegate
                {
                    mTimeWhite = mTimeWhite.Add(TimeSpan.FromSeconds(-1));
                    WhiteTimer.Text = mTimeWhite.ToString(@"mm\:ss");
                    if (mTimeWhite == TimeSpan.Zero)
                    {
                        mTimerWhite.Stop();
                        MessageBox.Show("White ran out of time. Game over!", "Time's up!");
                        Close();
                    }

                }, Dispatcher.CurrentDispatcher);
            }

            if(mTimerSetting.BlackEnabled)
            {
                BlackTimer.Text = mTimeWhite.ToString(@"mm\:ss");

                mTimerBlack = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, delegate
                {
                    mTimeBlack = mTimeBlack.Add(TimeSpan.FromSeconds(-1));
                    BlackTimer.Text = mTimeBlack.ToString(@"mm\:ss");
                    if (mTimeBlack == TimeSpan.Zero)
                    {
                        mTimerBlack.Stop();
                        MessageBox.Show("Black ran out of time. Game over!", "Time's up!");
                        Close();
                    }
                }, Dispatcher.CurrentDispatcher);
            }

            mTimerWhite?.Stop();
            mTimerBlack?.Stop();
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
            if (mSinglePlayer && mChessGame.SelectedPiece == null && sSelectedPiece != null && sSelectedPiece.Color != mSinglePlayerColor)
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
                HighlightCell(sMove.ToRow, sMove.ToCol, sMove.MoveType == MoveTypeEnum.Capture ? Colors.IndianRed : Colors.LightBlue, "ValidMove");
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
        private void MovePiece(Move aMove, bool aComputerMove = false)
        {
            //Validate move
            if (!mChessGame.IsValidMove(aMove.FromRow, aMove.FromCol, aMove.ToRow, aMove.ToCol))
            {
                PlaySound("illegal-move.wav");
                ClearHighLights(false);
                mChessGame.SelectedPiece = null;
                return;
            }

            //Save the selected piece to move
            aMove.PieceToMove = mChessGame.SelectedPiece;

            //Handle promotions (only for UI)
            if (!aComputerMove && aMove.PieceToMove is Pawn sPawn && (aMove.ToRow == 0 || aMove.ToRow == 7))
            {
                var sPawnPromotion = new PawnPromotion(mChessGame.ColorToMove);
                sPawnPromotion.ShowDialog();
                aMove.PromotionPiece = sPawnPromotion.SelectedPromotionPiece;
                aMove.MoveType = MoveTypeEnum.Promotion;
            }

            //Format promotion string for move list
            string sPromotion = aMove.MoveType == MoveTypeEnum.Promotion
                ? $"={aMove.PromotionPiece.Abbreviation}"
                : "";

            //Get the move string before we make the move
            string sMoveString = GetMoveStringBeforeCheck(aMove, sPromotion);

            //Actually make the move (handles board state internally)
            mChessGame.MakeMove(aMove, false);

            //Update UI
            RefreshBoard();

            //Play sound
            if (aMove.MoveType == MoveTypeEnum.Capture || aMove.MoveType == MoveTypeEnum.EnPassant)
            {
                PlaySound("capture.wav");
            }
            else if (aMove.MoveType == MoveTypeEnum.Promotion)
            {
                PlaySound("promotion.wav");
            }
            else
            {
                PlaySound("move-self.wav");
            }

            //Clear highlights and show new move
            ClearHighLights();
            HighlightCell(aMove.FromRow, aMove.FromCol, Colors.Yellow, "LastMove");
            HighlightCell(aMove.ToRow, aMove.ToCol, Colors.Yellow, "LastMove");

            //Check for check/checkmate/stalemate
            bool sCheck = mChessGame.IsKingInCheck(mChessGame.ColorToMove);

            //Add the move to the list
            AddMoveToList(sMoveString, sCheck, aMove.MoveType == MoveTypeEnum.EnPassant);

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
            else if (mChessGame.IsThreefoldRule())
            {
                PlaySound("game-end.wav");
                MessageBox.Show("Stalemate! The game is a draw by Threefold rule.", "Game over!");
                Close();
            }
            else if(mChessGame.HalfMoveClock == 50)
            {
                PlaySound("game-end.wav");
                MessageBox.Show("Stalemate! The game is a draw by 50 move rule.", "Game over!");
                Close();
            }

            //UI and game flow
            UpdateEvalBar();
            ToggleTimer();

            if (mSinglePlayer && mChessGame.ColorToMove != mSinglePlayerColor)
            {
                Dispatcher.InvokeAsync(() => ComputerMove(), DispatcherPriority.Background);
            }
        }

        /// <summary>
        /// Toggles the timer for the current player
        /// </summary>
        private void ToggleTimer()
        {
            if (mChessGame.ColorToMove == ColorEnum.White)
            {
                if (mTimerSetting.BlackEnabled)
                {
                    mTimerBlack?.Stop();
                    mTimeBlack += TimeSpan.FromSeconds(mTimerSetting.BlackIncrement);
                    BlackTimer.Text = mTimeWhite.ToString(@"mm\:ss");
                }
                if (mTimerSetting.WhiteEnabled)
                {
                    mTimerWhite.Start();
                }
            }
            else
            {
                if (mTimerSetting.WhiteEnabled)
                {
                    mTimerWhite.Stop();
                    if (mChessGame.CurrentMove > 2)
                    {
                        mTimeWhite += TimeSpan.FromSeconds(mTimerSetting.BlackIncrement);
                    }
                    WhiteTimer.Text = mTimeWhite.ToString(@"mm\:ss");
                }
                if (mTimerSetting.BlackEnabled)
                {
                    mTimerBlack.Start();
                }
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
            bool sMaximizing = mChessGame.ColorToMove == ColorEnum.White ? true : false;
            var sBestMove = mChessGame.MiniMax(mMinimaxDepth, sMaximizing, out sPositionsTried, int.MinValue, int.MaxValue, true, mMinimaxDepth);

            sStopWatch.Stop();
            Console.WriteLine($"Tried {sPositionsTried} in {sStopWatch.ElapsedMilliseconds}ms");

            //Make the move
            if (sBestMove.BestMove != null)
            {
                mChessGame.SelectedPiece = mChessGame.ChessBoard[sBestMove.BestMove.FromRow, sBestMove.BestMove.FromCol];
                MovePiece(sBestMove.BestMove, true);
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
        private void AddMoveToList(string aMoveStringBeforeCheck, bool aCheck, bool aEnPassant)
        {
            string sCheck = aCheck ? "+" : "";
            string sEnPassant = aEnPassant ? " e.p" : "";

            aMoveStringBeforeCheck += $"{sCheck}{sEnPassant}";

            if (mChessGame.ColorToMove == ColorEnum.Black)
            {
                string sFullMove = string.Format("{0, -4} {1, -8}", mChessGame.CurrentMove-1, aMoveStringBeforeCheck);
                Moves.Items.Add(sFullMove);
            }
            else
            {
                string sPrevious = Moves.Items[Moves.Items.Count - 1].ToString();
                Moves.Items[Moves.Items.Count - 1] = $"{sPrevious} {aMoveStringBeforeCheck}";
            }
        }

        /// <summary>
        /// Gets the move string until the check mark
        /// </summary>
        /// <returns></returns>
        private string GetMoveStringBeforeCheck(Move aMove, string aPromotion)
        {
            string sMoveNumber = $"{(mChessGame.CurrentMove + 1) / 2}.";

            string sMoveText = "";

            if (aMove.MoveType == MoveTypeEnum.CastlingKingSide)
            {
                sMoveText = "O-O";
            }
            else if (aMove.MoveType == MoveTypeEnum.CastlingQueenSide)
            {
                sMoveText = "O-O-O";
            }
            else
            {
                bool sIsPawnCapture = aMove.PieceToMove is Pawn && aMove.PieceCaptured != null;
                string sFromFile = sIsPawnCapture ? $"{mChessGame.GetColumnCoordinate(aMove.FromCol)}" : "";
                string sCapture = aMove.PieceCaptured != null ? "x" : "";
                string sToSquare = $"{mChessGame.GetColumnCoordinate(aMove.ToCol)}{mChessGame.GetRowCoordinate(aMove.ToRow)}";

                sMoveText = $"{sFromFile}{aMove.PieceToMove.Abbreviation}{sCapture}{sToSquare}{aPromotion}";
            }

            return sMoveText;
        }

        /// <summary>
        /// Disposes the timers
        /// </summary>
        public void Dispose()
        {
            mTimerWhite?.Stop();
            mTimerBlack?.Stop();
            mTimerWhite = null;
            mTimerBlack = null;
        }
        #endregion
    }
}