namespace Chess.Models
{
    public class Knight : ChessPiece
    {
        public Knight(ColorEnum aColor)
        {
            Color = aColor;
            Value = aColor == ColorEnum.White ? 3 : -3;

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
