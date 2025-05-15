namespace Chess.Models
{
    /// <summary>
    /// Interface that represents a chess piece.
    /// </summary>
    public interface IChessPiece
    { 
        string Abbreviation { get; set; }
        ColorEnum Color { get; set; }
        string ImagePath { get; set; }
        int Value { get; set; }
    }
}