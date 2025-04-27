namespace Chess.Models
{
    /// <summary>
    /// Class representing a pawn chess piece.
    /// </summary>
    public class Pawn : ChessPiece
    {
        public Pawn(ColorEnum aColor)
        {
            Color = aColor;
            Value = aColor == ColorEnum.White ? 100 : -100;

            if (aColor == ColorEnum.Black)
            {
                ImagePath = "pack://application:,,,/Resources/blackPawn.png";
            }
            else
            {
                ImagePath = "pack://application:,,,/Resources/whitePawn.png";
            }

            Abbreviation = ""; //Pawns have no abbreviation :)
        }
    }
} 
