﻿namespace Chess.Models
{
    /// <summary>
    /// Class that represents a queen chess piece.
    /// </summary>
    public class Queen : ChessPiece
    {
        public Queen(ColorEnum aColor)
        { 
            Color = aColor;
            Value = aColor == ColorEnum.White ? 900 : -900;

            if (aColor == ColorEnum.Black)
            {
                ImagePath = "pack://application:,,,/Resources/blackQueen.png";
            }
            else
            {
                ImagePath = "pack://application:,,,/Resources/whiteQueen.png";
            }

            Abbreviation = "Q";
        }
    }
}
