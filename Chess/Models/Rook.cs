namespace Chess.Models
{
    public class Rook : ChessPiece
    {
        public Rook(ColorEnum aColor)
        {
            Color = aColor;
            Value = aColor == ColorEnum.White ? 5 : -5;

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
