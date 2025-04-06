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

            //If not single player, hide the evaluation bar
            if (!aSinglePlayer)
            {
                EvaluationGrid.Width = 0;
            }

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
            var sEval = mChessGame.EvaluateBoard();

            // Convert to 0.0 to 1.0: -10 = 0 (white losing), +10 = 1 (white winning)
            double sWhitePortion = ((double)sEval.White / sEval.Total);
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
        /// <param name="aColTo">The col of the chess piece</param>
        private void HighlightValidMoves(int aRowFrom, int aColTo)
        {
            for (int sRow = 0; sRow < 8; sRow++)
            {
                for (int sCol = 0; sCol < 8; sCol++)
                {
                    if (mChessGame.IsValidMove(aRowFrom, aColTo, sRow, sCol))
                    {
                        HighlightCell(sRow, sCol, Colors.LightBlue, "ValidMove");
                    }
                }
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
        /// Moves the selected piece to the given row & col
        /// </summary>
        /// <param name="aRowTo">The row to move the piece to</param>
        /// <param name="aColTo">The col to move the piece to</param>
        private void MovePiece(int aRowTo, int aColTo)
        {
            //Keep track if a piece was captured
            bool sPieceCaptured = false;

            //Get the position of the selected piece
            int sRowFrom = -1;
            int sColFrom = -1;

            GetPositionForPiece(out sRowFrom, out sColFrom);

            //Make sure the move is valid
            if (!mChessGame.IsValidMove(sRowFrom, sColFrom, aRowTo, aColTo))
            {
                PlaySound("illegal-move.wav");
                ClearHighLights(false);
                mChessGame.SelectedPiece = null;
                return;
            }

            sPieceCaptured = mChessGame.ChessBoard[aRowTo, aColTo] != null;
            mChessGame.ChessBoard[sRowFrom, sColFrom] = null;
            mChessGame.ChessBoard[aRowTo, aColTo] = mChessGame.SelectedPiece;

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

            if (mChessGame.SelectedPiece is Pawn && ((mChessGame.ColorToMove == ColorEnum.White && aRowTo == 0) || (mChessGame.ColorToMove == ColorEnum.Black && aRowTo == 7)))
            {
                PawnPromotion sPawnPromotion = new PawnPromotion(mChessGame.ColorToMove);
                sPawnPromotion.ShowDialog();

                mChessGame.ChessBoard[aRowTo, aColTo] = sPawnPromotion.SelectedPromotionPiece;
                RefreshBoard();
                PlaySound("promotion.wav");
                sPromotion = $"={sPawnPromotion.SelectedPromotionPiece.Abbreviation}";
            }

            //Increment the move count
            mChessGame.CurrentMove++;

            mChessGame.ColorToMove = mChessGame.ColorToMove == ColorEnum.White ? ColorEnum.Black : ColorEnum.White;

            //Clear the old highlights
            ClearHighLights();

            //Highlight the move made
            HighlightCell(sRowFrom, sColFrom, Colors.Yellow, "LastMove");
            HighlightCell(aRowTo, aColTo, Colors.Yellow, "LastMove");

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
                    int sRow = -1;
                    int sCol = -1;

                    var sKingPos = mChessGame.GetKingPosition(mChessGame.ColorToMove);
                    HighlightCell(sKingPos.aRow, sKingPos.aCol, Colors.Red, "Check");
                }
            }
            else if (mChessGame.IsStalemate(mChessGame.ColorToMove))
            {
                PlaySound("game-end.wav");
                MessageBox.Show("Stalemate! The game is a draw!", "Game over!");
                Close();
            }

            //Add move to list
            AddMoveToList(mChessGame.SelectedPiece, sRowFrom, sColFrom, aRowTo, aColTo, sPieceCaptured, sCheck, sPromotion);

            mChessGame.SelectedPiece = null;

            //Update eval bar
            if(mSinglePlayer)
            {
                UpdateEvalBar();
            }

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
                ComputerMove();
            }
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
            var sBestMove = mChessGame.MiniMax(3, false, out sPositionsTried);

            sStopWatch.Stop();
            Console.WriteLine($"Tried {sPositionsTried} in {sStopWatch.ElapsedMilliseconds}ms");

            //Make the move
            if (sBestMove.RowFrom != -1 && sBestMove.ColFrom != -1 && sBestMove.RowTo != -1 && sBestMove.ColTo != -1)
            {
                mChessGame.SelectedPiece = mChessGame.ChessBoard[sBestMove.RowFrom, sBestMove.ColFrom];
                MovePiece(sBestMove.RowTo, sBestMove.ColTo);
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
        /// <param name="aChessPiece">The piece that moved</param>
        /// <param name="aRowFrom">The row the piece moved from</param>
        /// <param name="aColFrom">The col the piece moved from</param>
        /// <param name="aRowTo">The row the piece moved to</param>
        /// <param name="aColTo">The row the piece moved to</param>
        /// <param name="aTake">Did the moved piece capture a piece</param>
        /// <param name="aCheck">Did the move result in a check?</param>
        /// <param name="aPromotion">Did the move result in a promotion? (pawns only)</param>
            //TODO, castling kingside & queen side
            //TODO, checkmate need to end with #
        private void AddMoveToList(ChessPiece aChessPiece, int aRowFrom, int aColFrom, int aRowTo, int aColTo, bool aTake, bool aCheck, string aPromotion = "")
        {
            if ((mChessGame.CurrentMove + 1) % 2 == 1)
            {
                //White
                string sText = string.Format("{0, -4} {1, -8}", $"{(mChessGame.CurrentMove + 1) / 2}.", $"{(aChessPiece is Pawn && aTake ? mChessGame.GetColumnCoordinate(aColFrom) : "")}{aChessPiece.Abbreviation}{(aTake ? "x" : "")}{mChessGame.GetColumnCoordinate(aColTo)}{mChessGame.GetRowCoordinate(aRowTo)}{aPromotion}{(aCheck ? "+" : "")}");
                Moves.Items.Add(sText);
            }
            else
            {
                //Black
                string sText = Moves.Items[Moves.Items.Count - 1].ToString();
                Moves.Items.RemoveAt(Moves.Items.Count - 1);
                sText = sText + $" {(aChessPiece is Pawn && aTake ? mChessGame.GetColumnCoordinate(aColFrom) : "")}{aChessPiece.Abbreviation}{(aTake ? "x" : "")}{mChessGame.GetColumnCoordinate(aColTo)}{mChessGame.GetRowCoordinate(aRowTo)}{aPromotion}{(aCheck ? "+" : "")}";
                Moves.Items.Add(sText);
            }
        }
        #endregion
    }
}