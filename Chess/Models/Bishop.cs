namespace Chess.Models
{
    /// <summary>
    /// Class that represents a bishop chess piece.
    /// </summary>
    public class Bishop : ChessPiece 
    {
        public Bishop(ColorEnum aColor)
        {
            Color = aColor;
            Value = aColor == ColorEnum.White ? 300 : -300;
            
            if(aColor == ColorEnum.Black)
            {
                ImagePath = "pack://application:,,,/Resources/blackBishop.png";
            }
            else
            {
                ImagePath = "pack://application:,,,/Resources/whiteBishop.png";
            }

            Abbreviation = "B";
        }
    }
}
