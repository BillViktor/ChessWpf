namespace Chess.Models
{
    /// <summary>
    /// Class that represents a King chess piece.
    /// </summary>
    public class King : ChessPiece
    {
        public King(ColorEnum aColor)
        {
            Color = aColor; 
            Value = 0;

            if (aColor == ColorEnum.Black)
            {
                ImagePath = "pack://application:,,,/Resources/blackKing.png";
            }
            else
            {
                ImagePath = "pack://application:,,,/Resources/whiteKing.png";
            }

            Abbreviation = "K";
        }
    }
}
