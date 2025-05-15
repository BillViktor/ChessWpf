namespace Chess.Models
{
    /// <summary>
    /// Class representing a Knight chess piece.
    /// </summary>
    public class Knight : ChessPiece
    { 
        public Knight(ColorEnum aColor)
        {
            Color = aColor;
            Value = aColor == ColorEnum.White ? 300 : -300;

            if (aColor == ColorEnum.Black)
            {
                ImagePath = "pack://application:,,,/Resources/blackKnight.png";
            }
            else
            {
                ImagePath = "pack://application:,,,/Resources/whiteKnight.png";
            }

            Abbreviation = "N";
        }
    }
}
