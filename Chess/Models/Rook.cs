namespace Chess.Models
{
    /// <summary>
    /// Class that represents a rook chess piece.
    /// </summary>
    public class Rook : ChessPiece
    {
        public Rook(ColorEnum aColor)
        { 
            Color = aColor;
            Value = aColor == ColorEnum.White ? 500 : -500;

            if (aColor == ColorEnum.Black)
            {
                ImagePath = "pack://application:,,,/Resources/blackRook.png";
            }
            else
            {
                ImagePath = "pack://application:,,,/Resources/whiteRook.png";
            }

            Abbreviation = "R";
        }
    }
}
