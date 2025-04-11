namespace Chess.Models
{
    /// <summary>
    /// Enumeration representing the type of move in chess.
    /// </summary>
    public enum MoveTypeEnum
    {
        Normal,
        Capture,
        EnPassant,
        CastlingQueenSide,
        CastlingKingSide,
        Promotion,
    }
}
