using Chess.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chess
{
    /// <summary>
    /// Interaction logic for PawnPromotion.xaml
    /// </summary>
    public partial class PawnPromotion : Window
    {
        //Fields
        private Models.ColorEnum mColor;
        private ChessPiece? mSelectedPromotionPiece = null;
        private List<ChessPiece> mPieceList = new List<ChessPiece>();

        //Properties
        public ChessPiece? SelectedPromotionPiece { get { return mSelectedPromotionPiece; } }

        public PawnPromotion(Models.ColorEnum aColor)
        {
            InitializeComponent();
            mColor = aColor;
            InitializeGui();
        }

        /// <summary>
        /// Add the chess pieces to choose from
        /// </summary>
        private void InitializeGui()
        {
            mPieceList.Add(new Queen(mColor));
            mPieceList.Add(new Rook(mColor));
            mPieceList.Add(new Bishop(mColor));
            mPieceList.Add(new Knight(mColor));

            for(int i = 0; i<mPieceList.Count; i++)
            {
                Image sPieceImage = new Image
                {
                    Source = new BitmapImage(new Uri(mPieceList[i].ImagePath)),
                    Stretch = Stretch.Uniform,

                };

                Grid.SetRow(sPieceImage, 0);
                Grid.SetColumn(sPieceImage, i);
                PawnPromotionGrid.Children.Add(sPieceImage);
            }

            //Set default selection to Queen (in case user closes window)
            mSelectedPromotionPiece = mPieceList.First();
        }

        /// <summary>
        /// Closes the window and returns the selected piece
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point sClickPosition = e.GetPosition(PawnPromotionGrid);
            double sCellWidth = PawnPromotionGrid.ActualWidth / 4;
            int sClickedColumn = (int)(sClickPosition.X / sCellWidth);
            mSelectedPromotionPiece = mPieceList[sClickedColumn];
            Close();
        }
    }
}
